using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AP5PW_Helpdesk.Controllers
{
	public class AuthController : Controller
	{
		private readonly AppDbContext _db;
		private readonly IPasswordHasher<AP5PW_Helpdesk.Entities.User> _hasher;
		private readonly ILogger<AuthController> _logger;

		public AuthController(AppDbContext db, IPasswordHasher<Entities.User> hasher, ILogger<AuthController> logger)
		{
			_db			= db;
			_hasher		= hasher;
			_logger		= logger;
		}

		// GET /Auth/Login
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			_logger.LogInformation("GET /Auth/Login requested. ReturnUrl={ReturnUrl}", returnUrl);
			return View(new LoginVM { ReturnUrl = returnUrl });
		}

		// POST /Auth/Login
		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			_logger.LogInformation("Attempting login for user {UserName}", vm.UserName);
			if (!ModelState.IsValid) return View(vm);

			Entities.User? user = await _db.Users
				.Include(u => u.Role)
				.Include(u => u.Company)
				.FirstOrDefaultAsync(u => u.UserName == vm.UserName);

			if (user == null)
			{
				_logger.LogWarning("Login failed: user {UserName} not found", vm.UserName);
				ModelState.AddModelError(string.Empty, "Invalid username or password.");
				return View(vm);
			}

			bool ok = false;

			// 1) preferuj hash
			if (!string.IsNullOrEmpty(user.PasswordHash))
			{
				PasswordVerificationResult res = _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);
				if (res == PasswordVerificationResult.Success || res == PasswordVerificationResult.SuccessRehashNeeded)
				{
					ok = true;
					if (res == PasswordVerificationResult.SuccessRehashNeeded)
					{
						user.PasswordHash = _hasher.HashPassword(user, vm.Password);
						_db.Update(user);
						await _db.SaveChangesAsync();
					}
				}
			}
			// 2) fallback: plaintext -> prevest na hash
			else if (!string.IsNullOrEmpty(user.Password) && user.Password == vm.Password)
			{
				user.PasswordHash = _hasher.HashPassword(user, vm.Password);

				_db.Entry(user).Property(u => u.Password).IsModified = false;

				await _db.SaveChangesAsync();
				ok = true;
			}


			if (!ok)
			{
				_logger.LogWarning("Login failed: wrong password for user {UserName}", vm.UserName);
				ModelState.AddModelError(string.Empty, "Invalid username or password.");
				return View(vm);
			}

			var roleValue = (user.Role?.Name ?? "user").Trim().ToLowerInvariant();

			List<Claim> claims =
			[
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.UserName),
				new(ClaimTypes.Role, roleValue),
			];

			ClaimsIdentity identity = new(
				claims,
				CookieAuthenticationDefaults.AuthenticationScheme,
				ClaimTypes.Name,
				ClaimTypes.Role              
			);

			if (user.Company != null)
				claims.Add(new Claim("companyName", user.Company.Name));

			ClaimsIdentity? id = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));

			_logger.LogInformation("User {UserName} signed in successfully", user.UserName);

			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return Redirect(vm.ReturnUrl);

			return RedirectToAction("Index", "Home");
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Register()
		{
			RegisterVM? vm = new()
			{
				CompanyList = await _db.Companies
					.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
					.ToListAsync()
			};
			return View(vm);
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterVM vm)
		{
			if (!ModelState.IsValid)
			{
				vm.CompanyList = await _db.Companies
					.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
					.ToListAsync();
				return View(vm);
			}

			bool exists = await _db.Users.AnyAsync(u => u.UserName == vm.UserName);
			if (exists)
			{
				_logger.LogWarning("This {username} is already taken.", vm.UserName);
				ModelState.AddModelError(nameof(vm.UserName), "This username is already taken.");
				vm.CompanyList = await _db.Companies
					.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
					.ToListAsync();
				return View(vm);
			}

			Entities.Role? defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "user");
			if (defaultRole == null)
			{
				ModelState.AddModelError(string.Empty, "Default role 'user' not found in database.");
				return View(vm);
			}

			Entities.User? user = new()
			{
				UserName	= vm.UserName,
				LastName	= vm.LastName ?? "",
				CompanyId	= vm.CompanyId,
				RoleId		= defaultRole.Id
			};

			// Hash
			user.PasswordHash = _hasher.HashPassword(user, vm.Password);

			_db.Users.Add(user);
			await _db.SaveChangesAsync();

			TempData["Success"] = "Registration successful! You can now sign in.";
			return RedirectToAction("Login");
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			_logger.LogInformation("User {UserName} logging out", User?.Identity?.Name ?? "(anonymous?)");
			return RedirectToAction(nameof(Login));
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult AccessDenied()
		{
			_logger.LogWarning("AccessDenied shown to user {UserName}", User?.Identity?.Name ?? "(anon)");
			return View();
		}
	}
}

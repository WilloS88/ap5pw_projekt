using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.ViewModels.Auth;

namespace AP5PW_Helpdesk.Controllers
{
	public class AuthController : Controller
	{
		private readonly AppDbContext _db;
		private readonly IPasswordHasher<AP5PW_Helpdesk.Entities.User> _hasher;

		public AuthController(AppDbContext db, IPasswordHasher<AP5PW_Helpdesk.Entities.User> hasher)
		{
			_db = db;
			_hasher = hasher;
		}

		// GET /Auth/Login  -> jen zobrazi formular
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
			=> View(new LoginVM { ReturnUrl = returnUrl });

		// POST /Auth/Login -> overi, vytvori cookie
		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			var user = await _db.Users
				.Include(u => u.Role)
				.Include(u => u.Company)
				.FirstOrDefaultAsync(u => u.UserName == vm.UserName);

			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "Invalid username or password.");
				return View(vm);
			}

			bool ok = false;

			// 1) preferuj hash
			if (!string.IsNullOrEmpty(user.PasswordHash))
			{
				var res = _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);
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

				// neprepisuj plaintext na null, at DB s NOT NULL neselhava
				_db.Entry(user).Property(u => u.Password).IsModified = false;

				await _db.SaveChangesAsync();
				ok = true;
			}


			if (!ok)
			{
				ModelState.AddModelError(string.Empty, "Invalid username or password.");
				return View(vm);
			}

			var roleValue = (user.Role?.Name ?? "user").Trim().ToLowerInvariant();

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Role, roleValue),
			};

			var identity = new ClaimsIdentity(
				claims,
				CookieAuthenticationDefaults.AuthenticationScheme,
				ClaimTypes.Name,
				ClaimTypes.Role               // explicitně řekneme, co je role claim
			);

			if (user.Company != null)
				claims.Add(new Claim("companyName", user.Company.Name));

			var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));

			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return Redirect(vm.ReturnUrl);

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			return RedirectToAction(nameof(Login));
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult AccessDenied() => View();
	}
}

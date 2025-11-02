using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.Entities;
using AP5PW_Helpdesk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace AP5PW_Helpdesk.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _repo;
        private readonly AppDbContext _db;
		private readonly ILogger<UsersController> _logger;

		public UsersController(IUserRepository repo, AppDbContext db, ILogger<UsersController> logger)
        {
            _repo		= repo;
            _db			= db;
			_logger		= logger;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            List<UserVM>? vm = await _db.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Company)
                .OrderBy(u => u.LastName)
                .Select(u => new UserVM
                {
                    Id          = u.Id,
                    UserName    = u.UserName,
                    LastName    = u.LastName,
                    RoleId      = u.RoleId,
                    RoleName    = u.Role != null ? u.Role.Name : "",
                    CompanyId   = u.CompanyId,
					CompanyName = u.Company != null ? u.Company.Name : ""
				})
                .ToListAsync();

			_logger.LogDebug("Loaded {Count} users from database", vm.Count);
			return View(vm);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            User? entita = await _repo.GetByIdAsync(id);
            if (entita == null) 
			{
				_logger.LogWarning("User not found");
				return NotFound();
			}

			UserVM? vm = new()
            {
                Id          = entita.Id,
                UserName    = entita.UserName,
                LastName    = entita.LastName,
                RoleId      = entita.RoleId,
                RoleName    = entita.Role?.Name ?? "",
				CompanyId   = entita.CompanyId,
				CompanyName = entita.Company != null ? entita.Company.Name : ""
			};

			_logger.LogDebug("User details loaded successfully, UserName={UserName}", vm.UserName);
			return View(vm);
        }

		private async Task PopulateRolesAsync(int? selectedId = null)
		{
			List<Role> roles	= await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
			ViewBag.RoleList	= new SelectList(roles, "Id", "Name", selectedId);
		}

		private async Task PopulateCompaniesAsync(int? selectedId = null)
		{
			List<Company> companies		= await _db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
			ViewBag.CompanyList			= new SelectList(companies, "Id", "Name", selectedId);
		}

		// GET: /Users/Create
		public async Task<IActionResult> Create()
		{
			await PopulateRolesAsync();
			await PopulateCompaniesAsync();
			return View(new UserVM());
		}

		// POST: /Users/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserVM vm)
		{
			if (!ModelState.IsValid)
			{
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			if (await _repo.UsernameExistsAsync(vm.UserName))
			{
				ModelState.AddModelError(nameof(vm.UserName), "User with this login name already exists.");
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			User? user = new()
			{
				UserName    = vm.UserName,
				LastName    = vm.LastName ?? "",
				Password    = vm.Password!,     
				RoleId      = vm.RoleId,
				CompanyId   = vm.CompanyId ?? 0
			};

			await _repo.AddAsync(user);
			_logger.LogInformation("User created successfully: Username={Username}", user.UserName);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Users/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			User? entity = await _repo.GetByIdAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("Edit requested for non-existent user");
				return NotFound();
			}

			var vm = new UserVM
			{
				Id			= entity.Id,
				UserName	= entity.UserName,
				LastName	= entity.LastName,
				RoleId		= entity.RoleId,
				CompanyId	= entity.CompanyId
			};

			await PopulateRolesAsync(vm.RoleId);
			await PopulateCompaniesAsync(vm.CompanyId);
			return View(vm);
		}

		// POST: /Users/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, UserVM vm)
		{
			if (id != vm.Id) return BadRequest();

			if (!ModelState.IsValid)
			{
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			if (await _repo.UsernameExistsAsync(vm.UserName, excludeId: id))
			{
				ModelState.AddModelError(nameof(vm.UserName), "User with this login name already exists.");
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			User? entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			entity.UserName		= vm.UserName;
			entity.LastName		= vm.LastName ?? "";
			entity.RoleId		= vm.RoleId;
			entity.CompanyId	= vm.CompanyId ?? entity.CompanyId;

			if (!string.IsNullOrWhiteSpace(vm.Password))
				entity.Password = vm.Password; 

			await _repo.UpdateAsync(entity);
			_logger.LogInformation("User updated successfully: Username={Username}", entity.UserName);
			return RedirectToAction(nameof(Index));
		}


		// DELETE POST
		[HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
			_logger.LogInformation("User deleted successfully");
			return RedirectToAction(nameof(Index));
        }
    }
}

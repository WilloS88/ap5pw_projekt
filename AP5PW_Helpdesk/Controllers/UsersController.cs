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

        public UsersController(IUserRepository repo, AppDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var vm = await _db.Users
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

            return View(vm);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var entita = await _repo.GetByIdAsync(id);
            if (entita == null) return NotFound();

            var vm = new UserVM
            {
                Id          = entita.Id,
                UserName    = entita.UserName,
                LastName    = entita.LastName,
                RoleId      = entita.RoleId,
                RoleName    = entita.Role?.Name ?? "",
				CompanyId   = entita.CompanyId,
				CompanyName = entita.Company != null ? entita.Company.Name : ""
			};
            return View(vm);
        }

		private async Task PopulateRolesAsync(int? selectedId = null)
		{
			var roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
			ViewBag.RoleList = new SelectList(roles, "Id", "Name", selectedId);
		}

		private async Task PopulateCompaniesAsync(int? selectedId = null)
		{
			var companies = await _db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
			ViewBag.CompanyList = new SelectList(companies, "Id", "Name", selectedId);
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
				ModelState.AddModelError(nameof(vm.UserName), "Uživatel s tímto přihlašovacím jménem už existuje.");
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			var user = new User
			{
				UserName    = vm.UserName,
				LastName    = vm.LastName ?? "",
				Password    = vm.Password!,     // TODO: hash
				RoleId      = vm.RoleId,
				CompanyId   = vm.CompanyId ?? 0 // nebo udělej CompanyId v VM povinné (int)
			};

			await _repo.AddAsync(user);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Users/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var u = await _repo.GetByIdAsync(id);
			if (u == null) return NotFound();

			var vm = new UserVM
			{
				Id = u.Id,
				UserName = u.UserName,
				LastName = u.LastName,
				RoleId = u.RoleId,
				CompanyId = u.CompanyId
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
				ModelState.AddModelError(nameof(vm.UserName), "Uživatel s tímto přihlašovacím jménem už existuje.");
				await PopulateRolesAsync(vm.RoleId);
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			var u = await _repo.GetByIdAsync(id);
			if (u == null) return NotFound();

			u.UserName = vm.UserName;
			u.LastName = vm.LastName ?? "";
			u.RoleId = vm.RoleId;
			u.CompanyId = vm.CompanyId ?? u.CompanyId; // pokud je nullable; jinak int a [Required]

			if (!string.IsNullOrWhiteSpace(vm.Password))
				u.Password = vm.Password; // TODO: hash

			await _repo.UpdateAsync(u);
			return RedirectToAction(nameof(Index));
		}


		// DELETE POST
		[HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);	
            return RedirectToAction(nameof(Index));
        }
    }
}

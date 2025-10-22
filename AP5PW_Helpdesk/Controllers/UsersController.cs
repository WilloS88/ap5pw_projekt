using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.Entities;
using AP5PW_Helpdesk.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .OrderBy(u => u.Username)
                .Select(u => new UserVM
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : ""
                })
                .ToListAsync();

            return View(vm);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u == null) return NotFound();

            var vm = new UserVM
            {
                Id = u.Id,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? ""
            };
            return View(vm);
        }

        // CREATE GET
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
            return View(new UserVM());
        }

        // CREATE POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
                return View(dto);
            }

            if (await _repo.UsernameExistsAsync(dto.Username))
            {
                ModelState.AddModelError(nameof(dto.Username), "Uživatel s tímto přihlašovacím jménem už existuje.");
                ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
                return View(dto);
            }

            var entity = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                RoleId = dto.RoleId
            };

            await _repo.AddAsync(entity);
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
                Username = u.Username,
                // Password necháme prázdné – nechceme předvyplňovat hash
                RoleId = u.RoleId
            };

            ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
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
                ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
                return View(vm);
            }

            if (await _repo.UsernameExistsAsync(vm.Username, excludeId: id))
            {
                ModelState.AddModelError(nameof(vm.Username), "Uživatel s tímto přihlašovacím jménem už existuje.");
                ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
                return View(vm);
            }

            var u = await _repo.GetByIdAsync(id);
            if (u == null) return NotFound();

            u.Username = vm.Username;
            u.RoleId = vm.RoleId;

            // Heslo měň jen pokud bylo vyplněno
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                u.Password = vm.Password; // TODO: zahashovat
            }

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

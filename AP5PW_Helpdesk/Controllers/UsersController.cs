using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.ViewModels;

namespace AP5PW_Helpdesk.Controllers
{
	public class UsersController : Controller
	{
		private readonly AppDbContext _db;

		public UsersController(AppDbContext db)
		{
			_db = db;
		}

		// GET: /Users
		public async Task<IActionResult> Index()
		{
			var vm = await _db.Users
				.AsNoTracking()
				.Include(u => u.Role)                     // kvůli RoleName
				.OrderBy(u => u.Username)
				.Select(u => new UserVM
				{
					Id = u.Id,
					Username = u.Username,
					RoleId = u.RoleId,
					RoleName = u.Role != null ? u.Role.Name : ""
				})
				.ToListAsync();

			return View(vm); // Views/Users/Index.cshtml -> IEnumerable<UserDto>
		}

		// Zatím jen prázdné scaffolding akce (můžeš doplnit později)
		public IActionResult Create() => View();
		public IActionResult Edit(int id) => View();
		public IActionResult Details(int id) => View();
	}
}

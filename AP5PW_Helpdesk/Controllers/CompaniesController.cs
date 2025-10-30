using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.ViewModels;
using AP5PW_Helpdesk.Entities;

namespace AP5PW_Helpdesk.Controllers
{
	public class CompaniesController : Controller
	{
		private readonly ICompanyRepository _repo;
		public CompaniesController(ICompanyRepository repo) => _repo = repo;

		// GET: /Companies
		public async Task<IActionResult> Index()
		{
			List<CompanyVM>? vm = [ ..(await _repo.GetAllAsync())
				.Select(c => new CompanyVM
				{
					Id			= c.Id,
					Name		= c.Name,
					Street		= c.Street,
					City		= c.City,
					Postcode	= c.Postcode
				})];

			return View(vm);
		}

		// GET: /Companies/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			CompanyVM? vm = new()
			{
				Id			= entity.Id,
				Name		= entity.Name,
				City		= entity.City,
				Street		= entity.Street,
				Postcode	= entity.Postcode
			};
			return View(vm);
		}

		// GET: /Companies/Create
		public IActionResult Create() => View(new CompanyVM());

		// POST: /Companies/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CompanyVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			if (await _repo.NameExistsAsync(vm.Name))
			{
				ModelState.AddModelError(nameof(vm.Name), "Company with this name already exists.");
				return View(vm);
			}

			Company? entity = new()
			{
				Name		= vm.Name,
				City		= vm.City,
				Street		= vm.Street,
				Postcode	= vm.Postcode
			};

			await _repo.AddAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Companies/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Company? entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			CompanyVM? vm = new()
			{
				Id			= entity.Id,
				Name		= entity.Name,
				City		= entity.City,
				Street		= entity.Street,
				Postcode	= entity.Postcode
			};
			return View(vm);
		}

		// POST: /Companies/Edit/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, CompanyVM vm)
		{
			if (id != vm.Id) return BadRequest();
			if (!ModelState.IsValid) return View(vm);

			if (await _repo.NameExistsAsync(vm.Name, excludeId: id))
			{
				ModelState.AddModelError(nameof(vm.Name), "Company with this name already exists..");
				return View(vm);
			}

			Company? entity = new()
			{
				Id			= vm.Id,
				Name		= vm.Name,
				City		= vm.City,
				Street		= vm.Street,
				Postcode	= vm.Postcode
			};

			await _repo.UpdateAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Companies/Delete/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}

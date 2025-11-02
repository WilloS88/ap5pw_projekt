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
		private readonly ILogger<CompaniesController> _logger;

		public CompaniesController(ICompanyRepository repo, ILogger<CompaniesController> logger)
		{
			_repo		= repo;
			_logger		= logger;
		}

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

			_logger.LogDebug("Loaded {Count} companies from repository", vm.Count);
			return View(vm);
		}

		// GET: /Companies/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity == null) 
			{
				_logger.LogWarning("Company with ID={Id} not found", id);
				return NotFound();
			}

			CompanyVM? vm = new()
			{
				Id			= entity.Id,
				Name		= entity.Name,
				City		= entity.City,
				Street		= entity.Street,
				Postcode	= entity.Postcode
			};

			_logger.LogDebug("Company details loaded for Name={Name}", vm.Name);
			return View(vm);
		}

		// GET: /Companies/Create
		public IActionResult Create() => View(new CompanyVM());

		// POST: /Companies/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CompanyVM vm)
		{
			if (!ModelState.IsValid) 
			{
				_logger.LogWarning("Invalid model state during company creation");
				return View(vm);
			}

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
			_logger.LogInformation("New company created successfully");
			return RedirectToAction(nameof(Index));
		}

		// GET: /Companies/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Company? entity = await _repo.GetByIdAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("Edit page requested for non-existent company ID={Id}", id);
				return NotFound();
			}

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
			_logger.LogInformation("Company updated successfully: Name={Name}", entity.Name);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Companies/Delete/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			_logger.LogInformation("Company deleted successfully");
			return RedirectToAction(nameof(Index));
		}
	}
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.ViewModels;
using AP5PW_Helpdesk.Entities;

namespace AP5PW_Helpdesk.Controllers
{
	public class WarehousesController : Controller
	{
		private readonly IWarehouseRepository _repo;
		private readonly AppDbContext _db;
		public WarehousesController(IWarehouseRepository repo, AppDbContext db)
		{
			_repo	= repo;
			_db		= db;
		}

		// helper: companies SelectList
		private async Task PopulateCompaniesAsync(int? selectedId = null)
		{
			List<Company> companies		= await _db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
			ViewBag.CompanyList			= new SelectList(companies, "Id", "Name", selectedId);
		}

		// GET: /Warehouses
		public async Task<IActionResult> Index()
		{
			List<Warehouse>? list		= await _repo.GetAllAsync();
			List<WarehouseVM>? vm		= [.. list.Select(vm => new WarehouseVM
			{
				Id				= vm.Id,
				Name			= vm.Name,
				CompanyId		= vm.CompanyId,
				CompanyName		= vm.Company != null ? vm.Company.Name : ""
			})];

			return View(vm);
		}

		// GET: /Warehouses/Details/5
		public async Task<IActionResult> Details(int id)
		{
			Warehouse? entity = await _db.Warehouses
				.AsNoTracking()
				.Include(w => w.Company)
				.Include(w => w.WarehousesGoods)
					.ThenInclude(wg => wg.Goods)
				.FirstOrDefaultAsync(w => w.Id == id);

			if (entity == null) return NotFound();

			WarehouseDetailVM? vm = new()
			{
				Id				= entity.Id,
				Name			= entity.Name,
				CompanyId		= entity.CompanyId,
				CompanyName		= entity.Company?.Name ?? "",

				Stock = [ ..entity.WarehousesGoods
					.OrderBy(wg => wg.Goods.Name)
					.Select(wg => new WarehouseStockItemVM
					{
						GoodId		= wg.GoodsId,
						GoodName	= wg.Goods.Name,
						Quantity	= wg.Quantity
					})]
			};
			return View(vm);
		}


		// GET: /Warehouses/Create
		public async Task<IActionResult> Create()
		{
			await PopulateCompaniesAsync();
			return View(new WarehouseVM());
		}

		// POST: /Warehouses/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(WarehouseVM vm)
		{
			if (!ModelState.IsValid)
			{
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			if (await _repo.NameExistsInCompanyAsync(vm.Name, vm.CompanyId))
			{
				ModelState.AddModelError(nameof(vm.Name), "This warehouse already exists in this selected company.");
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			Warehouse? entity = new()
			{
				Name		= vm.Name,
				CompanyId	= vm.CompanyId
			};

			await _repo.AddAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Warehouses/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Warehouse? entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			WarehouseVM? vm = new()
			{
				Id			= entity.Id,
				Name		= entity.Name,
				CompanyId	= entity.CompanyId
			};

			await PopulateCompaniesAsync(vm.CompanyId);
			return View(vm);
		}

		// POST: /Warehouses/Edit/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, WarehouseVM vm)
		{
			if (id != vm.Id) return BadRequest();
			if (!ModelState.IsValid)
			{
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			if (await _repo.NameExistsInCompanyAsync(vm.Name, vm.CompanyId, excludeId: id))
			{
				ModelState.AddModelError(nameof(vm.Name), "This warehouse already exists in this selected company.");
				await PopulateCompaniesAsync(vm.CompanyId);
				return View(vm);
			}

			Warehouse? entity = new()
			{
				Id			= vm.Id,
				Name		= vm.Name,
				CompanyId	= vm.CompanyId
			};

			await _repo.UpdateAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Warehouses/Delete/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}

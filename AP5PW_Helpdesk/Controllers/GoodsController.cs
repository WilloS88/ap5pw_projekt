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
    public class GoodsController : Controller
    {
        private readonly IGoodRepository _repo;
        private readonly AppDbContext _db;

        public GoodsController(IGoodRepository repo, AppDbContext db)
        {
            _repo	= repo;
            _db		= db;
        }

		// GET: /Goods
		public async Task<IActionResult> Index()
		{
			List<GoodVM>? vm = [ ..(await _repo.GetAllAsync())
				.Select(entity => new GoodVM
				{
					Id			= entity.Id,
					Name		= entity.Name,
					Price		= entity.Price,
					ProductNum	= entity.ProductNum
				})];

			return View(vm);
		}

		// GET: /Goods/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			GoodVM? vm = new()
			{
				Id			= entity!.Id,
				Name		= entity.Name,
				Price		= entity.Price,
				ProductNum	= entity.ProductNum
			};
			return View(vm);
		}

		// GET: /Goods/Create
		public IActionResult Create() => View(new GoodVM());

		// POST: /Goods/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(GoodVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			if (!string.IsNullOrWhiteSpace(vm.ProductNum) &&
				await _repo.ProductNumExistsAsync(vm.ProductNum))
			{
				ModelState.AddModelError(nameof(vm.ProductNum), "This product number already exists.");
				return View(vm);
			}

			Good? entity = new()
			{
				Name		= vm.Name,
				Price		= vm.Price,
				ProductNum	= vm.ProductNum
			};

			await _repo.AddAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Goods/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Good? entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			GoodVM? vm = new()
			{
				Id			= entity!.Id,
				Name		= entity.Name,
				Price		= entity.Price,
				ProductNum	= entity.ProductNum
			};
			return View(vm);
		}

		// POST: /Goods/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, GoodVM vm)
		{
			if (id != vm.Id) return BadRequest();
			if (!ModelState.IsValid) return View(vm);

			if (!string.IsNullOrWhiteSpace(vm.ProductNum) &&
				await _repo.ProductNumExistsAsync(vm.ProductNum, excludeId: id))
			{
				ModelState.AddModelError(nameof(vm.ProductNum), "This product number already exists.");
				return View(vm);
			}

			Good? entity = await _repo.GetByIdAsync(id);
			if (entity == null) return NotFound();

			entity!.Name		= vm.Name;
			entity.Price		= vm.Price;
			entity.ProductNum	= vm.ProductNum;

			await _repo.UpdateAsync(entity);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Goods/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}

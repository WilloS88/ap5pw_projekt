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
		private readonly ILogger<GoodsController> _logger;

		public GoodsController(IGoodRepository repo, AppDbContext db, ILogger<GoodsController> logger)
        {
            _repo	= repo;
            _db		= db;
			_logger = logger;

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

			_logger.LogDebug("Loaded {Count} goods from repository", vm.Count);
			return View(vm);
		}

		// GET: /Goods/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var entity = await _repo.GetByIdAsync(id);
			if (entity == null) 
			{
				_logger.LogWarning("Goods.Details: product with ID={Id} not found", id);
				return NotFound();
			}

			GoodVM? vm = new()
			{
				Id			= entity!.Id,
				Name		= entity.Name,
				Price		= entity.Price,
				ProductNum	= entity.ProductNum
			};

			_logger.LogDebug("Goods.Details loaded successfully for Name={Name}", vm.Name);
			return View(vm);
		}

		// GET: /Goods/Create
		public IActionResult Create() 
		{
			_logger.LogInformation("User {User} opened Goods.Create page", User?.Identity?.Name ?? "unknown");
			return View(new GoodVM());
		}

		// POST: /Goods/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(GoodVM vm)
		{
			if (!ModelState.IsValid)
			{
				_logger.LogWarning("Invalid model state while creating good");
				return View(vm);
			}

			if (!string.IsNullOrWhiteSpace(vm.ProductNum) &&
				await _repo.ProductNumExistsAsync(vm.ProductNum))
			{
				_logger.LogWarning("Failed to create product because it already exists");
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
			_logger.LogInformation("New product created successfully: Name={Name}", entity.Name);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Goods/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Good? entity = await _repo.GetByIdAsync(id);
			if (entity == null) 
			{
				_logger.LogWarning("Goods.Edit: product not found");
				return NotFound();
			}

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
			if (id != vm.Id) 
			{
				_logger.LogError("Goods.Edit: ID mismatch");
				return BadRequest();
			}
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
			_logger.LogInformation("Product updated successfully: Name={Name}", entity.Name);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Goods/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			_logger.LogInformation("Product with ID={Id} deleted successfully", id);
			return RedirectToAction(nameof(Index));
		}
	}
}

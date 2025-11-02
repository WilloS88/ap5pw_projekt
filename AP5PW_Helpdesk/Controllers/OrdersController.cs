using AP5PW_Helpdesk.Data;
using AP5PW_Helpdesk.Data.Repositories;
using AP5PW_Helpdesk.Entities;
using AP5PW_Helpdesk.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AP5PW_Helpdesk.Controllers
{
	public class OrdersController : Controller
	{
		private readonly IOrderRepository _repo;
		private readonly AppDbContext _db;
		private readonly ILogger<OrdersController> _logger;

		public OrdersController(IOrderRepository repo, AppDbContext db, ILogger<OrdersController> logger)
		{
			_repo		= repo;
			_db			= db;
			_logger		= logger;
		}
		
		private async Task PopulateSelectsAsync(int? companyId = null, int? userId = null, int? warehouseId = null)
		{
			List<Company>? companies	= await _db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
			List<User>? users			= await _db.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
			List<Good>? goods			= await _db.Goods.AsNoTracking().OrderBy(g => g.Name).ToListAsync();

			List<Warehouse>? warehouses = companyId.HasValue
				? await _db.Warehouses.AsNoTracking().Where(w => w.CompanyId == companyId).OrderBy(w => w.Name).ToListAsync()
				: [];

			ViewBag.CompanyList		= new SelectList(companies, "Id", "Name", companyId);
			ViewBag.UserList		= new SelectList(users, "Id", "UserName", userId);
			ViewBag.GoodList		= new SelectList(goods, "Id", "Name");
			ViewBag.WarehouseList	= new SelectList(warehouses, "Id", "Name", warehouseId);

			_logger.LogDebug("Select lists populated successfully");
		}

		// AJAX: warehouses for company
		[HttpGet]
		public async Task<IActionResult> WarehousesForCompany(int companyId)
		{
			var data = await _db.Warehouses
				.AsNoTracking()
				.Where(w => w.CompanyId == companyId)
				.OrderBy(w => w.Name)
				.Select(w => new { id = w.Id, name = w.Name })
				.ToListAsync();

			_logger.LogDebug("Loaded {Count} warehouses", data.Count);
			return Json(data);
		}


		// GET: /Orders
		public async Task<IActionResult> Index()
		{
			List<Order> list = await _repo.GetAllAsync();
			List<OrderVM>? vm = [ ..list.Select(o => new OrderVM
			{
				Id				= o.Id,
				CompanyId		= o.CompanyId,
				CompanyName		= o.Company?.Name ?? "",
				UserId			= o.UserId,
				UserName		= o.User?.UserName ?? "",
				WarehouseId		= o.WarehouseId,
				WarehouseName	= o.Warehouse?.Name ?? "",
				ExpeditionDate	= o.ExpeditionDate,
				IsBuyOrder		= o.IsBuyOrder,
				ItemsCount		= o.OrderedGoods?.Count ?? 0,
				TotalPrice		= null
			})];

			_logger.LogDebug("Loaded {Count} orders", vm.Count);
			return View(vm);
		}

		// GET: /Orders/Details/5
		public async Task<IActionResult> Details(int id)
		{
			Order? entity = await _repo.GetByIdWithItemsAsync(id);
			if (entity == null) return NotFound();

			OrderEditVM? vm = new()
			{
				Id				= entity.Id,
				CompanyId		= entity.CompanyId,
				CompanyName		= entity.Company?.Name ?? "",
				UserId			= entity.UserId,
				UserName		= entity.User?.UserName ?? "",
				ExpeditionDate	= entity.ExpeditionDate,
				IsBuyOrder		= entity.IsBuyOrder,

				Items = [ ..entity.OrderedGoods
					.OrderBy(i => i.Good!.Name)
					.Select(i => new OrderItemVM
					{
						GoodId		= i.GoodId,
						GoodName	= i.Good!.Name,
						Quantity	= i.Quantity
					})
				]
			};

			_logger.LogDebug("Order details loaded successfully");
			await PopulateSelectsAsync(entity.CompanyId, entity.UserId);
			return View(vm);
		}

		// GET: /Orders/Create
		public async Task<IActionResult> Create()
		{
			await PopulateSelectsAsync();
			return View(new OrderEditVM { IsBuyOrder = true, Items = new List<OrderItemVM>() });
		}

		// POST: /Orders/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(OrderEditVM vm)
		{
			if (vm.Items == null || vm.Items.Count == 0)
				ModelState.AddModelError(nameof(vm.Items), "Order needs to have atleast one product.");

			// Warehouse must belong to company
			if (vm.CompanyId > 0 && vm.WarehouseId > 0)
			{
				bool ok = await _db.Warehouses.AnyAsync(w => w.Id == vm.WarehouseId && w.CompanyId == vm.CompanyId);
				if (!ok) ModelState.AddModelError(nameof(vm.WarehouseId), "Zvoleny sklad nepatri do vybrane firmy.");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning("Invalid model state during order creation");
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId, vm.WarehouseId);
				return View(vm);
			}

			Order? order = new()
			{
				CompanyId		= vm.CompanyId,
				UserId			= vm.UserId,
				ExpeditionDate	= vm.ExpeditionDate,
				IsBuyOrder		= vm.IsBuyOrder,
				WarehouseId		= vm.WarehouseId
			};

			List<OrderedGoods>? items = [ ..vm.Items!.Select(i => new OrderedGoods
			{
				GoodId		= i.GoodId,
				Quantity	= i.Quantity
			})];

			try
			{
				await _repo.AddAsync(order, items);
				_logger.LogInformation("Order created successfully");
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogError(ex, "Error creating order: {Message}", ex.Message);
				ModelState.AddModelError(string.Empty, ex.Message);
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId, vm.WarehouseId);
				return View(vm);
			}
		}

		// GET: /Orders/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var entity = await _repo.GetByIdWithItemsAsync(id);
			if (entity == null)
			{
				_logger.LogWarning("Order not found");
				return NotFound();
			}

			OrderEditVM? vm = new()
			{
				Id				= entity.Id,
				CompanyId		= entity.CompanyId,
				UserId			= entity.UserId,
				ExpeditionDate	= entity.ExpeditionDate,
				IsBuyOrder		= entity.IsBuyOrder,
				WarehouseId		= entity.WarehouseId,
				Items = [ ..entity.OrderedGoods.OrderBy(i => i.Good!.Name).Select(i => new OrderItemVM
				{
					GoodId		= i.GoodId,
					GoodName	= i.Good!.Name,
					Quantity	= i.Quantity
				})]
			};

			await PopulateSelectsAsync(vm.CompanyId, vm.UserId, vm.WarehouseId);
			return View(vm);
		}

		// POST: /Orders/Edit/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, OrderEditVM vm)
		{
			if (id != vm.Id) return BadRequest();

			if (vm.Items == null || vm.Items.Count == 0)
				ModelState.AddModelError(nameof(vm.Items), "The order need to have atleast one item.");

			if (vm.CompanyId > 0 && vm.WarehouseId > 0)
			{
				bool ok = await _db.Warehouses.AnyAsync(w => w.Id == vm.WarehouseId && w.CompanyId == vm.CompanyId);
				if (!ok) ModelState.AddModelError(nameof(vm.WarehouseId), "Selected warehouse does not belog to selected company.");
			}

			if (!ModelState.IsValid)
			{
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId, vm.WarehouseId);
				return View(vm);
			}

			Order? order = new()
			{
				Id				= vm.Id,
				CompanyId		= vm.CompanyId,
				UserId			= vm.UserId,
				ExpeditionDate	= vm.ExpeditionDate,
				IsBuyOrder		= vm.IsBuyOrder,
				WarehouseId		= vm.WarehouseId
			};

			List<OrderedGoods> items = [ ..vm.Items!.Select(i => new OrderedGoods
			{
				OrderId		= vm.Id,
				GoodId		= i.GoodId,
				Quantity	= i.Quantity
			})];

			try
			{
				await _repo.UpdateAsync(order, items);
				_logger.LogInformation("Order updated successfully");
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				_logger.LogError(ex, "Error updating order: {Message}", ex.Message);
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId, vm.WarehouseId);
				return View(vm);
			}
		}

		// POST: /Orders/Delete/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			_logger.LogInformation("Order deleted successfully");
			return RedirectToAction(nameof(Index));
		}
	}
}

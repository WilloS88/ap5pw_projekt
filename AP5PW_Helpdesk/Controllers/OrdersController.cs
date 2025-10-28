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

		public OrdersController(IOrderRepository repo, AppDbContext db)
		{
			_repo	= repo;
			_db		= db;
		}

		private async Task PopulateSelectsAsync(int? companyId = null, int? userId = null)
		{
			List<Company>? companies	= await _db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
			List<User>? users			= await _db.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
			List<Good>? goods			= await _db.Goods.AsNoTracking().OrderBy(g => g.Name).ToListAsync();

			ViewBag.CompanyList		= new SelectList(companies, "Id", "Name", companyId);
			ViewBag.UserList		= new SelectList(users, "Id", "UserName", userId);
			ViewBag.GoodList		= new SelectList(goods, "Id", "Name");
		}

		// GET: /Orders
		public async Task<IActionResult> Index()
		{
			List<Order>? list = await _repo.GetAllAsync();
			List<OrderVM>? vm = [.. list.Select(vm => new OrderVM
			{
				Id				= vm.Id,
				CompanyId		= vm.CompanyId,
				CompanyName		= vm.Company?.Name ?? "",
				UserId			= vm.UserId,
				UserName		= vm.User?.UserName ?? "",
				ExpeditionDate	= vm.ExpeditionDate,
				IsBuyOrder		= vm.IsBuyOrder,
				ItemsCount		= vm.OrderedGoods?.Count ?? 0,
				TotalPrice		= null
			})];

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
						// UnitPrice = i.UnitPrice
					})
				]
			};

			await PopulateSelectsAsync(entity.CompanyId, entity.UserId);
			return View(vm);
		}

		// GET: /Orders/Create
		public async Task<IActionResult> Create()
		{
			await PopulateSelectsAsync();
			return View(new OrderEditVM { IsBuyOrder = true, Items = [new()] });
		}

		// POST: /Orders/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(OrderEditVM vm)
		{
			if (vm.Items == null || vm.Items.Count == 0)
				ModelState.AddModelError(nameof(vm.Items), "Objednávka musí mít alespoň jednu položku.");

			if (!ModelState.IsValid)
			{
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId);
				return View(vm);
			}

			// TODO: Pro prodejni objednávku overit skladovou dostupnost.

			Order? order = new()
			{
				CompanyId		= vm.CompanyId,
				UserId			= vm.UserId,
				ExpeditionDate	= vm.ExpeditionDate,
				IsBuyOrder		= vm.IsBuyOrder
			};

			List<OrderedGoods>? items = [ ..vm.Items!
				.Select(i => new OrderedGoods
				{
					GoodId		= i.GoodId,
					Quantity	= i.Quantity
					// UnitPrice = i.UnitPrice
				})
			];

			await _repo.AddAsync(order, items);
			return RedirectToAction(nameof(Index));
		}

		// GET: /Orders/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			Order? entity = await _repo.GetByIdWithItemsAsync(id);
			if (entity == null) return NotFound();

			OrderEditVM? vm = new()
			{
				Id				= entity.Id,
				CompanyId		= entity.CompanyId,
				UserId			= entity.UserId,
				ExpeditionDate	= entity.ExpeditionDate,
				IsBuyOrder		= entity.IsBuyOrder,
				Items = [ ..entity.OrderedGoods
					.OrderBy(i => i.Good!.Name)
					.Select(i => new OrderItemVM
					{
						GoodId		= i.GoodId,
						GoodName	= i.Good!.Name,
						Quantity	= i.Quantity
						// UnitPrice = i.UnitPrice
					})
				]
			};

			await PopulateSelectsAsync(vm.CompanyId, vm.UserId);
			return View(vm);
		}

		// POST: /Orders/Edit/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, OrderEditVM vm)
		{
			if (id != vm.Id) return BadRequest();

			if (vm.Items == null || vm.Items.Count == 0)
				ModelState.AddModelError(nameof(vm.Items), "Objednávka musí mít alespoň jednu položku.");

			if (!ModelState.IsValid)
			{
				await PopulateSelectsAsync(vm.CompanyId, vm.UserId);
				return View(vm);
			}

			Order? order = new()
			{
				Id				= vm.Id,
				CompanyId		= vm.CompanyId,
				UserId			= vm.UserId,
				ExpeditionDate	= vm.ExpeditionDate,
				IsBuyOrder		= vm.IsBuyOrder
			};

			List<OrderedGoods>? items = [ ..vm.Items!
				.Select(i => new OrderedGoods
				{
					OrderId		= vm.Id,
					GoodId		= i.GoodId,
					Quantity	= i.Quantity
					// UnitPrice = i.UnitPrice
				})
			];

			await _repo.UpdateAsync(order, items);
			return RedirectToAction(nameof(Index));
		}

		// POST: /Orders/Delete/5
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _repo.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AP5PW_Helpdesk.Entities;
using Microsoft.EntityFrameworkCore;

namespace AP5PW_Helpdesk.Data.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly AppDbContext _db;
		public OrderRepository(AppDbContext db) => _db = db;

		public Task<List<Order>> GetAllAsync() =>
			_db.Orders
				.AsNoTracking()
				.Include(o => o.Company)
				.Include(o => o.User)
				.Include(o => o.Warehouse)
				.Include(o => o.OrderedGoods)
				.ToListAsync();

		public Task<Order?> GetByIdWithItemsAsync(int id) =>
			_db.Orders
				.Include(o => o.Company)
				.Include(o => o.User)
				.Include(o => o.Warehouse)
				.Include(o => o.OrderedGoods).ThenInclude(og => og.Good)
				.FirstOrDefaultAsync(o => o.Id == id);

		// CHANGED: IEnumerable
		public async Task AddAsync(Order order, IEnumerable<OrderedGoods> items)
		{
			var itemsList = items?.ToList() ?? new List<OrderedGoods>();

			// Validate company-warehouse relation
			bool belongs = await _db.Warehouses.AnyAsync(w => w.Id == order.WarehouseId && w.CompanyId == order.CompanyId);
			if (!belongs) throw new InvalidOperationException("Vybrany sklad nepatri do zvolene firmy.");

			using var tx = await _db.Database.BeginTransactionAsync();

			_db.Orders.Add(order);
			await _db.SaveChangesAsync();

			foreach (var it in itemsList)
			{
				it.OrderId = order.Id;
				_db.OrderedGoods.Add(it);
			}
			await _db.SaveChangesAsync();

			// Adjust stock
			foreach (var it in itemsList)
			{
				int delta = order.IsBuyOrder ? it.Quantity : -it.Quantity;
				await ApplyStockDelta(order.WarehouseId, it.GoodId, delta, validateForSell: !order.IsBuyOrder);
			}

			await tx.CommitAsync();
		}


		public async Task UpdateAsync(Order newOrder, IEnumerable<OrderedGoods> newItems)
		{
			var newItemsList = newItems?.ToList() ?? new List<OrderedGoods>();

			var dbOrder = await _db.Orders
				.Include(o => o.OrderedGoods)
				.FirstOrDefaultAsync(o => o.Id == newOrder.Id);

			if (dbOrder == null) throw new InvalidOperationException("Objednavka nenalezena.");

			bool belongs = await _db.Warehouses.AnyAsync(w => w.Id == newOrder.WarehouseId && w.CompanyId == newOrder.CompanyId);
			if (!belongs) throw new InvalidOperationException("Vybrany sklad nepatri do zvolene firmy.");

			using var tx = await _db.Database.BeginTransactionAsync();

			foreach (var old in dbOrder.OrderedGoods)
			{
				int delta = dbOrder.IsBuyOrder ? -old.Quantity : +old.Quantity;
				await ApplyStockDelta(dbOrder.WarehouseId, old.GoodId, delta, validateForSell: false);
			}

			_db.OrderedGoods.RemoveRange(dbOrder.OrderedGoods);
			await _db.SaveChangesAsync();

			foreach (var it in newItemsList)
			{
				it.OrderId = dbOrder.Id;
				_db.OrderedGoods.Add(it);
			}
			await _db.SaveChangesAsync();

			dbOrder.CompanyId = newOrder.CompanyId;
			dbOrder.UserId = newOrder.UserId;
			dbOrder.ExpeditionDate = newOrder.ExpeditionDate;
			dbOrder.IsBuyOrder = newOrder.IsBuyOrder;
			dbOrder.WarehouseId = newOrder.WarehouseId;
			await _db.SaveChangesAsync();

			foreach (var it in newItemsList)
			{
				int delta = dbOrder.IsBuyOrder ? it.Quantity : -it.Quantity;
				await ApplyStockDelta(dbOrder.WarehouseId, it.GoodId, delta, validateForSell: !dbOrder.IsBuyOrder);
			}

			await tx.CommitAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var order = await _db.Orders
				.Include(o => o.OrderedGoods)
				.FirstOrDefaultAsync(o => o.Id == id);
			if (order == null) return;

			using var tx = await _db.Database.BeginTransactionAsync();

			// revert stock impact
			foreach (var it in order.OrderedGoods)
			{
				int delta = order.IsBuyOrder ? -it.Quantity : +it.Quantity;
				await ApplyStockDelta(order.WarehouseId, it.GoodId, delta, validateForSell: order.IsBuyOrder);
			}

			_db.OrderedGoods.RemoveRange(order.OrderedGoods);
			_db.Orders.Remove(order);
			await _db.SaveChangesAsync();

			await tx.CommitAsync();
		}

		private async Task ApplyStockDelta(int warehouseId, int goodId, int delta, bool validateForSell)
		{
			var wg = await _db.WarehousesGoods
				.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.GoodsId == goodId);

			if (wg == null)
			{
				wg = new WarehouseGood { WarehouseId = warehouseId, GoodsId = goodId, Quantity = 0 };
				_db.WarehousesGoods.Add(wg);
				await _db.SaveChangesAsync();
			}

			if (validateForSell && delta < 0 && wg.Quantity + delta < 0)
				throw new InvalidOperationException("Na sklade neni dostatecne mnozstvi polozky.");

			wg.Quantity += delta;
			if (wg.Quantity < 0) wg.Quantity = 0;

			await _db.SaveChangesAsync();
		}
	}
}

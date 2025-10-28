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
			   .Include(o => o.OrderedGoods).ThenInclude(og => og.Good)
			   .OrderByDescending(o => o.Id)
			   .ToListAsync();

		public Task<Order?> GetByIdWithItemsAsync(int id) =>
			_db.Orders
			   .AsNoTracking()
			   .Include(o => o.Company)
			   .Include(o => o.User)
			   .Include(o => o.OrderedGoods).ThenInclude(og => og.Good)
			   .FirstOrDefaultAsync(o => o.Id == id);

		public async Task AddAsync(Order order, IEnumerable<OrderedGoods> items)
		{
			_db.Orders.Add(order);
			await _db.SaveChangesAsync();

			foreach (var it in items)
			{
				it.OrderId = order.Id;
				_db.OrderedGoods.Add(it);
			}
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(Order order, IEnumerable<OrderedGoods> items)
		{
			_db.Orders.Update(order);
			await _db.SaveChangesAsync();

			var oldItems = await _db.OrderedGoods.Where(x => x.OrderId == order.Id).ToListAsync();
			_db.OrderedGoods.RemoveRange(oldItems);
			await _db.SaveChangesAsync();

			foreach (var it in items)
			{
				it.OrderId = order.Id;
				_db.OrderedGoods.Add(it);
			}
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var e = await _db.Orders.FindAsync(id);
			if (e != null)
			{
				_db.Orders.Remove(e);
				await _db.SaveChangesAsync();
			}
		}
	}
}

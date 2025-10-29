using System.Collections.Generic;
using System.Threading.Tasks;
using AP5PW_Helpdesk.Entities;

namespace AP5PW_Helpdesk.Data.Repositories
{
	public interface IOrderRepository
	{
		Task<List<Order>> GetAllAsync();
		Task<Order?> GetByIdWithItemsAsync(int id);

		Task AddAsync(Order order, IEnumerable<OrderedGoods> items);
		Task UpdateAsync(Order order, IEnumerable<OrderedGoods> items);
		Task DeleteAsync(int id);
	}
}

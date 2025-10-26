using System.Collections.Generic;
using System.Threading.Tasks;
using AP5PW_Helpdesk.Entities;

namespace AP5PW_Helpdesk.Data.Repositories
{
	public interface IWarehouseRepository
	{
		Task<List<Warehouse>> GetAllAsync();
		Task<Warehouse?> GetByIdAsync(int id);
		Task AddAsync(Warehouse entity);
		Task UpdateAsync(Warehouse entity);
		Task DeleteAsync(int id);

		Task<bool> NameExistsInCompanyAsync(string name, int companyId, int? excludeId = null);
	}
}

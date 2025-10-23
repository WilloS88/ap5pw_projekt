using System.Collections.Generic;
using System.Threading.Tasks;
using GoodEntity = AP5PW_Helpdesk.Entities.Good;

namespace AP5PW_Helpdesk.Data.Repositories
{
    public interface IGoodRepository
    {
		Task<List<GoodEntity>> GetAllAsync();
		Task<GoodEntity?> GetByIdAsync(int id);
		Task AddAsync(GoodEntity item);
		Task UpdateAsync(GoodEntity item);
		Task DeleteAsync(int id);

		Task<bool> ProductNumExistsAsync(string productNum, int? excludeId = null);
		Task<List<GoodEntity>> SearchAsync(string? query);
	}
}
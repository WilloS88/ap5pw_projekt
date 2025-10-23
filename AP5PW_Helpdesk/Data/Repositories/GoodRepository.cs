using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GoodEntity = AP5PW_Helpdesk.Entities.Good;

namespace AP5PW_Helpdesk.Data.Repositories
{
    public class GoodRepository : IGoodRepository
    {
        private readonly AppDbContext _db;
        public GoodRepository(AppDbContext db) => _db = db;

		public Task<List<GoodEntity>> GetAllAsync() =>
			 _db.Goods.AsNoTracking()
					  .OrderBy(g => g.Name)
					  .ToListAsync();

		public Task<GoodEntity?> GetByIdAsync(int id) =>
			_db.Goods.AsNoTracking()
					 .FirstOrDefaultAsync(g => g.Id == id);

		public async Task AddAsync(GoodEntity item)
		{
			_db.Goods.Add(item);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(GoodEntity item)
		{
			_db.Goods.Update(item);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _db.Goods.FindAsync(id);
			if (entity != null)
			{
				_db.Goods.Remove(entity);
				await _db.SaveChangesAsync();
			}
		}

		public Task<bool> ProductNumExistsAsync(string productNum, int? excludeId = null) =>
			_db.Goods.AnyAsync(g => g.ProductNum == productNum && (!excludeId.HasValue || g.Id != excludeId.Value));

		public Task<List<GoodEntity>> SearchAsync(string? query)
		{
			var q = _db.Goods.AsNoTracking().AsQueryable();
			if (!string.IsNullOrWhiteSpace(query))
			{
				q = q.Where(g =>
					g.Name.Contains(query) ||
					(g.ProductNum != null && g.ProductNum.Contains(query)));
			}
			return q.OrderBy(g => g.Name).ToListAsync();
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AP5PW_Helpdesk.Entities;
using Microsoft.EntityFrameworkCore;

namespace AP5PW_Helpdesk.Data.Repositories
{
	public class WarehouseRepository : IWarehouseRepository
	{
		private readonly AppDbContext _db;
		public WarehouseRepository(AppDbContext db) => _db = db;

		public Task<List<Warehouse>> GetAllAsync() =>
			_db.Warehouses
			   .AsNoTracking()
			   .Include(w => w.Company)
			   .OrderBy(w => w.Name)
			   .ToListAsync();

		public Task<Warehouse?> GetByIdAsync(int id) =>
			_db.Warehouses
			   .AsNoTracking()
			   .Include(w => w.Company)
			   .FirstOrDefaultAsync(w => w.Id == id);

		public async Task AddAsync(Warehouse entity)
		{
			_db.Warehouses.Add(entity);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(Warehouse entity)
		{
			_db.Warehouses.Update(entity);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var e = await _db.Warehouses.FindAsync(id);
			if (e != null)
			{
				_db.Warehouses.Remove(e);
				await _db.SaveChangesAsync();
			}
		}

		public Task<bool> NameExistsInCompanyAsync(string name, int companyId, int? excludeId = null) =>
			_db.Warehouses.AnyAsync(w =>
				w.CompanyId == companyId &&
				w.Name == name &&
				(!excludeId.HasValue || w.Id != excludeId.Value));
	}
}

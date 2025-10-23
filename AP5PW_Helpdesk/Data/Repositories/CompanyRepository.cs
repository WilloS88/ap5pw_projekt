using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AP5PW_Helpdesk.Entities;
using Microsoft.EntityFrameworkCore;

namespace AP5PW_Helpdesk.Data.Repositories
{
	public class CompanyRepository : ICompanyRepository
	{
		private readonly AppDbContext _db;
		public CompanyRepository(AppDbContext db) => _db = db;

		public Task<List<Company>> GetAllAsync() =>
			_db.Companies.AsNoTracking()
						 .OrderBy(c => c.Name)
						 .ToListAsync();

		public Task<Company?> GetByIdAsync(int id) =>
			_db.Companies.AsNoTracking()
						 .FirstOrDefaultAsync(c => c.Id == id);

		public async Task AddAsync(Company entity)
		{
			_db.Companies.Add(entity);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(Company entity)
		{
			_db.Companies.Update(entity);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _db.Companies.FindAsync(id);
			if (entity != null)
			{
				_db.Companies.Remove(entity);
				await _db.SaveChangesAsync();
			}
		}

		public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
			_db.Companies.AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value));
	}
}

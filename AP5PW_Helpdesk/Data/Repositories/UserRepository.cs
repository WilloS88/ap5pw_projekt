using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserEntity = AP5PW_Helpdesk.Entities.User;

namespace AP5PW_Helpdesk.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<List<UserEntity>> GetAllAsync() =>
            _db.Users.AsNoTracking()
                     .Include(u => u.Role)
                     .OrderBy(u => u.UserName)
                     .ToListAsync();

        public Task<UserEntity?> GetByIdAsync(int id) =>
            _db.Users.Include(u => u.Role)
                     .FirstOrDefaultAsync(u => u.Id == id);

        public Task<bool> UsernameExistsAsync(string username, int? excludeId = null) =>
            _db.Users.AnyAsync(u => u.UserName == username && (!excludeId.HasValue || u.Id != excludeId.Value));

        public async Task AddAsync(UserEntity user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserEntity user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Users.FindAsync(id);
            if (entity != null)
            {
                _db.Users.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
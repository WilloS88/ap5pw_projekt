using System.Collections.Generic;
using System.Threading.Tasks;
using UserEntity = AP5PW_Helpdesk.Entities.User;

namespace AP5PW_Helpdesk.Data.Repositories
{
    public interface IUserRepository
    {
        Task<List<UserEntity>> GetAllAsync();
        Task<UserEntity?> GetByIdAsync(int id);
        Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
        Task AddAsync(UserEntity user);
        Task UpdateAsync(UserEntity user);
        Task DeleteAsync(int id);
    }
}
using AP5PW_Helpdesk.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompanyEntity = AP5PW_Helpdesk.Entities.Company;

namespace AP5PW_Helpdesk.Data.Repositories
{
    public interface ICompanyRepository
    {
		Task<List<CompanyEntity>> GetAllAsync();
		Task<CompanyEntity?> GetByIdAsync(int id);
		Task AddAsync(CompanyEntity entity);
		Task UpdateAsync(CompanyEntity entity);
		Task DeleteAsync(int id);

		Task<bool> NameExistsAsync(string name, int? excludeId = null);
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<Organization>> GetAllAsync();
        Task<Organization> GetByIdAsync(int id);
        Task<Organization> AddAsync(Organization organization);
        Task<Organization> UpdateAsync(Organization organization);
        Task<bool> DeleteAsync(int id);
        
    }
}

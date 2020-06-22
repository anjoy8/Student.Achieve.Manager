using Student.Achieve.IRepository.Base;
using Student.Achieve.Model.Models;
using System.Threading.Tasks;

namespace Student.Achieve.IRepository
{	
	/// <summary>
	/// IRoleRepository
	/// </summary>	
	public interface IRoleRepository : IBaseRepository<Role>//类名
    {

        Task<Role> SaveRole(string roleName);
    }
}

using Student.Achieve.IRepository.Base;
using Student.Achieve.Model.Models;
using System.Threading.Tasks;

namespace Student.Achieve.IRepository
{	
	/// <summary>
	/// ISysAdminRepository
	/// </summary>	
	public interface ISysAdminRepository : IBaseRepository<SysAdmin>//类名
    {

        Task<SysAdmin> SaveUserInfo(string loginName, string loginPwd);
        Task<string> GetUserRoleNameStr(string loginName, string loginPwd);

    }
}

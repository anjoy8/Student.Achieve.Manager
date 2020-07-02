using Student.Achieve.IRepository.Base;
using Student.Achieve.Model.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Student.Achieve.IRepository
{	
	/// <summary>
	/// IRoleModulePermissionRepository
	/// </summary>	
	public interface IRoleModulePermissionRepository : IBaseRepository<RoleModulePermission>//类名
    {
        Task<List<RoleModulePermission>> WithChildrenModel();
        Task<List<RoleModulePermission>> GetRoleModule();
        Task<List<RoleModulePermission>> RoleModuleMaps();

    }
}

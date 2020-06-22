using Student.Achieve.IRepository;
using Student.Achieve.Model.Models;
using Student.Achieve.Repository.Base;
using System.Linq;
using System.Threading.Tasks;

namespace Student.Achieve.Repository
{	
	/// <summary>
	/// SysAdminRepository
	/// </summary>	
	public class SysAdminRepository : BaseRepository<SysAdmin>, ISysAdminRepository
    {
        IUserRoleRepository _userRoleRepository;
        IRoleRepository _roleRepository;
        public SysAdminRepository( IUserRoleRepository userRoleRepository, IRoleRepository roleRepository)
        {
            this._userRoleRepository = userRoleRepository;
            this._roleRepository = roleRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        public async Task<SysAdmin> SaveUserInfo(string loginName, string loginPwd)
        {
            SysAdmin SysAdmin = new SysAdmin(loginName, loginPwd);
            SysAdmin model = new SysAdmin();
            var userList = await base.Query(a => a.uLoginName == SysAdmin.uLoginName && a.uLoginPWD == SysAdmin.uLoginPWD);
            if (userList.Count > 0)
            {
                model = userList.FirstOrDefault();
            }
            else
            {
                var id = await base.Add(SysAdmin);
                model = await base.QueryById(id);
            }

            return model;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        public async Task<string> GetUserRoleNameStr(string loginName, string loginPwd)
        {
            string roleName = "";
            var user = (await base.Query(a => a.uLoginName == loginName && a.uLoginPWD == loginPwd)).FirstOrDefault();
            var roleList = await _roleRepository.Query(a => a.IsDeleted == false);
            if (user != null)
            {
                var userRoles = await _userRoleRepository.Query(ur => ur.UserId == user.uID);
                if (userRoles.Count > 0)
                {
                    var arr = userRoles.Select(ur => ur.RoleId.ObjToString()).ToList();
                    var roles = roleList.Where(d => arr.Contains(d.Id.ObjToString()));

                    roleName = string.Join(',', roles.Select(r => r.Name).ToArray());
                }
            }
            return roleName;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.IRepository;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 用户角色关系
    /// 【无权限】
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class UserRoleController : Controller
    {
        readonly ISysAdminRepository _SysAdminRepository;
        readonly IUserRoleRepository _userRoleRepository;
        readonly IRoleRepository _roleRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SysAdminRepository"></param>
        /// <param name="userRoleRepository"></param>
        /// <param name="roleRepository"></param>
        public UserRoleController(ISysAdminRepository SysAdminRepository, IUserRoleRepository userRoleRepository, IRoleRepository roleRepository)
        {
            this._SysAdminRepository = SysAdminRepository;
            this._userRoleRepository = userRoleRepository;
            this._roleRepository = roleRepository;
        }



        /// <summary>
        /// 新建用户
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> AddUser(string loginName, string loginPwd)
        {
            var model = await _SysAdminRepository.SaveUserInfo(loginName, loginPwd);
            return Ok(new
            {
                success = true,
                data = model
            });
        }

        /// <summary>
        /// 新建Role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> AddRole(string roleName)
        {
            var model = await _roleRepository.SaveRole(roleName);
            return Ok(new
            {
                success = true,
                data = model
            });
        }

        /// <summary>
        /// 新建用户角色关系
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> AddUserRole(int uid, int rid)
        {
            var model = await _userRoleRepository.SaveUserRole(uid, rid);
            return Ok(new
            {
                success = true,
                data = model
            });
        }




    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Student.Achieve.AuthHelper;
using Student.Achieve.Common.Helper;
using Student.Achieve.Common.HttpContextUser;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class UserController : ControllerBase
    {
        readonly ISysAdminRepository _SysAdminRepository;
        readonly IUserRoleRepository _userRoleRepository;
        readonly IRoleRepository _roleRepository;
        private readonly IUser _user;
        private readonly IModuleRepository moduleRepository;
        private readonly IPermissionRepository permissionRepository;
        private readonly IRoleModulePermissionRepository roleModulePermissionRepository;
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly IUser _iUser;

        public UserController(ISysAdminRepository SysAdminRepository, IUserRoleRepository userRoleRepository, IRoleRepository roleRepository, IUser user, IModuleRepository moduleRepository, IPermissionRepository permissionRepository, IRoleModulePermissionRepository roleModulePermissionRepository, IUser iUser, ITeacherRepository iTeacherRepository)
        {
            _SysAdminRepository = SysAdminRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _user = user;
            this.moduleRepository = moduleRepository;
            this.permissionRepository = permissionRepository;
            this.roleModulePermissionRepository = roleModulePermissionRepository;
            this._iTeacherRepository = iTeacherRepository;
            this._iUser = iUser;

        }


        [HttpGet]
        public async Task<string> Get3()
        {
            var module = await moduleRepository.Query(d => d.Id > 0);
            foreach (var item in module)
            {
                item.CreateTime = ("2019-01-01").ObjToDate();
            }
            var permissions = await permissionRepository.Query(d => d.Id > 0);
            foreach (var item in permissions)
            {
                item.CreateTime = ("2019-01-01").ObjToDate();
            }
            var role = await _roleRepository.Query(d => d.Id > 0);
            var rolemoudlepermiss = await roleModulePermissionRepository.Query(d => d.Id > 0);
            foreach (var item in rolemoudlepermiss)
            {
                item.CreateTime = ("2019-01-01").ObjToDate();
            }
            var admin = await _SysAdminRepository.Query(d => d.uID > 0);
            var userRoles = await _userRoleRepository.Query(d => d.Id > 0);

            var json = JsonConvert.SerializeObject(module);
            var json2 = JsonConvert.SerializeObject(permissions);
            var json3 = JsonConvert.SerializeObject(role);
            var json4 = JsonConvert.SerializeObject(rolemoudlepermiss);
            var json5 = JsonConvert.SerializeObject(admin);
            var json6 = JsonConvert.SerializeObject(userRoles);

            return "value";
        }





        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/User
        [HttpGet]
        //[ResponseCache(Duration = 60)]
        public async Task<MessageModel<PageModel<SysAdmin>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
                page = 1;
            }
            int intPageSize = 50;


            var data = await _SysAdminRepository.QueryPage(a => a.tdIsDelete != true && a.uStatus >= 0 && ((a.uLoginName != null && a.uLoginName.Contains(key)) || (a.uRealName != null && a.uRealName.Contains(key))), page, intPageSize, " uID desc ");


            #region MyRegion
            var allUserRoles = await _userRoleRepository.Query(d => d.IsDeleted == false);
            var allRoles = await _roleRepository.Query(d => d.IsDeleted == false);

            var SysAdmins = data.data;
            foreach (var item in SysAdmins)
            {
                item.RID = (allUserRoles.FirstOrDefault(d => d.UserId == item.uID)?.RoleId).ObjToInt();
                item.RoleName = allRoles.FirstOrDefault(d => d.Id == item.RID)?.Name;
            }

            data.data = SysAdmins;
            #endregion


            return new MessageModel<PageModel<SysAdmin>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return "value";
        }

        // GET: api/User/5
        /// <summary>
        /// 获取用户详情根据token
        /// 【无权限】
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SysAdmin>> GetInfoByToken(string token)
        {
            var data = new MessageModel<SysAdmin>();
            if (!string.IsNullOrEmpty(token))
            {
                var tokenModel = JwtToken.SerializeJwt(token);
                if (tokenModel != null && tokenModel.Uid > 0)
                {
                    // 单独走教师表
                    var roletype = _iUser.GetClaimValueByType(ClaimTypes.Role);
                    if (roletype.Contains("Teacher_Role"))
                    {
                        var teacherinfo = await _iTeacherRepository.QueryById(tokenModel.Uid);
                        if (teacherinfo != null)
                        {
                            data.response = new SysAdmin()
                            {
                                uLoginName = teacherinfo.Account,
                                uLoginPWD = teacherinfo.Password,
                                uRealName = teacherinfo.Name,
                                uID = teacherinfo.Id,
                            };
                            data.success = true;
                            data.msg = "获取成功";
                        }
                        return data;

                    }



                    var userinfo = await _SysAdminRepository.QueryById(tokenModel.Uid);
                    if (userinfo != null)
                    {
                        data.response = userinfo;
                        data.success = true;
                        data.msg = "获取成功";
                    }
                }

            }
            return data;
        }

        /// <summary>
        /// 添加一个用户
        /// </summary>
        /// <param name="SysAdmin"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] SysAdmin SysAdmin)
        {
            var data = new MessageModel<string>();

            SysAdmin.uLoginPWD = MD5Helper.MD5Encrypt32(SysAdmin.uLoginPWD);
            SysAdmin.uRemark = _user.Name;

            var id = await _SysAdminRepository.Add(SysAdmin);
            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新用户与角色
        /// </summary>
        /// <param name="SysAdmin"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] SysAdmin SysAdmin)
        {
            // 这里也要做后期处理，会有用户个人中心的业务

            var data = new MessageModel<string>();
            if (SysAdmin != null && SysAdmin.uID > 0)
            {
                if (SysAdmin.RID > 0)
                {
                    var usrerole = await _userRoleRepository.Query(d => d.UserId == SysAdmin.uID && d.RoleId == SysAdmin.RID);
                    if (usrerole.Count == 0)
                    {
                        await _userRoleRepository.Add(new UserRole(SysAdmin.uID, SysAdmin.RID));
                    }
                }

                data.success = await _SysAdminRepository.Update(SysAdmin);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = SysAdmin?.uID.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(int id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var userDetail = await _SysAdminRepository.QueryById(id);
                userDetail.tdIsDelete = true;
                data.success = await _SysAdminRepository.Update(userDetail);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = userDetail?.uID.ObjToString();
                }
            }

            return data;
        }
    }
}

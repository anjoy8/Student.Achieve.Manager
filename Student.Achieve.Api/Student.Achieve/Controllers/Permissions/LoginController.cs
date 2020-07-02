using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.AuthHelper;
using Student.Achieve.Common.Helper;
using Student.Achieve.IRepository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 登录管理【无权限】
    /// </summary>
    [Produces("application/json")]
    [Route("api/Login")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        readonly ISysAdminRepository _SysAdminRepository;
        readonly IUserRoleRepository _userRoleRepository;
        readonly IRoleRepository _roleRepository;
        readonly PermissionRequirement _requirement;
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly IRoleModulePermissionRepository _roleModulePermissionRepository;


        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="SysAdminRepository"></param>
        /// <param name="userRoleRepository"></param>
        /// <param name="roleRepository"></param>
        /// <param name="requirement"></param>
        /// <param name="iTeacherRepository"></param>
        /// <param name="roleModulePermissionRepository"></param>
        public LoginController(ISysAdminRepository SysAdminRepository, IUserRoleRepository userRoleRepository, IRoleRepository roleRepository, PermissionRequirement requirement,ITeacherRepository iTeacherRepository, IRoleModulePermissionRepository roleModulePermissionRepository)
        {
            this._SysAdminRepository = SysAdminRepository;
            this._userRoleRepository = userRoleRepository;
            this._roleRepository = roleRepository;
            this._requirement = requirement;
            this._iTeacherRepository = iTeacherRepository;
            _roleModulePermissionRepository = roleModulePermissionRepository;
        }




        /// <summary>
        /// 获取JWT的方法3：整个系统主要方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("JWTToken3.0")]
        public async Task<object> GetJwtToken3(string name = "", string pass = "")
        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            {
                return new JsonResult(new
                {
                    Status = false,
                    message = "用户名或密码不能为空"
                });
            }

            pass = MD5Helper.MD5Encrypt32(pass);

            var user = await _SysAdminRepository.Query(d => d.uLoginName == name && d.uLoginPWD == pass);
            var teacher = await _iTeacherRepository.Query(d => d.Account == name && d.Password == pass);

            var data = await _roleModulePermissionRepository.RoleModuleMaps();
            var list = new List<PermissionItem>();

            {
                list = (from item in data
                        where item.IsDeleted == false
                        orderby item.Id
                        select new PermissionItem
                        {
                            Url = item.Module?.LinkUrl,
                            Role = item.Role?.Name.ObjToString(),
                        }).ToList();
            }
            _requirement.Permissions = list;

            if (user.Count > 0)
            {
                var userRoles = await _SysAdminRepository.GetUserRoleNameStr(name, pass);
                //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(JwtRegisteredClaimNames.Jti, user.FirstOrDefault().uID.ToString()),
                    new Claim("GID", "-9999"),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                //用户标识
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);

                var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                return new JsonResult(token);
            } else if (teacher.Count>0) {
                var userRoles = "Teacher_Role";//单独手写教师权限
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(JwtRegisteredClaimNames.Jti, teacher.FirstOrDefault().Id.ToString()),
                    new Claim("GID", teacher.FirstOrDefault().gradeId.ToString()),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                //用户标识
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);

                var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                return new JsonResult(token);
            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "认证失败"
                });
            }



        }

        /// <summary>
        /// 请求刷新Token（以旧换新）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RefreshToken")]
        public async Task<object> RefreshToken(string token = "")
        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new
                {
                    Status = false,
                    message = "token无效，请重新登录！"
                });
            }
            var tokenModel = JwtToken.SerializeJwt(token);
            if (tokenModel != null && tokenModel.Uid > 0)
            {
                var user = await _SysAdminRepository.QueryById(tokenModel.Uid);
                if (user != null)
                {
                    var userRoles = await _SysAdminRepository.GetUserRoleNameStr(user.uLoginName, user.uLoginPWD);
                    //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                    var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.uLoginName),
                    new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ObjToString()),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                    claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                    //用户标识
                    var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                    identity.AddClaims(claims);

                    var refreshToken = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                    return new JsonResult(refreshToken);
                }
            }

            return new JsonResult(new
            {
                success = false,
                message = "认证失败"
            });
        }



        /// <summary>
        /// 测试 MD5 加密字符串
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Md5Password")]
        public string Md5Password(string password = "")
        {
            return MD5Helper.MD5Encrypt32(password);
        }
    }
}
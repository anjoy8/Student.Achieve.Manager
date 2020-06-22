using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 用户跟角色关联表
    /// </summary>
    public class UserRole : RootEntity
    {
        public UserRole() { }

        public UserRole(int uid, int rid)
        {
            UserId = uid;
            RoleId = rid;
            CreateTime = DateTime.Now;
            IsDeleted = false;
            CreateTime = DateTime.Now;
        }



        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }

    }
}

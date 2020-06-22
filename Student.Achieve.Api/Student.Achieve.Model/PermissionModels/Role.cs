using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class Role : RootEntity
    {
        public Role()
        {
            OrderSort = 1;
            CreateTime = DateTime.Now;
            IsDeleted = false;
        }
        public Role(string name)
        {
            Name = name;
            Description = "";
            OrderSort = 1;
            Enabled = true;
            CreateTime = DateTime.Now;

        }

        /// <summary>
        /// 角色名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        ///描述
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string Description { get; set; }
        /// <summary>
        ///排序
        /// </summary>
        public int OrderSort { get; set; }
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }


    }
}

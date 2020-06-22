using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 年级表
    /// </summary>
    public class Grade : RootEntity
    {


        /// <summary>
        /// 入学年份（2018：2018级高一）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string EnrollmentYear { get; set; }
        /// <summary>
        /// 名称(高一)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }


        [SugarColumn(IsIgnore = true)]
        public List<Clazz> clazzList { get; set; } = new List<Clazz>(); //年级下的班级列表

        [SugarColumn(IsIgnore = true)]
        public List<Course> courseList { get; set; } = new List<Course>(); //本年级的课程

        [SugarColumn(IsIgnore = true)]
        public List<Students> studentList { get; set; } = new List<Students>(); //年级下的学生



    }
}

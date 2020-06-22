using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 课程-班级-教师表
    /// </summary>
    public class CCT : RootEntity
    {

        [SugarColumn(IsIgnore = true)]
        public Grade grade { get; set; } //年级

        public int gradeid { get; set; } //年级ID



        [SugarColumn(IsIgnore = true)]
        public Clazz clazz { get; set; } //班级

        public int clazzid { get; set; } //班级ID




        [SugarColumn(IsIgnore = true)]
        public Course course { get; set; } //课程

        public int courseid { get; set; } //课程ID




        [SugarColumn(IsIgnore = true)]
        public Teacher teacher { get; set; } //教师

        public int teacherid { get; set; } //教师ID



    }
}

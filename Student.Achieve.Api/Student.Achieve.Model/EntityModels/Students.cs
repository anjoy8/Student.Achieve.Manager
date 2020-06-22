using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 学生表
    /// </summary>
    public class Students : RootEntity
    {

        [SugarColumn(IsIgnore = true)]
        public Clazz clazz { get; set; } //班级

        public int clazzid { get; set; } //班级ID



        [SugarColumn(IsIgnore = true)]
        public Grade grade { get; set; } //年级

        public int gradeid { get; set; } //年级ID


        [SugarColumn(IsIgnore = true)]
        public List<ExScore> scoreList { get; set; } = new List<ExScore>(); //成绩集合



        /// <summary>
        /// 学号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string StudentNo { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        /// 学科一
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string SubjectA { get; set; }
        /// <summary>
        /// 学科二
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string SubjectB { get; set; }
        /// <summary>
        /// 在校情况
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string InSchoolSituation { get; set; }
        /// <summary>
        /// 入学年份
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string EnrollmentYear { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Gender { get; set; }
        /// <summary>
        /// 是否算指标
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string IsIndicators { get; set; }
        /// <summary>
        /// 高考考号
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string UniversityEntranceNumber { get; set; }
        /// <summary>
        /// 备注1
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Note1 { get; set; }
        /// <summary>
        /// 备注2
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Note2 { get; set; }
        /// <summary>
        /// 备注3
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Note3 { get; set; }


    }
}

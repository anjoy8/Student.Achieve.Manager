using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 考试表
    /// </summary>
    public class Exam : RootEntity
    {


        /// <summary>
        /// 学年（2018-2019）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string AcademicYear { get; set; }
        /// <summary>
        /// 学期（上学期）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string SchoolTerm { get; set; }


        /// <summary>
        /// 考试名称（期末）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string ExamName { get; set; }




        [SugarColumn(IsIgnore = true)]
        public Grade grade {get;set;} //考试年级

        public int gradeid {get;set;} //年级ID


        

        [SugarColumn(IsIgnore = true)]
        public Course course {get;set;} //考试科目：单科情况

        public int courseid {get;set;} //考试科目ID

    }
}

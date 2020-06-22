using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 某科考试详情表
    /// </summary>
    public class ExamDetail : RootEntity
    {


        /// <summary>
        /// 题目名称（第1题）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Name { get; set; }

        /// <summary>
        /// 题目类型（客观题）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string EDType { get; set; }



        [SugarColumn(IsIgnore = true)]
        public Exam exam { get; set; } //考试

        public int examid { get; set; } //考试ID

        /// <summary>
        /// 题目总分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 题目正确答案（A）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Answer { get; set; }




        [SugarColumn(IsIgnore = true)]
        public Grade grade {get;set;} //考试年级

        public int gradeid {get;set;} //年级ID


        

        [SugarColumn(IsIgnore = true)]
        public Course course {get;set;} //考试科目：单科情况

        public int courseid {get;set;} //考试科目ID

    }
}

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
    public class ExamDetailScore : RootEntity
    {



        [SugarColumn(IsIgnore = true)]
        public Students student { get; set; } //考试学生
        public int studentid { get; set; } //学生ID




        [SugarColumn(IsIgnore = true)]
        public ExamDetail ExamDetail { get; set; } //考试题目
        public int ExamDetailId { get; set; } //考试题目ID


        /// <summary>
        /// 学生得分
        /// </summary>
        public int StudentScore { get; set; }//题目总分
        /// <summary>
        /// 考生答案（A）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string StudentAnswer { get; set; }

    }
}

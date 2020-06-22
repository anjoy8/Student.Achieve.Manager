using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    /// <summary>
    /// 成绩表
    /// </summary>
    public class ExScore : RootEntity
    {

        [SugarColumn(IsIgnore = true)]
        public Clazz clazz { get; set; } //考试班级

        public int clazzid { get; set; } //班级ID



        [SugarColumn(IsIgnore = true)]
        public Students student { get; set; } //考试学生

        public int studentid { get; set; } //学生ID



        [SugarColumn(IsIgnore = true)]
        public Exam exam { get; set; } //考试

        public int examid { get; set; } //考试ID



        [SugarColumn(IsIgnore = true)]
        public Course course { get; set; } //考试科目

        public int courseid { get; set; } //科目ID

        /// <summary>
        /// 总成绩
        /// </summary>
        public int score { get; set; } //考试成绩

        /// <summary>
        /// 基础排名
        /// </summary>
        public int? BaseSort { get; set; } = 0; //基础排名

        /// <summary>
        /// 主观成绩
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string SubjectiveScore { get; set; } //主观成绩
        /// <summary>
        /// 客观成绩
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string ObjectiveScore { get; set; } //客观成绩



        [SugarColumn(IsIgnore = true)]
        public string Teacher { get; set; } //考试科目

    }
}

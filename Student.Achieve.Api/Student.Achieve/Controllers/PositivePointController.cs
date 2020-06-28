using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.Common.HttpContextUser;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class PositivePointController : ControllerBase
    {

        private readonly IExScoreRepository _iExScoreRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICCTRepository _iCCTRepository;
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly IUser _iUser;
        private int GID = 0;

        public PositivePointController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, IUser iUser, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository)
        {
            this._iExScoreRepository = iExScoreRepository;
            this._iStudentsRepository = iStudentsRepository;
            this._iExamRepository = iExamRepository;
            this._iCourseRepository = iCourseRepository;
            this._iClazzRepository = iClazzRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCCTRepository = iCCTRepository;
            this._iTeacherRepository = iTeacherRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }


        [HttpGet]
        public async Task<MessageModel<List<PositivePointTotal>>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int CourseId, int ClazzId, int page = 1)
        {
            int intPageSize = 100;
            if (!(GradeId > 0 && CourseId > 0 && !string.IsNullOrEmpty(AcademicYearSchoolTerm) && !string.IsNullOrEmpty(ExamName)))
            {
                return new MessageModel<List<PositivePointTotal>>();
            }

            var exScoreList = await _iExScoreRepository.Query(d => d.IsDeleted == false);

            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false && d.Id == GradeId);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false && d.GradeId == GradeId);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var cctList = await _iCCTRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var teachersList = await _iTeacherRepository.Query(d => d.IsDeleted == false && d.gradeId == GradeId);


            foreach (var item in examList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var exscore in exScoreList)
            {
                try
                {
                    exscore.exam = examList.Where(d => d.Id == exscore.examid).FirstOrDefault();
                    var teacherid = cctList.Where(d => d.clazzid == exscore.clazzid && d.gradeid == exscore.exam.gradeid && d.courseid == exscore.exam.courseid).FirstOrDefault()?.teacherid;
                    exscore.Teacher = teachersList.Where(d => d.Id == teacherid.ObjToInt()).FirstOrDefault()?.Name;


                    exscore.clazz = clazzList.Where(d => d.Id == exscore.clazzid).FirstOrDefault();
                    exscore.student = studentsList.Where(d => d.Id == exscore.studentid).FirstOrDefault();
                }
                catch (Exception ex)
                {

                }
            }


            exScoreList = exScoreList.Where(d => (d.exam != null && d.exam.grade != null && d.exam.grade.Id == GID || (GID == -9999 && true))).ToList();

            // 统计 全年级的 某次考试 全部科目的 全部成绩
            var examSortAllCourse = exScoreList.Where(d => AcademicYearSchoolTerm == (d.exam?.AcademicYear + d.exam?.SchoolTerm) && d.exam?.ExamName == ExamName && d.exam?.gradeid == GradeId).ToList();


            // 统计 全年级的 某次考试 某门科目中 的全部成绩
            var exscoreGrade = examSortAllCourse.Where(d => d.courseid == CourseId).ToList();


            List<PositivePoint> totalGradePositivePoints = new List<PositivePoint>();


            foreach (var item in exscoreGrade)
            {

                PositivePoint PositivePoint = new PositivePoint()
                {
                    StudentNo = item.student.StudentNo,
                    StudentName = item.student.Name,
                    BaseSort = item.BaseSort.ObjToInt(),
                    Score = item.score.ObjToDecimal(),
                    Clazz = item.clazz.ClassNo,
                    Teacher = item.Teacher,
                    Clazzid = item.clazz.Id,

                };
                totalGradePositivePoints.Add(PositivePoint);
            }

            // 基础名次排序 —— 求出来 基础名次ok，序号，本次基础
            totalGradePositivePoints = totalGradePositivePoints.OrderBy(d => d.BaseSort).ToList();

            List<int> baseSortArr = new List<int>();
            var j = 1;
            var clazzCount = clazzList.Count;
            var totalStudentCount = studentsList.Count;
            var examStudentCount = totalGradePositivePoints.Count;

            for (int i = 1; i <= totalGradePositivePoints.Count; i++)
            {
                var item = totalGradePositivePoints[i - 1];
                item.Xuhao = i;

                if (!baseSortArr.Contains(item.BaseSort))
                {
                    j = i;
                    baseSortArr.Add(item.BaseSort);
                }

                item.BaseSortOK = j;
                item.ThisBase = (examStudentCount - (i - 1))*0.01 * clazzCount * 100 / (0.01 * ((examStudentCount + 1)/ 2)* examStudentCount);
            }

            // 基础名次OK排名 —— 求出来 本次基础ok
            totalGradePositivePoints = totalGradePositivePoints.OrderBy(d => d.BaseSortOK).ToList();
            for (int i = 1; i <= totalGradePositivePoints.Count; i++)
            {
                var item = totalGradePositivePoints[i - 1];
                var avgList = totalGradePositivePoints.Where(d => d.BaseSortOK == item.BaseSortOK).Select(d => d.ThisBase).ToList();
                item.ThisBaseOk = avgList.Sum() / avgList.Count;
            }

            // 根据分数排序 —— 得到年级排序ok，序号，本次考试
            totalGradePositivePoints = totalGradePositivePoints.OrderByDescending(d => d.Score).ToList();

            List<decimal> scoreArr = new List<decimal>();
            for (int i = 1; i <= totalGradePositivePoints.Count; i++)
            {
                var item = totalGradePositivePoints[i - 1];
                item.Xuhao = i;

                if (!scoreArr.Contains(item.Score))
                {
                    j = i;
                    scoreArr.Add(item.Score);
                }

                item.GradeSortOk = j;
                item.ThisExam = (examStudentCount - (i - 1)) * 0.01 * clazzCount * 100 / (0.01 * ((examStudentCount + 1) / 2) * examStudentCount);
            }



            // 年级重排OK排名 —— 求出来 本次考试ok
            totalGradePositivePoints = totalGradePositivePoints.OrderBy(d => d.GradeSortOk).ToList();
            for (int i = 1; i <= totalGradePositivePoints.Count; i++)
            {
                var item = totalGradePositivePoints[i - 1];
                var avgList = totalGradePositivePoints.Where(d => d.GradeSortOk == item.GradeSortOk).Select(d => d.ThisExam).ToList();
                item.ThisExamOk = avgList.Sum() / avgList.Count;
            }



            var clazzids = clazzList.Where(d => d.GradeId == GradeId).GroupBy(x => new { x.Id }).Select(s => s.First()).ToList();



            // ↑↑↑↑每个考生正负分完成↑↑↑↑↑↑



            List<PositivePointTotal>  positivePointTotals = new List<PositivePointTotal>();

            foreach (var item in clazzids)
            {
                var totalGradePositivePointsClazz = totalGradePositivePoints.Where(d => d.Clazzid == item.Id).ToList();

                PositivePointTotal positivePointTotal = new PositivePointTotal() {
                    Clazz = item.ClassNo,
                    Teacher = totalGradePositivePointsClazz.FirstOrDefault()?.Teacher,
                    TotalStudentCount = studentsList.Where(d => d.clazzid == item.Id).Count(),
                    ExamStudentCount= totalGradePositivePointsClazz.Count,
                    ThisBaseOk= totalGradePositivePointsClazz.Select(d=>d.ThisBaseOk).Sum(),
                    ThisExamOk= totalGradePositivePointsClazz.Select(d=>d.ThisExamOk).Sum(),
                };

                positivePointTotal.NoExamStudentCount = positivePointTotal.TotalStudentCount- positivePointTotal.ExamStudentCount;
                positivePointTotal.RankBase = positivePointTotal.ThisExamOk - positivePointTotal.ThisBaseOk;


                positivePointTotal.ThisBaseOk= Math.Round(positivePointTotal.ThisBaseOk, 2, MidpointRounding.AwayFromZero);
                positivePointTotal.ThisExamOk = Math.Round(positivePointTotal.ThisExamOk, 2, MidpointRounding.AwayFromZero);
                positivePointTotal.ThisOk = positivePointTotal.ThisExamOk;
                positivePointTotal.RankBase = Math.Round(positivePointTotal.RankBase, 2, MidpointRounding.AwayFromZero);


                positivePointTotals.Add(positivePointTotal);
            }




            return new MessageModel<List<PositivePointTotal>>()
            {
                msg = "获取成功",
                success = positivePointTotals.Count >= 0,
                response = positivePointTotals
            };

        }

    }

    public class PositivePoint
    {
        public string GradeId { get; set; }
        public string AcademicYearSchoolTerm { get; set; }

        public string ExamName { get; set; }
        public int courseid { get; set; } //考试科目ID
        public int Clazzid { get; set; }



        public string StudentNo { get; set; }
        public string StudentName { get; set; }
        public int BaseSort { get; set; }
        public int BaseSortOK { get; set; }
        public decimal Score { get; set; }
        public int GradeSort { get; set; }
        public int GradeSortOk { get; set; }
        public int Xuhao { get; set; }

        public string Clazz { get; set; }
        public string Teacher { get; set; }


        public double ThisBase { get; set; }
        public double ThisBaseOk { get; set; }
        public double ThisExam { get; set; }
        public double ThisExamOk { get; set; }



    }


    public class PositivePointTotal
    {


        public string Clazz { get; set; }
        public string Teacher { get; set; }

        public int TotalStudentCount { get; set; }
        public int ExamStudentCount { get; set; }
        public int NoExamStudentCount { get; set; }

        public double ThisBaseOk { get; set; }
        public double ThisExamOk { get; set; }
        public double ThisOk { get; set; }
        public double RankBase { get; set; }



    }
}
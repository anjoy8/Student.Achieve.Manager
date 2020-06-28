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
    public class CourseScoreDistributeController : ControllerBase
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

        public CourseScoreDistributeController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, IUser iUser, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository)
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
        public async Task<MessageModel<PageModel<CourseScoreDistribute>>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int CourseId, int page = 1)
        {
            int intPageSize = 100;

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
                exscore.exam = examList.Where(d => d.Id == exscore.examid).FirstOrDefault();
                var teacherid = cctList.Where(d => d.clazzid == exscore.clazzid && d.gradeid == exscore.exam.gradeid && d.courseid == exscore.exam.courseid).FirstOrDefault()?.teacherid;
                exscore.Teacher = teachersList.Where(d => d.Id == teacherid.ObjToInt()).FirstOrDefault()?.Name;


                exscore.clazz = clazzList.Where(d => d.Id == exscore.clazzid).FirstOrDefault();
                exscore.student = studentsList.Where(d => d.Id == exscore.studentid).FirstOrDefault();
            }


            exScoreList = exScoreList.Where(d => (d.exam != null && d.exam.grade != null && d.exam.grade.Id == GID || (GID == -9999 && true))).ToList();


            if (GradeId > 0)
            {
                exScoreList = exScoreList.Where(d => d.exam != null && d.exam.grade != null && d.exam.gradeid == GradeId).ToList();
            }


            if (!string.IsNullOrEmpty(AcademicYearSchoolTerm))
            {
                exScoreList = exScoreList.Where(d => d.exam != null && d.exam.grade != null && AcademicYearSchoolTerm == (d.exam.AcademicYear + d.exam.SchoolTerm)).ToList();
            }


            if (!string.IsNullOrEmpty(ExamName))
            {
                exScoreList = exScoreList.Where(d => d.exam != null && d.exam.grade != null && d.exam.ExamName == ExamName).ToList();
            }


            if (CourseId > 0)
            {
                exScoreList = exScoreList.Where(d => d.exam != null && d.exam.grade != null && d.courseid == CourseId).ToList();
            }


            var clazzGroups = exScoreList.Select(d => new { d.clazzid, d.exam.gradeid }).ToList();

            clazzGroups = clazzGroups.GroupBy(x => new { x.clazzid, x.gradeid }).Select(x => x.First()).ToList();
            List<CourseScoreDistribute> courseScoreDistributes = new List<CourseScoreDistribute>();
            foreach (var item in clazzGroups)
            {
                var exscore = exScoreList.Where(d => d.clazzid == item.clazzid && d.exam.gradeid == item.gradeid).ToList();

                var examStuCount = (exscore.GroupBy(x => new { x.studentid }).Select(x => x.First()).Count());

                var scores = exscore.Select(d => d.score).ToList();

                CourseScoreDistribute courseScoreDistribute = new CourseScoreDistribute()
                {
                    Clazz = exscore.FirstOrDefault().clazz.ClassNo,
                    Teacher = exscore.FirstOrDefault().Teacher,
                    ExamStuCount = examStuCount.ObjToString(),
                    C140_150 = (exscore.Where(d => d.score >= 140 && d.score <= 150).Count()),
                    C130_139 = (exscore.Where(d => d.score >= 130 && d.score < 140).Count()),
                    C120_129 = (exscore.Where(d => d.score >= 120 && d.score < 130).Count()),
                    C110_119 = (exscore.Where(d => d.score >= 110 && d.score < 120).Count()),
                    C100_109 = (exscore.Where(d => d.score >= 100 && d.score < 110).Count()),
                    C90_99 = (exscore.Where(d => d.score >= 90 && d.score < 100).Count()),
                    C80_89 = (exscore.Where(d => d.score >= 80 && d.score < 90).Count()),
                    C70_79 = (exscore.Where(d => d.score >= 70 && d.score < 80).Count()),
                    C60_69 = (exscore.Where(d => d.score >= 60 && d.score < 70).Count()),
                    C50_59 = (exscore.Where(d => d.score >= 50 && d.score < 60).Count()),
                    C40_49 = (exscore.Where(d => d.score >= 40 && d.score < 50).Count()),
                    C40_0 = (exscore.Where(d => d.score >= 0 && d.score < 40).Count()),
                    C_Good = ((exscore.Where(d => d.score >= 120).Count())),
                    C_Pass = ((exscore.Where(d => d.score >= 90).Count())),
                    C_Good_Rate = ((((decimal)(exscore.Where(d => d.score >= 120).Count()) / (decimal)examStuCount).ToString("#0.00")).ObjToDecimal()) * 100,
                    C_Pass_Rate = ((((decimal)(exscore.Where(d => d.score >= 90).Count()) / (decimal)examStuCount).ToString("#0.00")).ObjToDecimal()) * 100,
                    C_Max=(scores.Max()),
                    C_Min=(scores.Min()),
                    C_Avg=(scores.Average().ToString("#0.00")).ObjToInt(),

                };
                courseScoreDistributes.Add(courseScoreDistribute);
            }





            var totalCount = courseScoreDistributes.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exScores = courseScoreDistributes.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();



            PageModel<CourseScoreDistribute> data = new PageModel<CourseScoreDistribute>()
            {
                data = courseScoreDistributes,
                dataCount = courseScoreDistributes.Count,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };


            return new MessageModel<PageModel<CourseScoreDistribute>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

    }

    public class CourseScoreDistribute
    {
        public string GradeId { get; set; }
        public string AcademicYearSchoolTerm { get; set; }

        public string ExamName { get; set; }
        public int courseid { get; set; } //考试科目ID



        public string Clazz { get; set; }
        public string Teacher { get; set; }
        public string ExamStuCount { get; set; }
        public int C140_150 { get; set; }
        public int C130_139 { get; set; }
        public int C120_129 { get; set; }
        public int C110_119 { get; set; }
        public int C100_109 { get; set; }
        public int C90_99 { get; set; }
        public int C80_89 { get; set; }
        public int C70_79 { get; set; }
        public int C60_69 { get; set; }
        public int C50_59 { get; set; }
        public int C40_49 { get; set; }
        public int C40_0 { get; set; }
        public int C_Max { get; set; }
        public int C_Min { get; set; }
        public int C_Avg { get; set; }
        public int C_Good { get; set; }
        public int C_Pass { get; set; }
        public decimal C_Good_Rate { get; set; }
        public decimal C_Pass_Rate { get; set; }


    }
}
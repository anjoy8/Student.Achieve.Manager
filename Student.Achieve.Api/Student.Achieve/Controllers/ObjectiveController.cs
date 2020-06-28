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
    public class ObjectiveController : ControllerBase
    {

        private readonly IExScoreRepository _iExScoreRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICCTRepository _iCCTRepository;
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly IExamDetailRepository _iExamDetailRepository;
        private readonly IExamDetailScoreRepository _iExamDetailScoreRepository;
        private readonly IUser _iUser;
        private int GID = 0;

        public ObjectiveController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, IUser iUser, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository, IExamDetailRepository iExamDetailRepository, IExamDetailScoreRepository iExamDetailScoreRepository)
        {
            this._iExScoreRepository = iExScoreRepository;
            this._iStudentsRepository = iStudentsRepository;
            this._iExamRepository = iExamRepository;
            this._iCourseRepository = iCourseRepository;
            this._iClazzRepository = iClazzRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCCTRepository = iCCTRepository;
            this._iTeacherRepository = iTeacherRepository;
            this._iExamDetailRepository = iExamDetailRepository;
            this._iExamDetailScoreRepository = iExamDetailScoreRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }


        [HttpGet]
        public async Task<MessageModel<PageModel<Objective>>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int CourseId, int ClazzId, int page = 1)
        {
            int intPageSize = 100;
            if (!(GradeId > 0 && CourseId > 0 && !string.IsNullOrEmpty(AcademicYearSchoolTerm) && !string.IsNullOrEmpty(ExamName)))
            {
                return new MessageModel<PageModel<Objective>>();
            }


            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false && d.Id == GradeId);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false && d.GradeId == GradeId);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var cctList = await _iCCTRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var teachersList = await _iTeacherRepository.Query(d => d.IsDeleted == false && d.gradeId == GradeId);

            var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId && d.EDType == "客观题");
            var examDetailScoreList = await _iExamDetailScoreRepository.Query(d => d.IsDeleted == false);



            foreach (var item in examList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var item in examDetailList)
            {
                item.exam = examList.Where(d => d.Id == item.examid).FirstOrDefault();
            }

            //题目
            examDetailList = examDetailList.Where(d => AcademicYearSchoolTerm == (d.exam.AcademicYear + d.exam.SchoolTerm) && d.exam.ExamName == ExamName && d.exam.gradeid == GradeId && d.exam.courseid == CourseId && d.courseid == CourseId).ToList();


            // 如果选中班级，则是部分学生
            if (ClazzId > 0)
            {
                studentsList = studentsList.Where(d => d.gradeid == GradeId && d.clazzid == ClazzId).ToList();
            }

            var lookStudentIds = studentsList.Select(d => d.Id).ToList();

            var studentCount = examDetailScoreList.Where(d => lookStudentIds.Contains(d.studentid)).Select(d => d.studentid).GroupBy(x => new { x }).Select(x => x.First()).Count();



            List<Objective> objectives = new List<Objective>();

            foreach (var item in examDetailList)
            {
                var examDetailScoreCurrent = examDetailScoreList.Where(d => d.ExamDetailId == item.Id & lookStudentIds.Contains(d.studentid));

                Objective objective = new Objective()
                {
                    Name = item.Name,
                    Answer = item.Answer,
                    A = examDetailScoreCurrent.Where(d => d.StudentAnswer == "A").Count(),
                    B = examDetailScoreCurrent.Where(d => d.StudentAnswer == "B").Count(),
                    C = examDetailScoreCurrent.Where(d => d.StudentAnswer == "C").Count(),
                    D = examDetailScoreCurrent.Where(d => d.StudentAnswer == "D").Count(),
                    AB = examDetailScoreCurrent.Where(d => d.StudentAnswer == "AB").Count(),
                    ABC = examDetailScoreCurrent.Where(d => d.StudentAnswer == "ABC").Count(),
                    ABCD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "ABCD").Count(),
                    AC = examDetailScoreCurrent.Where(d => d.StudentAnswer == "AC").Count(),
                    ACD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "ACD").Count(),
                    AD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "AD").Count(),
                    BC = examDetailScoreCurrent.Where(d => d.StudentAnswer == "BC").Count(),
                    BCD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "BCD").Count(),
                    BD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "BD").Count(),
                    CD = examDetailScoreCurrent.Where(d => d.StudentAnswer == "CD").Count(),
                    NoWrite = 0,
                    ReadCardCount = studentCount,
                    TotalScore = item.Score * studentCount,
                    StudentTotalScore = examDetailScoreCurrent.Select(d => d.StudentScore).Sum(),

                };

                objective.NoWrite = studentCount - (objective.A + objective.B + objective.C + objective.D + objective.AB + objective.ABC + objective.ABCD + objective.AC + objective.ACD + objective.AD + objective.BC + objective.BCD + objective.BD + objective.CD);
                objective.TotalScoreRate = (((objective.StudentTotalScore / objective.TotalScore).ToString("#0.00")).ObjToDecimal()) * 100;
                objectives.Add(objective);
            }



            var totalCount = objectives.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exScores = objectives.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();



            PageModel<Objective> data = new PageModel<Objective>()
            {
                data = exScores,
                dataCount = totalCount,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };


            return new MessageModel<PageModel<Objective>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

    }

    public class Objective
    {
        public string GradeId { get; set; }
        public string AcademicYearSchoolTerm { get; set; }

        public string ExamName { get; set; }
        public int courseid { get; set; } //考试科目ID
        public int Clazzid { get; set; }



        public string Name { get; set; }
        public string Answer { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int AB { get; set; }
        public int ABC { get; set; }
        public int ABCD { get; set; }
        public int AC { get; set; }
        public int ACD { get; set; }
        public int AD { get; set; }
        public int BC { get; set; }
        public int BCD { get; set; }
        public int BD { get; set; }
        public int CD { get; set; }
        public int NoWrite { get; set; }
        public int ReadCardCount { get; set; }
        public decimal StudentTotalScore { get; set; }
        public decimal TotalScore { get; set; }
        public decimal TotalScoreRate { get; set; }


    }
}


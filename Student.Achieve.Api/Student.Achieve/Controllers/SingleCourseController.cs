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
    /// <summary>
    /// 单科
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class SingleCourseController : ControllerBase
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

        public SingleCourseController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, IUser iUser, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository)
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
        public async Task<MessageModel<PageModel<SingleCourse>>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int CourseId, int ClazzId, int page = 1)
        {
            int intPageSize = 100;
            if (!(GradeId > 0 && CourseId > 0 && !string.IsNullOrEmpty(AcademicYearSchoolTerm) && !string.IsNullOrEmpty(ExamName)))
            {
                return new MessageModel<PageModel<SingleCourse>>();
            }

            var exScoreList = await _iExScoreRepository.Query(d => d.IsDeleted == false);

            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false && d.Id == GradeId);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false && d.GradeId == GradeId);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var cctList = await _iCCTRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var teachersList = await _iTeacherRepository.Query(d => d.IsDeleted == false && d.gradeId == GradeId);


            var leaveCourseIds = courseList.Where(d => d.Name == "化学" || d.Name == "生物" || d.Name == "政治" || d.Name == "地理").Select(d => d.Id).ToList();

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


            List<SingleCourse> totalGradeSingleCourses = new List<SingleCourse>();


            foreach (var item in exscoreGrade)
            {
                var studentAllCourseScore = (examSortAllCourse.Where(d => d.studentid == item.studentid).ToList()).Select(d => d.score).ToList();

                SingleCourse SingleCourse = new SingleCourse()
                {
                    StudentNo = item.student.StudentNo,
                    StudentName = item.student.Name,
                    TotalScore = item.score.ObjToDecimal(),
                    SubjectiveScore = item.SubjectiveScore.ObjToDecimal(),
                    ObjectiveScore = item.ObjectiveScore.ObjToDecimal(),
                    Clazz = item.clazz.ClassNo,
                    Clazzid = item.clazz.Id,
                    courseid=item.courseid,
                    TotalNineScore = (studentAllCourseScore.Sum()).ObjToDecimal(),
                    TotalNineScoreSort = item.BaseSort.ObjToInt(),
                };
                totalGradeSingleCourses.Add(SingleCourse);
            }

            //totalGradeSingleCourses = totalGradeSingleCourses.OrderByDescending(d => d.TotalNineScore).ToList();

            //for (int i = 0; i < totalGradeSingleCourses.Count; i++)
            //{
            //    var item = totalGradeSingleCourses[i];
            //    item.TotalNineScoreSort = (i + 1);
            //}

            totalGradeSingleCourses = totalGradeSingleCourses.OrderByDescending(d => d.TotalScore).ToList();

            List<decimal> scoreLeaveA = new List<decimal>();
            List<decimal> scoreLeaveB = new List<decimal>();
            List<decimal> scoreLeaveC = new List<decimal>();
            List<decimal> scoreLeaveD = new List<decimal>();
            List<decimal> scoreLeaveE = new List<decimal>();

            decimal leaveIndex = 0;
            foreach (var item in totalGradeSingleCourses)
            {
                leaveIndex++;
                item.GradeSort = leaveIndex.ObjToInt();


                if (leaveCourseIds.Contains(item.courseid))
                {

                    if (scoreLeaveA.Contains(item.TotalScore))
                    {
                        item.Leave = "A";
                    }
                    else if (scoreLeaveB.Contains(item.TotalScore))
                    {
                        item.Leave = "B";
                    }
                    else if (scoreLeaveC.Contains(item.TotalScore))
                    {
                        item.Leave = "C";
                    }
                    else if (scoreLeaveD.Contains(item.TotalScore))
                    {
                        item.Leave = "D";
                    }
                    else if (scoreLeaveE.Contains(item.TotalScore))
                    {
                        item.Leave = "E";
                    }
                    else
                    {
                        var totalStudent = totalGradeSingleCourses.Count.ObjToDecimal();

                        if (leaveIndex / totalStudent <= (decimal)0.17)
                        {
                            item.Leave = "A";
                            scoreLeaveA.Add(item.TotalScore);
                        }
                        else if (leaveIndex / totalStudent <= (decimal)0.50)
                        {
                            item.Leave = "B";
                            scoreLeaveB.Add(item.TotalScore);
                        }
                        else if (leaveIndex / totalStudent <= (decimal)0.83)
                        {
                            item.Leave = "C";
                            scoreLeaveC.Add(item.TotalScore);
                        }
                        else if (leaveIndex / totalStudent <= (decimal)0.98)
                        {
                            item.Leave = "D";
                            scoreLeaveD.Add(item.TotalScore);
                        }
                        else
                        {
                            item.Leave = "E";
                            scoreLeaveE.Add(item.TotalScore);
                        }
                    }
                }
            }




            // ↓↓↓四科参考分↓↓↓

            var aMaxStudent = GetStudentMax(totalGradeSingleCourses,"A");
            var aMinStudent = GetStudentMin(totalGradeSingleCourses, "A");
            var aMaxLeave = 100;
            var aMinLeave = 83;

            var bMaxStudent = GetStudentMax(totalGradeSingleCourses, "B");
            var bMinStudent = GetStudentMin(totalGradeSingleCourses, "B");
            var bMaxLeave = 82;
            var bMinLeave = 71;

            var cMaxStudent = GetStudentMax(totalGradeSingleCourses, "C");
            var cMinStudent = GetStudentMin(totalGradeSingleCourses, "C");
            var cMaxLeave = 70;
            var cMinLeave = 59;

            var dMaxStudent = GetStudentMax(totalGradeSingleCourses, "D");
            var dMinStudent = GetStudentMin(totalGradeSingleCourses, "D");
            var dMaxLeave = 58;
            var dMinLeave = 41;

            var eMaxStudent = GetStudentMax(totalGradeSingleCourses, "E");
            var eMinStudent = GetStudentMin(totalGradeSingleCourses, "E");
            var eMaxLeave = 40;
            var eMinLeave = 30;

            for (int i = 0; i < totalGradeSingleCourses.Count; i++)
            {
                var item = totalGradeSingleCourses[i];
                if (leaveCourseIds.Contains(item.courseid))
                {
                    if (item.Leave == "A")
                    {
                        item.ReferenceLeaveScore = (aMaxLeave - aMinLeave) / (aMaxStudent - aMinStudent) * (item.TotalScore - aMinStudent) + aMinLeave;
                    }
                    else if (item.Leave == "B")
                    {
                        item.ReferenceLeaveScore = (bMaxLeave - bMinLeave) / (bMaxStudent - bMinStudent) * (item.TotalScore - bMinStudent) + bMinLeave;
                    }
                    else if (item.Leave == "C")
                    {
                        item.ReferenceLeaveScore = (cMaxLeave - cMinLeave) / (cMaxStudent - cMinStudent) * (item.TotalScore - cMinStudent) + cMinLeave;
                    }
                    else if (item.Leave == "D")
                    {
                        item.ReferenceLeaveScore = (dMaxLeave - dMinLeave) / (dMaxStudent - dMinStudent) * (item.TotalScore - dMinStudent) + dMinLeave;
                    }
                    else if (item.Leave == "E")
                    {
                        item.ReferenceLeaveScore = (eMaxLeave - eMinLeave) / (eMaxStudent - eMinStudent) * (item.TotalScore - eMinStudent) + eMinLeave;
                    }
                    else
                    {
                        item.ReferenceLeaveScore = 0;
                    }
                }

                item.ReferenceLeaveScore = Math.Round(item.ReferenceLeaveScore, 1, MidpointRounding.AwayFromZero);
            }


            // ↑↑↑四科参考分↑↑↑


            var clazzids = clazzList.Where(d => d.GradeId == GradeId).GroupBy(x => new { x.Id }).Select(s => s.First()).ToList();

            List<SingleCourse> dataSC = new List<SingleCourse>();

            foreach (var item in clazzids)
            {
                var totalGradeSingleCoursesClazz = totalGradeSingleCourses.Where(d => d.Clazzid == item.Id).ToList();
                totalGradeSingleCoursesClazz = totalGradeSingleCoursesClazz.OrderByDescending(d => d.TotalScore).ToList();


                for (int i = 0; i < totalGradeSingleCoursesClazz.Count; i++)
                {
                    var itemClazz = totalGradeSingleCoursesClazz[i];
                    itemClazz.ClazzSort = (i + 1);
                }
                dataSC.AddRange(totalGradeSingleCoursesClazz);
            }



            if (ClazzId > 0)
            {
                dataSC = dataSC.Where(d => d.Clazzid == ClazzId).ToList();
            }


            var totalCount = dataSC.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exScores = dataSC.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();



            PageModel<SingleCourse> data = new PageModel<SingleCourse>()
            {
                data = exScores,
                dataCount = totalCount,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };


            return new MessageModel<PageModel<SingleCourse>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }


        private decimal GetStudentMax(List<SingleCourse> totalGradeSingleCourses, string leave)
        {
            if (totalGradeSingleCourses.Where(d => d.Leave == leave).Count()>0)
            {
                return (totalGradeSingleCourses.Where(d => d.Leave == leave)?.Select(d => d.TotalScore)?.Max()).ObjToDecimal();
            }
            else
            {
                return 0;
            }
        }

        private decimal GetStudentMin(List<SingleCourse> totalGradeSingleCourses, string leave)
        {
            if (totalGradeSingleCourses.Where(d => d.Leave == leave).Count()>0)
            {
                return (totalGradeSingleCourses.Where(d => d.Leave == leave)?.Select(d => d.TotalScore)?.Min()).ObjToDecimal();
            }
            else
            {
                return 0;
            }
        }




    }

    public class SingleCourse
    {
        public string GradeId { get; set; }
        public string AcademicYearSchoolTerm { get; set; }

        public string ExamName { get; set; }
        public int courseid { get; set; } //考试科目ID
        public int Clazzid { get; set; }



        public string StudentNo { get; set; }
        public string StudentName { get; set; }
        public decimal TotalNineScore { get; set; }
        public int TotalNineScoreSort { get; set; }
        public int GradeSort { get; set; }
        public decimal SubjectiveScore { get; set; }
        public decimal ObjectiveScore { get; set; }
        public decimal TotalScore { get; set; }
        public int ClazzSort { get; set; }
        public string Clazz { get; set; }


        public string Leave { get; set; }

        public decimal ReferenceLeaveScore { get; set; }//参考等级分


    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [AllowAnonymous]
    public class FSNStudentController : ControllerBase
    {

        private readonly IExScoreRepository _iExScoreRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICCTRepository _iCCTRepository;
        private readonly ITeacherRepository _iTeacherRepository;

        public FSNStudentController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository)
        {
            this._iExScoreRepository = iExScoreRepository;
            this._iStudentsRepository = iStudentsRepository;
            this._iExamRepository = iExamRepository;
            this._iCourseRepository = iCourseRepository;
            this._iClazzRepository = iClazzRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCCTRepository = iCCTRepository;
            this._iTeacherRepository = iTeacherRepository;
        }


        [HttpGet]
        public async Task<MessageModel<PageModel<FSNStudent>>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int FSNStudent, int ClazzId, int page = 1)
        {
            int intPageSize = 100;
            if (!(GradeId > 0 && ClazzId > 0 && !string.IsNullOrEmpty(AcademicYearSchoolTerm) && !string.IsNullOrEmpty(ExamName)))
            {
                return new MessageModel<PageModel<FSNStudent>>();
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
                exscore.exam = examList.Where(d => d.Id == exscore.examid).FirstOrDefault();
                var teacherid = cctList.Where(d => d.clazzid == exscore.clazzid && d.gradeid == exscore.exam.gradeid && d.courseid == exscore.exam.courseid).FirstOrDefault()?.teacherid;
                exscore.Teacher = teachersList.Where(d => d.Id == teacherid.ObjToInt()).FirstOrDefault()?.Name;


                exscore.clazz = clazzList.Where(d => d.Id == exscore.clazzid).FirstOrDefault();
                exscore.student = studentsList.Where(d => d.Id == exscore.studentid).FirstOrDefault();
            }


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



            List<FSNStudent> FSNStudents = new List<FSNStudent>();

            foreach (var item in studentsList)
            {
                var clazzModel = clazzList.Where(d => d.Id == item.clazzid).FirstOrDefault();
                var exscoreStudentList = exScoreList.Where(d => d.studentid == item.Id).ToList();

                FSNStudent FSNStudentModel = new FSNStudent()
                {
                    StudentNo = item.StudentNo,
                    StudentName = item.Name,
                    Clazz = clazzModel.ClassNo,
                    ClazzId = clazzModel.Id,
                    SubjectA = item.SubjectA,
                    SubjectB = item.SubjectB,
                    Chinese = (exscoreStudentList.Where(d => d.exam.course.Name == "语文").FirstOrDefault()?.score).ObjToInt(),
                    Meth = (exscoreStudentList.Where(d => d.exam.course.Name == "数学").FirstOrDefault()?.score).ObjToInt(),
                    English = (exscoreStudentList.Where(d => d.exam.course.Name == "英语").FirstOrDefault()?.score).ObjToInt(),
                    Physics = (exscoreStudentList.Where(d => d.exam.course.Name == "物理").FirstOrDefault()?.score).ObjToInt(),
                    Chemistry = (exscoreStudentList.Where(d => d.exam.course.Name == "化学").FirstOrDefault()?.score).ObjToInt(),
                    Politics = (exscoreStudentList.Where(d => d.exam.course.Name == "政治").FirstOrDefault()?.score).ObjToInt(),
                    History = (exscoreStudentList.Where(d => d.exam.course.Name == "历史").FirstOrDefault()?.score).ObjToInt(),
                    Biology = (exscoreStudentList.Where(d => d.exam.course.Name == "生物").FirstOrDefault()?.score).ObjToInt(),
                    Geography = (exscoreStudentList.Where(d => d.exam.course.Name == "地理").FirstOrDefault()?.score).ObjToInt(),
                };

                FSNStudentModel.T = FSNStudentModel.Chinese + FSNStudentModel.Chinese + FSNStudentModel.Chinese;

                if (FSNStudentModel.SubjectA == "物理")
                {
                    FSNStudentModel.F = FSNStudentModel.T + FSNStudentModel.Physics;
                }
                else if (FSNStudentModel.SubjectA == "历史")
                {
                    FSNStudentModel.F = FSNStudentModel.T + FSNStudentModel.History;
                }

                FSNStudentModel.S += FSNStudentModel.F;
                switch (FSNStudentModel.SubjectB)
                {
                    case "1":
                        FSNStudentModel.S = FSNStudentModel.Chemistry + FSNStudentModel.Biology; break;

                    case "2":
                        FSNStudentModel.S = FSNStudentModel.Chemistry + FSNStudentModel.Politics; break;

                    case "3":
                        FSNStudentModel.S = FSNStudentModel.Chemistry + FSNStudentModel.Geography; break;

                    case "4":
                        FSNStudentModel.S = FSNStudentModel.Politics + FSNStudentModel.Biology; break;

                    case "5":
                        FSNStudentModel.S = FSNStudentModel.Geography + FSNStudentModel.Biology; break;

                    case "6":
                        FSNStudentModel.S = FSNStudentModel.Politics + FSNStudentModel.Geography; break;

                    case "7":
                        FSNStudentModel.S = FSNStudentModel.Politics + FSNStudentModel.Geography; break;

                    case "8":
                        FSNStudentModel.S = FSNStudentModel.Politics + FSNStudentModel.Chemistry; break;

                    case "9":
                        FSNStudentModel.S = FSNStudentModel.Politics + FSNStudentModel.Biology; break;

                    case "10":
                        FSNStudentModel.S = FSNStudentModel.Geography + FSNStudentModel.Chemistry; break;

                    case "11":
                        FSNStudentModel.S = FSNStudentModel.Geography + FSNStudentModel.Biology; break;

                    case "12":
                        FSNStudentModel.S = FSNStudentModel.Chemistry + FSNStudentModel.Biology; break;

                    default:
                        break;
                }

                FSNStudentModel.N = FSNStudentModel.Chinese + FSNStudentModel.Meth + FSNStudentModel.English + FSNStudentModel.Physics + FSNStudentModel.Chemistry + FSNStudentModel.Biology + FSNStudentModel.Politics + FSNStudentModel.History + FSNStudentModel.Geography;

                FSNStudents.Add(FSNStudentModel);
            }




            if (ClazzId > 0)
            {
                FSNStudents = FSNStudents.Where(d => d.ClazzId == ClazzId).ToList();
            }



            var totalCount = FSNStudents.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exScores = FSNStudents.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();



            PageModel<FSNStudent> data = new PageModel<FSNStudent>()
            {
                data = exScores,
                dataCount = totalCount,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };


            return new MessageModel<PageModel<FSNStudent>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

    }

    public class FSNStudent
    {
        public string GradeId { get; set; }
        public string AcademicYearSchoolTerm { get; set; }

        public string ExamName { get; set; }
        public int courseid { get; set; } //考试科目ID



        public string StudentNo { get; set; }
        public string Clazz { get; set; }
        public int ClazzId { get; set; }
        public string StudentName { get; set; }
        public string SubjectA { get; set; }
        public string SubjectB { get; set; }

        public int Chinese { get; set; }
        //public int ChineseSort { get; set; }
        public int Meth { get; set; }
        //public int MethSort { get; set; }
        public int English { get; set; }
        //public int EnglishSort { get; set; }
        public int Physics { get; set; }
        //public int PhysicsSort { get; set; }
        public int Chemistry { get; set; }
        //public int ChemistrySort { get; set; }
        public int Politics { get; set; }
        //public int PoliticsSort { get; set; }
        public int History { get; set; }
        //public int HistorySort { get; set; }
        public int Biology { get; set; }
        //public int BiologySort { get; set; }
        public int Geography { get; set; }
        //public int GeographySort { get; set; }
        public int T { get; set; }
        //public int TSort { get; set; }
        public int F { get; set; }
        //public int FSort { get; set; }
        public int S { get; set; }
        //public int SSort { get; set; }
        public int N { get; set; }
        //public int NSort { get; set; }


    }
}
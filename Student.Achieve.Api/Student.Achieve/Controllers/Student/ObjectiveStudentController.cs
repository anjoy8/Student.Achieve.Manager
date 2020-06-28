using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class ObjectiveStudentController : ControllerBase
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

        public ObjectiveStudentController(IExScoreRepository iExScoreRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, ICourseRepository iCourseRepository, IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, ICCTRepository iCCTRepository, ITeacherRepository iTeacherRepository, IExamDetailRepository iExamDetailRepository, IExamDetailScoreRepository iExamDetailScoreRepository)
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
        }


        [HttpGet]
        public async Task<MessageModel<ObjectiveStudent>> Get(int GradeId, string AcademicYearSchoolTerm, string ExamName, int CourseId, int ClazzId, int page = 1)
        {
            int intPageSize = 100;
            if (!(GradeId > 0 && CourseId > 0 && ClazzId > 0 && !string.IsNullOrEmpty(AcademicYearSchoolTerm) && !string.IsNullOrEmpty(ExamName)))
            {
                return new MessageModel<ObjectiveStudent>();
            }


            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false && d.Id == GradeId);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false && d.GradeId == GradeId);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var cctList = await _iCCTRepository.Query(d => d.IsDeleted == false && d.gradeid == GradeId);
            var teachersList = await _iTeacherRepository.Query(d => d.IsDeleted == false && d.gradeId == GradeId);

            //题目
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
            studentsList = studentsList.Where(d => d.gradeid == GradeId && d.clazzid == ClazzId).ToList();

            StringBuilder jsonBuilder = new StringBuilder();
            StringBuilder jsonBuilderHeader = new StringBuilder();
            jsonBuilder.Append("[");
            jsonBuilderHeader.Append("[");
            jsonBuilderHeader.Append("{\"prop\": \"学号\", \"label\": \"学号\"},");
            jsonBuilderHeader.Append("{\"prop\": \"姓名\", \"label\": \"姓名\"},");
            jsonBuilderHeader.Append("{\"prop\": \"班级\", \"label\": \"班级\"},");


            foreach (var item in studentsList)
            {
                var clazzModel = clazzList.Where(d => d.Id == item.clazzid).FirstOrDefault();
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"");
                jsonBuilder.Append("学号");
                jsonBuilder.Append("\":\"");
                jsonBuilder.Append(item.StudentNo.ObjToString().Replace("\"", "\\\""));
                jsonBuilder.Append("\",");

                jsonBuilder.Append("\"");
                jsonBuilder.Append("姓名");
                jsonBuilder.Append("\":\"");
                jsonBuilder.Append(item.Name.ObjToString().Replace("\"", "\\\""));
                jsonBuilder.Append("\",");

                jsonBuilder.Append("\"");
                jsonBuilder.Append("班级");
                jsonBuilder.Append("\":\"");
                jsonBuilder.Append(clazzModel.ClassNo.ObjToString().Replace("\"", "\\\""));
                jsonBuilder.Append("\",");


                for (int j = 0; j < examDetailList.Count; j++)
                {
                    var examDetailScore = examDetailScoreList.Where(d => d.ExamDetailId == examDetailList[j].Id && d.studentid == item.Id).FirstOrDefault();
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(examDetailList[j].Name);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(examDetailScore.StudentAnswer.ObjToString().Replace("\"", "\\\""));
                    jsonBuilder.Append("\",");
                }


                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }


            for (int j = 0; j < examDetailList.Count; j++)
            {
                jsonBuilderHeader.Append("{\"prop\": \"" + examDetailList[j].Name + "\", \"label\": \"" + examDetailList[j].Name + "\"},");
            }


            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilderHeader.Remove(jsonBuilderHeader.Length - 1, 1);
            jsonBuilder.Append("]");
            jsonBuilderHeader.Append("]");


            ObjectiveStudent objectiveStudent = new ObjectiveStudent()
            {
                Header = jsonBuilderHeader.ObjToString(),
                Content = jsonBuilder.ObjToString()

            };


            return new MessageModel<ObjectiveStudent>()
            {
                msg = "获取成功",
                success = true,
                response = objectiveStudent
            };

        }

    }

    public class ObjectiveStudent
    {
        public string Header { get; set; }
        public string Content { get; set; }


    }
}


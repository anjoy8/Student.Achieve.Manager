using System.Threading.Tasks;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Student.Achieve.Common.HttpContextUser;
using Microsoft.AspNetCore.Authorization;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 考试管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ExamController : ControllerBase
    {
        private readonly IExamRepository _iExamRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IUser _iUser;
        private int GID = 0;


        public ExamController(IExamRepository iExamRepository,IGradeRepository iGradeRepository,ICourseRepository iCourseRepository, IUser iUser)
        {
            this._iExamRepository = iExamRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCourseRepository = iCourseRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部考试
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Exam
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<Exam>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            else
            {
                page = 1;
            }
            int intPageSize = 50;


            var data = await _iExamRepository.QueryPage(a => (a.IsDeleted == false && (a.ExamName != null && a.ExamName.Contains(key)))&& (a.gradeid == GID || (GID == -9999 && true)), page, intPageSize, " Id asc ");

            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var coureseList = await _iCourseRepository.Query(d => d.IsDeleted == false);

            foreach (var item in data.data)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = coureseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            return new MessageModel<PageModel<Exam>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Exam/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Exam>> Get(string id)
        {
            var data = await _iExamRepository.QueryById(id);

            return new MessageModel<Exam>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个考试
        /// </summary>
        /// <param name="Exam"></param>
        /// <returns></returns>
        // POST: api/Exam
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Exam Exam)
        {
            var data = new MessageModel<string>();

            var id = await _iExamRepository.Add(Exam);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新考试
        /// </summary>
        /// <param name="Exam"></param>
        /// <returns></returns>
        // PUT: api/Exam/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Exam Exam)
        {
            var data = new MessageModel<string>();
            if (Exam != null && Exam.Id > 0)
            {

                data.success = await _iExamRepository.Update(Exam);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = Exam?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除考试
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(int id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var model = await _iExamRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iExamRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Exam/GetExamTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetExamTree(int gid = 0)
        {
            var ExamList = await _iExamRepository.Query(d => d.IsDeleted == false);
            if (gid > 0)
            {
                ExamList = ExamList.Where(d => d.gradeid == gid).ToList();
            }
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            foreach (var item in ExamList)
            {
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
            }
            var data = ExamList.Select(d => new TreeModel { value = d.Id, label = d.grade.EnrollmentYear + "级" + d.grade.Name + " " + d.AcademicYear + "学年" + d.SchoolTerm + " " + d.ExamName + " " + d.course.Name }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
        // GET: api/Exam/GetExamTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel2>>> GetExamTreeYearTerm(int gid = 0)
        {
            var ExamList = await _iExamRepository.Query(d => d.IsDeleted == false);
            if (gid > 0)
            {
                ExamList = ExamList.Where(d => d.gradeid == gid).ToList();
            }

            var data = ExamList.Select(d => new TreeModel2 { value = d.AcademicYear + d.SchoolTerm, label = d.AcademicYear + d.SchoolTerm }).ToList();

            data = data.GroupBy(x => new { x.value, x.label }).Select(x => x.First()).ToList();

            return new MessageModel<List<TreeModel2>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
        // GET: api/Exam/GetExamTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel2>>> GetExamTreeExam(int gid = 0)
        {
            var ExamList = await _iExamRepository.Query(d => d.IsDeleted == false);
            if (gid > 0)
            {
                ExamList = ExamList.Where(d => d.gradeid == gid).ToList();
            }

            var data = ExamList.Select(d => new TreeModel2 { value = d.ExamName, label = d.ExamName }).ToList();

            data = data.GroupBy(x => new { x.value, x.label }).Select(x => x.First()).ToList();
            return new MessageModel<List<TreeModel2>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}

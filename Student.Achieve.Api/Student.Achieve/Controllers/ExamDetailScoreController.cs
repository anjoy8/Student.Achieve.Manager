using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.Common.HttpContextUser;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 题目得分管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ExamDetailScoreController : ControllerBase
    {
        private readonly IExamDetailScoreRepository _iExamDetailScoreRepository;
        private readonly IExamDetailRepository _iExamDetailRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IUser _iUser;
        private int GID = 0;

        public ExamDetailScoreController(IExamDetailRepository iExamDetailRepository, IExamRepository iExamRepository, IGradeRepository iGradeRepository, ICourseRepository iCourseRepository, IExamDetailScoreRepository iExamDetailScoreRepository,IStudentsRepository iStudentsRepository, IUser iUser)
        {
            this._iExamDetailRepository = iExamDetailRepository;
            this._iExamRepository = iExamRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCourseRepository = iCourseRepository;
            this._iExamDetailScoreRepository = iExamDetailScoreRepository;
            this._iStudentsRepository = iStudentsRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部题目得分
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/ExamDetailScore
        [HttpGet]
        public async Task<MessageModel<PageModel<ExamDetailScore>>> Get(int page = 1, string key = "")
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

            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false);
            var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false);
            var examDeatilScoreDetailList = await _iExamDetailScoreRepository.Query(d => d.IsDeleted == false);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);

            foreach (var item in examList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var item in examDetailList)
            {
                item.exam = examList.Where(d => d.Id == item.examid).FirstOrDefault();
            }
            foreach (var item in examDeatilScoreDetailList)
            {
                item.ExamDetail = examDetailList.Where(d => d.Id == item.ExamDetailId).FirstOrDefault();
                item.student = studentsList.Where(d => d.Id == item.studentid).FirstOrDefault();
            }


            examDeatilScoreDetailList = examDeatilScoreDetailList.Where(d => (d.ExamDetail.exam.grade.Id == GID || (GID == -9999 && true)) && d.student.Name.Contains(key)).ToList();


            if (!string.IsNullOrEmpty(key))
            {
                examDeatilScoreDetailList = examDeatilScoreDetailList.Where(d => d.student.Name.Contains(key)).ToList();
            }
            var totalCount = examDeatilScoreDetailList.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exdetails = examDeatilScoreDetailList.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();
            var data = new PageModel<ExamDetailScore>()
            {
                data = exdetails,
                dataCount = totalCount,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };




            return new MessageModel<PageModel<ExamDetailScore>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/ExamDetailScore/5
        [HttpGet("{id}")]
        public async Task<MessageModel<ExamDetailScore>> Get(string id)
        {
            var data = await _iExamDetailScoreRepository.QueryById(id);

            return new MessageModel<ExamDetailScore>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个题目得分
        /// </summary>
        /// <param name="ExamDetailScore"></param>
        /// <returns></returns>
        // POST: api/ExamDetailScore
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] ExamDetailScore ExamDetailScore)
        {
            var data = new MessageModel<string>();

            var id = await _iExamDetailScoreRepository.Add(ExamDetailScore);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新题目得分
        /// </summary>
        /// <param name="ExamDetailScore"></param>
        /// <returns></returns>
        // PUT: api/ExamDetailScore/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] ExamDetailScore ExamDetailScore)
        {
            var data = new MessageModel<string>();
            if (ExamDetailScore != null && ExamDetailScore.Id > 0)
            {

                data.success = await _iExamDetailScoreRepository.Update(ExamDetailScore);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = ExamDetailScore?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除题目得分
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
                var model = await _iExamDetailScoreRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iExamDetailScoreRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



       
    }
}

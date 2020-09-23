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
    /// 成绩管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ExScoreController : ControllerBase
    {
        private readonly IExScoreRepository _iExScoreRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly IUser _iUser;
        private int GID = 0;

        public ExScoreController(IExScoreRepository iExScoreRepository,IStudentsRepository iStudentsRepository,IExamRepository iExamRepository,ICourseRepository iCourseRepository,IClazzRepository iClazzRepository,IGradeRepository iGradeRepository, IUser iUser)
        {
            this._iExScoreRepository = iExScoreRepository;
            this._iStudentsRepository = iStudentsRepository;
            this._iExamRepository = iExamRepository;
            this._iCourseRepository = iCourseRepository;
            this._iClazzRepository = iClazzRepository;
            this._iGradeRepository = iGradeRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部成绩
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/ExScore
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<ExScore>>> Get(int page = 1, string key = "")
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
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false);
            var exScoreList = await _iExScoreRepository.Query(d => d.IsDeleted == false);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);

            foreach (var item in examList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var item in exScoreList)
            {
                item.clazz = clazzList.Where(d => d.Id == item.clazzid).FirstOrDefault();
                item.exam = examList.Where(d => d.Id == item.examid).FirstOrDefault();
                item.student = studentsList.Where(d => d.Id == item.studentid).FirstOrDefault();
            }


            exScoreList = exScoreList.Where(d =>(d.exam != null && d.exam.grade != null && d.exam.grade.Id == GID|| (GID == -9999 && true)) && d.student.Name.Contains(key)).ToList();


            if (!string.IsNullOrEmpty(key))
            {
                exScoreList = exScoreList.Where(d => d.student != null&&d.student.Name.Contains(key)).ToList();
            }
            var totalCount = exScoreList.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exScores = exScoreList.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();
            PageModel<ExScore> data = new PageModel<ExScore>() {
                data=exScores,
                dataCount= exScoreList.Count,
                page=page,
                pageCount= pageCount,
                PageSize=intPageSize
            };


            return new MessageModel<PageModel<ExScore>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/ExScore/5
        [HttpGet("{id}")]
        public async Task<MessageModel<ExScore>> Get(string id)
        {
            var data = await _iExScoreRepository.QueryById(id);

            return new MessageModel<ExScore>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个成绩
        /// </summary>
        /// <param name="ExScore"></param>
        /// <returns></returns>
        // POST: api/ExScore
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] ExScore ExScore)
        {
            var data = new MessageModel<string>();

            var examModel = await _iExamRepository.QueryById(ExScore.examid);
            var studentModel = await _iStudentsRepository.QueryById(ExScore.studentid);
            ExScore.clazzid = studentModel.clazzid;
            ExScore.courseid = examModel.courseid;


            var id = await _iExScoreRepository.Add(ExScore);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新成绩
        /// </summary>
        /// <param name="ExScore"></param>
        /// <returns></returns>
        // PUT: api/ExScore/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] ExScore ExScore)
        {
            var data = new MessageModel<string>();
            if (ExScore != null && ExScore.Id > 0)
            {

                data.success = await _iExScoreRepository.Update(ExScore);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = ExScore?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除成绩
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
                var model = await _iExScoreRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iExScoreRepository.Update(model);
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

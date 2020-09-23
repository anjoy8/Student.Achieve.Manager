using System.Threading.Tasks;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using Student.Achieve.Common.HttpContextUser;
using Microsoft.AspNetCore.Authorization;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 题目管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ExamDetailController : ControllerBase
    {
        private readonly IExamDetailRepository _iExamDetailRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IUser _iUser;
        private int GID = 0;


        public ExamDetailController(IExamDetailRepository iExamDetailRepository, IExamRepository iExamRepository, IGradeRepository iGradeRepository, ICourseRepository iCourseRepository, IUser iUser)
        {
            this._iExamDetailRepository = iExamDetailRepository;
            this._iExamRepository = iExamRepository;
            this._iGradeRepository = iGradeRepository;
            this._iCourseRepository = iCourseRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部题目
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/ExamDetail
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<ExamDetail>>> Get(int page = 1, string key = "")
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
            var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false&& (d.gradeid == GID || (GID == -9999 && true)));

            foreach (var item in examList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var item in examDetailList)
            {
                item.exam = examList.Where(d => d.Id == item.examid).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(key))
            {
                examDetailList = examDetailList.Where(d => d.Name.Contains(key)).ToList();
            }
            var totalCount = examDetailList.Count;
            int pageCount = (Math.Ceiling(totalCount.ObjToDecimal() / intPageSize.ObjToDecimal())).ObjToInt();


            var exdetails = examDetailList.Skip((page - 1) * intPageSize).Take(intPageSize).ToList();
            var data = new PageModel<ExamDetail>()
            {
                data = exdetails,
                dataCount = totalCount,
                page = page,
                pageCount = pageCount,
                PageSize = intPageSize
            };




            return new MessageModel<PageModel<ExamDetail>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/ExamDetail/5
        [HttpGet("{id}")]
        public async Task<MessageModel<ExamDetail>> Get(string id)
        {
            var data = await _iExamDetailRepository.QueryById(id);

            return new MessageModel<ExamDetail>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个题目
        /// </summary>
        /// <param name="ExamDetail"></param>
        /// <returns></returns>
        // POST: api/ExamDetail
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] ExamDetail ExamDetail)
        {
            var data = new MessageModel<string>();

            var id = await _iExamDetailRepository.Add(ExamDetail);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新题目
        /// </summary>
        /// <param name="ExamDetail"></param>
        /// <returns></returns>
        // PUT: api/ExamDetail/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] ExamDetail ExamDetail)
        {
            var data = new MessageModel<string>();
            if (ExamDetail != null && ExamDetail.Id > 0)
            {

                data.success = await _iExamDetailRepository.Update(ExamDetail);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = ExamDetail?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除题目
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
                var model = await _iExamDetailRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iExamDetailRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/ExamDetail/GetExamDetailTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetExamDetailTree()
        {
            var ExamDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false);
            var data = ExamDetailList.Select(d => new TreeModel { value = d.Id, label = d.Name }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}

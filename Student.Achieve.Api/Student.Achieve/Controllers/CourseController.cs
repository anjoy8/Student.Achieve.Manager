using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 课程管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _iCourseRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCourseRepository"></param>
        public CourseController(ICourseRepository iCourseRepository)
        {
            this._iCourseRepository = iCourseRepository;
        }

        /// <summary>
        /// 获取全部课程
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Course
        [HttpGet]
        public async Task<MessageModel<PageModel<Course>>> Get(int page = 1, string key = "")
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


            var data = await _iCourseRepository.QueryPage(a => a.IsDeleted == false && (a.Name != null && a.Name.Contains(key)), page, intPageSize, " Id asc ");



            return new MessageModel<PageModel<Course>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Course>> Get(string id)
        {
            var data = await _iCourseRepository.QueryById(id);

            return new MessageModel<Course>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个课程
        /// </summary>
        /// <param name="Course"></param>
        /// <returns></returns>
        // POST: api/Course
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Course Course)
        {
            var data = new MessageModel<string>();

            var id = await _iCourseRepository.Add(Course);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新课程
        /// </summary>
        /// <param name="Course"></param>
        /// <returns></returns>
        // PUT: api/Course/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Course Course)
        {
            var data = new MessageModel<string>();
            if (Course != null && Course.Id > 0)
            {

                data.success = await _iCourseRepository.Update(Course);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = Course?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除课程
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
                var model = await _iCourseRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iCourseRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Course/GetCourseTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetCourseTree()
        {
            var gradeList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var data = gradeList.Select(d => new TreeModel { value = d.Id, label = d.Name }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}

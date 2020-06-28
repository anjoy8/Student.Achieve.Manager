using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Achieve.Common.HttpContextUser;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 学生管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly IUser _iUser;
        private int GID = 0;


        public StudentsController(IStudentsRepository iStudentsRepository, IGradeRepository iGradeRepository, IClazzRepository iClazzRepository, IUser iUser)
        {
            this._iStudentsRepository = iStudentsRepository;
            this._iGradeRepository = iGradeRepository;
            this._iClazzRepository = iClazzRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部年级
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Students
        [HttpGet]
        public async Task<MessageModel<PageModel<Students>>> Get(int page = 1, string key = "")
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


            var data = await _iStudentsRepository.QueryPage(a => (a.IsDeleted == false && (a.Name != null && a.Name.Contains(key)))&& (a.gradeid == GID || (GID == -9999 && true)), page, intPageSize, " Id asc ");


            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);

            foreach (var item in data.data)
            {
                item.clazz = clazzList.Where(d => d.Id == item.clazzid).FirstOrDefault();
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
            }

            return new MessageModel<PageModel<Students>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Students>> Get(string id)
        {
            var data = await _iStudentsRepository.QueryById(id);

            return new MessageModel<Students>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个年级
        /// </summary>
        /// <param name="Students"></param>
        /// <returns></returns>
        // POST: api/Students
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Students Students)
        {
            var data = new MessageModel<string>();

            var id = await _iStudentsRepository.Add(Students);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新年级
        /// </summary>
        /// <param name="Students"></param>
        /// <returns></returns>
        // PUT: api/Students/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Students Students)
        {
            var data = new MessageModel<string>();
            if (Students != null && Students.Id > 0)
            {

                data.success = await _iStudentsRepository.Update(Students);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = Students?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除年级
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
                var model = await _iStudentsRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iStudentsRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Students/GetStudentsTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetStudentsTree(string name="")
        {
            var studentList = await _iStudentsRepository.Query(d => d.IsDeleted == false);

            if (name!="")
            {
                studentList = studentList.Where(d => d.Name.Contains(name)).ToList();
            }

            var data = studentList.Select(d => new TreeModel { value = d.Id, label = d.Name }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}

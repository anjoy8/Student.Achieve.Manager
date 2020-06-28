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
    /// 年级管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class GradeController : ControllerBase
    {
        private readonly IGradeRepository _iGradeRepository;
        private readonly IUser _iUser;
        private int GID = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iGradeRepository"></param>
        public GradeController(IGradeRepository iGradeRepository, IUser iUser)
        {
            this._iGradeRepository = iGradeRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部年级
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Grade
        [HttpGet]
        public async Task<MessageModel<PageModel<Grade>>> Get(int page = 1, string key = "")
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


            var data = await _iGradeRepository.QueryPage(a => (a.IsDeleted == false && (a.Name != null && a.Name.Contains(key)))&&(a.Id== GID||(GID==-9999&&true)), page, intPageSize, " Id asc ");



            return new MessageModel<PageModel<Grade>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Grade/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Grade>> Get(string id)
        {
            var data = await _iGradeRepository.QueryById(id);

            return new MessageModel<Grade>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个年级
        /// </summary>
        /// <param name="Grade"></param>
        /// <returns></returns>
        // POST: api/Grade
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Grade Grade)
        {
            var data = new MessageModel<string>();

            var id = await _iGradeRepository.Add(Grade);

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
        /// <param name="Grade"></param>
        /// <returns></returns>
        // PUT: api/Grade/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Grade Grade)
        {
            var data = new MessageModel<string>();
            if (Grade != null && Grade.Id > 0)
            {

                data.success = await _iGradeRepository.Update(Grade);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = Grade?.Id.ObjToString();
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
                var model = await _iGradeRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iGradeRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Grade/GetGradeTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetGradeTree()
        {
            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var data = gradeList.Select(d => new TreeModel { value = d.Id, label = d.EnrollmentYear+"级"+d.Name }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}

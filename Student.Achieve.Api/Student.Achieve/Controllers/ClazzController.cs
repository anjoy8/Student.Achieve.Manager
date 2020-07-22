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
    /// 班级管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class ClazzController : ControllerBase
    {
        private readonly IClazzRepository _iClazzRepository;
        private readonly IUser _iUser;
        private int GID = 0;


        public ClazzController(IClazzRepository iClazzRepository, IGradeRepository iGradeRepository, IUser iUser)
        {
            _iClazzRepository = iClazzRepository;
            _iUser = iUser;
            GID = (_iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部班级
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Clazz
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<Clazz>>> Get(int page = 1, string key = "")
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

#if DEBUG
            GID = 1;
#endif

            var data = await _iClazzRepository.GetQueryPageOfMapperTb(a => (a.IsDeleted == false && (a.Name != null && a.Name.Contains(key))) && (a.GradeId == GID || (GID == -9999 && true)), page, intPageSize, " Id asc ");


            return new MessageModel<PageModel<Clazz>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Clazz/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Clazz>> Get(string id)
        {
            var data = await _iClazzRepository.QueryById(id);

            return new MessageModel<Clazz>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个班级
        /// </summary>
        /// <param name="Clazz"></param>
        /// <returns></returns>
        // POST: api/Clazz
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Clazz Clazz)
        {
            var data = new MessageModel<string>();

            var id = await _iClazzRepository.Add(Clazz);

            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新班级
        /// </summary>
        /// <param name="Clazz"></param>
        /// <returns></returns>
        // PUT: api/Clazz/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Clazz Clazz)
        {
            var data = new MessageModel<string>();
            if (Clazz != null && Clazz.Id > 0)
            {

                data.success = await _iClazzRepository.Update(Clazz);
                if (data.success)
                {
                    data.msg = "更新成功";
                    data.response = Clazz?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除班级
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
                var model = await _iClazzRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iClazzRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Clazz/GetClazzTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetClazzTree(int gid = 0)
        {
            List<Clazz> clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
            if (gid > 0)
            {
                clazzList = clazzList.Where(d => d.GradeId == gid).ToList();
            }

            var data = clazzList.Select(d => new TreeModel { value = d.Id, label = d.ClassNo }).ToList();

            return new MessageModel<List<TreeModel>>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }
    }
}


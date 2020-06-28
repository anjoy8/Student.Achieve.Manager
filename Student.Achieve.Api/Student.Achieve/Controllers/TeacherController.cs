using System.Threading.Tasks;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Student.Achieve.Common.Helper;
using Student.Achieve.Common.HttpContextUser;
using Microsoft.AspNetCore.Authorization;

namespace Student.Achieve.Controllers
{
    /// <summary>
    /// 年级管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly ICCTRepository _iCCTRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly IUser _iUser;
        private int GID = 0;

        public TeacherController(ITeacherRepository iTeacherRepository, ICCTRepository iCCTRepository, IGradeRepository iGradeRepository, IClazzRepository iClazzRepository, ICourseRepository iCourseRepository, IUser iUser)
        {
            this._iTeacherRepository = iTeacherRepository;
            this._iCCTRepository = iCCTRepository;
            this._iGradeRepository = iGradeRepository;
            this._iClazzRepository = iClazzRepository;
            this._iCourseRepository = iCourseRepository;
            GID = (iUser.GetClaimValueByType("GID").FirstOrDefault()).ObjToInt();
        }

        /// <summary>
        /// 获取全部年级
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        // GET: api/Teacher
        [HttpGet]
        public async Task<MessageModel<PageModel<Teacher>>> Get(int page = 1, string key = "")
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


            var data = await _iTeacherRepository.QueryPage(a => (a.IsDeleted == false && (a.Name != null && a.Name.Contains(key)))&& (a.gradeId == GID || (GID == -9999 && true)), page, intPageSize, " Id asc ");

            var cctList = await _iCCTRepository.Query(d => d.IsDeleted == false);
            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
            var coureseList = await _iCourseRepository.Query(d => d.IsDeleted == false);

            foreach (var item in cctList)
            {
                item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                item.clazz = clazzList.Where(d => d.Id == item.clazzid).FirstOrDefault();
                item.course = coureseList.Where(d => d.Id == item.courseid).FirstOrDefault();
            }

            foreach (var item in data.data)
            {
                item.cct = cctList.Where(d => d.teacherid == item.Id).ToList();
                item.gradeId = (item.cct.FirstOrDefault()?.gradeid).ObjToInt();
                item.courseId = (item.cct.FirstOrDefault()?.courseid).ObjToInt();
                item.clazzIds = item.cct.Select(d => d.clazzid).ToArray();
            }

            return new MessageModel<PageModel<Teacher>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };

        }

        // GET: api/Teacher/5
        [HttpGet("{id}")]
        public async Task<MessageModel<Teacher>> Get(string id)
        {
            var data = await _iTeacherRepository.QueryById(id);

            return new MessageModel<Teacher>()
            {
                msg = "获取成功",
                success = data != null,
                response = data
            };
        }


        /// <summary>
        /// 添加一个年级
        /// </summary>
        /// <param name="Teacher"></param>
        /// <returns></returns>
        // POST: api/Teacher
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Teacher Teacher)
        {
            var data = new MessageModel<string>();
            Teacher.Password = MD5Helper.MD5Encrypt32(Teacher.Account);
            var id = await _iTeacherRepository.Add(Teacher);

            data.success = id > 0;
            if (data.success)
            {
                List<CCT> cCTs = (from item in Teacher.clazzIds
                                  select new CCT
                                  {
                                      IsDeleted = false,
                                      clazzid = item,
                                      courseid = Teacher.courseId,
                                      teacherid = id,
                                      gradeid = Teacher.gradeId,
                                  }).ToList();

                var newDataSave = await _iCCTRepository.Add(cCTs);

                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新年级
        /// </summary>
        /// <param name="Teacher"></param>
        /// <returns></returns>
        // PUT: api/Teacher/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Teacher Teacher)
        {
            var data = new MessageModel<string>();
            if (Teacher != null && Teacher.Id > 0)
            {

                data.success = await _iTeacherRepository.Update(Teacher);
                if (data.success)
                {
                    var cctCureent = await _iCCTRepository.Query(d => d.teacherid == Teacher.Id);
                    var deleteSave = await _iCCTRepository.DeleteByIds(cctCureent.Select(d => d.Id.ToString()).ToArray());

                    List<CCT> cCTs = (from item in Teacher.clazzIds
                                      select new CCT
                                      {
                                          IsDeleted = false,
                                          clazzid = item,
                                          courseid = Teacher.courseId,
                                          teacherid = Teacher.Id,
                                          gradeid = Teacher.gradeId,
                                      }).ToList();

                    var newDataSave = await _iCCTRepository.Add(cCTs);

                    data.msg = "更新成功";
                    data.response = Teacher?.Id.ObjToString();
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
                var model = await _iTeacherRepository.QueryById(id);
                model.IsDeleted = true;
                data.success = await _iTeacherRepository.Update(model);
                if (data.success)
                {
                    data.msg = "删除成功";
                    data.response = model?.Id.ObjToString();
                }
            }

            return data;
        }



        // GET: api/Teacher/GetTeacherTree
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<TreeModel>>> GetTeacherTree()
        {
            var gradeList = await _iTeacherRepository.Query(d => d.IsDeleted == false);
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

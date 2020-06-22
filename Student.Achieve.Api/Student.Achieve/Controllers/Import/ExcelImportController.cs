using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Student.Achieve.Common.Helper;
using Student.Achieve.IRepository;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using Student.Achieve.Tran;

namespace Student.Achieve.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class ExcelImportController : ControllerBase
    {
        private readonly ICCTRepository _iCCTRepository;
        private readonly IClazzRepository _iClazzRepository;
        private readonly ICourseRepository _iCourseRepository;
        private readonly ITeacherRepository _iTeacherRepository;
        private readonly IGradeRepository _iGradeRepository;
        private readonly IStudentsRepository _iStudentsRepository;
        private readonly IExamRepository _iExamRepository;
        private readonly IExScoreRepository _iExScoreRepository;
        private readonly IExamDetailRepository _iExamDetailRepository;
        private readonly IExamDetailScoreRepository _iExamDetailScoreRepository;

        public ExcelImportController(ICCTRepository iCCTRepository, IClazzRepository iClazzRepository, ICourseRepository iCourseRepository, ITeacherRepository iTeacherRepository, IGradeRepository iGradeRepository, IStudentsRepository iStudentsRepository, IExamRepository iExamRepository, IExScoreRepository iExScoreRepository, IExamDetailRepository iExamDetailRepository, IExamDetailScoreRepository iExamDetailScoreRepository)
        {
            this._iCCTRepository = iCCTRepository;
            this._iClazzRepository = iClazzRepository;
            this._iCourseRepository = iCourseRepository;
            this._iTeacherRepository = iTeacherRepository;
            this._iGradeRepository = iGradeRepository;
            this._iStudentsRepository = iStudentsRepository;
            this._iExamRepository = iExamRepository;
            this._iExScoreRepository = iExScoreRepository;
            this._iExamDetailRepository = iExamDetailRepository;
            this._iExamDetailScoreRepository = iExamDetailScoreRepository;
        }





        [HttpPost]
        public async Task<MessageModel<string>> ImportTeacherDataFile([FromServices]IHostingEnvironment environment, string datatype)
        {
            var data = new MessageModel<string>();
            string path = string.Empty;
            string foldername = "Excels";
            IFormFileCollection files = null;

            try
            {
                files = Request.Form.Files;
            }
            catch (Exception)
            {
                files = null;
            }

            if (files == null || !files.Any()) { data.msg = "请选择上传的文件。"; return data; }
            //格式限制
            var allowType = new string[] { "xlsx" };

            string folderpath = Path.Combine(environment.WebRootPath, foldername);
            if (!System.IO.Directory.Exists(folderpath))
            {
                System.IO.Directory.CreateDirectory(folderpath);
            }

            //if (files.Any(c => allowType.Contains(c.ContentType)))
            {
                if (files.Sum(c => c.Length) <= 1024 * 1024 * 10)
                {
                    //foreach (var file in files)
                    var file = files.FirstOrDefault();
                    string strpath = Path.Combine(foldername, DateTime.Now.ToString("MMddHHmmss") + file.FileName);
                    path = Path.Combine(environment.WebRootPath, strpath);

                    using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        await file.CopyToAsync(stream);
                    }

                    bool isInsertSuc = false;
                    if (datatype == "t")
                    {
                        isInsertSuc = await ImportTeacherData(path);
                    }
                    else if (datatype == "s")
                    {
                        isInsertSuc = await ImportStudentData(path);
                    }
                    else if (datatype == "es")
                    {
                        isInsertSuc = await ImportExScoreData(path);
                    }
                    else if (datatype == "ed")
                    {
                        isInsertSuc = await ImportExamDetailData(path);
                    }
                    else if (datatype == "eds")
                    {
                        isInsertSuc = await ImportEDetailScoreData(path);
                    }
                    else if (datatype == "edsa")
                    {
                        isInsertSuc = await ImportEDetailScoreAnswerData(path);
                    }
                    else if (datatype == "bs")
                    {
                        isInsertSuc = await ImportBaseSortData(path);
                    }


                    data = new MessageModel<string>()
                    {
                        response = strpath,
                        msg = isInsertSuc ? "添加成功" : "添加失败",
                        success = isInsertSuc,
                    };
                    return data;
                }
                else
                {
                    data.msg = "文件过大";
                    return data;
                }
            }

        }




        [HttpGet]
        public async Task<bool> ImportTeacherData(string filepath)
        {
            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("级组", "ClazzLevel");
            sqlNameDics.Add("级长", "Manager");
            sqlNameDics.Add("班级", "Clazz");
            sqlNameDics.Add("班类", "ClazzType");
            sqlNameDics.Add("选科", "ChooseSub");
            sqlNameDics.Add("班主任", "TeacherCharge");
            sqlNameDics.Add("语文", "Chinese");
            sqlNameDics.Add("数学", "Meth");
            sqlNameDics.Add("英语", "English");
            sqlNameDics.Add("物理", "Physics");
            sqlNameDics.Add("化学", "Chemistry");
            sqlNameDics.Add("生物", "Biology");
            sqlNameDics.Add("政治", "Politics");
            sqlNameDics.Add("历史", "History");
            sqlNameDics.Add("地理", "Geography");

            List<string> courseArr = new List<string>();//科目
            List<TCCRoot> tCCRoots = new List<TCCRoot>();
            List<CCT> cCTs = new List<CCT>();

            try
            {
                var data = GetTeacherDataSetFromExcel(filepath, sqlNameDics, ref courseArr, ref tCCRoots);

                var tCCRootsOrder = tCCRoots.GroupBy(x => new { x.courseName, x.teacherName }).Select(x => x.First()).ToList();


                var jsons = ExcelHelper.DatasetToJson(data);

                List<TeacherDataRoot> teacherDataRoots = new List<TeacherDataRoot>();
                teacherDataRoots = JsonConvert.DeserializeObject<List<TeacherDataRoot>>(jsons);


                List<Course> courses = (from item in courseArr
                                        select new Course
                                        {
                                            Name = item,
                                            IsDeleted = false,
                                            CreateTime = DateTime.Now
                                        }).GroupBy(x => new { x.Name }).Select(x => x.First()).ToList();

                List<Grade> grades = (from item in teacherDataRoots
                                      select new Grade
                                      {
                                          Name = item.Grade,
                                          EnrollmentYear = item.EnrollmentYear,
                                          IsDeleted = false,
                                          CreateTime = DateTime.Now
                                      }).GroupBy(x => new { x.Name, x.EnrollmentYear }).Select(x => x.First()).ToList();


                var courseSqlList = await _iCourseRepository.Query(d => d.IsDeleted == false);
                if (!courseSqlList.Any())
                {
                    var courseSave = await _iCourseRepository.Add(courses);
                }


                var gradeSqlList = await _iGradeRepository.Query(d => d.IsDeleted == false);
                courseSqlList = await _iCourseRepository.Query(d => d.IsDeleted == false);



                BaseDao<Grade> gradDao = new BaseDao<Grade>();
                BaseDao<Clazz> clazzDao = new BaseDao<Clazz>();
                BaseDao<Teacher> teacherDao = new BaseDao<Teacher>();
                BaseDao<Course> courseDao = new BaseDao<Course>();
                BaseDao<CCT> cctDao = new BaseDao<CCT>();

                try
                {
                    gradDao.BeginTran();

                    foreach (var grade in grades)
                    {
                        int gradeId = 0;
                        //这里直接保存 grade 单个实体;
                        //先去数据库查，看是否已经存下来了，主要是保存，获取年级id
                        var gradeSqlModel = gradeSqlList.Where(d => d.EnrollmentYear == grade.EnrollmentYear && d.Name == grade.Name);
                        if (gradeSqlModel.Any())
                        {
                            gradeId = gradeSqlModel.FirstOrDefault().Id;
                        }
                        else
                        {
                            //gradeId = await _iGradeRepository.Add(grade);
                            gradeId = gradDao.Add(grade);
                        }

                        if (gradeId > 0)
                        {

                            List<Clazz> clazzs = (from item in teacherDataRoots
                                                  where item.Grade == grade.Name && item.EnrollmentYear == grade.EnrollmentYear
                                                  select new Clazz
                                                  {
                                                      ClassNo = item.Clazz,
                                                      Name = item.Clazz,
                                                      Manager = item.Manager,
                                                      GradeId = gradeId,
                                                      IsDeleted = false,
                                                      ClazzLevel = item.ClazzLevel,
                                                      ClazzType = item.ClazzType,
                                                      CreateTime = DateTime.Now,
                                                      TeacherCharge = item.TeacherCharge,
                                                      ChooseSub = item.ChooseSub,
                                                  }).ToList();

                            foreach (var clazz in clazzs)
                            {
                                int clazzid = 0;
                                //这里保存 clazz 单个实体
                                //得到班级id

                                //clazzid = await _iClazzRepository.Add(clazz);
                                clazzid = clazzDao.Add(clazz);
                                if (clazzid > 0)
                                {
                                    foreach (var course in courses)
                                    {
                                        // 教师id
                                        var t3id = 0;

                                        //这里保存 course 单个实体
                                        //先去数据库检查，如果不存在，则保存，主要是为了保存，并获取courseid；
                                        var courseid = courseSqlList.Where(d => d.Name == course.Name).FirstOrDefault()?.Id;



                                        var teacher = tCCRootsOrder.Where(d => d.courseName == course.Name && d.className == clazz.Name).FirstOrDefault();
                                        if (teacher != null)
                                        {
                                            var account = "";
                                            var name = "";
                                            if (teacher.teacherName != "" && teacher.teacherName.Contains("_"))
                                            {
                                                name = teacher.teacherName.Split("_")[0];
                                                account = teacher.teacherName.Split("_")[1];
                                            }
                                            else
                                            {
                                                name = teacher.teacherName;
                                            }
                                            Teacher model = new Teacher()
                                            {
                                                IsDeleted = false,
                                                Account = account,
                                                CreateTime = DateTime.Now,
                                                Name = name,
                                                Password = MD5Helper.MD5Encrypt32(account),
                                                TeacherNo = account,
                                                gradeId = gradeId,
                                            };

                                            // 保存教师 teacher 实体；
                                            //获取教师id；

                                            //var teacherModelId = await _iTeacherRepository.Add(model);
                                            var teacherModelId = teacherDao.Add(model);

                                            teacher.teacherId = teacherModelId;

                                            t3id = teacherModelId;


                                        }
                                        else
                                        {
                                            // 已经保存过了
                                            var teacher2 = tCCRoots.Where(d => d.courseName == course.Name && d.className == clazz.Name).FirstOrDefault();
                                            var teacher3 = tCCRootsOrder.Where(d => d.courseName == course.Name && d.teacherName == teacher2.teacherName).FirstOrDefault();

                                            t3id = teacher3.teacherId;


                                        }

                                        if (clazzid > 0 && courseid > 0 && gradeId > 0 && t3id > 0)
                                        {
                                            CCT cCT = new CCT()
                                            {
                                                IsDeleted = false,
                                                clazzid = clazzid,
                                                courseid = courseid.ObjToInt(),
                                                CreateTime = DateTime.Now,
                                                gradeid = gradeId,
                                                teacherid = t3id,
                                            };

                                            cCTs.Add(cCT);
                                        }
                                        else
                                        {
                                            throw new Exception("教师数据有误");
                                        }
                                    }
                                }

                            }


                        }



                    }

                    if (cCTs.Any())
                    {
                        //var cctSave = await _iCCTRepository.Add(cCTs);
                        var cctSave = cctDao.Add(cCTs);
                    }
                    else
                    {
                        throw new Exception("教师选课数据有误");

                    }
                    gradDao.CommitTran();
                }
                catch (Exception)
                {
                    gradDao.RollbackTran();
                    throw;
                }

            }
            catch (Exception ex)
            {
                //_iGradeRepository.RollbackTran();

                return false;
            }

            return true;
        }

        #region 将教师资料Excel中的内容转换成DataSet
        /// <summary>
        /// 将教师资料Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetTeacherDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic, ref List<string> course, ref List<TCCRoot> tCCRoots)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;
                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));
                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {
                                headers.Add("" + i, obj.ToString());

                                if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "年级") && (i == 2 && obj.ToString() == "级组") && (i == 3 && obj.ToString() == "级长") && (i == 4 && obj.ToString() == "班级") && (i == 5 && obj.ToString() == "班类"))
                                {
                                    throw new Exception("提交资料错误");

                                }

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));
                                if (j >= 8)
                                {
                                    var courseName = headers[j.ToString()];
                                    var teacherName = row.GetCell(j)?.ToString();
                                    var className = row.GetCell(4).ToString();
                                    TCCRoot tCCRoot = new TCCRoot()
                                    {
                                        className = className,
                                        courseName = courseName,
                                        teacherName = teacherName,
                                    };
                                    tCCRoots.Add(tCCRoot);

                                    course.Add(courseName);
                                }

                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                return ds;
            }
        }
        #endregion



        [HttpGet]
        public async Task<bool> ImportStudentData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("学号", "StudentNo");
            sqlNameDics.Add("姓名", "Name");
            sqlNameDics.Add("班级", "Clazz");
            sqlNameDics.Add("学科一", "SubjectA");
            sqlNameDics.Add("学科二", "SubjectB");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("在校情况", "InSchoolSituation");
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("性别", "Gender");
            sqlNameDics.Add("是否算指标", "IsIndicators");
            sqlNameDics.Add("高考考号", "UniversityEntranceNumber");
            sqlNameDics.Add("备注1", "Note1");
            sqlNameDics.Add("备注2", "Note2");
            sqlNameDics.Add("备注3", "Note3");


            try
            {
                var data = GetStudentDataSetFromExcel(filepath, sqlNameDics);
                var jsons = ExcelHelper.DatasetToJson(data);


                List<StudentDataRoot> studentDataRoots = new List<StudentDataRoot>();
                studentDataRoots = JsonConvert.DeserializeObject<List<StudentDataRoot>>(jsons);

                var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
                var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);

                List<Students> students = new List<Students>();

                foreach (var item in studentDataRoots)
                {
                    var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.EnrollmentYear && d.Name == item.Grade).FirstOrDefault()?.Id).ObjToInt();

                    var claazid = (clazzList.Where(d => d.GradeId == gradeid && d.ClassNo == item.Clazz.PadLeft(2, '0')).FirstOrDefault()?.Id).ObjToInt();

                    if (claazid > 0 && gradeid > 0)
                    {
                        Students student = new Students()
                        {
                            IsDeleted = false,
                            clazzid = claazid,
                            CreateTime = DateTime.Now,
                            Gender = item.Gender,
                            gradeid = gradeid,
                            InSchoolSituation = item.InSchoolSituation,
                            IsIndicators = item.IsIndicators,
                            Name = item.Name,
                            Note1 = item.Note1,
                            Note2 = item.Note2,
                            Note3 = item.Note3,
                            StudentNo = item.StudentNo,
                            EnrollmentYear = item.EnrollmentYear,
                            SubjectA = item.SubjectA,
                            SubjectB = item.SubjectB,
                            UniversityEntranceNumber = item.UniversityEntranceNumber,

                        };

                        students.Add(student);
                    }
                    else
                    {
                        throw new Exception("班级或年级错误！");
                    }
                }



                BaseDao<Students> studentsDao = new BaseDao<Students>();
                try
                {

                    studentsDao.BeginTran();
                    var studentSave = studentsDao.Add(students);
                    studentsDao.CommitTran();
                }
                catch (Exception)
                {
                    studentsDao.RollbackTran();
                    throw;
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #region 将学生资料Excel中的内容转换成DataSet
        /// <summary>
        /// 将学生资料Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetStudentDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;
                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));
                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {
                                headers.Add("" + i, obj.ToString());
                                if ((i == 0 && obj.ToString() == "学号") && (i == 1 && obj.ToString() == "姓名") && (i == 2 && obj.ToString() == "班级") && (i == 3 && obj.ToString() == "学科一") && (i == 4 && obj.ToString() == "学科二") && (i == 5 && obj.ToString() == "年级"))
                                {
                                    throw new Exception("提交资料错误");

                                }
                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));


                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                return ds;
            }
        }
        #endregion



        [HttpGet]
        public async Task<bool> ImportExScoreData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("学年", "AcademicYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("学生", "StudentNo");
            sqlNameDics.Add("班", "Clazz");
            sqlNameDics.Add("姓名", "StudentName");
            sqlNameDics.Add("语文", "Chinese");
            sqlNameDics.Add("数学", "Meth");
            sqlNameDics.Add("英语", "English");
            sqlNameDics.Add("物理", "Physics");
            sqlNameDics.Add("化学", "Chemistry");
            sqlNameDics.Add("政治", "Politics");
            sqlNameDics.Add("历史", "History");
            sqlNameDics.Add("生物", "Biology");
            sqlNameDics.Add("地理", "Geography");

            List<ESRoot> eSRoots = new List<ESRoot>();

            try
            {
                var data = GetExScoreDataSetFromExcel(filepath, sqlNameDics, ref eSRoots);

                //var jsons = ExcelHelper.DatasetToJson(data);

                var eSRootsGroup = eSRoots.GroupBy(x => new { x.academicYear2, x.enrollmentYear, x.grade, x.schoolTerm, x.examName, x.course }).Select(x => x.First()).ToList();


                var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
                var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);
                var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
                var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);

                List<Exam> exams = new List<Exam>();
                foreach (var item in eSRootsGroup)
                {
                    var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();
                    var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();

                    if (gradeid > 0 && courseid > 0)
                    {
                        Exam exam = new Exam()
                        {
                            AcademicYear = item.academicYear2,
                            ExamName = item.examName,
                            IsDeleted = false,
                            CreateTime = DateTime.Now,
                            SchoolTerm = item.schoolTerm,
                            gradeid = gradeid,
                            courseid = courseid,
                        };
                        exams.Add(exam);
                    }
                    else
                    {
                        throw new Exception("年级或科目不存在！！");
                    }
                }

                //var examSave = await _iExamRepository.Add(exams);


                BaseDao<Exam> examDao = new BaseDao<Exam>();
                try
                {

                    examDao.BeginTran();
                    var studentSave = examDao.Add(exams);
                    examDao.CommitTran();
                }
                catch (Exception)
                {
                    examDao.RollbackTran();
                    throw;
                }



                var examList = await _iExamRepository.Query(d => d.IsDeleted == false);

                List<ExScore> exScores = new List<ExScore>();
                foreach (var item in eSRoots)
                {
                    var studentModel = studentsList.Where(d => d.StudentNo == item.studentNo).FirstOrDefault();
                    if (studentModel != null)
                    {
                        var studentid = studentModel.Id;
                        var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();


                        var gradeid2 = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();
                        var clazzid2 = (clazzList.Where(d => d.GradeId == gradeid2 && d.ClassNo == item.clazz.PadLeft(2, '0')).FirstOrDefault()?.Id).ObjToInt();
                        var examid = (examList.Where(d => d.AcademicYear == item.academicYear2 && d.gradeid == gradeid2 && d.SchoolTerm == item.schoolTerm && d.ExamName == item.examName && d.courseid == courseid).FirstOrDefault()?.Id).ObjToInt();

                        if (courseid > 0 && clazzid2 > 0 && studentid > 0 && examid > 0)
                        {
                            ExScore exScore = new ExScore()
                            {
                                IsDeleted = false,
                                CreateTime = DateTime.Now,
                                clazzid = clazzid2,
                                courseid = courseid,
                                studentid = studentid,
                                examid = examid,
                                score = item.score.ObjToInt(),

                            };
                            exScores.Add(exScore);
                        }
                        else
                        {
                            throw new Exception("数据不完整！！");
                        }
                    }

                }

                var eachOperate = 10000;
                var operateCount = Math.Ceiling((double)(exScores.Count) / (double)(eachOperate));



                BaseDao<ExScore> exScoreDao = new BaseDao<ExScore>();
                try
                {
                    exScoreDao.BeginTran();

                    for (int i = 0; i < operateCount; i++)
                    {
                        var wanttosave = exScores.Skip(i * eachOperate).Take(eachOperate).ToList();
                        //var exScoreSave = await _iExScoreRepository.Add(wanttosave);

                        var studentSave = exScoreDao.Add(wanttosave);
                    }

                    exScoreDao.CommitTran();

                }
                catch (Exception)
                {
                    exScoreDao.RollbackTran();
                    throw;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region 将六次成绩资料Excel中的内容转换成DataSet
        /// <summary>
        /// 将六次成绩资料Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetExScoreDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic, ref List<ESRoot> eSRoots)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;

                        var sheetName = workbook.GetSheetName(a);

                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));


                            if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "学年") && (i == 2 && obj.ToString() == "年级") && (i == 3 && obj.ToString() == "学生") && (i == 4 && obj.ToString() == "班") && (i == 5 && obj.ToString() == "姓名"))
                            {
                                throw new Exception("提交资料错误");

                            }

                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));

                                if (j >= 6)
                                {
                                    var enrollmentYear = row.GetCell(0).ToString();
                                    var academicYear2 = row.GetCell(1).ToString();
                                    var grade = row.GetCell(2).ToString();
                                    var studentNo = row.GetCell(3).ToString();
                                    var clazz = row.GetCell(4).ToString();
                                    var course = obj.ToString();
                                    var score = row.GetCell(j).ToString();
                                    var schoolTerm = sheetName.Contains("上学期") ? "上学期" : "下学期";
                                    var examName = sheetName.Contains("上学期") ? sheetName.Replace("上学期", "") : sheetName.Replace("下学期", "");

                                    ESRoot eSRoot = new ESRoot()
                                    {
                                        enrollmentYear = enrollmentYear,
                                        academicYear2 = academicYear2,
                                        grade = grade,
                                        score = score,
                                        studentNo = studentNo,
                                        course = course,
                                        schoolTerm = schoolTerm,
                                        examName = examName,
                                        clazz = clazz,
                                    };

                                    eSRoots.Add(eSRoot);
                                }


                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion




        [HttpGet]
        public async Task<bool> ImportBaseSortData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("学年", "AcademicYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("学生", "StudentNo");
            sqlNameDics.Add("班", "Clazz");
            sqlNameDics.Add("姓名", "StudentName");
            sqlNameDics.Add("语文", "Chinese");
            sqlNameDics.Add("数学", "Meth");
            sqlNameDics.Add("英语", "English");
            sqlNameDics.Add("物理", "Physics");
            sqlNameDics.Add("化学", "Chemistry");
            sqlNameDics.Add("政治", "Politics");
            sqlNameDics.Add("历史", "History");
            sqlNameDics.Add("生物", "Biology");
            sqlNameDics.Add("地理", "Geography");

            List<ESRoot> eSRoots = new List<ESRoot>();

            try
            {
                var data = GetBaseSortDataSetFromExcel(filepath, sqlNameDics, ref eSRoots);

                //var jsons = ExcelHelper.DatasetToJson(data);

                var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
                var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);
                var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
                var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);

                var examList = await _iExamRepository.Query(d => d.IsDeleted == false);
                var exScoresList = await _iExScoreRepository.Query(d => d.IsDeleted == false);



                List<ExScore> exScores = new List<ExScore>();
                foreach (var item in eSRoots)
                {
                    var studentModel = studentsList.Where(d => d.StudentNo == item.studentNo).FirstOrDefault();
                    if (studentModel != null)
                    {
                        var studentid = studentModel.Id;
                        var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();


                        var gradeid2 = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();
                        var clazzid2 = (clazzList.Where(d => d.GradeId == gradeid2 && d.ClassNo == item.clazz.PadLeft(2, '0')).FirstOrDefault()?.Id).ObjToInt();
                        var examid = (examList.Where(d => d.AcademicYear == item.academicYear2 && d.gradeid == gradeid2 && d.SchoolTerm == item.schoolTerm && d.ExamName == item.examName && d.courseid == courseid).FirstOrDefault()?.Id).ObjToInt();

                        if (courseid > 0 && clazzid2 > 0 && studentid > 0 && examid > 0)
                        {
                            var model = exScoresList.Where(d => d.clazzid == clazzid2 && d.courseid == courseid && d.studentid == studentid && d.examid == examid).FirstOrDefault();

                            if (model != null)
                            {
                                model.BaseSort = item.score.ObjToInt();
                                exScores.Add(model);
                            }

                        }
                        else
                        {
                            //throw new Exception("数据不完整！！");
                        }
                    }

                }

                var eachOperate = 1000;
                var operateCount = Math.Ceiling((double)(exScores.Count) / (double)(eachOperate));

                BaseDao<ExScore> exScoreDao = new BaseDao<ExScore>();
                try
                {
                    exScoreDao.BeginTran();

                    for (int i = 0; i < operateCount; i++)
                    {
                        var wanttosave = exScores.Skip(i * eachOperate).Take(eachOperate).ToList();

                        var studentSave = exScoreDao.Update(wanttosave);
                    }

                    exScoreDao.CommitTran();

                }
                catch (Exception)
                {
                    exScoreDao.RollbackTran();
                    throw;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region 将考生基础名次资料Excel中的内容转换成DataSet
        /// <summary>
        /// 将考生基础名次资料Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetBaseSortDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic, ref List<ESRoot> eSRoots)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;

                        var sheetName = workbook.GetSheetName(a);

                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));


                            if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "学年") && (i == 2 && obj.ToString() == "年级") && (i == 3 && obj.ToString() == "学生") && (i == 4 && obj.ToString() == "班") && (i == 5 && obj.ToString() == "姓名"))
                            {
                                throw new Exception("提交资料错误");

                            }

                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));

                                if (j >= 6)
                                {
                                    var enrollmentYear = row.GetCell(0).ToString();
                                    var academicYear2 = row.GetCell(1).ToString();
                                    var grade = row.GetCell(2).ToString();
                                    var studentNo = row.GetCell(3).ToString();
                                    var clazz = row.GetCell(4).ToString();
                                    var course = obj.ToString();
                                    var score = row.GetCell(j).ToString();
                                    var schoolTerm = sheetName.Contains("上学期") ? "上学期" : "下学期";
                                    var examName = sheetName.Contains("上学期") ? sheetName.Replace("上学期", "") : sheetName.Replace("下学期", "");

                                    ESRoot eSRoot = new ESRoot()
                                    {
                                        enrollmentYear = enrollmentYear,
                                        academicYear2 = academicYear2,
                                        grade = grade,
                                        score = score,
                                        studentNo = studentNo,
                                        course = course,
                                        schoolTerm = schoolTerm,
                                        examName = examName,
                                        clazz = clazz,
                                    };

                                    eSRoots.Add(eSRoot);
                                }


                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion







        [HttpGet]
        public async Task<bool> ImportExamDetailData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("学年", "AcademicYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("学期", "SchoolTerm");
            sqlNameDics.Add("考试名称", "ExamName");
            sqlNameDics.Add("科目", "Course");
            sqlNameDics.Add("题型", "EDType");
            sqlNameDics.Add("学号", "StudentNo");
            sqlNameDics.Add("姓名", "StudentName");
            sqlNameDics.Add("班级", "Clazz");


            List<ExamDetailRoot> examDetailRoots = new List<ExamDetailRoot>();

            try
            {
                var data = GetExamDetailDataSetFromExcel(filepath, sqlNameDics, ref examDetailRoots);

                //var jsons = ExcelHelper.DatasetToJson(data);

                var examDetailRootsNames = examDetailRoots.Select(x => new { x.academicYear2, x.enrollmentYear, x.grade, x.schoolTerm, x.examName, x.course, x.edType, x.name }).ToList();

                var examDetailRootsNamesGroup = examDetailRootsNames.GroupBy(x => new { x.academicYear2, x.enrollmentYear, x.grade, x.schoolTerm, x.examName, x.course, x.edType, x.name }).Select(x => x.First()).ToList();

                var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
                var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);
                var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
                var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
                var examList = await _iExamRepository.Query(d => d.IsDeleted == false);

                List<ExamDetail> examDetails = new List<ExamDetail>();

                foreach (var item in examDetailRootsNamesGroup)
                {
                    var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();

                    var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();

                    var examid = (examList.Where(d => d.gradeid == gradeid && d.courseid == courseid && d.AcademicYear == item.academicYear2 && d.SchoolTerm == item.schoolTerm && d.ExamName == item.examName).FirstOrDefault()?.Id).ObjToInt();

                    if (gradeid > 0 && courseid > 0 && examid > 0)
                    {
                        ExamDetail examDetail = new ExamDetail()
                        {
                            EDType = item.edType,
                            IsDeleted = false,
                            Answer = "",
                            CreateTime = DateTime.Now,
                            Name = item.name,
                            Score = 0,
                            courseid = courseid,
                            gradeid = gradeid,
                            examid = examid

                        };
                        examDetails.Add(examDetail);
                    }
                    else
                    {
                        throw new Exception("考试信息不完整");
                    }

                }

                //var examDetailSave = await _iExamDetailRepository.Add(examDetails);
                BaseDao<ExamDetail> examDetailDao = new BaseDao<ExamDetail>();
                try
                {

                    examDetailDao.BeginTran();
                    var studentSave = examDetailDao.Add(examDetails);
                    examDetailDao.CommitTran();
                }
                catch (Exception)
                {
                    examDetailDao.RollbackTran();
                    throw;
                }



                var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false);

                List<ExamDetailScore> examDetailScores = new List<ExamDetailScore>();
                foreach (var item in examDetailRoots)
                {
                    var studentid = (studentsList.Where(d => d.StudentNo == item.studentNo).FirstOrDefault()?.Id).ObjToInt();

                    var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();

                    var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();
                    var examid = (examList.Where(d => d.gradeid == gradeid && d.courseid == courseid && d.AcademicYear == item.academicYear2 && d.SchoolTerm == item.schoolTerm && d.ExamName == item.examName).FirstOrDefault()?.Id).ObjToInt();

                    var examDetailid = (examDetailList.Where(d => d.examid == examid && d.EDType == item.edType && d.Name == item.name && d.courseid == courseid && d.gradeid == gradeid).FirstOrDefault()?.Id).ObjToInt();

                    if (examDetailid > 0)
                    {
                        ExamDetailScore examDetailScore = new ExamDetailScore()
                        {
                            studentid = studentid,
                            CreateTime = DateTime.Now,
                            ExamDetailId = examDetailid,
                            IsDeleted = false,
                            StudentAnswer = item.studentAnswer,
                            StudentScore = 0,
                        };
                        examDetailScores.Add(examDetailScore);
                    }
                    else
                    {
                        throw new Exception("考试题目信息出错！");
                    }
                }



                var eachOperate = 100;
                var operateCount = Math.Ceiling((double)(examDetailScores.Count) / (double)(eachOperate));

                for (int i = 0; i < operateCount; i++)
                {
                    var wanttosave = examDetailScores.Skip(i * eachOperate).Take(eachOperate).ToList();
                    //var exScoreSave = await _iExamDetailScoreRepository.Add(wanttosave);

                    BaseDao<ExamDetailScore> examDetailScoreDao = new BaseDao<ExamDetailScore>();
                    try
                    {

                        examDetailScoreDao.BeginTran();
                        var studentSave = examDetailScoreDao.Add(wanttosave);
                        examDetailScoreDao.CommitTran();
                    }
                    catch (Exception)
                    {
                        examDetailScoreDao.RollbackTran();
                        throw;
                    }
                }


            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region 将学生单科客观题选题情况Excel中的内容转换成DataSet
        /// <summary>
        /// 将学生单科客观题选题情况Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetExamDetailDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic, ref List<ExamDetailRoot> examDetailRoots)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;

                        var sheetName = workbook.GetSheetName(a);

                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));


                            if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "学年") && (i == 2 && obj.ToString() == "年级") && (i == 3 && obj.ToString() == "学期") && (i == 4 && obj.ToString() == "考试名称") && (i == 5 && obj.ToString() == "科目") && (i == 6 && obj.ToString() == "题型") && (i == 7 && obj.ToString() == "学号") && (i == 8 && obj.ToString() == "姓名") && (i == 9 && obj.ToString() == "班级"))
                            {
                                throw new Exception("提交资料错误");

                            }

                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    column = new DataColumn(obj.ToString());
                                    //throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));

                                if (j >= 10)
                                {
                                    var enrollmentYear = row.GetCell(0).ToString();
                                    var grade = row.GetCell(1).ToString();
                                    var academicYear2 = row.GetCell(2).ToString();
                                    var schoolTerm = row.GetCell(3).ToString();
                                    var examName = row.GetCell(4).ToString();
                                    var course = row.GetCell(5).ToString();
                                    var edtype = row.GetCell(6).ToString();
                                    var studentNo = row.GetCell(7).ToString();
                                    var studentName = row.GetCell(8).ToString();
                                    var clazz = row.GetCell(9).ToString();
                                    var studentAnswer = row.GetCell(j).ToString();
                                    var name = obj.ObjToString();


                                    ExamDetailRoot examDetailRoot = new ExamDetailRoot()
                                    {
                                        enrollmentYear = enrollmentYear,
                                        academicYear2 = academicYear2,
                                        grade = grade,
                                        schoolTerm = schoolTerm,
                                        clazz = clazz,
                                        course = course,
                                        studentAnswer = studentAnswer,
                                        studentName = studentName,
                                        studentNo = studentNo,
                                        edType = edtype,
                                        examName = examName,
                                        name = name,
                                    };

                                    examDetailRoots.Add(examDetailRoot);
                                }


                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion




        [HttpGet]
        public async Task<bool> ImportEDetailScoreData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("学年", "AcademicYear");
            sqlNameDics.Add("学期", "SchoolTerm");
            sqlNameDics.Add("考试名称", "ExamName");
            sqlNameDics.Add("科目", "Course");
            sqlNameDics.Add("学号", "StudentNo");
            sqlNameDics.Add("姓名", "StudentName");
            sqlNameDics.Add("班级", "Clazz");
            sqlNameDics.Add("科目总分", "coureTotalScore");
            sqlNameDics.Add("客观分", "objectiveScore");
            sqlNameDics.Add("主观分", "subjectiveScore");


            List<EDetailScoreRoot> eDetailScoreRoots = new List<EDetailScoreRoot>();

            var data = GetEDetailScoreDataSetFromExcel(filepath, sqlNameDics, ref eDetailScoreRoots);

            //var jsons = ExcelHelper.DatasetToJson(data);

            var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
            var studentsList = await _iStudentsRepository.Query(d => d.IsDeleted == false);
            var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
            var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);
            var examList = await _iExamRepository.Query(d => d.IsDeleted == false);
            var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false);
            var examDetailScoreList = await _iExamDetailScoreRepository.Query(d => d.IsDeleted == false);
            var exScoreList = await _iExScoreRepository.Query(d => d.IsDeleted == false);

            List<ExamDetailScore> examDetailScores = new List<ExamDetailScore>();
            List<ExamDetail> examDetailsNew = new List<ExamDetail>();
            List<ExamDetailScore> examDetailScoresSubject = new List<ExamDetailScore>();
            foreach (var item in eDetailScoreRoots)
            {
                var studentid = (studentsList.Where(d => d.StudentNo == item.studentNo).FirstOrDefault()?.Id).ObjToInt();

                var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.enrollmentYear && d.Name == item.grade).FirstOrDefault()?.Id).ObjToInt();

                var courseid = (courseList.Where(d => d.Name == item.course).FirstOrDefault()?.Id).ObjToInt();
                var examid = (examList.Where(d => d.gradeid == gradeid && d.courseid == courseid && d.AcademicYear == item.academicYear2 && d.SchoolTerm == item.schoolTerm && d.ExamName == item.examName).FirstOrDefault()?.Id).ObjToInt();

                var exScoreItem = exScoreList.Where(d => d.examid == examid && d.studentid == studentid).FirstOrDefault();
                exScoreItem.ObjectiveScore = item.objectiveScore;
                exScoreItem.SubjectiveScore = item.subjectiveScore;



                var examDetailid = (examDetailList.Where(d => d.examid == examid && d.Name == item.name && d.courseid == courseid && d.gradeid == gradeid).FirstOrDefault()?.Id).ObjToInt();

                if (examDetailid > 0)
                {

                    var examDetailScore = examDetailScoreList.Where(d => d.ExamDetailId == examDetailid && d.studentid == studentid).FirstOrDefault();
                    if (examDetailScore != null)
                    {
                        examDetailScore.StudentScore = item.studentScore.ObjToInt();
                        examDetailScores.Add(examDetailScore);
                    }
                    else
                    {
                        ExamDetailScore examDetailScoreSub = new ExamDetailScore()
                        {
                            CreateTime = DateTime.Now,
                            ExamDetailId = examDetailid,
                            IsDeleted = false,
                            StudentAnswer = "",
                            studentid = studentid,
                            StudentScore = item.studentScore.ObjToInt(),

                        };
                        examDetailScoresSubject.Add(examDetailScoreSub);
                    }
                }
                else
                {
                    //throw new Exception("考试题目信息出错！");
                    if (examid > 0)
                    {
                        ExamDetail examDetail = new ExamDetail()
                        {
                            EDType = "主观题",
                            IsDeleted = false,
                            Answer = "",
                            examid = examid,
                            courseid = courseid,
                            CreateTime = DateTime.Now,
                            gradeid = gradeid,
                            Name = item.name,
                            Score = 0,
                        };
                        //examDetailsNew.Add(examDetail);
                        var examdetailid = await _iExamDetailRepository.Add(examDetail);
                        if (examdetailid > 0)
                        {
                            examDetail.Id = examdetailid;
                            examDetailList.Add(examDetail);

                            ExamDetailScore examDetailScore = new ExamDetailScore()
                            {
                                CreateTime = DateTime.Now,
                                ExamDetailId = examdetailid,
                                IsDeleted = false,
                                StudentAnswer = "",
                                studentid = studentid,
                                StudentScore = item.studentScore.ObjToInt(),

                            };
                            examDetailScoresSubject.Add(examDetailScore);
                        }


                    }
                    else
                    {
                        throw new Exception("考试信息出错！");
                    }
                }
            }




            {


                BaseDao<ExamDetailScore> examDetailScoreDao = new BaseDao<ExamDetailScore>();
                var eachOperate = 1000;
                var operateCount = Math.Ceiling((double)(examDetailScoresSubject.Count) / (double)(eachOperate));
                try
                {
                    examDetailScoreDao.BeginTran();
                    for (int i = 0; i < operateCount; i++)
                    {
                        var wanttosave = examDetailScoresSubject.Skip(i * eachOperate).Take(eachOperate).ToList();


                        var studentSave = examDetailScoreDao.Add(wanttosave);

                    }
                    examDetailScoreDao.CommitTran();
                }
                catch (Exception)
                {
                    examDetailScoreDao.RollbackTran();
                    throw;
                }
            }


            {
                var eachOperate = 1000;
                var operateCount = Math.Ceiling((double)(examDetailScores.Count) / (double)(eachOperate));

                for (int i = 0; i < operateCount; i++)
                {
                    var wanttosave = examDetailScores.Skip(i * eachOperate).Take(eachOperate).ToList();
                    var exScoreSave = await _iExamDetailScoreRepository.Update(wanttosave);
                }
            }

            {
                var eachOperate = 1000;
                var operateCount = Math.Ceiling((double)(exScoreList.Count) / (double)(eachOperate));

                for (int i = 0; i < operateCount; i++)
                {
                    var wanttosave = exScoreList.Skip(i * eachOperate).Take(eachOperate).ToList();
                    var exScoreSave = await _iExScoreRepository.Update(wanttosave);
                }

            }

            return true;
        }

        #region 将学生单科成绩Excel中的内容转换成DataSet
        /// <summary>
        /// 将学生单科成绩Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetEDetailScoreDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic, ref List<EDetailScoreRoot> eDetailScoreRoots)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;

                        var sheetName = workbook.GetSheetName(a);

                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));



                            if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "学年") && (i == 2 && obj.ToString() == "年级") && (i == 3 && obj.ToString() == "学期") && (i == 4 && obj.ToString() == "考试名称") && (i == 5 && obj.ToString() == "科目") && (i == 6 && obj.ToString() == "学号") && (i == 7 && obj.ToString() == "姓名") && (i == 8 && obj.ToString() == "班级") && (i == 9 && obj.ToString() == "科目总分") && (i == 10 && obj.ToString() == "客观分"))
                            {
                                throw new Exception("提交资料错误");

                            }

                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    column = new DataColumn(obj.ToString());
                                    //throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));

                                if (j >= 12)
                                {
                                    var enrollmentYear = row.GetCell(0).ToString();
                                    var grade = row.GetCell(1).ToString();
                                    var academicYear2 = row.GetCell(2).ToString();
                                    var schoolTerm = row.GetCell(3).ToString();
                                    var examName = row.GetCell(4).ToString();
                                    var course = row.GetCell(5).ToString();
                                    var studentNo = row.GetCell(6).ToString();
                                    var studentName = row.GetCell(7).ToString();
                                    var clazz = row.GetCell(8).ToString();
                                    var courseTotalScore = row.GetCell(9).ToString();
                                    var objectiveScore = row.GetCell(10).ToString();
                                    var subjectiveScore = row.GetCell(11).ToString();

                                    var studentScore = row.GetCell(j).ToString();
                                    var name = obj.ObjToString();


                                    EDetailScoreRoot eDetailScoreRoot = new EDetailScoreRoot()
                                    {
                                        enrollmentYear = enrollmentYear,
                                        academicYear2 = academicYear2,
                                        grade = grade,
                                        schoolTerm = schoolTerm,
                                        clazz = clazz,
                                        course = course,
                                        studentName = studentName,
                                        studentNo = studentNo,
                                        examName = examName,
                                        name = name,
                                        coureTotalScore = courseTotalScore,
                                        studentScore = studentScore,
                                        subjectiveScore = subjectiveScore,
                                        objectiveScore = objectiveScore,
                                    };

                                    eDetailScoreRoots.Add(eDetailScoreRoot);
                                }


                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion




        [HttpGet]
        public async Task<bool> ImportEDetailScoreAnswerData(string filepath)
        {

            Dictionary<string, string> sqlNameDics = new Dictionary<string, string>();
            sqlNameDics.Add("入学年份", "EnrollmentYear");
            sqlNameDics.Add("年级", "Grade");
            sqlNameDics.Add("学年", "AcademicYear");
            sqlNameDics.Add("学期", "SchoolTerm");
            sqlNameDics.Add("考试名称", "ExamName");
            sqlNameDics.Add("科目", "Course");
            sqlNameDics.Add("题号", "Name");
            sqlNameDics.Add("题型", "EDType");
            sqlNameDics.Add("答案", "Answer");
            sqlNameDics.Add("得分", "Score");
            sqlNameDics.Add("备注", "Note");

            try
            {


                var data = GetEDetailScoreAnswerDataSetFromExcel(filepath, sqlNameDics);

                var jsons = ExcelHelper.DatasetToJson(data);

                List<EDetailScoreAnswerDataRoot> eDetailScoreAnswerDataRoots = new List<EDetailScoreAnswerDataRoot>();
                eDetailScoreAnswerDataRoots = JsonConvert.DeserializeObject<List<EDetailScoreAnswerDataRoot>>(jsons);

                var examDetailList = await _iExamDetailRepository.Query(d => d.IsDeleted == false);
                var examList = await _iExamRepository.Query(d => d.IsDeleted == false);
                var courseList = await _iCourseRepository.Query(d => d.IsDeleted == false);
                var gradeList = await _iGradeRepository.Query(d => d.IsDeleted == false);
                var clazzList = await _iClazzRepository.Query(d => d.IsDeleted == false);

                foreach (var item in examList)
                {
                    item.grade = gradeList.Where(d => d.Id == item.gradeid).FirstOrDefault();
                    item.course = courseList.Where(d => d.Id == item.courseid).FirstOrDefault();
                }

                foreach (var item in examDetailList)
                {
                    item.exam = examList.Where(d => d.Id == item.examid).FirstOrDefault();
                }

                foreach (var item in eDetailScoreAnswerDataRoots)
                {

                    var gradeid = (gradeList.Where(d => d.EnrollmentYear == item.EnrollmentYear && d.Name == item.Grade).FirstOrDefault()?.Id).ObjToInt();

                    var courseid = (courseList.Where(d => d.Name == item.Course).FirstOrDefault()?.Id).ObjToInt();
                    var examid = (examList.Where(d => d.gradeid == gradeid && d.courseid == courseid && d.AcademicYear == item.AcademicYear && d.SchoolTerm == item.SchoolTerm && d.ExamName == item.ExamName).FirstOrDefault()?.Id).ObjToInt();

                    var examDetailModel = examDetailList.Where(d => d.examid == examid && d.Name == item.Name && d.courseid == courseid && d.gradeid == gradeid).FirstOrDefault();
                    examDetailModel.Answer = item.Answer;
                    examDetailModel.Score = item.Score.ObjToInt();
                    var saveUpdate = await _iExamDetailRepository.Update(examDetailModel);

                }



            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region 将学生单科成绩Excel中的内容转换成DataSet
        /// <summary>
        /// 将学生单科成绩Excel中的内容转换成DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sqlnamesDic"></param>
        /// <param name="course"></param>
        /// <param name="tCCRoots"></param>
        /// <returns></returns>
        private static DataSet GetEDetailScoreAnswerDataSetFromExcel(string filePath, Dictionary<string, string> sqlnamesDic)
        {
            DataSet ds = new DataSet();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(filePath).ToLower();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fileExt == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(fs);//2007之后版本的excel
                    }
                    else
                    {
                        workbook = new HSSFWorkbook(fs);//2003版本的excel
                    }
                    for (int a = 0, b = workbook.NumberOfSheets; a < b; a++)
                    {
                        //获取读取的Sheet表的索引
                        ISheet sheet = workbook.GetSheetAt(a);
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        int cellCount = headerRow.LastCellNum;

                        var sheetName = workbook.GetSheetName(a);

                        //将第一行的文本作为列名
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                        {
                            DataColumn column = new DataColumn();
                            object obj = ExcelHelper.GetValueType(headerRow.GetCell(i));



                            if ((i == 0 && obj.ToString() == "入学年份") && (i == 1 && obj.ToString() == "年级") && (i == 2 && obj.ToString() == "学年") && (i == 3 && obj.ToString() == "学期") && (i == 4 && obj.ToString() == "考试名称") && (i == 5 && obj.ToString() == "科目") && (i == 6 && obj.ToString() == "题号") && (i == 7 && obj.ToString() == "题型") && (i == 8 && obj.ToString() == "答案") && (i == 9 && obj.ToString() == "得分"))
                            {
                                throw new Exception("提交资料错误");

                            }

                            if (obj == null || obj.ToString() == string.Empty)
                            {
                                column = new DataColumn("Columns" + i.ToString());
                            }
                            else
                            {

                                //column = new DataColumn(GetType(obj.ToString()));
                                string sqlname = "";
                                if (sqlnamesDic.TryGetValue(obj.ToString(), out sqlname))
                                {
                                    column = new DataColumn(sqlname);
                                }
                                else
                                {
                                    column = new DataColumn(obj.ToString());
                                    //throw new Exception("标题匹配失败");
                                }
                            }
                            table.Columns.Add(column);
                        }
                        //读取第一行下面的数据,将他们作为数据行存储
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null || row.GetCell(0) == null || row.GetCell(0).ToString().Trim() == "")
                            {
                                // 如果遇到第一个空行，跳出本次循环，继续向下读取                              
                                continue;
                            }
                            DataRow dataRow = table.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {

                                DataColumn column = new DataColumn();
                                object obj = ExcelHelper.GetValueType(headerRow.GetCell(j));

                                if (row.GetCell(j) != null)
                                {
                                    dataRow[j] = row.GetCell(j).ToString();
                                }
                            }
                            table.Rows.Add(dataRow);
                        }

                        ds.Tables.Add(table);

                    }
                    workbook = null;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion





    }

    public class EDetailScoreAnswerDataRoot
    {
        public string EnrollmentYear { get; set; }
        public string Grade { get; set; }
        public string AcademicYear { get; set; }
        public string SchoolTerm { get; set; }
        public string ExamName { get; set; }
        public string Course { get; set; }
        public string Name { get; set; }
        public string EDType { get; set; }
        public string Answer { get; set; }
        public string Score { get; set; }
        public string Note { get; set; }
    }

    public class TeacherDataRoot
    {
        /// <summary>
        /// 2018级
        /// </summary>
        public string EnrollmentYear { get; set; }
        /// <summary>
        /// 高一
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// A组
        /// </summary>
        public string ClazzLevel { get; set; }
        /// <summary>
        /// 盛
        /// </summary>
        public string Manager { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Clazz { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClazzType { get; set; }
        /// <summary>
        /// 物理
        /// </summary>
        public string ChooseSub { get; set; }
        /// <summary>
        /// 李
        /// </summary>
        public string TeacherCharge { get; set; }
        /// <summary>
        /// 彭
        /// </summary>
        public string Chinese { get; set; }
        /// <summary>
        /// 范
        /// </summary>
        public string Meth { get; set; }
        /// <summary>
        /// 罗
        /// </summary>
        public string English { get; set; }
        /// <summary>
        /// 王
        /// </summary>
        public string Physics { get; set; }
        /// <summary>
        /// 孙
        /// </summary>
        public string Chemistry { get; set; }
        /// <summary>
        /// 肖
        /// </summary>
        public string Politics { get; set; }
        /// <summary>
        /// 曾
        /// </summary>
        public string History { get; set; }
        /// <summary>
        /// 李
        /// </summary>
        public string Biology { get; set; }
        /// <summary>
        /// 齐
        /// </summary>
        public string Geography { get; set; }
    }

    public class TCCRoot
    {
        /// <summary>
        /// 班级
        /// </summary>
        public string className { get; set; }
        /// <summary>
        /// 教师
        /// </summary>
        public string teacherName { get; set; }
        public string teacherAccount { get; set; }
        public int teacherId { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        public string courseName { get; set; }

    }


    public class StudentDataRoot
    {
        /// <summary>
        /// 
        /// </summary>
        public string StudentNo { get; set; }
        /// <summary>
        /// 胡高
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Clazz { get; set; }
        /// <summary>
        /// 物理
        /// </summary>
        public string SubjectA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SubjectB { get; set; }
        /// <summary>
        /// 高一
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// 在校
        /// </summary>
        public string InSchoolSituation { get; set; }
        /// <summary>
        /// 入学年份
        /// </summary>
        public string EnrollmentYear { get; set; }
        /// <summary>
        /// 男
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 是
        /// </summary>
        public string IsIndicators { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UniversityEntranceNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Note1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Note2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Note3 { get; set; }
    }



    public class ESRoot
    {


        /// <summary>
        /// 入学年份
        /// </summary>
        public string enrollmentYear { get; set; }

        /// <summary>
        /// 学年
        /// </summary>
        public string academicYear2 { get; set; }
        /// <summary>
        /// 年级
        /// </summary>
        public string grade { get; set; }
        /// <summary>
        /// 学生学号
        /// </summary>
        public string studentNo { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        public string course { get; set; }
        /// <summary>
        /// 成绩
        /// </summary>
        public string score { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        public string schoolTerm { get; set; }
        /// <summary>
        /// 考试
        /// </summary>
        public string examName { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public string clazz { get; set; }

    }
    public class ExamDetailRoot
    {


        /// <summary>
        /// 入学年份
        /// </summary>
        public string enrollmentYear { get; set; }
        /// <summary>
        /// 学年
        /// </summary>
        public string academicYear2 { get; set; }
        /// <summary>
        /// 年级
        /// </summary>
        public string grade { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        public string schoolTerm { get; set; }
        /// <summary>
        /// 考试名称
        /// </summary>
        public string examName { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        public string course { get; set; }
        /// <summary>
        /// 题型
        /// </summary>
        public string edType { get; set; }
        /// <summary>
        /// 学生学号
        /// </summary>
        public string studentNo { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string studentName { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public string clazz { get; set; }
        /// <summary>
        /// 题目名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 学生答案
        /// </summary>
        public string studentAnswer { get; set; }

    }



    public class EDetailScoreRoot
    {

        /// <summary>
        /// 入学年份
        /// </summary>
        public string enrollmentYear { get; set; }
        /// <summary>
        /// 学年
        /// </summary>
        public string academicYear2 { get; set; }
        /// <summary>
        /// 年级
        /// </summary>
        public string grade { get; set; }
        /// <summary>
        /// 学期
        /// </summary>
        public string schoolTerm { get; set; }
        /// <summary>
        /// 考试名称
        /// </summary>
        public string examName { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        public string course { get; set; }
        /// <summary>
        /// 学生学号
        /// </summary>
        public string studentNo { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string studentName { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public string clazz { get; set; }
        /// <summary>
        /// 科目总分
        /// </summary>
        public string coureTotalScore { get; set; }
        /// <summary>
        /// 客观分
        /// </summary>
        public string objectiveScore { get; set; }
        /// <summary>
        /// 主观分
        /// </summary>
        public string subjectiveScore { get; set; }
        /// <summary>
        /// 题目名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 学生得分
        /// </summary>
        public string studentScore { get; set; }

    }

}
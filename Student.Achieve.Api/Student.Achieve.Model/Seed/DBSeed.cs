using Student.Achieve.Common.Helper;
using Student.Achieve.Model.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Student.Achieve.Model.Models
{
    public class DBSeed
    {
        private static string GitJsonFileFormat = "https://gitee.com/laozhangIsPhi/Blog.Data.Share/raw/master/Student.Achieve.json/{0}.tsv";
        /// <summary>
        /// 异步添加种子数据
        /// </summary>
        /// <param name="myContext"></param>
        /// <returns></returns>
        public static async Task SeedAsync(MyContext myContext)
        {
            try
            {
                // 创建数据库
                myContext.Db.DbMaintenance.CreateDatabase();

                // 创建表
                myContext.CreateTableByEntity(false,
                    typeof(Module),
                    typeof(Permission),
                    typeof(Role),
                    typeof(RoleModulePermission),
                    typeof(SysAdmin),
                    typeof(CCT),
                    typeof(Clazz),
                    typeof(Course),
                    typeof(Exam),
                    typeof(ExamDetail),
                    typeof(ExamDetailScore),
                    typeof(ExScore),
                    typeof(Grade),
                    typeof(Students),
                    typeof(Teacher),
                    typeof(UserRole));


                Console.WriteLine("Database:WMBlog created success!");
                Console.WriteLine();

                Console.WriteLine("Seeding database...");




                #region Module
                if (!await myContext.Db.Queryable<Module>().AnyAsync())
                {
                    myContext.GetEntityDB<Module>().InsertRange(JsonHelper.ParseFormByJson<List<Module>>(GetNetData.Get(string.Format(GitJsonFileFormat, "Module"))));
                    Console.WriteLine("Table:Module created success!");
                }
                else
                {
                    Console.WriteLine("Table:Module already exists...");
                }
                #endregion


                #region Permission
                if (!await myContext.Db.Queryable<Permission>().AnyAsync())
                {
                    myContext.GetEntityDB<Permission>().InsertRange(JsonHelper.ParseFormByJson<List<Permission>>(GetNetData.Get(string.Format(GitJsonFileFormat, "Permission"))));
                    Console.WriteLine("Table:Permission created success!");
                }
                else
                {
                    Console.WriteLine("Table:Permission already exists...");
                }
                #endregion


                #region Role
                if (!await myContext.Db.Queryable<Role>().AnyAsync())
                {
                    myContext.GetEntityDB<Role>().InsertRange(JsonHelper.ParseFormByJson<List<Role>>(GetNetData.Get(string.Format(GitJsonFileFormat, "Role"))));
                    Console.WriteLine("Table:Role created success!");
                }
                else
                {
                    Console.WriteLine("Table:Role already exists...");
                }
                #endregion


                #region RoleModulePermission
                if (!await myContext.Db.Queryable<RoleModulePermission>().AnyAsync())
                {
                    myContext.GetEntityDB<RoleModulePermission>().InsertRange(JsonHelper.ParseFormByJson<List<RoleModulePermission>>(GetNetData.Get(string.Format(GitJsonFileFormat, "RoleModulePermission"))));
                    Console.WriteLine("Table:RoleModulePermission created success!");
                }
                else
                {
                    Console.WriteLine("Table:RoleModulePermission already exists...");
                }
                #endregion


                #region UserRole
                if (!await myContext.Db.Queryable<UserRole>().AnyAsync())
                {
                    myContext.GetEntityDB<UserRole>().InsertRange(JsonHelper.ParseFormByJson<List<UserRole>>(GetNetData.Get(string.Format(GitJsonFileFormat, "UserRole"))));
                    Console.WriteLine("Table:UserRole created success!");
                }
                else
                {
                    Console.WriteLine("Table:UserRole already exists...");
                }
                #endregion

                #region SysAdmin
                if (!await myContext.Db.Queryable<SysAdmin>().AnyAsync())
                {
                    myContext.GetEntityDB<SysAdmin>().InsertRange(JsonHelper.ParseFormByJson<List<SysAdmin>>(GetNetData.Get(string.Format(GitJsonFileFormat, "SysAdmin"))));
                    Console.WriteLine("Table:SysAdmin created success!");
                }
                else
                {
                    Console.WriteLine("Table:SysAdmin already exists...");
                }
                #endregion

                #region Teacher
                if (!await myContext.Db.Queryable<Teacher>().AnyAsync())
                {
                    myContext.GetEntityDB<Teacher>().Insert(new Teacher()
                    {
                        TeacherNo = "G2999",
                        Name = "G2999",
                        Account = "G2999",
                        Password = "D11731129AED59E997323E35FB63730",
                        gradeId = 1,
                        CreateTime = DateTime.Now,
                        IsDeleted = false,
                    });
                    Console.WriteLine("Table:Teacher created success!");
                }
                else
                {
                    Console.WriteLine("Table:SysAdmin already exists...");
                }
                #endregion
                Console.WriteLine("Done seeding database.");
                Console.WriteLine();

            }
            catch (Exception ex)
            {
                throw new Exception("1、注意要先创建空的数据库\n2、" + ex.Message);
            }
        }
    }
}

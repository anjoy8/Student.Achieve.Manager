using Autofac;
using Autofac.Extras.DynamicProxy;
using log4net;
using Student.Achieve.AOP;
using Student.Achieve.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Student.Achieve.Extensions
{
    public class AutofacModuleRegister : Autofac.Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AutofacModuleRegister));
        protected override void Load(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;


            builder.RegisterType<BlogLogAOP>();
            builder.RegisterType<BlogCacheAOP>();//可以直接替换其他拦截器

            #region Service.dll 注入，有对应接口
            try
            {
                var servicesDllFile = Path.Combine(basePath, "Student.Achieve.Repository.dll");
                var assemblysServices = Assembly.LoadFrom(servicesDllFile);

                var cacheType = new List<Type>();
                if (Appsettings.app(new string[] { "AppSettings", "LogAOP", "Enabled" }).ObjToBool())
                {
                    cacheType.Add(typeof(BlogLogAOP));
                }
                if (Appsettings.app(new string[] { "AppSettings", "MemoryCachingAOP", "Enabled" }).ObjToBool())
                {
                    cacheType.Add(typeof(BlogCacheAOP));
                }

                builder.RegisterAssemblyTypes(assemblysServices)
                          .AsImplementedInterfaces()
                          .InstancePerLifetimeScope()
                          .EnableInterfaceInterceptors()
                          .InterceptedBy(cacheType.ToArray());
                #endregion


            }
            catch (Exception ex)
            {
                throw new Exception("※※★※※ 如果你是第一次下载项目，请先对整个解决方案dotnet build（F6编译），然后再对api层 dotnet run（F5执行），\n因为解耦了，如果你是发布的模式，请检查bin文件夹是否存在Repository.dll和service.dll ※※★※※" + ex.Message + "\n" + ex.InnerException);
            }


        }
    }

}

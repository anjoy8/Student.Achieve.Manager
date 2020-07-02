using Autofac;
using AutoMapper;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Student.Achieve.AuthHelper;
using Student.Achieve.Common;
using Student.Achieve.Common.HttpContextUser;
using Student.Achieve.Common.LogHelper;
using Student.Achieve.Common.MemoryCache;
using Student.Achieve.Extensions;
using Student.Achieve.Filter;
using Student.Achieve.Log;
using Student.Achieve.Middlewares;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Student.Achieve
{
    public class Startup
    {

        /// <summary>
        /// log4net 仓储库
        /// </summary>
        public static ILoggerRepository Repository { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            //log4net
            Repository = LogManager.CreateRepository(Configuration["Logging:Log4Net:Name"]);
            //指定配置文件，如果这里你遇到问题，应该是使用了InProcess模式，请查看Student.Achieve.csproj,并删之
            var contentPath = env.ContentRootPath;
            var log4Config = Path.Combine(contentPath, "log4net.config");
            XmlConfigurator.Configure(Repository, new FileInfo(log4Config));

        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }
        private const string ApiName = "Student.Achieve";
        private readonly string version = "V1";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 部分服务注入-netcore自带方法
            // 缓存注入
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });
            // log日志注入
            services.AddSingleton<ILoggerHelper, LogHelper>();
            #endregion

            #region 初始化DB
            services.AddScoped<Student.Achieve.Model.Models.DBSeed>();
            services.AddScoped<Student.Achieve.Model.Models.MyContext>();
            #endregion

            #region Automapper
            services.AddAutoMapper(typeof(Startup));
            #endregion

            #region CORS
            services.AddCors(c =>
            {
                //一般采用这种方法
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    .WithOrigins("http://127.0.0.1:6918", "http://localhost:6918")
                    .AllowAnyHeader()//Ensures that the policy allows any header.
                    .AllowAnyMethod();
                });
            });

            #endregion

            #region Swagger UI Service

            var basePath = AppContext.BaseDirectory;
            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc(version, new OpenApiInfo
                {
                    Version = version,
                    Title = $"{ApiName} 接口文档——{RuntimeInformation.FrameworkDescription}",
                    Description = $"{ApiName} HTTP API " + version,
                    Contact = new OpenApiContact { Name = ApiName, Email = "Blog.Core@xxx.com", Url = new Uri("https://www.jianshu.com/u/94102b59cc2a") },
                    License = new OpenApiLicense { Name = ApiName + " 官方文档", Url = new Uri("http://apk.neters.club/.doc/") }
                });
                c.OrderActionsBy(o => o.RelativePath);


                //就是这里
                var xmlPath = Path.Combine(basePath, "Student.Achieve.xml");
                c.IncludeXmlComments(xmlPath, true);

                var xmlModelPath = Path.Combine(basePath, "Student.Achieve.Model.xml");
                c.IncludeXmlComments(xmlModelPath);

                #region Token绑定到ConfigureServices


                // 开启加权小锁
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // 在header中添加token，传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // Jwt Bearer 认证，必须是 oauth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
                #endregion
            });

            #endregion

            #region MVC + GlobalExceptions

            //注入全局异常捕获
            services.AddControllers(o =>
            {
                // 全局异常过滤
                o.Filters.Add(typeof(GlobalExceptionsFilter));
                // 全局路由权限公约
                //o.Conventions.Insert(0, new GlobalRouteAuthorizeConvention());
                // 全局路由前缀，统一修改路由
                o.Conventions.Insert(0, new GlobalRoutePrefixFilter(new RouteAttribute(RoutePrefix.Name)));
            })
            //全局配置Json序列化处理
            .AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //设置时间格式
                //options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
            });

            #endregion

            #region Httpcontext

            // Httpcontext 注入
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();

            #endregion

            #region Authorize 权限认证三步走


            #region 参数
            //读取配置文件
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);


            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // 如果要数据库动态绑定，这里先留个空，后边处理器里动态赋值
            var permission = new List<PermissionItem>();

            // 角色与接口的权限要求参数
            var permissionRequirement = new PermissionRequirement(
                "/api/denied",// 拒绝授权的跳转地址（目前无用）
                permission,
                ClaimTypes.Role,//基于角色的授权
                audienceConfig["Issuer"],//发行人
                audienceConfig["Audience"],//听众
                signingCredentials,//签名凭据
                expiration: TimeSpan.FromSeconds(60 * 60)//接口的过期时间
                );
            #endregion

            //【授权】
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.Name,
                         policy => policy.Requirements.Add(permissionRequirement));
            });


            // 令牌验证参数
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Issuer"],//发行人
                ValidateAudience = true,
                ValidAudience = audienceConfig["Audience"],//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RequireExpirationTime = true,
            };

            services.AddAuthentication("Bearer")
             .AddJwtBearer(o =>
             {
                 o.TokenValidationParameters = tokenValidationParameters;
                 o.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         // 如果过期，则把<是否过期>添加到，返回头信息中
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             context.Response.Headers.Add("Token-Expired", "true");
                         }
                         return Task.CompletedTask;
                     }
                 };
             })
             .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });


            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton(permissionRequirement);

            #endregion

            services.AddSingleton(new Appsettings(Env.ContentRootPath));
            services.AddSingleton(new LogLock(Env.ContentRootPath));



        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            #region ReuestResponseLog

            if (Appsettings.app("AppSettings", "Middleware_RequestResponse", "Enabled").ObjToBool())
            {
                app.UseReuestResponseLog();//记录请求与返回数据 
            }

            #endregion

            #region Environment
            if (env.IsDevelopment())
            {
                // 在开发环境中，使用异常页面，这样可以暴露错误堆栈信息，所以不要放在生产环境。
                app.UseDeveloperExceptionPage();

            }
            else
            {
                app.UseExceptionHandler("/Error");

            }
            #endregion

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{ApiName} {version}");
                //c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Student.Achieve.index.html");
                c.RoutePrefix = "";
            });


            #endregion

            // CORS跨域
            app.UseCors("LimitRequests");
            // 跳转https
            //app.UseHttpsRedirection();
            // 使用静态文件
            app.UseStaticFiles();
            // 使用cookie
            app.UseCookiePolicy();
            // 返回错误码
            app.UseStatusCodePages();
            // Routing
            app.UseRouting();
            // 这种自定义授权中间件，可以尝试，但不推荐
            // app.UseJwtTokenAuth();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();
            // 开启异常中间件，要放到最后
            //app.UseExceptionHandlerMidd();
            // 性能分析
            app.UseMiniProfiler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }

    }
}

using Student.Achieve.AuthHelper;
using Microsoft.AspNetCore.Builder;

namespace Student.Achieve.Middlewares
{
    public static class MiddlewareHelpers
    {
        public static IApplicationBuilder UseReuestResponseLog(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequRespLogMildd>();
        }
    }
}

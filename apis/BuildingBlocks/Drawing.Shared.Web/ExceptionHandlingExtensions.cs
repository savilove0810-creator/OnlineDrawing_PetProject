using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Drawing.Shared.Web
{
    public static class ExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(ex.Message);
                }
            });
        }
    }
}

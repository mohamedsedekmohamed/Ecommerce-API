using EcommerceAPI.Errors;
using System.Net;
using System.Text.Json;

namespace EcommerceAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType = "application/json";

                // تحديد نوع الخطأ (اختياري)
                var statusCode = ex switch
                {
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = statusCode;

                // تحديد اللغة من الهيدر
                var lang = context.Request.Headers["Accept-Language"].ToString();

                // إنشاء الرسالة
                var response = new ApiException(
                    statusCode,
                    lang,
                    _env.IsDevelopment() ? ex.StackTrace?.ToString() : null
                );

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
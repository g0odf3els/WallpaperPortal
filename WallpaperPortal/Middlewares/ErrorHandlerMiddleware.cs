using System.Net;
using System.Text.Json;

namespace WallpaperPortal.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

                using (var writer = new StreamWriter("error.log", true))
                {
                    await writer.WriteLineAsync($"Time: {DateTime.Now}");
                    await writer.WriteLineAsync($"Error: {ex.Message}");
                    await writer.WriteLineAsync($"Stack: {ex.StackTrace}");
                    await writer.WriteLineAsync();
                }
            }
        }
    }
}

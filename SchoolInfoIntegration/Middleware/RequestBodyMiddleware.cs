using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace ECC.Institute.CRM.IntegrationAPI.Middleware
{
	public class RequestBodyMiddleware
	{
        private readonly RequestDelegate _next;

        public RequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Enable request body rewinding
            context.Request.EnableBuffering();

            // Read the request body
            var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var print = bodyAsText == "" ? "None" : bodyAsText;

            // Print the raw request body
            Console.WriteLine($"Raw Http Request:[{context.Request.Method}] {context.Request.Path} | body:{print}");

            // Important: reset the request body stream position to the beginning
            context.Request.Body.Position = 0;

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline
    public static class RawRequestBodyMiddlewareExtensions
    {
        public static IApplicationBuilder UseRawRequestBodyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestBodyMiddleware>();
        }
    }
}


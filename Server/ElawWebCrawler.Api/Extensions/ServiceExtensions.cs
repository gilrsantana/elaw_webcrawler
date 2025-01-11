using System.Text.Json.Serialization;
using ElawWebCrawler.Api.Notifications;
using ElawWebCrawler.Common;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ElawWebCrawler.Api.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureApiApp(this IServiceCollection service)
    {
        service.AddOpenApi();
        service.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));;
        service.AddEndpointsApiExplorer();
        service.AddCors();
        service.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Any())
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage).ToArray();

                    var messages = new List<MessageModel>();
                    foreach (var error in errors)
                    {
                        messages.Add(new MessageModel(error, MessageType.ERROR));
                    }
                    var toReturn = new ResultViewModelApi<string>(messages);

                    return new BadRequestObjectResult(toReturn);
                };
            });
    }
    
    public static void ConfigureSerilog(this IServiceCollection service, WebApplicationBuilder buider)
    {
        Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
        buider.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
    }

    public static void LoadConfiguration(this IServiceCollection service, WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseCors(options =>
        {
            options.AllowAnyOrigin();
            options.AllowAnyMethod();
            options.AllowAnyHeader();
        });
        app.MapControllers();
        app.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Resource not found.");
        });
    }
}
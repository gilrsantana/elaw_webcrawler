using System.Text.Json.Serialization;
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
        app.Run();
    }
}
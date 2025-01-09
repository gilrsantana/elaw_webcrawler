using Microsoft.Extensions.DependencyInjection;

namespace ElawWebCrawler.Application;

public static class ServiceExtensions
{
    public static void ConfigureApiApplication(this IServiceCollection services)
    {
        services.AddScoped<IApplicationService, ApplicationService>();
    }
}
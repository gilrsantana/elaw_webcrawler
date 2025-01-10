using ElawWebCrawler.Application.Interfaces;
using ElawWebCrawler.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ElawWebCrawler.Application;

public static class ServiceExtensions
{
    public static void ConfigureApiApplication(this IServiceCollection services)
    {
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IPuppeteerService, PuppeteerService>();
    }
}
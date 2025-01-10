using Microsoft.Extensions.DependencyInjection;

namespace ElawWebCrawler.Provider.Azure;

public static class ServiceExtensions
{
    public static void ConfigureAzureProvider(this IServiceCollection service)
    {
        service.AddScoped<IAzureFileHandler, AzureFileHandler>();
    }
}
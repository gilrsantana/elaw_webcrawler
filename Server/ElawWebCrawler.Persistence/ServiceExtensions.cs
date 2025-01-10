using ElawWebCrawler.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ElawWebCrawler.Persistence;

public static class Extension
{
    public static void ConfigureApiPersistence(this IServiceCollection service)
    {
        service.AddScoped<IGetDataEventPersist, GetDataEventPersist>();
        service.AddScoped<IHtmlFilePersist, HtmlFilePersist>();
    }
}
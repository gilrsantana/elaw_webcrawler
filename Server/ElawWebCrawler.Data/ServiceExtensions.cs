using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElawWebCrawler.Data;

public static class ServiceExtensions
{
    public static void ConfigureApiData(this IServiceCollection service, IConfiguration configuration)
    {
        service.ConfigureOptions<DatabaseOptionsSetup>();
        service.AddDbContext<WebCrawlerContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>()!.Value;

            options.UseSqlServer(databaseOptions.ConnectionString, sqlServerAction =>
            {
                sqlServerAction.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                sqlServerAction.CommandTimeout(databaseOptions.CommandTimeout);
            });
            options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
            options.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
        });
    }
}
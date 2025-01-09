using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ElawWebCrawler.Data;

internal class DatabaseOptionsSetup(IConfiguration configuration) 
    : IConfigureOptions<DatabaseOptions>
{
    private const string ConfigurationSectionName = "DatabaseOptions";
    public void Configure(DatabaseOptions options)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? throw new ApplicationException("Not found connection string");
        options.ConnectionString = connectionString;
        var section = configuration.GetSection(ConfigurationSectionName);
        options.MaxRetryCount = int.Parse(section["MaxRetryCount"] ?? "");
        options.EnableSensitiveDataLogging = bool.Parse(section["EnableSensitiveDataLogging"] ?? "");
        options.CommandTimeout = int.Parse(section["CommandTimeout"] ?? "");
        options.EnableDetailedErrors = bool.Parse(section["EnableDetailedErrors"] ?? "");
    }
}
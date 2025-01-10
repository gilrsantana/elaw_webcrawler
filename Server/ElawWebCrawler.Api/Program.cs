using ElawWebCrawler.Api.Extensions;
using ElawWebCrawler.Application;
using ElawWebCrawler.Data;
using ElawWebCrawler.Persistence;
using ElawWebCrawler.Provider.Azure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureApiApp();
builder.Services.ConfigureApiApplication();
builder.Services.ConfigureApiData(builder.Configuration);
builder.Services.ConfigureApiPersistence();
builder.Services.ConfigureAzureProvider();
builder.Services.ConfigureSerilog(builder);
builder.Services.LoadConfiguration(builder.Build());

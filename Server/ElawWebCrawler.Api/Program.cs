using ElawWebCrawler.Api.Extensions;
using ElawWebCrawler.Application;
using ElawWebCrawler.Data;
using ElawWebCrawler.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureApiApp();
builder.Services.ConfigureApiApplication();
builder.Services.ConfigureApiData(builder.Configuration);
builder.Services.ConfigureApiPersistence();
builder.Services.LoadConfiguration(builder.Build());

using PuppeteerSharp;

namespace ElawWebCrawler.Application.Interfaces;

public interface IPuppeteerService
{
    Task<string?> FetchRenderedHtmlAsync(string url);
    Task<IBrowser> LaunchBrowserAsync();
}
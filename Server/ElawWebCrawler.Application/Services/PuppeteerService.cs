using ElawWebCrawler.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace ElawWebCrawler.Application.Services;

public class PuppeteerService(ILogger<PuppeteerService> logger) : IPuppeteerService
{
    public async Task<string?> FetchRenderedHtmlAsync(string url)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        logger.LogInformation("Browser downloaded");
        await using var browser = await LaunchBrowserAsync();
        logger.LogInformation("Browser launched");
        await using var page = await browser.NewPageAsync();
        logger.LogInformation("New page created");
        await page.GoToAsync(url);
        logger.LogInformation("Navigated to url");
        try
        {
            var tableElement = await page.QuerySelectorAsync("table");
            logger.LogInformation("Searched table element found");
            if (tableElement == null)
            {
                logger.LogWarning("Table element not found");
                return string.Empty;
            }
        }
        catch (WaitTaskTimeoutException)
        {
            logger.LogWarning("Timeout waiting for table element");
            return string.Empty;
        }

        await page.WaitForSelectorAsync("table");
        logger.LogInformation("Table element found");
        var content = await page.GetContentAsync();
        logger.LogInformation("Content fetched");
        browser.Disconnect();
        logger.LogInformation("Browser disconnected");
        await page.CloseAsync();
        logger.LogInformation("Page closed");
        logger.LogInformation("Returning content");
        return content;
    }

    public async Task<IBrowser> LaunchBrowserAsync()
    {
        return await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });
    }

}
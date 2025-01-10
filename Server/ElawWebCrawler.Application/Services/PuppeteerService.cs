using ElawWebCrawler.Application.Interfaces;
using PuppeteerSharp;

namespace ElawWebCrawler.Application.Services;

public class PuppeteerService : IPuppeteerService
{
    public async Task<string?> FetchRenderedHtmlAsync(string url)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);

        try
        {
            var tableElement = await page.QuerySelectorAsync("table");
            if (tableElement == null)
            {
                return string.Empty;
            }
        }
        catch (WaitTaskTimeoutException)
        {
            return string.Empty;
        }

        await page.WaitForSelectorAsync("table");
        var content = await page.GetContentAsync();
        browser.Disconnect();
        await page.CloseAsync();
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
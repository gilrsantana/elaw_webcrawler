using ElawWebCrawler.Common;

namespace ElawWebCrawler.Application;

public interface IApplicationService
{
    Task<DataContainer<GetDataEventNotification>> ScrapDataAsync(string url);
}
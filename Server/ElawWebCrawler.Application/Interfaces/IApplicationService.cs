using ElawWebCrawler.Application.Notifications;
using ElawWebCrawler.Common;

namespace ElawWebCrawler.Application.Interfaces;

public interface IApplicationService
{
    Task<DataContainer<GetDataEventNotification>> ScrapDataAsync(string url);
}
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using ElawWebCrawler.Application.Interfaces;
using ElawWebCrawler.Application.Notifications;
using ElawWebCrawler.Common;
using ElawWebCrawler.Domain.Entities;
using ElawWebCrawler.Domain.Interfaces;
using ElawWebCrawler.Provider.Azure;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("ElawWebCrawler.Test")]
namespace ElawWebCrawler.Application.Services;

public class ApplicationService(IGetDataEventPersist eventPersist,
    IHtmlFilePersist htmlFilePersist,
    IAzureFileHandler azureFileHandler,
    IConfiguration configuration,
    IPuppeteerService puppeteerService)
    : BaseService, IApplicationService
{
    private static readonly ConcurrentBag<ProxyData> ProxyList = new ConcurrentBag<ProxyData>();
    private SemaphoreSlim? semaphore;
    private int _maxThreads = 3;
    private readonly string _ipKey = "IP Address";
    private readonly string _portKey = "Port";
    private readonly string _countryKey = "Country";
    private readonly string _protocolKey = "Protocol";
    private readonly string _htmlFileFolder = "html-files";
    private readonly string _jsonFileFolder = "json-files";
    public async Task<DataContainer<GetDataEventNotification>> ScrapDataAsync(string url)
    {
        var maxThreads = configuration.GetSection("MaxThreads").Value ?? "";
        if (int.TryParse(maxThreads, out var number))
        {
            _maxThreads = number > 0 ? number : _maxThreads;
        }
        semaphore = new SemaphoreSlim(_maxThreads);

        if (!IsValidUrl(url))
        {
            return HandleResult<GetDataEventNotification>(null, ["URL inválida."]);
        }
        var startTime = DateTime.Now;
        var pagesCount = 0;
        var rowsCount = 0;

        var tasks = new List<Task>();
        var cancellationTokenSource = new CancellationTokenSource();

        var pageNumber = 1;
        var morePages = true;
        var requestKey = Guid.NewGuid().ToString();
        var errorOccurred = false; 

        while (morePages)
        {
            var urlForm = $"{url}/page/{pageNumber}";
            await semaphore.WaitAsync(cancellationTokenSource.Token); // Aguarda uma vaga para executar a tarefa


            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var proxies = await ExtractProxyDataAsync(urlForm);
                    Console.WriteLine($"Processado: {urlForm}");
                    if (proxies.Count == 0)
                    {
                        morePages = false;
                        return;
                    }

                    foreach (var proxy in proxies)
                    {
                        ProxyList.Add(proxy);
                    }

                    pagesCount++;
                    rowsCount += proxies.Count;

                    await SaveHtmlToFileAsync(urlForm, await FetchRenderedHtmlAsync(urlForm), requestKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar {urlForm}: {ex.Message}");
                    errorOccurred = true;
                    cancellationTokenSource.Cancel();
                }
                finally
                {
                    semaphore.Release(); // Libera a vaga
                    
                }
            }, cancellationTokenSource.Token));
            pageNumber++;

            await Task.WhenAny(tasks);
            tasks.RemoveAll(t => t.IsCompleted);
        }

        await Task.WhenAll(tasks);
        if (errorOccurred)
        {
            return HandleResult<GetDataEventNotification>(null, ["Erro ao processar a requisição."]);
        }

        var result = await StoreDataEventAsync(startTime, pagesCount, rowsCount, requestKey);
        if (result is null)
        {
            return HandleResult<GetDataEventNotification>(null, ["Erro ao salvar os dados no banco de dados."]);
        }
        var notification = await BuildNotificationAsync(result);
        return HandleResult(notification);
    }

    private bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }
        return false;
    }


    internal async Task<GetDataEventNotification> BuildNotificationAsync(GetDataEvent dataEvent)
    {
        var pages = await htmlFilePersist.GetByRequestKeyAsync(dataEvent.RequestKey);
        var pagesNotification = pages.Select(p => new HtmlFileNotification(p.FileUrl, p.FileContentAddress)).ToArray();
        return new GetDataEventNotification(
            dataEvent.Id,
            dataEvent.StartTime,
            dataEvent.EndTime,
            dataEvent.PagesCount,
            dataEvent.RowsCount,
            dataEvent.RequestKey,
            dataEvent.JsonFile,
            pagesNotification);
    }

    internal async Task<GetDataEvent?> StoreDataEventAsync(DateTime startTime, int pagesCount, int rowsCount, string requestKey)
    {
        var endTime = DateTime.Now;
        var fileUrl = await SaveDataToAzure(ProxyList.ToList());
        var dataEvent = new GetDataEvent(startTime, endTime, pagesCount, rowsCount, fileUrl, requestKey);
        return await StoreDataEventToDatabaseAsync(dataEvent);
    }

    internal async Task<GetDataEvent?> StoreDataEventToDatabaseAsync(GetDataEvent dataEvent)
    {
        eventPersist.Add(dataEvent);
        return await eventPersist.SaveChangesAsync() ? dataEvent : null;
    }

    internal async Task<List<ProxyData>> ExtractProxyDataAsync(string url)
    {
        var response = await FetchRenderedHtmlAsync(url);
        if (string.IsNullOrEmpty(response))
            return [];
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var rows = doc.DocumentNode.SelectNodes("//table//tr");
        if (rows == null || rows.Count < 2)
            return [];

        var indexes = GetIndexes(rows[0]);
        rows.RemoveAt(0);
        var proxies = new List<ProxyData>();

        foreach (var row in rows)
        {
            var columns = row.SelectNodes("td");
            if (columns == null) continue;

            var proxy = new ProxyData
            (
                columns[indexes[_ipKey]].InnerText.Trim(),
                columns[indexes[_portKey]].InnerText.Trim(),
                columns[indexes[_countryKey]].InnerText.Trim(),
                columns[indexes[_protocolKey]].InnerText.Trim()
            );
            proxies.Add(proxy);
        }

        return proxies;
    }

    internal async Task<string> FetchRenderedHtmlAsync(string url)
    {
        try
        {
            return await puppeteerService.FetchRenderedHtmlAsync(url) ?? "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    private Dictionary<string, int> GetIndexes(HtmlNode node)
    {
        var dict = new Dictionary<string, int>();
        var columns = node.SelectNodes("th");

        var keyMap = new Dictionary<string, string>
        {
            { _ipKey, _ipKey },
            { _portKey, _portKey },
            { _countryKey, _countryKey },
            { _protocolKey, _protocolKey }
        };

        foreach (var column in columns)
        {
            foreach (var key in keyMap.Keys)
            {
                if (!column.InnerText.Trim().Contains(keyMap[key])) continue;
                dict[key] = columns.IndexOf(column);
                break;
            }
        }
        return dict;
    }

    internal async Task SaveHtmlToFileAsync(string url, string htmlContent, string requestKey)
    {
        var fileName = $"page_{Guid.NewGuid()}.html";
        var fileBytesArray = Encoding.UTF8.GetBytes(htmlContent);
        var fileUrl = await azureFileHandler.UploadFileToAzureStaAsync(fileBytesArray, $"{_htmlFileFolder}/{fileName}");
        var entity = new HtmlFile(fileName, fileUrl, url, requestKey);

        await SaveHtmlToDatabaseAsync(entity);
    }

    internal async Task SaveHtmlToDatabaseAsync(HtmlFile entity)
    {
        htmlFilePersist.Add(entity);
        await htmlFilePersist.SaveChangesAsync();
    }

    internal async Task<string> SaveDataToAzure(List<ProxyData> data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var fileName = $"{_jsonFileFolder}/proxies_{Guid.NewGuid().ToString()}.json";
        var fileBytesArray = Encoding.UTF8.GetBytes(json);
        var fileUrl = await azureFileHandler.UploadFileToAzureStaAsync(fileBytesArray, fileName);
        return fileUrl;
    }
}
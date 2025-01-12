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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("ElawWebCrawler.Test")]
namespace ElawWebCrawler.Application.Services;

public class ApplicationService(IGetDataEventPersist eventPersist,
    IHtmlFilePersist htmlFilePersist,
    IAzureFileHandler azureFileHandler,
    IConfiguration configuration,
    IPuppeteerService puppeteerService,
    ILogger<ApplicationService> logger)
    : BaseService, IApplicationService
{
    private ConcurrentBag<ProxyData> ProxyBag = new ();
    private SemaphoreSlim? _semaphore;
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
        _semaphore = new SemaphoreSlim(_maxThreads);

        if (!IsValidUrl(url))
        {
            return HandleResult<GetDataEventNotification>(null, ["URL inválida."]);
        }
        var startTime = DateTime.Now;
        var pagesCount = 0;
        var rowsCount = 0;

        var tasks = new List<Task>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var pageNumber = 1;
        var requestKey = Guid.NewGuid().ToString();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(cancellationToken);

                var currentPage = pageNumber++;
                var urlForm = $"{url}/page/{currentPage}";

                var task = Task.Run(async () =>
                {
                    try
                    {
                        logger.LogInformation("Iniciando thread");
                        await ExtractProxyDataAsync(urlForm, ProxyBag, cancellationToken);
                        Console.WriteLine($"Processado: {urlForm}");
                        logger.LogInformation($"Processado: Página de destino");
                        Interlocked.Increment(ref pagesCount);
                        Interlocked.Add(ref rowsCount, ProxyBag.Count);

                        await SaveHtmlToFileAsync(urlForm, await FetchRenderedHtmlAsync(urlForm), requestKey);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"Processamento cancelado para a página {currentPage}: {ex.Message}");
                        logger.LogWarning($"Processamento cancelado para a página");
                        cancellationTokenSource.Cancel();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Erro ao processar página");
                    }
                    finally
                    {
                        _semaphore.Release();
                        logger.LogInformation("Finalizando thread");
                    }
                }, cancellationToken);

                tasks.Add(task);
                tasks.RemoveAll(t => t.IsCompleted);
            }

            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Processamento cancelado");
        }
        finally
        {
            _semaphore.Dispose();
        }

        logger.LogInformation("Threads finalizadas");

        var result = await StoreDataEventAsync(startTime, pagesCount, rowsCount, requestKey);
        if (result is null)
        {
            return HandleResult<GetDataEventNotification>(null, ["Erro ao salvar os dados no banco de dados."]);
        }
        var notification = await BuildNotificationAsync(result);
        logger.LogInformation("Notificação construída");
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
        var fileUrl = await SaveDataToAzure(ProxyBag.ToList());
        var dataEvent = new GetDataEvent(startTime, endTime, pagesCount, rowsCount, fileUrl, requestKey);
        return await StoreDataEventToDatabaseAsync(dataEvent);
    }

    internal async Task<GetDataEvent?> StoreDataEventToDatabaseAsync(GetDataEvent dataEvent)
    {
        eventPersist.Add(dataEvent);
        return await eventPersist.SaveChangesAsync() ? dataEvent : null;
    }

    internal async Task ExtractProxyDataAsync(string url, ConcurrentBag<ProxyData> proxyBag, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        var response = await FetchRenderedHtmlAsync(url);
        if (string.IsNullOrEmpty(response))
            throw new OperationCanceledException("Nenhum HTML foi retornado.");
        
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var rows = doc.DocumentNode.SelectNodes("//table//tr");
        if (rows == null || rows.Count < 2)
            throw new OperationCanceledException("Nenhuma linha foi encontrada na tabela.");

        var indexes = GetIndexes(rows[0]);
        rows.RemoveAt(0);

        foreach (var row in rows)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            var columns = row.SelectNodes("td");
            if (columns == null) continue;

            var proxy = new ProxyData
            (
                columns[indexes[_ipKey]].InnerText.Trim(),
                columns[indexes[_portKey]].InnerText.Trim(),
                columns[indexes[_countryKey]].InnerText.Trim(),
                columns[indexes[_protocolKey]].InnerText.Trim()
            );
            proxyBag.Add(proxy);
        }
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
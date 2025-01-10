﻿using System.Collections.Concurrent;
using System.Text;
using ElawWebCrawler.Common;
using ElawWebCrawler.Domain.Entities;
using ElawWebCrawler.Domain.Interfaces;
using ElawWebCrawler.Provider.Azure;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace ElawWebCrawler.Application;

public class ApplicationService(IGetDataEventPersist eventPersist, 
    IHtmlFilePersist htmlFilePersist,
    IAzureFileHandler azureFileHandler, 
    IConfiguration configuration) 
    : BaseService, IApplicationService
{
    private static readonly ConcurrentBag<ProxyData> ProxyList = new ConcurrentBag<ProxyData>();
    private HttpClient client = new HttpClient();
    private SemaphoreSlim semaphore;
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
            _maxThreads = number;
        }
        semaphore = new SemaphoreSlim(_maxThreads);
        
        var container = new DataContainer<GetDataEventNotification>();
        if (string.IsNullOrEmpty(url))
        {
            return HandleResult<GetDataEventNotification>(null, ["URL inválida."]);
        }
        var startTime = DateTime.Now;
        var pagesCount = 0;
        var rowsCount = 0;
        
        var tasks = new List<Task>();
        
        var pageNumber = 1; 
        var morePages = true; 


        while (morePages)
        {
            var urlForm = $"{url}/page/{pageNumber}";
            await semaphore.WaitAsync(); // Aguarda uma vaga para executar a tarefa

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var proxies = await ExtractProxyDataAsync(urlForm);

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

                    await SaveHtmlToFileAsync(urlForm, await client.GetStringAsync(urlForm));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar {urlForm}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release(); // Libera a vaga
                }
            }));
            pageNumber++;

            await Task.WhenAny(tasks);
            tasks.RemoveAll(t => t.IsCompleted);
        }
        
        await Task.WhenAll(tasks);
        
        var endTime = DateTime.Now;
        var fileUrl = await SaveDataToAzure(ProxyList.ToList());

        var dataEvent = new GetDataEvent(startTime, endTime, pagesCount, rowsCount, fileUrl);
        eventPersist.Add(dataEvent);
        if (!await eventPersist.SaveChangesAsync())
        {
            return HandleResult<GetDataEventNotification>(null, ["Erro ao salvar os dados no banco de dados."]);
        }
        var notification = new GetDataEventNotification(dataEvent.Id, startTime, endTime, pagesCount, rowsCount);
        return HandleResult(notification);
    }

    private async Task<List<ProxyData>> ExtractProxyDataAsync(string url)
    {
        var response = await client.GetStringAsync(url);
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

    private async Task SaveHtmlToFileAsync(string url, string htmlContent)
    {
        var fileName = $"page_{url.GetHashCode()}.html";
        var fileBytesArray = Encoding.UTF8.GetBytes(htmlContent);
        var fileUrl = await azureFileHandler.UploadFileToAzureStaAsync(fileBytesArray, $"{_htmlFileFolder}/{fileName}");
        var entity = new HtmlFile(fileName, fileUrl, url);
        
        await SaveHtmlToDatabaseAsync(entity);
    }
    
    private async Task SaveHtmlToDatabaseAsync(HtmlFile entity)
    {
        htmlFilePersist.Add(entity);
        await htmlFilePersist.SaveChangesAsync();
    }
    
    private async Task<string> SaveDataToAzure(List<ProxyData> data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var fileName = $"{_jsonFileFolder}/proxies_{Guid.NewGuid().ToString()}.json";
        var fileBytesArray = Encoding.UTF8.GetBytes(json);
        var fileUrl = await azureFileHandler.UploadFileToAzureStaAsync(fileBytesArray, fileName);
        return fileUrl;
    }
}
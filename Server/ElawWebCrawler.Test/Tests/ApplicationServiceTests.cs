using System.Text;
using Microsoft.Extensions.Configuration;
using ElawWebCrawler.Application;
using ElawWebCrawler.Application.Interfaces;
using ElawWebCrawler.Application.Services;
using ElawWebCrawler.Domain.Entities;
using ElawWebCrawler.Domain.Interfaces;
using ElawWebCrawler.Provider.Azure;
using Moq;
using PuppeteerSharp;

namespace ElawWebCrawler.Test.Tests;

public class ApplicationServiceTests
{
    private readonly Mock<IGetDataEventPersist> _mockEventPersist;
    private readonly Mock<IHtmlFilePersist> _mockHtmlFilePersist;
    private readonly Mock<IAzureFileHandler> _mockAzureFileHandler;
    private readonly Mock<IPuppeteerService> _mockPuppeteerService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ApplicationService _service;
    private const string _url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
    
    public ApplicationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEventPersist = new Mock<IGetDataEventPersist>();
        _mockHtmlFilePersist = new Mock<IHtmlFilePersist>();
        _mockAzureFileHandler = new Mock<IAzureFileHandler>();
        _mockPuppeteerService = new Mock<IPuppeteerService>();
        _service = new ApplicationService(
            _mockEventPersist.Object,
            _mockHtmlFilePersist.Object,
            _mockAzureFileHandler.Object,
            _mockConfiguration.Object,
            _mockPuppeteerService.Object);
    }
    
    [Fact]
    public async Task ScrapDataAsync_ShouldReturnError_WhenUrlIsEmpty()
    {
        _mockConfiguration.Setup(c => c.GetSection("MaxThreads").Value).Returns("3");
        var result = await _service.ScrapDataAsync(string.Empty);

        Assert.Null(result.Data);
        Assert.NotEmpty(result.Messages);
    }
    
    [Fact]
    public async Task ScrapDataAsync_ShouldProcessData_WhenUrlIsValid()
    {
        // Arrange
        _mockConfiguration.Setup(c => c.GetSection("MaxThreads").Value).Returns("3");
        var fileUrl = "fileUrl";
        var dataEvent = new GetDataEvent(DateTime.Now, DateTime.Now, 1, 1, "fileUrl", "requestKey");
        var mockAddress = "127.0.0.1";
        var mockPort = "8080";
        var mockCountry = "US";
        var mockProtocol = "HTTP";
        var htmlContent = $"<html><table><tr><th>IP Address</th><th>Port</th><th>Country</th><th>Protocol</th></tr>" +
                          $"<tr><td>{mockAddress}</td><td>{mockPort}</td><td>{mockCountry}</td><td>{mockProtocol}</td></tr></table></html>";
        _mockAzureFileHandler.Setup(a => a.UploadFileToAzureStaAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync(fileUrl);
        _mockHtmlFilePersist.Setup(p => p.Add(It.IsAny<HtmlFile>()));
        _mockHtmlFilePersist.Setup(p => p.SaveChangesAsync()).ReturnsAsync(true);
        _mockHtmlFilePersist.Setup(p => p.GetByRequestKeyAsync(It.IsAny<string>())).ReturnsAsync(new List<HtmlFile>());
        _mockEventPersist.Setup(e => e.Add(It.IsAny<GetDataEvent>()));
        _mockEventPersist.Setup(e => e.SaveChangesAsync()).ReturnsAsync(true);
        _mockEventPersist.Setup(e => e.GetByRequestKeyAsync(It.IsAny<string>())).ReturnsAsync([dataEvent]);
        _mockPuppeteerService.Setup(p => p.FetchRenderedHtmlAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<string>());
        _mockPuppeteerService.Setup(p => p.LaunchBrowserAsync()).ReturnsAsync(Mock.Of<IBrowser>());

        // Act
        var result = await _service.ScrapDataAsync(_url);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public async Task BuildNotificationAsync_ShouldReturnNotification()
    {
        // Arrange
        var dataEvent = new GetDataEvent(DateTime.Now, DateTime.Now, 1, 1, "fileUrl", "requestKey");
        _mockHtmlFilePersist.Setup(p => p.GetByRequestKeyAsync(It.IsAny<string>())).ReturnsAsync(new List<HtmlFile>());

        // Act
        var notification = await _service.BuildNotificationAsync(dataEvent);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(dataEvent.Id, notification.Id);
    }

    [Fact]
    public async Task StoreDataEventAsync_ShouldReturnDataEvent()
    {
        // Arrange
        var startTime = DateTime.Now;
        var pagesCount = 1;
        var rowsCount = 1;
        var requestKey = "requestKey";
        _mockAzureFileHandler.Setup(a => a.UploadFileToAzureStaAsync(It.IsAny<byte[]>(), It.IsAny<string>())).ReturnsAsync("fileUrl");
        _mockEventPersist.Setup(e => e.Add(It.IsAny<GetDataEvent>()));
        _mockEventPersist.Setup(e => e.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var dataEvent = await _service.StoreDataEventAsync(startTime, pagesCount, rowsCount, requestKey);

        // Assert
        Assert.NotNull(dataEvent);
        Assert.Equal(pagesCount, dataEvent.PagesCount);
    }

    [Fact]
    public async Task StoreDataEventToDatabaseAsync_ShouldReturnDataEvent()
    {
        // Arrange
        var dataEvent = new GetDataEvent(DateTime.Now, DateTime.Now, 1, 1, "fileUrl", "requestKey");
        _mockEventPersist.Setup(e => e.Add(It.IsAny<GetDataEvent>()));
        _mockEventPersist.Setup(e => e.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.StoreDataEventToDatabaseAsync(dataEvent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dataEvent, result);
    }

    [Fact]
    public async Task ExtractProxyDataAsync_ShouldReturnProxyData()
    {
        // Arrange
        var mockAddress = "127.0.0.1";
        var mockPort = "8080";
        var mockCountry = "US";
        var mockProtocol = "HTTP";
        var htmlContent = $"<html><table><tr><th>IP Address</th><th>Port</th><th>Country</th><th>Protocol</th></tr>" +
                          $"<tr><td>{mockAddress}</td><td>{mockPort}</td><td>{mockCountry}</td><td>{mockProtocol}</td></tr></table></html>";
        _mockPuppeteerService.Setup(p => p.FetchRenderedHtmlAsync(It.IsAny<string>())).ReturnsAsync(htmlContent);

        // Act
        var proxies = await _service.ExtractProxyDataAsync(_url);

        // Assert
        Assert.Single(proxies);
        Assert.Equal(mockAddress, proxies[0].IpAddress);
        Assert.Equal(mockPort, proxies[0].Port);
        Assert.Equal(mockCountry, proxies[0].Country);
        Assert.Equal(mockProtocol, proxies[0].Protocol);
    }
    
    [Fact]
    public async Task FetchRenderedHtmlAsync_ShouldReturnHtmlContent()
    {
        // Arrange
        var htmlContent = "<html><table><tr><th>IP Address</th><th>Port</th><th>Country</th><th>Protocol</th></tr></table></html>";
        _mockPuppeteerService.Setup(p => p.FetchRenderedHtmlAsync(It.IsAny<string>())).ReturnsAsync(htmlContent);

        // Act
        var content = await _service.FetchRenderedHtmlAsync(_url);

        // Assert
        Assert.Contains("<table>", content);
    }

    [Fact]
    public async Task SaveHtmlToFileAsync_ShouldSaveHtmlFile()
    {
        // Arrange
        var url = "http://example.com";
        var htmlContent = "<html></html>";
        var requestKey = "requestKey";
        _mockAzureFileHandler.Setup(a => a.UploadFileToAzureStaAsync(It.IsAny<byte[]>(), It.IsAny<string>())).ReturnsAsync("fileUrl");

        // Act
        await _service.SaveHtmlToFileAsync(url, htmlContent, requestKey);

        // Assert
        _mockHtmlFilePersist.Verify(p => p.Add(It.IsAny<HtmlFile>()), Times.Once);
        _mockHtmlFilePersist.Verify(p => p.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveDataToAzure_ShouldReturnFileUrl()
    {
        // Arrange
        var data = new List<ProxyData> { new ProxyData("127.0.0.1", "8080", "US", "HTTP") };
        _mockAzureFileHandler.Setup(a => a.UploadFileToAzureStaAsync(It.IsAny<byte[]>(), It.IsAny<string>())).ReturnsAsync("fileUrl");

        // Act
        var fileUrl = await _service.SaveDataToAzure(data);

        // Assert
        Assert.Equal("fileUrl", fileUrl);
    }

    [Fact]
    public async Task SaveHtmlToDatabaseAsync_ShouldReturnHtmlFile()
    {
        // Arrange
        var requestKey = "request_key";
        var fileName = $"page_{_url.GetHashCode()}.html";
        var fileUrl = "fileUrl";
        var entity = new HtmlFile(fileName, fileUrl, _url, requestKey);
        _mockAzureFileHandler.Setup(a => a.UploadFileToAzureStaAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync(fileUrl);
        _mockHtmlFilePersist.Setup(p => p.Add(It.IsAny<HtmlFile>()));
        _mockHtmlFilePersist.Setup(p => p.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _service.SaveHtmlToDatabaseAsync(entity);

        // Assert
        _mockHtmlFilePersist.Verify(p => p.Add(It.IsAny<HtmlFile>()), Times.Once);
        _mockHtmlFilePersist.Verify(p => p.SaveChangesAsync(), Times.Once);
    }


}

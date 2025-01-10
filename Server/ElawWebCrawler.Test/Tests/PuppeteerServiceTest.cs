using System.Runtime.CompilerServices;
using ElawWebCrawler.Application.Interfaces;
using ElawWebCrawler.Application.Services;
using Moq;
using PuppeteerSharp;

namespace ElawWebCrawler.Test.Tests;

public class PuppeteerServiceTest
{
    private readonly PuppeteerService _service;
    private const string _url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
    private readonly Mock<IPuppeteerService> _mockPuppeteerService;

    public PuppeteerServiceTest()
    {
        _mockPuppeteerService = new Mock<IPuppeteerService>();
        _service = new PuppeteerService();
    }

    [Fact]
    public async Task FetchRenderedHtmlAsync_ShouldReturnHtmlContent()
    {
        // Arrange
        var htmlContent = "<html><table><tr><th>IP Address</th><th>Port</th><th>Country</th><th>Protocol</th></tr></table></html>";
        var mockBrowser = new Mock<IBrowser>();
        var mockPage = new Mock<IPage>();

        _mockPuppeteerService.Setup(p => p.LaunchBrowserAsync()).ReturnsAsync(mockBrowser.Object);
        _mockPuppeteerService.Setup(p => p.FetchRenderedHtmlAsync(_url)).ReturnsAsync(htmlContent);

        mockPage.Setup(p => p.GoToAsync(It.IsAny<string>(), It.IsAny<NavigationOptions>())).Returns(Task.FromResult(Mock.Of<IResponse>()));
        mockPage.Setup(p => p.GetContentAsync(It.IsAny<GetContentOptions>())).ReturnsAsync(htmlContent);
        mockBrowser.Setup(b => b.NewPageAsync()).ReturnsAsync(mockPage.Object);

        var mockPuppeteerService = new Mock<IPuppeteerService>();
        mockPuppeteerService.Setup(p => p.LaunchBrowserAsync()).ReturnsAsync(mockBrowser.Object);


        // Act
        var content = await _service.FetchRenderedHtmlAsync(_url);

        // Assert
        Assert.IsType<string>(content);
    }

    [Fact]
    public async Task LaunchBrowserAsync_ShouldReturnBrowserInstance()
    {
        // Arrange
        _mockPuppeteerService.Setup(p => p.LaunchBrowserAsync()).ReturnsAsync(Mock.Of<IBrowser>());

        // Act
        var browser = await _service.LaunchBrowserAsync();

        // Assert
        Assert.NotNull(browser);
        Assert.IsAssignableFrom<IBrowser>(browser);
    }
}

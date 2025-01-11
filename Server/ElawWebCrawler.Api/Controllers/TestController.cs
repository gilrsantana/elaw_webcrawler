using Microsoft.AspNetCore.Mvc;

namespace ElawWebCrawler.Api.Controllers;

public class TestController(ILogger<TestController> logger)
    : ElawWebCrawlerControllerBase(logger)
{

    [HttpGet]
    public IActionResult TestAsync()
    {
        Console.WriteLine("TestAsync");
        return Ok($"Api is running. {DateTime.Now}");
    }
}
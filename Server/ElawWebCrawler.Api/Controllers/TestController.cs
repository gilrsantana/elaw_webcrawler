using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ElawWebCrawler.Api.Controllers;

public class TestController(ILogger<TestController> logger)
    : ElawWebCrawlerControllerBase(logger)
{

    [HttpGet]
    public IActionResult TestAsync()
    {
        return Ok($"Api is running. {DateTime.Now}");
    }
}
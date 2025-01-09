﻿using System.Net;
using ElawWebCrawler.Api.ExceptionHandler;
using ElawWebCrawler.Api.Notifications;
using ElawWebCrawler.Application;
using Microsoft.AspNetCore.Mvc;

namespace ElawWebCrawler.Api.Controllers;

public class WebCrawlerController(ILogger<WebCrawlerController> logger, IApplicationService service) : ElawWebCrawlerControllerBase(logger)
{
    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModelApi<WebCrawlerViewModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ResultViewModelApi<>), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ResultViewModelApi<>), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetDataAsync([FromQuery] string url)
    {
        try
        {
            var result = await service.ScrapDataAsync(url);
            if (result.Messages.Count > 0 || result.Data is null)
                return BadRequest(result);
            var viewModel = new WebCrawlerViewModel(
                result.Data.Id, 
                result.Data.StartTime,
                result.Data.EndTime,
                result.Data.PagesCount,
                result.Data.RowsCount);
            return Ok(new ResultViewModelApi<WebCrawlerViewModel>(viewModel));
        }
        catch (Exception ex)
        {
            return HandleException(
                new ExceptionModel(
                    "PTP054P",
                    $"Erro interno no servidor ao buscar os dados.",
                    "",
                    "",
                    ex));
        }
    }
}
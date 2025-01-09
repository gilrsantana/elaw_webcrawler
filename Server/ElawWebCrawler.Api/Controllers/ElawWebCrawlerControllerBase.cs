using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ElawWebCrawler.Api.ExceptionHandler;
using ElawWebCrawler.Api.Notifications;
using ElawWebCrawler.Common;
using Microsoft.AspNetCore.Mvc;

namespace ElawWebCrawler.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class ElawWebCrawlerControllerBase(ILogger<ElawWebCrawlerControllerBase> logger) : ControllerBase
{
    protected readonly int DefaultSkip = 0;
    protected readonly int DefaultTake = 25;
    protected JsonSerializerOptions? Options { get; set; } = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() },
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };
    private bool ResultIsValid(List<MessageModel> messages)
        => messages.All(x => x.Type != MessageType.ERROR);

    protected IActionResult HandleResult<T>(DataContainer<T> result, Func<T, IActionResult> successAction)
        => !ResultIsValid(result.Messages)
            ? BadRequest(new ResultViewModelApi<string>(result.Messages))
            : successAction(result.Data);

    protected IActionResult HandleException(ExceptionModel exceptionModel)
    {
        var msg = $"({exceptionModel.ErrorCode}) - {exceptionModel.ErrorMessage}";
        logger.LogCritical($"{msg} # {exceptionModel.Serialized} # {exceptionModel.ex} # {exceptionModel.UserName}");
        return StatusCode((int)HttpStatusCode.InternalServerError,
            new ResultViewModelApi<string>(msg, MessageType.CRITICAL_ERROR));
    }
}
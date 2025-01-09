namespace ElawWebCrawler.Api.ExceptionHandler;

public record ExceptionModel(string ErrorCode, string ErrorMessage, string UserName, string Serialized, Exception ex);
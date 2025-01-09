namespace ElawWebCrawler.Api.Notifications;

public record WebCrawlerViewModel(string Id, DateTime StartDate, DateTime EndDate, int Page, int Row);
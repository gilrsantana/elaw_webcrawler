namespace ElawWebCrawler.Application;

public record GetDataEventNotification(string Id, DateTime StartTime, DateTime EndTime, int PagesCount, int RowsCount);
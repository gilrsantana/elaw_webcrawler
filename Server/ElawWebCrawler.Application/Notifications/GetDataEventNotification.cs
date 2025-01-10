namespace ElawWebCrawler.Application.Notifications;

public record GetDataEventNotification(
    string Id, 
    DateTime StartTime, 
    DateTime EndTime, 
    int PagesCount, 
    int RowsCount,
    string RequestKey,
    string DataUrlFile,
    HtmlFileNotification[] PagesUrl);
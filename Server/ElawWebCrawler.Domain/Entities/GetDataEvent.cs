namespace ElawWebCrawler.Domain.Entities;

public class GetDataEvent
{
    public string Id { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int PagesCount { get; private set; }
    public int RowsCount { get; private set; }
    public string RequestKey { get; private set; }
    public string JsonFile { get; private set; }
    
    public GetDataEvent(DateTime startTime, DateTime endTime, int pagesCount, int rowsCount, string jsonFile, string requestKey)
    {
        Id = Guid.CreateVersion7().ToString();
        StartTime = startTime;
        EndTime = endTime;
        PagesCount = pagesCount;
        RowsCount = rowsCount;
        JsonFile = jsonFile;
        RequestKey = requestKey;
    }
}
namespace ElawWebCrawler.Domain.Entities;

public class HtmlFile
{
    public string Id { get; private set; }
    public string FileName { get; private set; }
    public string FileContentAddress { get; private set; }
    public string FileUrl { get; private set; }
    public string RequestKey { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public HtmlFile(string fileName, string fileContentAddress, string fileUrl, string requestKey)
    {
        Id = Guid.CreateVersion7().ToString();
        FileName = fileName;
        FileContentAddress = fileContentAddress;
        FileUrl = fileUrl;
        RequestKey = requestKey;
        CreatedAt = DateTime.Now;
    }
}
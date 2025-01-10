namespace ElawWebCrawler.Provider.Azure;

public interface IAzureFileHandler
{
    Task<string> UploadFileToAzureStaAsync(byte[] fileContent, string uniqueFileName);
}
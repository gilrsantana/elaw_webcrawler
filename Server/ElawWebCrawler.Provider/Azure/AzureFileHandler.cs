using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace ElawWebCrawler.Provider.Azure;

public class AzureFileHandler(IConfiguration configuration) : IAzureFileHandler
{
    private const string ConnectionStringName = "AzureStorageConnection";
    public async Task<string> UploadFileToAzureStaAsync(byte[] fileContent, string uniqueFileName)
    {
        var blobClient = new BlobClient(GetConnectionString(),  GetContainerName(), uniqueFileName);
        await blobClient.UploadAsync(new MemoryStream(fileContent));
        
        return blobClient.Uri.AbsoluteUri;
    }
    
    private string GetConnectionString()
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName) 
                               ?? throw new ApplicationException("Not found AzureStorageConnection");
        
        return connectionString;
    }
    
    private string GetContainerName()
    {
        var section = configuration.GetSection("AzureBlobStorage")
               ?? throw new ApplicationException("Not found AzureStorageConnection");
        
        var containerName = section["ContainerName"] 
                          ?? throw new ApplicationException("Not found ContainerName");
        
        return containerName;
    }
}
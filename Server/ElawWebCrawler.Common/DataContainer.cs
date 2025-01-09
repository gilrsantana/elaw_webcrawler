namespace ElawWebCrawler.Common;

public class DataContainer<T>
{
    public T? Data { get; set; }
    public List<MessageModel> Messages { get; set; }

    public DataContainer()
    {
        Messages = new List<MessageModel>();
    }
}
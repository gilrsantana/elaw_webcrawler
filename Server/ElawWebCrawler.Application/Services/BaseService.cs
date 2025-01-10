using ElawWebCrawler.Common;

namespace ElawWebCrawler.Application.Services;

public class BaseService
{
    internal DataContainer<T> HandleResult<T>(T? data, List<string>? messages = null, MessageType type = MessageType.ERROR)
    {
        var container = new DataContainer<T>();
        container.Messages
            .AddRange(messages?.Select(m => 
                new MessageModel(m, type)) ?? []);
        container.Data = data;
        return container;
    }
}
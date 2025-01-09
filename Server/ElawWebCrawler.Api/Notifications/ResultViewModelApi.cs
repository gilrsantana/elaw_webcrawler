using ElawWebCrawler.Common;

namespace ElawWebCrawler.Api.Notifications;

public class ResultViewModelApi<T> : ResultViewModelBase<T> where T : class 
{
    public ResultViewModelApi(T viewData) : base(viewData)
    {
    }

    public ResultViewModelApi(List<MessageModel> messages) : base(messages)
    {
    }

    public ResultViewModelApi(MessageModel message) : base(message)
    {
    }

    public ResultViewModelApi(T? viewData, List<MessageModel> messages) : base(viewData, messages)
    {
    }

    public ResultViewModelApi(string message, MessageType type = MessageType.INFORMATION) : base(message, type)
    {
    }

    public ResultViewModelApi(IReadOnlyCollection<string> list, MessageType type = MessageType.INFORMATION) : base(list, type)
    {
    }
}
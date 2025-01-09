namespace ElawWebCrawler.Common;

public class ResultViewModelBase<T>
{
    public T? ViewData { get; private set; }
    public List<MessageModel> Messages { get; private set; } = new();

    public ResultViewModelBase(T? viewData, List<MessageModel> messages)
    {
        ViewData = viewData;
        Messages = messages;
    }

    public ResultViewModelBase(T viewData)
    {
        ViewData = viewData;
    }

    public ResultViewModelBase(List<MessageModel> messages)
    {
        Messages = messages;
    }

    public ResultViewModelBase(MessageModel message)
    {
        Messages.Add(message);
    }

    public ResultViewModelBase(string message, MessageType type = MessageType.INFORMATION)
    {
        Messages.Add(new MessageModel(message, type));
    }

    public ResultViewModelBase(IReadOnlyCollection<string> list, MessageType type = MessageType.INFORMATION)
    {
        Messages = !list.Any()
            ? new List<MessageModel>()
            : list.Select(message => new MessageModel(message, type)).ToList();  
    }
}
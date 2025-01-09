namespace ElawWebCrawler.Common;

public class MessageModel
{
    public MessageType Type { get; private set; }
    public string Text { get; private set; }

    public MessageModel(string message, MessageType type = MessageType.INFORMATION)
    {
        Type = type;
        Text = message;
    }
}
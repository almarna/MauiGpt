namespace MauiGpt.Dto;

public class ChatMessageDto
{
    public ChatMessageType Type { get; set;}
    // public string Message { get; set;}
    public IList<HtmlBlock> HtmlBlocks { get; set; } = new List<HtmlBlock>();
    public bool IsTemporary { get; set; }

    public static ChatMessageDto GetAnswer(IList<HtmlBlock> htmlBlocks)
    {
        return new ChatMessageDto { Type = ChatMessageType.Answer, HtmlBlocks = htmlBlocks };
    }

    public static ChatMessageDto GetQuestion(string question)
    {
        return GetSingle(ChatMessageType.Question, question);
    }

    public static ChatMessageDto GetError(string error)
    {
        return GetSingle(ChatMessageType.Error, error);
    }

    private static ChatMessageDto GetSingle(ChatMessageType type, string html)
    {
        IList<HtmlBlock> htmlBlocks = new List<HtmlBlock>();
        htmlBlocks.Add(new HtmlBlock { Html = html, Type = BlockType.Plain });
        return new ChatMessageDto { Type = type, HtmlBlocks = htmlBlocks };
    }
}
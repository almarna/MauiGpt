using MauiGpt.Dto;

namespace MauiGpt.Data;

public class ChatMessages
{
    public IList<ChatMessageDto> AllMessages { get; private set; }= new List<ChatMessageDto>();

    public void SetTemporaryLine(IList<HtmlBlock> htmlBlocks)
    {
        var temporaryLine = AllMessages.LastOrDefault();
        if (temporaryLine?.IsTemporary ?? false)
        {
            temporaryLine.HtmlBlocks = htmlBlocks;
        }
        else
        {
            temporaryLine = ChatMessageDto.GetAnswer(htmlBlocks);
            temporaryLine.IsTemporary = true;
            AllMessages.Add(temporaryLine);
        }
    }

    public void Clear()
    {
        AllMessages = new List<ChatMessageDto>();
    }

    public void AddQuestion(string question)
    {
        AllMessages.Add(ChatMessageDto.GetQuestion(question));
    }

    public void AddError(string error)
    {
        AllMessages.Add(ChatMessageDto.GetError(error));
    }

    public async Task AddAnswer(IList<HtmlBlock> htmlBlocks)
    {
        AllMessages = AllMessages.Where(item => item.IsTemporary == false).ToList();
        AllMessages.Add(ChatMessageDto.GetAnswer(htmlBlocks));
    }

    public static string CleanQuestion(string input)
    {
        string result = input
            .Replace("<", "[")
            .Replace(">", "]");

        return result;
    }
}
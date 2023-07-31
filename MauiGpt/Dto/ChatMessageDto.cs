namespace MauiGpt.Dto;

public enum ChatMessageType {
   Question,
   Answer,
   Error
}

public class ChatMessageDto
{
    public ChatMessageType Type { get; set;}
    public string Message { get; set;}
}
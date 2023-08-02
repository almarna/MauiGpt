namespace MauiGpt.Dto;

public enum ChatMessageType {
    Question,
    Answer,
    Error
}

public enum AiAnswerType
{
    Normal,
    Error
}

public enum BlockType
{
    Plain,
    Code
}
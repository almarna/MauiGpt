namespace MauiGpt.Dto;

public enum ChatMessageType {
    Question,
    Answer,
    Error,
    Info
}

public enum AiAnswerType
{
    Normal,
    Error
}

public enum BlockType
{
    Plain,
    Code,
    Editable
}
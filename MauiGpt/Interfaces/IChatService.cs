using MauiGpt.Dto;

namespace MauiGpt.Interfaces;

public interface IChatService
{
    Task<(AiAnswerType, string)> Ask(string question, Func<string, Task> callback, CancellationToken cancellationToken);
    void ClearHistory();
    void SetHistory(IEnumerable<ChatItemDto>  history);
    IEnumerable<ChatItemDto> GetHistory();
}
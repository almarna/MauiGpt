using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
namespace MauiGpt.Data.DbInfo;

public class ChatMemory
{
    private readonly string _prePrompt;
    private readonly IChatCompletion _chatCompletion;

    public OpenAIChatHistory History { get; private set; }

    public ChatMemory(IKernel kernel, string prePrompt)
    {
        _prePrompt = prePrompt;
        _chatCompletion = kernel.GetService<IChatCompletion>();
        History = (OpenAIChatHistory)_chatCompletion.CreateNewChat(prePrompt);
    }

    public void AddMessage(AuthorRole authorRole, string content)
    {
        History.AddMessage(authorRole, content);
    }

    public void Reset()
    {
        History = (OpenAIChatHistory)_chatCompletion.CreateNewChat(_prePrompt);
    }


    public IEnumerable<string> HistoryLog()
    {
        foreach (var chatMessageBase in History)
        {
            string role = chatMessageBase.Role.ToString();
            string message = chatMessageBase.Content;

            yield return @$"{role}: {message}";
        }
    }
}
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

namespace MauiGpt.Data.DbInfo;

public class AiFunctions
{
    private readonly OpenAiService _openAiService;
    private readonly ChatMemory _chatMemory;
    private readonly AIRequestSettings _chatRequestSettings;

    public Func<string, Task> Callback { get; set; }

    private async Task RunCallback(string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Callback != null)
            {
                await Callback.Invoke(message);
            }
        });
    }

    public AiFunctions(OpenAiService openAiService, ChatMemory chatMemory)
    {
        _openAiService = openAiService;
        _chatMemory = chatMemory;

        _chatRequestSettings = new OpenAIRequestSettings() { Temperature = 0 };

    }

    [SKFunction, Description("Send a prompt to the LLM. This function uses the chat-history and current prompt to return a meaningful text answer to the user"), SKName("Prompt")]
    public async Task<string> Prompt(string prompt)
    {
        try
        {
            _openAiService.SetHistory(_chatMemory.GetHistory());
            var (anwswerType, reply) = await _openAiService.Ask(prompt, RunCallback, CancellationToken.None);

            _chatMemory.AddMessage(AuthorRole.Assistant, reply);
            return reply;
        }
        catch (Exception aiex)
        {
            return $"OpenAI returned an error ({aiex.Message}). Please try again.";
        }
    }

        [SKFunction, Description("Add user message."), SKName("AddUserMessage")]
        public async Task AiAddUserMessage([Description("The user message")] string message)
        {
            _chatMemory.AddMessage(AuthorRole.System, message);

        }
}

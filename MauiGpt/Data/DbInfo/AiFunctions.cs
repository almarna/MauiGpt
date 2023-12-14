using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

namespace MauiGpt.Data.DbInfo;

public class AiFunctions
{
    private readonly ChatMemory _chatMemory;
    private readonly IChatCompletion _chatCompletion;
    private readonly AIRequestSettings _chatRequestSettings;
    private readonly string _prePrompt;

    public AiFunctions(IKernel kernel, ChatMemory chatMemory)
    {
        _chatMemory = chatMemory;
        _chatCompletion = kernel.GetService<IChatCompletion>();

        _chatRequestSettings = new OpenAIRequestSettings() { Temperature = 0 };

    }

    [SKFunction, Description("Send a prompt to the LLM. This function uses the chat-history and current prompt to return a meaningful text answer to the user"), SKName("Prompt")]
    public async Task<string> Prompt(string prompt)
    {
        try
        {
 //           _chatMemory.AddMessage(AuthorRole.User, prompt);
            var reply = await _chatCompletion.GenerateMessageAsync(_chatMemory.History, _chatRequestSettings);

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


    //[SKFunction, Description("Add user message. Do not user for assistent messages."), SKName("AddUserMessage")]
    //public async Task AddUserMessage([Description("The user message")] string message)
    //{
    //    _chatHistory.AddUserMessage(message);

    //}

    //[SKFunction, Description("Reset chat history."), SKName("ResetHistory")]
    //public async Task ResetHistory()
    //{
    //    _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(_prePrompt);
    //}
}

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

namespace MauiGpt.Data.DbInfo;

public class AiFunctions
{
    private readonly IChatCompletion _chatCompletion;
    private OpenAIChatHistory _chatHistory;
    private readonly AIRequestSettings _chatRequestSettings;
    private readonly string _prePrompt;

    public AiFunctions(IKernel kernel, string prePrompt)
    {
        _chatCompletion = kernel.GetService<IChatCompletion>();
        _prePrompt = prePrompt;
        _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(prePrompt);

        _chatRequestSettings = new OpenAIRequestSettings() { Temperature = 0 };

    }

    public IEnumerable<string> GetHistory()
    {
        foreach (var chatMessageBase in _chatHistory)
        {
            string role = chatMessageBase.Role.ToString();
            string message = chatMessageBase.Content;

            yield return @$"{role}: {message}";
        }
    }

    [SKFunction, Description("Send a prompt to the LLM."), SKName("Prompt")]
    public async Task<string> Prompt(string prompt)
    {
        try
        {
            _chatHistory.AddUserMessage(prompt);
            var reply = await _chatCompletion.GenerateMessageAsync(_chatHistory, _chatRequestSettings);

            _chatHistory.AddAssistantMessage(reply);
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
            _chatHistory.AddUserMessage(message);

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

    public void AddUserMessage(string message)
    {
        _chatHistory.AddUserMessage(message);

    }

    public void ResetHistory()
    {
        _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(_prePrompt);
    }

}

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace LabSemanticKernel.Lab3;

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

    [SKFunction, Description("Add user message."), SKName("AddUserMessage") ]
    public async Task AddUserMessage([Description("The user message")] string message)
    {
        _chatHistory.AddUserMessage(message);

    }

    [SKFunction, Description("Reset chat history."), SKName("ResetHistory")]
    public async Task ResetHistory()
    {
        _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(_prePrompt);
    }

    [SKFunction, Description("Send a prompt to the LLM."), SKName("Prompt")]
    public async Task<string> Prompt(string prompt)
    {
        var reply = string.Empty;
        try
        {
            // Add the question as a user message to the chat history, then send everything to OpenAI.
            // The chat history is used as context for the prompt
            _chatHistory.AddUserMessage(prompt);
            reply = await _chatCompletion.GenerateMessageAsync(_chatHistory, _chatRequestSettings);

            // Add the interaction to the chat history.
            _chatHistory.AddAssistantMessage(reply);
        }
        catch (Exception aiex)
        {
            // Reply with the error message if there is one
            reply = $"OpenAI returned an error ({aiex.Message}). Please try again.";
        }

        return reply;

    }

    //[SKFunction, Description("Get database table definition")]
    //public async Task<string> DbDescriptionAsync(
    //    [Description("The database name")] string database,
    //    [Description("The table name")] string table
    //)
    //{
    //    var sqlInfo = new SqlInfo(database);
    //    var tableData = await sqlInfo.GetColumnInformationAsync(table);

    //    var result = $"{{ database: {database}, table: {table} fields: {tableData}";
    //    return result;
    //}
}
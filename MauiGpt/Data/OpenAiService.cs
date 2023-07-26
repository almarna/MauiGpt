using System.Text;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.OpenAI;

namespace MauiGpt.Data;

public class OpenAiService
{
    private readonly string Endpoint;
    private readonly string AuthKey;
    private readonly OpenAIClient openAiClient;
    private readonly string languageModel = "gpt3";
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ChatCompletionsOptions _chatCompletionsOptions = new()
    {
        Messages =
        {
        }
    };

    public OpenAiService()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();
        Endpoint = config.GetValue<string>("Endpoint");
        AuthKey = config.GetValue<string>("AuthKey");

        openAiClient = new OpenAIClient(
            new Uri(Endpoint),
            new AzureKeyCredential(AuthKey)
        );

    }

    public async Task<string> Ask(string question, Func<string, Task> callback)
    {
        try
        {
            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, question));

            var chatCompletionsResponse = await openAiClient.GetChatCompletionsStreamingAsync(
                languageModel,
                _chatCompletionsOptions
            );

            var chatResponseBuilder = new StringBuilder();
            await foreach (var chatChoice in chatCompletionsResponse.Value.GetChoicesStreaming())
            {
                await foreach (var chatMessage in chatChoice.GetMessageStreaming())
                {
                    if ((chatMessage.Content ?? "") != "")
                    {
                        chatResponseBuilder.Append(chatMessage.Content);
                        await callback(chatResponseBuilder.ToString());
                    }

                }
            }

            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, chatResponseBuilder.ToString()));
            return chatResponseBuilder.ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public void ClearHistory()
    {
        _chatCompletionsOptions.Messages.Clear();
    }
}
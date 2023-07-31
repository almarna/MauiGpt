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

    public enum AnswerType
    {
        Normal,
        Error
    }

    public async Task<(AnswerType,string)> Ask(string question, Func<string, Task> callback)
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
            return (AnswerType.Normal, chatResponseBuilder.ToString());
        }
        catch (RequestFailedException azureException)
        {
            return (AnswerType.Error, HandleAzureExceptions(azureException));
        }
        catch (Exception ex)
        {
            return (AnswerType.Error, ex.Message);
        }
    }

    private static string HandleAzureExceptions(RequestFailedException azureException)
    {
        return azureException.ErrorCode switch
        {
            "content_filter" => "Question content is inappropriate!",
            "model_not_found" => "Requested model is not available.",
            "max_tokens_exceeded" => "Input text exceeds maximum tokens allowed.",
            "input_too_long" => "Inpu t text exceeds maximum length allowed.",
            "invalid_request" => "Request format is incorrect.",
            "authentication_failed" => "Api key is invalid or expired.",

            _ => azureException.Message
        };
    }

    public void ClearHistory()
    {
        _chatCompletionsOptions.Messages.Clear();
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.OpenAI;
using MauiGpt.Dto;
using MauiGpt.Interfaces;
using System.Threading;

namespace MauiGpt.Data;

public class OpenAiService: IChatService
{
    private readonly SettingsService _settingsService;
    private OpenAIClient _openAiClient;
    private ModelsDto _currentModel;

    private readonly ChatCompletionsOptions _chatCompletionsOptions = new()
    {
        Messages =
        {
        }
    };

    public OpenAiService(SettingsService settingsService)
    {
        _settingsService = settingsService;
     }

    private void SetModelIfChanged()
    {
        var modelsDto = _settingsService.GetCurrent();

        if ((_currentModel?.IsEquivalent(modelsDto) ?? false) == false)
        {
            _currentModel = modelsDto;
            ClearHistory();

            if (_currentModel == null)
            {
                _openAiClient = null;
            }
            else
            {
                _openAiClient = new OpenAIClient(
                    new Uri(_currentModel.Endpoint),
                    new AzureKeyCredential(_currentModel.AuthKey));

//                _chatCompletionsOptions.DeploymentName = _currentModel.Model;
            }

        }
    }

    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    [SuppressMessage("ReSharper", "UseCancellationTokenForIAsyncEnumerable")]
    public async Task<(AiAnswerType,string)> Ask(string question, Func<string, Task> callback, CancellationToken cancellationToken)
    {
        SetModelIfChanged();
        try
        {
            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, question));

            var chatCompletionsResponse = await _openAiClient.GetChatCompletionsStreamingAsync(
                _currentModel.Model,
                _chatCompletionsOptions,
                cancellationToken
            );

            var fullMessage = await HandleCallback(chatCompletionsResponse, callback, cancellationToken);
            _chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, fullMessage));
            return (AiAnswerType.Normal, fullMessage);
        }
        catch (RequestFailedException azureException)
        {
            return (AiAnswerType.Error, HandleAzureExceptions(azureException));
        }
        catch (Exception ex)
        {
            return (AiAnswerType.Error, ex.Message);
        }
    }

    private static async Task<string> HandleCallback(
        Response<StreamingChatCompletions> chatCompletionsResponse,
        Func<string, Task> callback,
        CancellationToken cancellationToken)
    {
        var chatResponseBuilder = new StringBuilder();

        await foreach (var chatChoice in chatCompletionsResponse.Value.GetChoicesStreaming())
        {
            var chatMessages = chatChoice.GetMessageStreaming();
            await foreach (var chatMessage in chatMessages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((chatMessage.Content ?? "") != "")
                {
                    chatResponseBuilder.Append(chatMessage.Content);
                    await callback(chatResponseBuilder.ToString());
                }
            }
        }

        return chatResponseBuilder.ToString();
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

    public Task ClearHistory()
    {
        _chatCompletionsOptions.Messages.Clear();
        return Task.CompletedTask;
    }
}
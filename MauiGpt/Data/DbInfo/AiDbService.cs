using MauiGpt.Dto;
using MauiGpt.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace MauiGpt.Data.DbInfo;

public class AiDbService: IChatService
{
    private const string prePrompt = "As an AI specialized in data processing, I can fetch table definitions from a database, analyze the structure, and gather all necessary details. " +
                                     "For this, I just need the specific databasename and tables you are interested in.";

    private readonly IKernel _kernel;
    private ISequentialPlanner _planner;

    private AiFunctions _aiFunctions;
    private DbFunctions _dbFunctions;
    private readonly ChatMemory _chatMemory;

    public AiDbService(ModelsDto modelConfig)
    {
        _kernel = new KernelBuilder()
            .WithAzureChatCompletionService(
                modelConfig.Model,
                modelConfig.Endpoint,
                modelConfig.AuthKey,
                setAsDefault: true
        )
        .Build();

        var openAiService = new OpenAiService(modelConfig);

        _chatMemory = new ChatMemory(_kernel, prePrompt);

        _aiFunctions = new AiFunctions(openAiService, _chatMemory);
        _dbFunctions = new DbFunctions(modelConfig.ConnectionStrings, _chatMemory);

        _kernel.ImportFunctions(_aiFunctions);
        _kernel.ImportFunctions(_dbFunctions);

        _planner = new SequentialPlanner(_kernel);
    }

    public async Task<(AiAnswerType, string)> Ask(string question, Func<string, Task> callback, CancellationToken cancellationToken)
    {
        try
        {
            
            SimpleMemLogger.Reset();
            var answer = await GetAnswer(question, callback);

            var lastHistory = _chatMemory.History.Last();

            bool overrideAnswer = lastHistory.Role == AuthorRole.Assistant && answer != lastHistory.Content;
            if (overrideAnswer)
            {
                answer = lastHistory.Content;
            }

            LogHistory(_chatMemory.HistoryLog(), overrideAnswer);

            return (AiAnswerType.Normal, answer);
        }
        catch (Exception e)
        {
            return (AiAnswerType.Error, e.Message);
        }
    }

    void IChatService.ClearHistory()
    {
        _chatMemory.Reset();
        _planner = new SequentialPlanner(_kernel);
    }

    public void SetHistory(IEnumerable<ChatItemDto> history)
    {
        _chatMemory.SetHistory(history);
        _planner = new SequentialPlanner(_kernel);
    }

    public IEnumerable<ChatItemDto> GetHistory()
    {
        return _chatMemory.GetHistory();
    }

    async Task<string> GetAnswer(string question, Func<string, Task> callback)
    {
        _aiFunctions.Callback = callback;

        try
        {
            _chatMemory.AddMessage(AuthorRole.User, question);
            await callback("Creating plan...");
            var plan = await _planner.CreatePlanAsync(question);
            var last = plan.Steps.Last();
            bool lastIsLLM = last.Name == "Prompt";
            if (!lastIsLLM)
            {
                plan.AddSteps(new Plan("Send a prompt to the LLM.", _kernel.Functions.GetFunction("Prompt")));
            }
            LogPlan(plan, !lastIsLLM);
            await callback("Running plan...");

           

            var result = await _kernel.RunAsync(plan);

            var answer = result.GetValue<string>().Trim();
            return answer;
        }
        catch (Exception e) // Panikåtgärd. Kör LLM om planner inte fungerar
        {
            LogNoPlan(e.Message);
            await callback("Plan has failed. Running LLM");
            var result = await _kernel.RunAsync(_kernel.Functions.GetFunction("Prompt"), new ContextVariables(question));
            return result.GetValue<string>()!.Trim();
        }
    }

    private void LogHistory(IEnumerable<string> chatHistory, bool OverrideAnswer)
    {
        SimpleMemLogger.Log("");
        SimpleMemLogger.Log("************************** History ***************************");
        foreach (var historyItem in chatHistory)
        {
            SimpleMemLogger.Log(historyItem);
        }

        if (OverrideAnswer)
        {
            SimpleMemLogger.Log("Answer overwritten using last history.");
        }
    }

    private static void LogPlan(Plan plan, bool lastAdded)
    {
        SimpleMemLogger.Log("");
        SimpleMemLogger.Log("************************** Plan ***************************");
        foreach (var planStep in plan.Steps)
        {
            SimpleMemLogger.Log(planStep.Description);
        }

        if (lastAdded)
        {
            SimpleMemLogger.Log("(Last step added manually)");
        }
    }

    private void LogNoPlan(string message)
    {
        SimpleMemLogger.Log("");
        SimpleMemLogger.Log("Plan has failed. Running LLM manually. Message:");
        SimpleMemLogger.Log(message);

    }
}
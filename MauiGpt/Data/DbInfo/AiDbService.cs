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
    //  private string prePrompt = "You are a skillful developer. Your answers are precise and descriptive.";
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

        _chatMemory = new ChatMemory(_kernel, prePrompt);

        _aiFunctions = new AiFunctions(_kernel, _chatMemory);
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
            var answer = await GetAnswer(question);

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

    public async Task ClearHistory()
    {
        _chatMemory.Reset();
        _planner = new SequentialPlanner(_kernel);
    }

    async Task<string> GetAnswer(string question)
    {
        _chatMemory.AddMessage(AuthorRole.User, question);
        try
        {
            var plan = await _planner.CreatePlanAsync(question);
            var last = plan.Steps.Last();
            bool lastIsLLM = last.Name == "Prompt";
            if (!lastIsLLM)
            {
                plan.AddSteps(new Plan("Send a prompt to the LLM.", _kernel.Functions.GetFunction("Prompt")));
            }
            LogPlan(plan, !lastIsLLM);
            var result = await _kernel.RunAsync(plan);

            var answer = result.GetValue<string>()!.Trim();
            return answer;
        }
        catch (Exception e) // Panikåtgärd. Kör LLM om planner inte fungerar
        {
            LogNoPlan(e.Message);
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
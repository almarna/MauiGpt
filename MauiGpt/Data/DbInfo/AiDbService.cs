using MauiGpt.Data;
using MauiGpt.Data.DbInfo;
using MauiGpt.Dto;
using MauiGpt.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletionWithData;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.TemplateEngine;
using Azure.AI.OpenAI;
using static Android.Renderscripts.Script;

namespace LabSemanticKernel.Lab3;

public class AiDbService: IChatService
{
    private string prePrompt = "You are a skillful developer. Your answers are precise and descriptive.";

    private readonly IKernel _kernel;
    private ISequentialPlanner _planner;

    private readonly Dictionary<string, string> _databases = new Dictionary<string, string>();
    private AiFunctions _aiFunctions;
    private DbFunctions _dbFunctions;

    public AiDbService(SettingsService settingsService)
    {
        _databases.Add("ritz", "Data Source=(local);Initial Catalog=Ritz;User Id=sa;Password=wallaHaT7;TrustServerCertificate=true");
        _databases.Add("ritzsettings", "Data Source=(local);Initial Catalog=RitzSettings;User Id=sa;Password=wallaHaT7;TrustServerCertificate=true");

        var modelConfig = settingsService.GetCurrent();
        // var modelConfig = settingsService.GetAll().Single(item => item.Model == "gpt-4");
        //var modelConfig = settingsService.GetAll().Single(item => item.Model == "gpt3");


        //_kernel = Kernel.Builder
        //    .WithAzureChatCompletionService(
        //        "gpt-4",
        //        "https://flamingo-ailabb-openai.openai.azure.com",
        //        "677b937c689b4135a1c3fbe35d2b184e",
        //        setAsDefault: true
        //    )
        //    .Build();
       

        _kernel = new KernelBuilder()
            .WithAzureChatCompletionService(
                modelConfig.Model,
                modelConfig.Endpoint,
                modelConfig.AuthKey,
                setAsDefault: true
        )
        .Build();



        _aiFunctions = new AiFunctions(_kernel, prePrompt);
        _dbFunctions = new DbFunctions(_databases, _aiFunctions);

        _kernel.ImportFunctions(_aiFunctions);
        _kernel.ImportFunctions(_dbFunctions);

        _planner = new SequentialPlanner(_kernel);
    }

    public async Task<(AiAnswerType, string)> Ask(string question, Func<string, Task> callback, CancellationToken cancellationToken)
    {
        try
        {
            var answer = await GetAnswer(question);

            LogHistory(_aiFunctions.GetHistory());

            return (AiAnswerType.Normal, answer + SimpleMemLogger.ToString(80));
        }
        catch (Exception e)
        {
            return (AiAnswerType.Error, e.Message + SimpleMemLogger.ToString(80));
        }
    }

    public async Task ClearHistory()
    {
        _aiFunctions.ResetHistory();
//        await _kernel.RunAsync(_kernel.Functions.GetFunction("ResetHistory"));
        _planner = new SequentialPlanner(_kernel);
    }

    async Task<string> GetAnswer(string question)
    {
        _aiFunctions.AddUserMessage(question);
 //       await _kernel.RunAsync(_kernel.Functions.GetFunction("AddUserMessage"), new ContextVariables(question));
        try
        {
            var plan = await _planner.CreatePlanAsync(question);
            LogPlan(plan);
            var result = await _kernel.RunAsync(plan);

            var answer = result.GetValue<string>()!.Trim();
            return answer;
        }
        catch (Exception e) // Panikåtgärd. Kör LLM om planner inte fungerar
        {
            var result = await _kernel.RunAsync(_kernel.Functions.GetFunction("Prompt"), new ContextVariables(question));
            return result.GetValue<string>()!.Trim();
        }
    }

    private void LogHistory(IEnumerable<string> chatHistory)
    {
        SimpleMemLogger.Log("  ");
        SimpleMemLogger.Log("************************** History ***************************  ");
        foreach (var historyItem in chatHistory)
        {
            SimpleMemLogger.Log(historyItem);
        }
    }

    private static void LogPlan(Plan plan)
    {
        SimpleMemLogger.Log("  ");
        SimpleMemLogger.Log("************************** Plan ***************************  ");
        foreach (var planStep in plan.Steps)
        {
            SimpleMemLogger.Log(planStep.Description);
        }
    }
}



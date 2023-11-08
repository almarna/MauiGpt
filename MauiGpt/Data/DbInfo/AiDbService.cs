using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletionWithData;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.TemplateEngine;

namespace LabSemanticKernel.Lab3;

public class AiDbService
{


//    private string prePrompt = "You are a skillful developer. You write precise answers.";
    private string prePrompt = "You are a skillful developer.";

    private readonly IKernel _kernel;
    private ISequentialPlanner _planner;

    private Dictionary<string, string> _databases = new Dictionary<string, string>();

    public AiDbService()
    {
        _databases.Add("ritz", "Data Source=(local);Initial Catalog=Ritz;Integrated Security=True");
        _databases.Add("ritzsettings", "Data Source=(local);Initial Catalog=RitzSettings;Integrated Security=True");

        //using ILoggerFactory loggerFactory =
        //    LoggerFactory.Create(builder =>
        //        builder.AddSimpleConsole(options =>
        //        {
        //            options.IncludeScopes = true;
        //            options.SingleLine = true;
        //            options.TimestampFormat = "HH:mm:ss ";
        //        }));

        _kernel = Kernel.Builder
            //           .WithLoggerFactory(loggerFactory)
            .WithAzureChatCompletionService(
                "gpt-4",  
                "https://flamingo-ailabb-openai.openai.azure.com",
                "677b937c689b4135a1c3fbe35d2b184e",
                setAsDefault: true
            )
            .Build();

        _kernel.ImportFunctions(new AiFunctions(_kernel, prePrompt));
        _kernel.ImportFunctions(new DbFunctions(_databases));

        _planner = new SequentialPlanner(_kernel);
    }

    public async Task ResetHistoryAsync()
    {
        await _kernel.RunAsync(_kernel.Functions.GetFunction("ResetHistory"));
        _planner = new SequentialPlanner(_kernel);
    }

    public async Task<string> ExecuteAsync(string question)
    {
       var answer = await GetAnswer(question);
        return answer;
    }

    async Task<string> GetAnswer(string question)
    {
        await _kernel.RunAsync(_kernel.Functions.GetFunction("AddUserMessage"), new ContextVariables(question));
        try
        {
            var plan = await _planner.CreatePlanAsync(question);
            var result = await _kernel.RunAsync(plan);

            var answer = result.GetValue<string>()!.Trim();
            return answer;
        }
        catch (Exception e)
        {
            var result = await _kernel.RunAsync(_kernel.Functions.GetFunction("Prompt"), new ContextVariables(question));
            return result.GetValue<string>()!.Trim();
        }
    }
}



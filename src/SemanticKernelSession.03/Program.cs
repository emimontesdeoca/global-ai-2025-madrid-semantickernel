using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Net.Http.Headers;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.Machine);
var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.Machine);
var deploymentName = "gpt-4o-mini";

var builder = Kernel.CreateBuilder();

IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(deploymentName, endpoint!, apikey!);

builder.Plugins.AddFromType<AgePlugin>();

var kernel = builder.Build();

var executionSettings = new AzureOpenAIPromptExecutionSettings()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var history = new ChatHistory();

while (true)
{
    Console.Write($"Question: ");
    var prompt = Console.ReadLine();

    history.AddUserMessage(prompt!);

    var result = await chatCompletionService.GetChatMessageContentAsync(history, executionSettings, kernel);

    Console.WriteLine($"Answer: {result}");

}

internal class AgePlugin
{
    [KernelFunction]
    [Description("This function returns the age depending in the name of the user")]
    public string GetAge(string name)
    {
        switch (name)
        {
            case "John":
                return "John is 25 years old";
            case "Jane":
                return "Jane is 30 years old";
            case "Alice":
                return "Alice is 35 years old";
            default:
                return "I don't know";
        }
    }
}
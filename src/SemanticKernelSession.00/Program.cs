using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Net.Http.Headers;

Console.WriteLine("Hello Global AI Bootcamp 2025!\n");

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.User);

var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.User);

var deploymentName = "gpt-4o-mini";

var builder = Kernel.CreateBuilder();

var chatService = new AzureOpenAIChatCompletionService(deploymentName, endpoint!, apikey!);

var kernel = builder.Build();

kernel.Plugins.AddFromType<AgePlugin>();
kernel.Plugins.AddFromType<TaskPlugin>();

var promptSettings = new AzureOpenAIPromptExecutionSettings()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var history = new ChatHistory();
history.AddSystemMessage("Hablame como si estuvieramos en 1850");

while (true)
{
    Console.Write("Q:");
    var prompt = Console.ReadLine();

    history.AddUserMessage(prompt);

    var answer = await chatService.GetChatMessageContentAsync(history, promptSettings, kernel);
    Console.WriteLine($"A: {answer}");
}


internal class AgePlugin
{
    [KernelFunction]
    [Description("gets the age by providing a name")]
    public string GetAge(string name)
    {
        switch (name)
        {
            case "John":
                return "25";
            case "Jane":
                return "30";
            default:
                break;
        }

        return "Unknown";
    }
}


public class TaskPlugin
{

    [KernelFunction]
    [Description("Add a task to the list")]
    public void AddTask(string task)
    {
        TaskService.Task.Add(task);
    }

    [KernelFunction]
    [Description("Get all tasks")]
    public List<string> GetTasks()
    {
        return TaskService.Task;
    }

    [KernelFunction]
    [Description("Remove a task from the list, make sure is the correct task")]
    public void RemoveTask(string task)
    {
        TaskService.Task.Remove(task);
    }
}

public class TaskService
{
    public static List<string> Task = new();
}

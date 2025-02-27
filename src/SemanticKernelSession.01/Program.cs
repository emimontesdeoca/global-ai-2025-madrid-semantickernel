using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.Machine);
var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.Machine);

var deploymentName = "gpt-4o-mini";

var builder = Kernel.CreateBuilder();

IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(deploymentName, endpoint!, apikey!);

var prompt = "What color is the sky?";
var result = await chatCompletionService.GetChatMessageContentAsync(prompt);

Console.WriteLine($"Q: {prompt}");
Console.WriteLine($"A: {result}");
Console.ReadLine();
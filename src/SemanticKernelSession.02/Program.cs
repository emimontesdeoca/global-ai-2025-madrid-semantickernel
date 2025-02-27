using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.Machine);
var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.Machine);

var deploymentName = "gpt-4o-mini";

IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(deploymentName, endpoint!, apikey!);

var history = new ChatHistory();

while (true)
{
    Console.Write($"Question: ");
    var prompt = Console.ReadLine();

    history.AddUserMessage(prompt!);

    var result = await chatCompletionService.GetChatMessageContentAsync(history);

    Console.WriteLine($"Answer: {result}");

}
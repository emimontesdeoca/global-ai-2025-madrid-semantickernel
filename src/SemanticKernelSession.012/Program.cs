#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;

// ollama run deepseek-r1:1.5b
// ollama serve
var endpoint = "http://localhost:11434";
var model = "deepseek-r1:1.5b";

using var ollamaApiClient = new OllamaApiClient(endpoint, model);

var chatCompletionService = ollamaApiClient.AsChatCompletionService();

var prompt = "What color is the sky?";
var result = await chatCompletionService.GetChatMessageContentAsync(prompt);

Console.WriteLine($"Q: {prompt}");
Console.WriteLine($"A: {result}");
Console.ReadLine();
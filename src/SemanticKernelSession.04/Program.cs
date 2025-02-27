using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json.Serialization;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.Machine);
var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.Machine);
var deploymentName = "gpt-4o-mini";

ServiceCollection c = new();

c.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apikey!);
c.AddKernel();

IServiceProvider services = c.BuildServiceProvider();

Kernel kernel = services.GetRequiredService<Kernel>();
kernel.ImportPluginFromType<LightsPlugin>();

PromptExecutionSettings settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
IChatCompletionService chatService = services.GetRequiredService<IChatCompletionService>();

ChatHistory history = new();
while (true)
{
    Console.Write("Q: ");
    history.AddUserMessage(Console.ReadLine());

    var assistant = await chatService.GetChatMessageContentAsync(history, settings, kernel);
    history.Add(assistant);
    Console.WriteLine(assistant);
}

public class CartService()
{
    public static List<string> CartItems { get; set; } = new();

    public static void AddItem(string item) => CartItems.Add(item);
    public static void RemoveItem(string item) => CartItems.Remove(item);
    public static void ClearCart() => CartItems.Clear();
}

public class LightsPlugin
{
    private readonly List<LightModel> _lights = new();

    public LightsPlugin()
    {
        _lights.Add(new LightModel() { Id = 0, Brightness = Brightness.High, Color = "white", IsOn = false, Name = "living room" });
        _lights.Add(new LightModel() { Id = 1, Brightness = Brightness.High, Color = "red", IsOn = false, Name = "kitchen" });
        _lights.Add(new LightModel() { Id = 2, Brightness = Brightness.High, Color = "yellow", IsOn = false, Name = "garage" });
    }

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        return _lights;
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    public async Task<LightModel?> ChangeStateAsync(LightModel changeState)
    {
        // Find the light to change
        var light = _lights.FirstOrDefault(l => l.Id == changeState.Id);

        // If the light does not exist, return null
        if (light == null)
        {
            return null;
        }

        // Update the light state
        light.IsOn = changeState.IsOn;
        light.Brightness = changeState.Brightness;
        light.Color = changeState.Color;

        return light;
    }
}

public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("brightness")]
    public Brightness? Brightness { get; set; }

    [JsonPropertyName("color")]
    [Description("The color of the light with a hex code (ensure you include the # symbol)")]
    public string? Color { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Brightness
{
    Low,
    Medium,
    High
}
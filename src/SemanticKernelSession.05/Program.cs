using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Runtime.InteropServices;

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT", EnvironmentVariableTarget.Machine);
var apikey = Environment.GetEnvironmentVariable("OPENAI_APIKEY", EnvironmentVariableTarget.Machine);
var deploymentName = "gpt-4o-mini";

ServiceCollection c = new();

c.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apikey!);
c.AddKernel();

IServiceProvider services = c.BuildServiceProvider();

Kernel kernel = services.GetRequiredService<Kernel>();
kernel.ImportPluginFromType<PizzaMenuPlugin>();
kernel.ImportPluginFromType<ShoppingCartPlugin>();
kernel.ImportPluginFromType<PaymentPlugin>();

PromptExecutionSettings settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
IChatCompletionService chatService = services.GetRequiredService<IChatCompletionService>();

ChatHistory history = new();
history.AddSystemMessage("You are a member of a pizza shop place called PizzerIA, people are going to come to you asking for information, order and pay.");
while (true)
{
    Console.Write("Q: ");
    history.AddUserMessage(Console.ReadLine()!);

    var assistant = await chatService.GetChatMessageContentAsync(history, settings, kernel);
    history.Add(assistant);
    Console.WriteLine(assistant);
}

// 1. Plugin para el menú de pizzas
public class PizzaMenuPlugin
{
    [KernelFunction("get_available_pizzas")]
    [Description("Retrieves the list of available pizzas with their details")]
    public async Task<List<Pizza>> GetAvailablePizzasAsync()
    {
        return new List<Pizza>
        {
            new Pizza { Name = "Pepperoni", Ingredients = "Pepperoni, cheese, tomato sauce", Price = 12 },
            new Pizza { Name = "Margherita", Ingredients = "Tomato, mozzarella, basil", Price = 10 },
            new Pizza { Name = "Hawaiian", Ingredients = "Ham, pineapple, cheese", Price = 11 },
            new Pizza { Name = "Vegetarian", Ingredients = "Mushrooms, peppers, onions, olives", Price = 13 }
        };
    }
}

public class Pizza
{
    public string Name { get; set; } = string.Empty;
    public string Ingredients { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// 2. Plugin para el carrito de compras
public class ShoppingCartPlugin
{
    [KernelFunction("add_to_cart")]
    [Description("Adds a pizza to the shopping cart")]
    public async Task<string> AddToCartAsync(
        [Description("The name of the pizza to add, use the one we have in our list if the user typed it wrong")] string pizzaName)
    {
        CartService.AddItem(pizzaName);
        return $"{pizzaName} added to cart.";
    }

    [KernelFunction("remove_from_cart")]
    [Description("Removes a pizza from the shopping cart")]
    public async Task<string> RemoveFromCartAsync(
        [Description("The name of the pizza to remove")] string pizzaName)
    {
        bool removed = CartService.CartItems.Remove(pizzaName);
        return removed ? $"{pizzaName} removed from cart." : "Item not found in cart.";
    }

    [KernelFunction("view_cart")]
    [Description("Displays the current contents of the shopping cart")]
    public async Task<List<string>> ViewCartAsync()
    {
        return CartService.CartItems;
    }

    [KernelFunction("clear_cart")]
    [Description("Clears all items from the shopping cart")]
    public async Task<string> ClearCartAsync()
    {
        CartService.ClearCart();
        return "Cart cleared.";
    }
}

// 3. Plugin para pagos
public class PaymentPlugin
{
    [KernelFunction("process_payment")]
    [Description("Processes the payment for the current order. Calculates total automatically.")]
    public async Task<string> ProcessPaymentAsync(
        [Description("Payment method (e.g., credit, paypal)")] string paymentMethod)
    {
        var validMethods = new[] { "credit", "paypal", "debit" };
        if (!validMethods.Contains(paymentMethod.ToLower()))
            return $"Error: Payment method '{paymentMethod}' is not supported.";

        decimal total = CalculateTotal();
        if (total == 0) return "Error: Cart is empty.";

        CartService.ClearCart();
        return $"Payment of {total:C} via {paymentMethod} processed successfully!";
    }

    private decimal CalculateTotal()
    {
        var prices = new Dictionary<string, decimal>
        {
            { "Pepperoni", 12m }, { "Margherita", 10m }, { "Hawaiian", 11m }, { "Vegetarian", 13m }
        };

        var total = 0m;

        foreach (var item in CartService.CartItems)
        {
            total += prices.GetValueOrDefault(item, 0);
        }

        return total;
    }
}

public class CartService
{
    public static List<string> CartItems { get; set; } = new();
    public static void AddItem(string item) => CartItems.Add(item);
    public static void RemoveItem(string item) => CartItems.Remove(item);
    public static void ClearCart() => CartItems.Clear();
}
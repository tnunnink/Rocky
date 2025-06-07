using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Rocky.Cli.Plugins;

var builder = Kernel.CreateBuilder();

builder.Services.AddOllamaChatCompletion(
    modelId: "llama3.2:latest",
    endpoint: new Uri("http://localhost:11434"));

// Add the TagPlugin
builder.Plugins.AddFromType<TagPlugin>();

var kernel = builder.Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();
history.AddSystemMessage(@"You are a helpful assistant that can analyze PLC L5X files. 
You have access to functions that can read tag data from L5X files. 
When users ask about tags or L5X files, use the available functions to help them.");

Console.WriteLine("Rocky Chat with L5X Plugin Ready!");
Console.WriteLine("You can ask me to analyze L5X files. For example:");
Console.WriteLine("- 'Get all tags from C:\\path\\to\\file.L5X'");
Console.WriteLine("- 'Find tag Motor1_Speed in C:\\path\\to\\file.L5X'");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("Prompt: ");
    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage) || userMessage.ToLower() == "exit")
    {
        break;
    }

    history.AddUserMessage(userMessage);

    Console.Write("Rocky: ");

    // Configure execution settings to enable automatic function calling
    var settings = new OllamaPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };


    var fullResponse = "";
    await foreach (var token in chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel))
    {
        Console.Write(token.Content);
        fullResponse += token.Content;
    }

    Console.WriteLine();
    history.AddAssistantMessage(fullResponse);
}
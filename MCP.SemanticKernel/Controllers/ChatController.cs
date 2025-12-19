using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.Text;

namespace MCP.SemanticKernel.Controllers;

/// <summary>
/// credits for the base: https://medium.com/@hany.habib1988/building-a-server-sent-event-sse-mcp-server-with-net-core-c-48ac55000336
/// In this controller, we:
// Initialize an MCP client that connects to our MCP server via SseClientTransport. This will allow our client to communicate with the server using SSE.
// Fetch available tools from the MCP server
// Create a chat session with system and user messages
// Stream the response from the AI model, providing the MCP tools for function calling
// Collect and return the response
/// </summary>


[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatClient _chatClient;
    private readonly McpClient _mcpClient;
    private readonly Kernel _kernel;

    public ChatController(ILogger<ChatController> logger, IChatClient chatClient, McpClient mcpClient)
    {
        _logger = logger;
        _chatClient = chatClient;
        _mcpClient = mcpClient;
    }

    [HttpPost(Name = "Chat")]
    public async Task<string> Chat(string prompt)
    {
        // Get available tools from the MCP server
        IList<McpClientTool> tools = await _mcpClient.ListToolsAsync();

        // Set up the chat messages
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(ChatRole.User, prompt)
        };

        // Get streaming response and collect updates
        List<ChatResponseUpdate> updates = new();
        StringBuilder result = new StringBuilder();

        var options = new ChatOptions
        {
            MaxOutputTokens = 1000,
            ModelId = "claude-haiku-4-5-20251001",// "gemini-2.5-flash",
            Tools = [.. tools]
        };

        await foreach (var update in _chatClient.GetStreamingResponseAsync(
            messages,
           options
        ))
        {
            result.Append(update);
            updates.Add(update);
        }

        // Add the assistant's responses to the message history
        messages.AddMessages(updates);
        return result.ToString();
    }

    //[HttpPost(Name = "ChatFunction")]
    //public async Task<string> ChatFunction(string prompt)
    //{
    //    var result = await _kernel.InvokePromptAsync(
    //        prompt,
    //        new KernelArguments(new PromptExecutionSettings
    //        {
    //            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    //        }));

    //    return result.ToString();
    //}
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Diagnostics;

namespace MCP.SemanticKernel.Controllers;

[Route("api/[controller]")]
public class McpController : Controller
{
    private readonly Stopwatch _timer = new();
    private readonly Kernel _kernel;
    private readonly IPdfPageRenderer _pdfRenderer;
    private readonly IChatClient _chatClient;

    public McpController(Kernel kernel,
        IPdfPageRenderer pdfPageRenderer,
        IChatClient chatClient)
    {
        _kernel = kernel;
        _pdfRenderer = pdfPageRenderer;
        _chatClient = chatClient;
    }


    [HttpGet()]
    [Route("chat-tools")]
    public async Task<IActionResult> ChatWithTools(string prompt)
    {
        //var chatClient = _kernel.GetRequiredService<IChatClient>();

        //List<ChatMessage> messages = [];
        //messages.Add(new(ChatRole.User, prompt));

        //List<ChatResponseUpdate> updates = [];

        //var tools = await McpClientService.McpToolsListAsync();
        //var response = await chatClient.GetResponseAsync(messages, new() { Tools = [.. tools] });//await chat.GetChatMessageContentAsync(history);

        return Ok();
    }

    [HttpGet()]
    [Route("products")]
    public async Task<IActionResult> ChatWithProducts(string prompt)
    {
        //List<ChatMessage> messages = [];
        //messages.Add(new(ChatRole.User, prompt));

        //List<ChatResponseUpdate> updates = [];

        //var tools = await McpClientService.McpToolsListAsync();
        //var response = await _chatClient.GetResponseAsync(messages, new() { Tools = [.. tools] });//await chat.GetChatMessageContentAsync(history);

       // var products = await _context.Products.AsNoTracking().ToListAsync();

        return Ok();
    }
}

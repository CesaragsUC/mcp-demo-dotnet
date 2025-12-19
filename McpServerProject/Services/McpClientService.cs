using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpServerProject.Services;

public  class McpClientService
{
    private readonly IServiceProvider _serviceProvider;
    private McpClient _mcpClient;

    public  McpClientService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mcpClient = _serviceProvider.GetRequiredService<McpClient>();
    }

    public  async Task<IList<McpClientTool>> McpToolsListAsync()
    {
        var toolList = await _mcpClient.ListToolsAsync();
        return toolList;
    }

    public async Task CallEchoToolTest()
    {
        // Execute a tool (this would normally be driven by LLM tool invocations).
        var result = await _mcpClient.CallToolAsync(
            "echo",
            new Dictionary<string, object?>() { ["message"] = "Hello MCP!" },
            cancellationToken: CancellationToken.None);

        Console.WriteLine(result.Content.OfType<TextContentBlock>().First().Text);
    }
}
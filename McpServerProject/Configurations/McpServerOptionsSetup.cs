using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace McpServerProject.Configurations;

public static class McpServerOptionsSetup
{
    public static McpServerOptions ConfigureMcpServerOptions()
    {
        McpServerOptions options = new()
        {
            ServerInfo = new Implementation { Name = "MyServer", Version = "1.0.0" },
            Handlers = new McpServerHandlers()
            {
                ListToolsHandler = (request, cancellationToken) =>
                    ValueTask.FromResult(new ListToolsResult
                    {
                        Tools =
                        [
                            new Tool
                                {
                                    Name = "echo",
                                    Description = "Echoes the input back to the client.",
                                    InputSchema = JsonSerializer.Deserialize<JsonElement>("""
                                        {
                                            "type": "object",
                                            "properties": {
                                              "message": {
                                                "type": "string",
                                                "description": "The input to echo back"
                                              }
                                            },
                                            "required": ["message"]
                                        }
                                        """),
                                }
                        ]
                    }),

                CallToolHandler = (request, cancellationToken) =>
                {
                    if (request.Params?.Name == "echo")
                    {
                        if (request.Params.Arguments?.TryGetValue("message", out var message) is not true)
                        {
                            throw new McpProtocolException("Missing required argument 'message'", McpErrorCode.InvalidParams);
                        }

                        return ValueTask.FromResult(new CallToolResult
                        {
                            Content = [new TextContentBlock { Text = $"Echo: {message}" }]
                        });
                    }

                    throw new McpProtocolException($"Unknown tool: '{request.Params?.Name}'", McpErrorCode.InvalidRequest);
                }
            }
        };

        return options;
    }
}


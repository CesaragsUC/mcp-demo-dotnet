using Anthropic;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using MCP.SemanticKernel;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using Serilog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// Documentation MCP https://github.com/modelcontextprotocol/csharp-sdk?tab=readme-ov-file
/// Doc Samples: https://github.com/modelcontextprotocol/csharp-sdk/tree/main/samples

var builder = WebApplication.CreateBuilder(args);

// Start Serilog configuration
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console() // Write logs to console
    .WriteTo.Seq(builder.Configuration["Serilog:SeqServerUrl"] ?? "http://localhost:5341") // Write to Seq server
    .CreateLogger();

Log.Logger = logger;

var geminiHttpClient = GeminiHttpClientHelper.CreateGeminiHttpClient(ignoreSslErrors: builder.Environment.IsDevelopment());


builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var kernelBuilder = Kernel.CreateBuilder();

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IPdfPageRenderer, PdfPageRenderer>();


builder.Services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace));

builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fundamentos AI Embeddings", Version = "v1" }));

var envType = builder.Environment.IsDevelopment();
var envTypeName = builder.Environment.EnvironmentName.ToLower();

var geminiApiKey = builder.Configuration["GOOGLE_API_KEY"]; // esse valor vem das variaveis de ambiente do sistema
var anthropicApiKey = builder.Configuration["ANTHROPIC_API_KEY"];

var geminiClient = new GeminiChatClient(new GeminiClientOptions
{
    ApiKey = geminiApiKey,
    ModelId = "gemini-2.5-flash"
});

using var anthropicClient = new AnthropicClient(new() { APIKey = builder.Configuration["ANTHROPIC_API_KEY"] })
    .AsIChatClient("claude-haiku-4-5-20251001")
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

builder.Services.AddChatClient(anthropicClient);


IClientTransport clientTransport;
var (command, arguments) = GetCommandAndArguments(args);

if (command == "http")
{
    // make sure AspNetCoreMcpServer is running
    clientTransport = new HttpClientTransport(new()
    {
        Endpoint = new Uri("http://localhost:5078")
    });
}
else
{
    clientTransport = new StdioClientTransport(new()
    {
        Name = "Demo Server",
        Command = command,
        Arguments = arguments,
    });
}

await using var mcpClient = await McpClient.CreateAsync(clientTransport!);
builder.Services.AddSingleton(mcpClient);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


/// <summary>
/// Determines the command (executable) to run and the script/path to pass to it. This allows different
/// languages/runtime environments to be used as the MCP server.
/// </summary>
/// <remarks>
/// This method uses the file extension of the first argument to determine the command, if it's py, it'll run python,
/// if it's js, it'll run node, if it's a directory or a csproj file, it'll run dotnet.
///
/// If no arguments are provided, it defaults to running the QuickstartWeatherServer project from the current repo.
///
/// This method would only be required if you're creating a generic client, such as we use for the quickstart.
/// </remarks>
static (string command, string[] arguments) GetCommandAndArguments(string[] args)
{

    var slnPath = Directory.GetParent(GetCurrentSourceDirectory())!.FullName;
    var serverCsprojPath = Path.Combine(
        slnPath,
        "McpServerProject"
    );

    return args switch
    {
        [var mode] when mode.Equals("http", StringComparison.OrdinalIgnoreCase) => ("http", args),
        [var script] when script.EndsWith(".py") => ("python", args),
        [var script] when script.EndsWith(".js") => ("node", args),
        [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script]),
        _ => ("dotnet", ["run", "--project", Path.Combine(GetCurrentSourceDirectory(serverCsprojPath), "McpServerProject")])
    };
}

static string GetCurrentSourceDirectory([CallerFilePath] string? currentFile = null)
{
    Debug.Assert(!string.IsNullOrWhiteSpace(currentFile));
    return Path.GetDirectoryName(currentFile) ?? throw new InvalidOperationException("Unable to determine source directory.");
}

// To create this kind of tempalte project: dotnet new web -n McpServer
// It will Create a new web application without controllers

using McpServerProject.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

var anthropicApiKey = builder.Configuration["ANTHROPIC_API_KEY"];

var envType = builder.Environment.IsDevelopment();
var envTypeName = builder.Environment.EnvironmentName.ToLower();


builder.Services.AddDbContextFactory<AppEmbeddingDbContext>(options =>
{

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseVector();
    })
    .EnableSensitiveDataLogging(false)  // Desabilitar dados sensíveis
    .EnableDetailedErrors(false)        // Desabilitar erros detalhados
    .LogTo(Console.WriteLine, LogLevel.None); // Desabilitar logs completamente
});

// Mcp Server setup
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(); // busca todas as classes que tem anotation McpServerToolType


using var httpClient = new HttpClient { BaseAddress = new Uri("https://api.weather.gov") };
httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
builder.Services.AddSingleton(httpClient);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


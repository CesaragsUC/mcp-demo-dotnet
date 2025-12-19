# MCP Server POC - Product Catalog with Semantic Search

A Proof of Concept (POC) demonstrating the Model Context Protocol (MCP) for building AI-powered tools with .NET, PostgreSQL, and Claude AI.

## 🎯 Overview

This project showcases how to create MCP-compliant tools that expose database operations and APIs to Large Language Models (LLMs) like Claude. It implements a product catalog system with semantic search capabilities using PostgreSQL with pgvector extension.

### Key Features

- 🔧 **MCP Server Implementation** - Expose .NET functions as MCP tools
- 🗄️ **PostgreSQL + pgvector** - Vector embeddings for semantic search
- 🤖 **Claude Integration** - AI-powered product recommendations
- 📦 **Product Management** - CRUD operations via conversational interface
- 🌐 **RESTful API** - HTTP endpoints for testing and integration
- 🐳 **Docker Support** - Containerized PostgreSQL with pgvector

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Claude AI / MCP Client                                  │
│  (Claude Desktop, API, or Custom Client)                 │
└────────────────┬────────────────────────────────────────┘
                 │ stdio/HTTP (MCP Protocol)
                 ↓
┌─────────────────────────────────────────────────────────┐
│  MCP Server (.NET 8)                                     │
│  ┌────────────────────────────────────────────────────┐ │
│  │  MCP Tools                                         │ │
│  │  - ProductTool: Get/Search products                │ │
│  │  - WeatherTool: Weather alerts (example)           │ │
│  │  - SemanticSearchTool: Vector similarity search    │ │
│  └────────────────────────────────────────────────────┘ │
└────────────────┬────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────┐
│  PostgreSQL 16 + pgvector                                │
│  - Products table                                        │
│  - Documents table (with embeddings)                     │
└─────────────────────────────────────────────────────────┘
```

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL)
- [Claude API Key](https://console.anthropic.com/) (optional, for testing)
- WSL2 (Windows) or Docker (Linux/Mac)

## 🚀 Getting Started

### 1. Clone and Setup

```bash
git clone <your-repo-url>
cd MCP.SemanticKernel
```

### 2. Start PostgreSQL with pgvector

```bash
docker-compose up -d
```

Or manually:

```bash
docker run -d \
  --name postgres-pgvector \
  -e POSTGRES_DB=AppEmbeddingDb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

### 3. Configure Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AppEmbeddingDb;Username=postgres;Password=postgres;Include Error Detail=true"
  },
  "ANTHROPIC_API_KEY": "sk-ant-api03-your-key-here"
}
```

**Or use User Secrets (recommended):**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=AppEmbeddingDb;Username=postgres;Password=postgres"
dotnet user-secrets set "ANTHROPIC_API_KEY" "sk-ant-api03-your-key-here"
```

### 4. Run Database Migrations

```bash
dotnet ef database update
```

Or create manually:

```sql
-- Connect to PostgreSQL
psql -U postgres -d AppEmbeddingDb

-- Create extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create products table (simplified example)
CREATE TABLE products (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    price DECIMAL(10,2),
    category VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create documents table for semantic search
CREATE TABLE documents (
    id SERIAL PRIMARY KEY,
    content TEXT NOT NULL,
    embedding vector(1536),
    source VARCHAR(500),
    category VARCHAR(100),
    chunk_index INTEGER,
    created_at TIMESTAMP DEFAULT NOW(),
    metadata JSONB
);

-- Create vector index for fast similarity search
CREATE INDEX ON documents USING ivfflat (embedding vector_cosine_ops);
```

### 5. Seed Sample Data

```bash
dotnet run --seed
```

Or insert manually:

```sql
INSERT INTO products (id, name, description, price, category) VALUES
(gen_random_uuid(), 'Smartphone XYZ', 'Features stunning display, powerful processor, and advanced camera system', 699.99, 'Electronics'),
(gen_random_uuid(), 'Wireless Headphones', 'Superior sound, noise cancellation, and long battery life', 199.99, 'Electronics'),
(gen_random_uuid(), 'Gaming Monitor', 'The Gaming Monitor delivers stunning visuals and fast refresh rates for an immersive gaming experience', 399.99, 'Games'),
(gen_random_uuid(), 'Gaming Keyboard', 'Customizable with RGB lighting', 129.99, 'Games'),
(gen_random_uuid(), 'Smart Refrigerator', 'Advanced cooling and remote control', 1199.99, 'Home');
```

### 6. Run the MCP Server

```bash
dotnet run
```

The server will start on `http://localhost:5110`

## 🧪 Testing the MCP Server

### Option 1: Swagger UI (HTTP/REST)

Navigate to: `http://localhost:5110/swagger`

Test the `/Chat` endpoint:
```json
{
  "prompt": "Get all products"
}
```

### Option 2: cURL

```bash
# Get all products
curl -X POST http://localhost:5110/Chat \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Get all products"}'

# Product recommendation
curl -X POST http://localhost:5110/Chat \
  -H "Content-Type: application/json" \
  -d '{"prompt": "I want a monitor for gaming, what would you suggest?"}'

# Weather tool example
curl -X POST http://localhost:5110/Chat \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Get weather alerts for TX"}'

# Health check
curl http://localhost:5110/health
```

### Option 3: Claude Desktop Integration

**Configure Claude Desktop** (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "product-catalog": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/path/to/your/MCP.SemanticKernel/MCP.SemanticKernel.csproj"
      ]
    }
  }
}
```

**Config file locations:**
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

Then in Claude Desktop:
```
User: "Show me all available gaming products"
Claude: [calls get_all_products tool automatically]
```

## 📚 Available MCP Tools

### ProductTool

**get_all_products**
- Description: Retrieves all products from the catalog
- Parameters: None
- Returns: JSON array of products

**get_product_by_id**
- Description: Retrieves a specific product by ID
- Parameters:
  - `id` (int): The product ID
- Returns: Product details or "Product not found"

### WeatherTool

**get_alerts**
- Description: Get weather alerts for a US state
- Parameters:
  - `state` (string): Two-letter state abbreviation (e.g., "TX", "NY")
- Returns: Active weather alerts or "No active alerts"

### SemanticSearchTool (Future Enhancement)

**semantic_search**
- Description: Search documents using semantic similarity
- Parameters:
  - `query` (string): Search query
  - `topK` (int, optional): Number of results (default: 5)
  - `category` (string, optional): Filter by category
- Returns: Most relevant documents with similarity scores

## 🏗️ Project Structure

```
MCP.SemanticKernel/
├── Infrastructure/
│   └── AppEmbeddingDbContext.cs      # Entity Framework DbContext
├── Models/
│   ├── Products.cs                    # Product entity
│   └── Document.cs                    # Document entity (for RAG)
├── PluginFunctions/
│   └── ProductPlugin.cs               # Semantic Kernel functions (legacy)
├── Tools/
│   ├── ProductTool.cs                 # MCP tool for products
│   ├── WeatherTool.cs                 # MCP tool for weather
│   └── SemanticSearchTool.cs          # MCP tool for vector search
├── Program.cs                         # Application entry point
├── appsettings.json                   # Configuration
└── docker-compose.yml                 # PostgreSQL + pgvector setup
```

## 🔧 Configuration

### Model Selection

The project uses **Claude Haiku 4.5** by default (cost-effective for POCs):

```csharp
var kernel = Kernel.CreateBuilder()
    .AddAnthropicChatCompletion(
        modelId: "claude-haiku-4-5-20251001",  // Fast and cheap
        apiKey: anthropicApiKey)
    .Build();
```

**Available models:**
- `claude-haiku-4-5-20251001` - Fast, cheap ($0.80/$4.00 per 1M tokens)
- `claude-sonnet-4-5-20250929` - Balanced ($3/$15 per 1M tokens)
- `claude-opus-4-20250514` - Most capable ($15/$75 per 1M tokens)

### Database Configuration

**Default connection string:**
```
Host=localhost;Port=5432;Database=AppEmbeddingDb;Username=postgres;Password=postgres
```

**For production**, use:
- Azure PostgreSQL with pgvector
- AWS RDS PostgreSQL with pgvector
- Connection pooling (Npgsql)
- Encrypted connections (SSL)

## 🎯 Example Conversations

### Product Search
```
User: "I want a monitor for gaming, what would you suggest?"

Claude: I'll retrieve the product list for you so I can see what gaming 
monitors are available. Great! I found a gaming monitor in the product 
list for you:

**Gaming Monitor** (ID: b7770738-9d3f-486e-a6ee-283302063cf2)
- **Price:** $399.99
- **Category:** Games
- **Description:** The Gaming Monitor delivers stunning visuals and fast 
  refresh rates for an immersive gaming experience. Perfect for gamers 
  who demand the best.

This would be my recommendation for you! It's specifically designed for 
gaming with stunning visuals and fast refresh rates...
```

### Multi-turn Conversation
```
User: "Show me all products under $200"
Claude: [filters and shows products]

User: "Which one is best for gaming?"
Claude: [remembers context, recommends from filtered list]

User: "Add a keyboard to match"
Claude: [suggests gaming keyboard from catalog]
```

## 📊 Performance Considerations

### Token Usage (Claude Haiku)
- Average request: 500 input tokens + 500 output tokens
- Cost per request: ~$0.0026
- 1M requests/month: ~$2,600

### Database Performance
- pgvector with IVFFlat index: ~100ms for 1M documents
- Connection pooling: 100 concurrent connections
- Recommended: Add caching layer (Redis) for frequent queries

### Scalability
- MCP Server: Stateless, can scale horizontally
- PostgreSQL: Vertical scaling or read replicas
- Consider: Azure PostgreSQL Flexible Server with auto-scaling

## 🔐 Security Best Practices

### API Keys
✅ Use User Secrets for development
✅ Use Azure Key Vault for production
✅ Never commit secrets to git

### Database
✅ Use connection pooling
✅ Enable SSL/TLS connections
✅ Implement row-level security (RLS)
✅ Regular backups

### MCP Server
✅ Input validation on all tools
✅ Rate limiting (optional)
✅ Audit logging for tool calls
✅ Authentication/authorization (if exposing publicly)

## 🐛 Troubleshooting

### Docker/PostgreSQL Issues

**Problem**: MCP server can't connect to database
```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Check logs
docker logs postgres-pgvector

# Restart Docker Desktop
# Restart WSL (Windows)
wsl --shutdown
```

**Problem**: pgvector extension not found
```sql
-- Connect to database
psql -U postgres -d AppEmbeddingDb

-- Create extension
CREATE EXTENSION vector;

-- Verify
\dx
```

### MCP Tool Not Found

**Problem**: Claude can't find your tools

1. Check tool registration:
```csharp
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();  // ← Must be present
```

2. Verify tool attributes:
```csharp
[McpServerToolType]  // ← On class
public sealed class ProductTool
{
    [McpServerTool, Description("...")]  // ← On method
    public async Task<string> GetAllProducts() { }
}
```

3. Check Claude Desktop config path
4. Restart Claude Desktop after config changes

### Common Errors

**"The 'get_all_products' function encountered an issue"**
- Usually a database connection issue
- Check connection string
- Verify PostgreSQL is running
- Check firewall rules

**"SKEXP0070 warning"**
- This is expected with the community Anthropic connector
- Suppress with: `#pragma warning disable SKEXP0070`

## 🚀 Next Steps

### Enhancements to Consider

1. **Vector Search (RAG)**
   - Implement document ingestion pipeline
   - Add semantic search tool
   - Support PDF, Word, HTML documents

2. **Advanced Product Features**
   - Product comparisons
   - Bundle recommendations
   - Price tracking and alerts
   - Inventory management

3. **Authentication & Authorization**
   - Azure AD / Okta integration
   - Role-based access control (RBAC)
   - API key management

4. **Monitoring & Observability**
   - Application Insights
   - Prometheus + Grafana
   - Request/response logging
   - Token usage tracking

5. **Production Deployment**
   - Azure App Service
   - Docker/Kubernetes
   - CI/CD pipelines
   - Auto-scaling

## 📖 Resources

### MCP (Model Context Protocol)
- [MCP Documentation](https://modelcontextprotocol.io/)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Claude Desktop Integration](https://docs.anthropic.com/claude/docs)

### Semantic Kernel
- [Semantic Kernel Docs](https://learn.microsoft.com/en-us/semantic-kernel/)
- [SK GitHub](https://github.com/microsoft/semantic-kernel)

### PostgreSQL + pgvector
- [pgvector Extension](https://github.com/pgvector/pgvector)
- [Npgsql Provider](https://www.npgsl.org/)

### Claude AI
- [Claude API Docs](https://docs.anthropic.com/)
- [Anthropic Console](https://console.anthropic.com/)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

**Cesar Augusto**  
Software Developer @ Vizient  
Healthcare Technology Solutions

---

## 🎓 Key Learnings from this POC

1. **MCP > Semantic Kernel Functions** (for external tools)
   - Simpler API surface
   - Language-agnostic
   - Better for reusability

2. **Hybrid Architecture Works Best**
   - Semantic Kernel for orchestration
   - MCP for tool exposure
   - PostgreSQL + pgvector for data

3. **Claude is Smart About Tool Calling**
   - Understands context across turns
   - Makes intelligent recommendations
   - Handles errors gracefully

4. **pgvector is Production-Ready**
   - Fast similarity search (< 100ms)
   - Scales to millions of vectors
   - Integrates seamlessly with EF Core

## 🔮 Future: MCP vs Semantic Kernel?

**Short answer**: Both will coexist

- **MCP**: Standard for tool exposure (like REST for APIs)
- **Semantic Kernel**: Framework for complex orchestration
- **Best Practice**: Use both together

This POC demonstrates the hybrid approach successfully!

---

**Questions?** Open an issue or reach out!

**⭐ If this helped you, consider starring the repo!**

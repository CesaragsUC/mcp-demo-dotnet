
using McpServerProject.Dto;
using McpServerProject.Entities;
using McpServerProject.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpServerProject.Tools;

/// <summary>
/// Para usar MCP precisa usar VS Code.  No Visual Studio ainda não tem suporte nativo.
/// </summary>

[McpServerToolType]
public sealed class ProductTool
{
    private readonly IDbContextFactory<AppEmbeddingDbContext> _factory;

    // Construtor para injeção de dependência
    public ProductTool(IDbContextFactory<AppEmbeddingDbContext> factory)
    {
        _factory = factory;
    }

    //prompt: get all products
    [McpServerTool, Description("Get all products")]
    public async Task<List<ProductsResponse>> GetAllProducts() 
    {
        await using var context = await _factory.CreateDbContextAsync();
        var result = await context.Products.AsNoTracking().ToListAsync();
        return result.Select(p => new ProductsResponse
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Description = p.Description
        }).ToList();
    }

    [McpServerTool, Description("Get product by ID")]
    public async Task<ProductsResponse?> GetProductById(
        [Description("The product ID")] Guid id)  
    {
        await using var context = await _factory.CreateDbContextAsync();
        var result = await context.Products.FirstOrDefaultAsync(x=> x.Id == id);
        return result == null ? null : new ProductsResponse
        {
            Id = result.Id,
            Name = result.Name,
            Category = result.Category,
            Price = result.Price,
            Description = result.Description
        };
    }

    [McpServerTool, Description("Update product by ID")]
    public async Task<ProductsResponse?> UpdateProductById(
        [Description("The product ID")] Guid id,
        [Description("The updated product")] ProductsUpdateRequest updatedProduct)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existingProduct = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (existingProduct == null) return null;

        existingProduct.Description = updatedProduct.Description;
        existingProduct.Price = updatedProduct.Price;
        existingProduct.Category = updatedProduct.Category;
        existingProduct.Name = updatedProduct.Name;

        // Update the existing product with the new values
        context.Products.Update(existingProduct);
        await context.SaveChangesAsync();
        return new ProductsResponse
        {
            Id = existingProduct.Id,
            Name = existingProduct.Name,
            Category = existingProduct.Category,
            Price = existingProduct.Price,
            Description = existingProduct.Description
        };
    }

    [McpServerTool, Description("Get product recommendations based on budget and preferences")]
    public async Task<string> GetRecommendations(
    [Description("Product category (Electronics, Games, Home, Footwear)")] string category,
    [Description("Maximum budget")] decimal maxBudget,
    [Description("Optional: specific features or keywords")] string? preferences = null)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var query = context.Products
            .Where(p => p.Category == category && p.Price <= maxBudget)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(preferences))
        {
            query = query.Where(p => EF.Functions.ILike(p.Description, $"%{preferences}%"));
        }

        var products = await query
            .OrderByDescending(p => p.Price) // Melhores primeiro
            .ToListAsync();

        return JsonSerializer.Serialize(new
        {
            budget = maxBudget,
            category,
            found = products.Count,
            recommendations = products
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get product bundle suggestions for a complete gaming setup")]
    public async Task<string> GetGamingBundle(
    [Description("Total budget for the bundle")] decimal totalBudget)
    {
        await using var context = await _factory.CreateDbContextAsync();

        // Pegar produtos de gaming
        var gamingProducts = await context.Products
            .Where(p => p.Category == "Games")
            .OrderBy(p => p.Price)
            .ToListAsync();

        // Criar bundles diferentes
        var bundles = new List<object>();

        // Bundle essencial
        var essentials = new[]
        {
        gamingProducts.FirstOrDefault(p => p.Name.Contains("Monitor")),
        gamingProducts.FirstOrDefault(p => p.Name.Contains("Keyboard")),
        gamingProducts.FirstOrDefault(p => p.Name.Contains("Mouse"))
    }.Where(p => p != null).ToList();

        var essentialTotal = essentials.Sum(p => p!.Price);

        if (essentialTotal <= totalBudget)
        {
            bundles.Add(new
            {
                name = "Essential Gaming Bundle",
                total = essentialTotal,
                savings = totalBudget - essentialTotal,
                products = essentials
            });
        }

        // Bundle completo
        var complete = gamingProducts.Where(p =>
            p.Name.Contains("Monitor") ||
            p.Name.Contains("Keyboard") ||
            p.Name.Contains("Mouse") ||
            p.Name.Contains("Headset") ||
            p.Name.Contains("Chair")).ToList();

        var completeTotal = complete.Sum(p => p.Price);

        if (completeTotal <= totalBudget)
        {
            bundles.Add(new
            {
                name = "Complete Gaming Setup",
                total = completeTotal,
                savings = totalBudget - completeTotal,
                products = complete
            });
        }

        return JsonSerializer.Serialize(new
        {
            budget = totalBudget,
            bundles_available = bundles.Count,
            bundles
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Compare multiple products by their IDs")]
    public async Task<string> CompareProducts(
    [Description("Comma-separated product IDs to compare")] string productIds)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var ids = productIds.Split(',').Select(id => id.Trim()).ToList();

        var products = await context.Products
            .Where(p => ids.Contains(p.Id.ToString()))
            .ToListAsync();

        return JsonSerializer.Serialize(new
        {
            comparison = new
            {
                products_compared = products.Count,
                price_range = new
                {
                    min = products.Min(p => p.Price),
                    max = products.Max(p => p.Price),
                    average = products.Average(p => p.Price)
                },
                categories = products.Select(p => p.Category).Distinct(),
                details = products.Select(p => new
                {
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Description
                })
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}


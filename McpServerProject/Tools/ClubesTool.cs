using McpServerProject.Dto;
using McpServerProject.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpServerProject.Tools;

[McpServerToolType]
public sealed class ClubesTool
{
    private readonly IDbContextFactory<AppEmbeddingDbContext> _factory;

    // Construtor para injeção de dependência
    public ClubesTool(IDbContextFactory<AppEmbeddingDbContext> factory)
    {
        _factory = factory;
    }


    [McpServerTool, Description("Get all football clubs")]
    public async Task<List<ClubeResponse>> GetAllFootbalClubes()
    {
        await using var context = await _factory.CreateDbContextAsync();
        var result = await context.Clubes.AsNoTracking().ToListAsync();
        return result.Select(p => new ClubeResponse
        {
            Id = p.Id,
            Name = p.Name,
            Country = p.Country,
            State = p.State,
            Description = p.Description
        }).ToList();
    }

    [McpServerTool, Description("Get football club by ID")]
    public async Task<ClubeResponse?> GetFootbalClubeById(
        [Description("The club ID")] Guid id) 
    {
        await using var context = await _factory.CreateDbContextAsync();
        var result = await context.Clubes.FirstOrDefaultAsync(x => x.Id == id);
        return result == null ? null : new ClubeResponse    
        {
            Id = result.Id,
            Name = result.Name,
            Country = result.Country,
            State = result.State,
            Description = result.Description
        };
    }

    [McpServerTool, Description("Update football club by ID")]
    public async Task<ClubeResponse?> UpdateFootbalClubeById(
        [Description("The club ID")] Guid id,
        [Description("The updated club")] ClubeUpdateRequest updatedClube)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existingClube = await context.Clubes.FirstOrDefaultAsync(x => x.Id == id);
        if (existingClube == null) return null;

        existingClube.Description = updatedClube.Description;
        existingClube.Name = updatedClube.Name;
        existingClube.Country = updatedClube.Country;
        existingClube.State = updatedClube.State;

        // Update the existing club with the new values
        context.Clubes.Update(existingClube);
        await context.SaveChangesAsync();
        return new ClubeResponse
        {
            Id = existingClube.Id,
            Name = existingClube.Name,
            Country = existingClube.Country,
            State = existingClube.State,
            Description = existingClube.Description
        };
    
    }

    [McpServerTool, Description("Get the best football club based on the country and history")]
    public async Task<string> GetRecommendations(
    [Description("The country of the football club")] string country,
    [Description("The historical performance criteria")] string historyCriteria)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var query = context.Clubes.AsNoTracking();

        if (!string.IsNullOrEmpty(country))
        {
            query = query.Where(c => c.Country == country);
        }

        if (!string.IsNullOrEmpty(historyCriteria))
        {
            query = query.Where(c => EF.Functions.ILike(c.Description, $"%{historyCriteria}%"));
        }

        var clubes = await query.ToListAsync();

        return JsonSerializer.Serialize(new
        {
            country,
            historyCriteria,
            found = clubes.Count,
            recommendations = clubes
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}

namespace McpServerProject.Dto;

public sealed class ProductsUpdateRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

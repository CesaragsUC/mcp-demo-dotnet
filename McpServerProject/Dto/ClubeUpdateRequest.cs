namespace McpServerProject.Dto;

public class ClubeUpdateRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string Description { get; set; }
}

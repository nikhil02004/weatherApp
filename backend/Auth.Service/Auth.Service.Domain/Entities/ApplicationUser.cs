namespace Auth.Service.Domain.Entities;

public class ApplicationUser
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? Email { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
}

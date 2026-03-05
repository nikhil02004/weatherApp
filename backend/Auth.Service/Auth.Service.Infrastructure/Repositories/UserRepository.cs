using Auth.Service.Application.Interfaces;
using Auth.Service.Domain.Entities;
using Auth.Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Service.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public ApplicationUser? FindByUsername(string username)
    {
        try
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in {Method} while looking up username '{Username}'.",
                nameof(FindByUsername), username);
            throw;
        }
    }

    // Returns true when the INSERT succeeds, false on duplicate username/email.
    public bool TryAddUser(ApplicationUser user)
    {
        try
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            _logger.LogWarning(
                "Duplicate key in {Method} for username '{Username}'.",
                nameof(TryAddUser), user.Username);
            // Detach the failed entity so the context remains usable
            _context.Entry(user).State = EntityState.Detached;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in {Method} while adding user '{Username}'.",
                nameof(TryAddUser), user.Username);
            throw;
        }
    }

    public ApplicationUser? FindByEmail(string email)
    {
        try
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in {Method} for email '{Email}'.",
                nameof(FindByEmail), email);
            throw;
        }
    }

    public ApplicationUser? FindByExternalId(string provider, string externalId)
    {
        try
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.ExternalProvider == provider && u.ExternalId == externalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in {Method} for provider '{Provider}' / externalId '{ExternalId}'.",
                nameof(FindByExternalId), provider, externalId);
            throw;
        }
    }

    // SQL Server error numbers 2601 and 2627 indicate unique-constraint / primary-key violations.
    private static bool IsDuplicateKeyException(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("UNIQUE KEY",  StringComparison.OrdinalIgnoreCase) == true
        || ex.InnerException?.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) == true;
}

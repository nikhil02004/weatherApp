using Auth.Service.Domain.Entities;

namespace Auth.Service.Application.Interfaces;

public interface IUserRepository
{
    ApplicationUser? FindByUsername(string username);
    ApplicationUser? FindByEmail(string email);
    ApplicationUser? FindByExternalId(string provider, string externalId);
    bool TryAddUser(ApplicationUser user);
}

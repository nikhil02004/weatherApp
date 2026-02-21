using Auth.Service.Domain.Entities;

namespace Auth.Service.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}

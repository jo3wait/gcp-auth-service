using AuthService.Domain;

namespace AuthService.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

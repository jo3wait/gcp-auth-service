using AuthService.Application.DTOs;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces;

public interface IMemberService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

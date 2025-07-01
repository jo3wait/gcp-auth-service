using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using System.Threading.Tasks;

namespace AuthService.Application.Services;

public class MemberService : IMemberService
{
    private readonly IUserRepository _users;
    private readonly IEncryptionService _crypto;
    private readonly IJwtService _jwt;

    public MemberService(IUserRepository users, IEncryptionService crypto, IJwtService jwt)
    {
        _users = users;
        _crypto = crypto;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing is not null)
            return new(false, "Email already registered");

        var (salt, hash, mac, ver) = _crypto.Hash(request.Password);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            PasswordMac = mac,
            KmsKeyVersion = ver
        };

        await _users.AddAsync(user);
        var token = _jwt.GenerateToken(user);
        return new(true, "Registered", token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email);
        if (user is null)
            return new(false, "Invalid credentials");

        bool result = _crypto.Verify(request.Password,
                                     user.PasswordSalt,
                                     user.PasswordHash,
                                     user.PasswordMac,
                                     user.KmsKeyVersion);

        return result ?
            new(true, "Authenticated", _jwt.GenerateToken(user)) :
            new(false, "Invalid credentials");
    }
}

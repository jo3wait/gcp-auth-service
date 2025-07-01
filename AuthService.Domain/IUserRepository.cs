using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

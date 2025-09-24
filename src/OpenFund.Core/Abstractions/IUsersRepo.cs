namespace OpenFund.Core.Abstractions;

public interface IUsersRepo
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
}

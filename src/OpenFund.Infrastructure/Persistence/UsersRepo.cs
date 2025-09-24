namespace OpenFund.Infrastructure.Persistence;

internal sealed class UsersRepo(OpenFundDbContext db) : IUsersRepo
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
}

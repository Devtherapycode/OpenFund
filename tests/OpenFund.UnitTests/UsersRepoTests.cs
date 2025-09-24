namespace OpenFund.UnitTests;

public class UsersRepoTests
{
    private static OpenFundDbContext NewDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<OpenFundDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new OpenFundDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        using var db = NewDb(nameof(AddAsync_PersistsUser));
        var repo = new UsersRepo(db);
        var u = new User { Email = "a@mail.com", DisplayName = "A", PasswordHash = "h" };

        var saved = await repo.AddAsync(u, CancellationToken.None);

        saved.Should().NotBeNull();
        (await db.Users.CountAsync()).Should().Be(1);
        (await db.Users.FirstAsync()).Email.Should().Be("a@mail.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_IfExists()
    {
        using var db = NewDb(nameof(GetByEmailAsync_ReturnsUser_IfExists));
        db.Users.Add(new User { Email = "x@mail.com", DisplayName = "X", PasswordHash = "h" });
        await db.SaveChangesAsync();

        var repo = new UsersRepo(db);
        var found = await repo.GetByEmailAsync("x@mail.com");

        found.Should().NotBeNull();
        found!.DisplayName.Should().Be("X");
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_IfMissing()
    {
        using var db = NewDb(nameof(GetByEmailAsync_ReturnsNull_IfMissing));
        var repo = new UsersRepo(db);

        var found = await repo.GetByEmailAsync("none@mail.com");

        found.Should().BeNull();
    }
}

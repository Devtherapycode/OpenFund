namespace OpenFund.UnitTests;

public class AuthServiceTests
{
    private static (Mock<IUsersRepo> users, Mock<IJwtProvider> jwt, Mock<IPasswordHasher<User>> hasher, AuthService sut)
        BuildSut()
    {
        var users = new Mock<IUsersRepo>(MockBehavior.Strict);
        var jwt = new Mock<IJwtProvider>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);

        var sut = new AuthService(users.Object, jwt.Object, hasher.Object, UserManager<User>());
        return (users, jwt, hasher, sut);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsInvalidOperation()
    {
        var (users, jwt, hasher, sut) = BuildSut();
        users.Setup(r => r.GetByEmailAsync("taken@mail.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(new User { Email = "taken@mail.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync(new RegisterRequest("taken@mail.com", "x", "Name")));

        users.VerifyAll();
        jwt.VerifyNoOtherCalls();
        hasher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword_SavesUser_ReturnsToken()
    {
        var (users, jwt, hasher, sut) = BuildSut();
        var email = "new@mail.com";
        var req = new RegisterRequest(email, "P@ssw0rd!", "New User");

        users.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
             .ReturnsAsync((User?)null);

        hasher.Setup(h => h.HashPassword(It.IsAny<User>(), req.Password))
              .Returns("hashed");

        users.Setup(r => r.AddAsync(It.Is<User>(u =>
                u.Email == email && u.PasswordHash == "hashed" && u.UserName == "New User"),
                It.IsAny<CancellationToken>()))
             .ReturnsAsync((User u, CancellationToken _) => u);

        jwt.Setup(j => j.Create(It.Is<User>(u => u.Email == email)))
           .Returns(("token123", DateTime.UtcNow.AddHours(1)));

        var res = await sut.RegisterAsync(req);

        res.AccessToken.Should().Be("token123");
        users.VerifyAll();
        jwt.VerifyAll();
        hasher.VerifyAll();
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotFound_ThrowsUnauthorized()
    {
        var (users, jwt, hasher, sut) = BuildSut();
        users.Setup(r => r.GetByEmailAsync("no@mail.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest("no@mail.com", "x")));

        users.VerifyAll();
        jwt.VerifyNoOtherCalls();
        hasher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ThrowsUnauthorized()
    {
        var (users, jwt, hasher, sut) = BuildSut();
        var user = new User { Email = "u@mail.com", PasswordHash = "hashed" };

        users.Setup(r => r.GetByEmailAsync("u@mail.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        hasher.Setup(h => h.VerifyHashedPassword(user, "hashed", "bad"))
              .Returns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest("u@mail.com", "bad")));

        users.VerifyAll();
        hasher.VerifyAll();
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenOk_ReturnsToken()
    {
        var (users, jwt, hasher, sut) = BuildSut();
        var user = new User { Email = "u@mail.com", PasswordHash = "hashed" };

        users.Setup(r => r.GetByEmailAsync("u@mail.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        hasher.Setup(h => h.VerifyHashedPassword(user, "hashed", "good"))
              .Returns(PasswordVerificationResult.Success);

        jwt.Setup(j => j.Create(user))
           .Returns(("jwt-token", DateTime.UtcNow.AddMinutes(30)));

        var res = await sut.LoginAsync(new LoginRequest("u@mail.com", "good"));

        res.AccessToken.Should().Be("jwt-token");
        users.VerifyAll();
        hasher.VerifyAll();
        jwt.VerifyAll();
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFund.UnitTests;

public class AuthServiceTests
{
    private static (Mock<IJwtProvider> jwt, Mock<UserManager<User>> userManager, AuthService sut)
        BuildSut()
    {
        var jwt = new Mock<IJwtProvider>(MockBehavior.Strict);
        
        var userStore = new Mock<IUserStore<User>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<User>>>();
        
        var userManager = new Mock<UserManager<User>>(
            userStore.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object);

        var sut = new AuthService(jwt.Object, userManager.Object);
        return (jwt, userManager, sut);
    }



    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsInvalidOperation()
    {
        var (jwt, userManager, sut) = BuildSut();
        
        var identityErrors = new[]
        {
            new IdentityError { Code = "DuplicateEmail", Description = "Email 'taken@mail.com' is already taken." }
        };
        
        userManager.Setup(um => um.CreateAsync(It.Is<User>(u => 
                u.Email == "taken@mail.com" && u.UserName == "Name"), "x"))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        await Assert.ThrowsAsync<AggregateException>(() =>
            sut.RegisterAsync(new RegisterRequest("taken@mail.com", "x", "Name")));

        userManager.VerifyAll();
        jwt.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task RegisterAsync_WhenUserManagerCreateFails_ThrowsAggregateException()
    {
        var (jwt, userManager, sut) = BuildSut();
        var email = "new@mail.com";
        var req = new RegisterRequest(email, "weak", "New User");

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Password must contain digits" }
        };

        userManager.Setup(um => um.CreateAsync(It.Is<User>(u => 
                u.Email == email.ToLowerInvariant() && u.UserName == "New User"), req.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var exception = await Assert.ThrowsAsync<AggregateException>(() =>
            sut.RegisterAsync(req));

        exception.Message.Should().Contain("Password too weak");
        exception.Message.Should().Contain("Password must contain digits");

        userManager.VerifyAll();
        jwt.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task RegisterAsync_WhenUserManagerCreateSucceeds_ReturnsToken()
    {
        var (jwt, userManager, sut) = BuildSut();
        var email = "new@mail.com";
        var req = new RegisterRequest(email, "P@ssw0rd!", "New User");

        userManager.Setup(um => um.CreateAsync(It.Is<User>(u => 
                u.Email == email.ToLowerInvariant() && u.UserName == "New User"), req.Password))
            .ReturnsAsync(IdentityResult.Success);

        jwt.Setup(j => j.Create(It.Is<User>(u => u.Email == email.ToLowerInvariant())))
            .Returns(("token123", DateTime.UtcNow.AddHours(1)));

        var res = await sut.RegisterAsync(req);

        res.AccessToken.Should().Be("token123");
        userManager.VerifyAll();
        jwt.VerifyAll();
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotFound_ThrowsUnauthorized()
    {
        var (jwt, userManager, sut) = BuildSut();
        
        userManager.Setup(um => um.FindByEmailAsync("no@mail.com"))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest("no@mail.com", "x")));

        userManager.VerifyAll();
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ThrowsUnauthorized()
    {
        var (jwt, userManager, sut) = BuildSut();
        var user = new User { Email = "u@mail.com", PasswordHash = "hashed" };

        userManager.Setup(um => um.FindByEmailAsync("u@mail.com"))
            .ReturnsAsync(user);

        userManager.Setup(um => um.CheckPasswordAsync(user, "bad"))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest("u@mail.com", "bad")));

        userManager.VerifyAll();
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginAsync_WhenOk_ReturnsToken()
    {
        var (jwt, userManager, sut) = BuildSut();
        var user = new User { Email = "u@mail.com", PasswordHash = "hashed" };

        userManager.Setup(um => um.FindByEmailAsync("u@mail.com"))
            .ReturnsAsync(user);

        userManager.Setup(um => um.CheckPasswordAsync(user, "good"))
            .ReturnsAsync(true);

        jwt.Setup(j => j.Create(user))
            .Returns(("jwt-token", DateTime.UtcNow.AddMinutes(30)));

        var res = await sut.LoginAsync(new LoginRequest("u@mail.com", "good"));

        res.AccessToken.Should().Be("jwt-token");
        userManager.VerifyAll();
        jwt.VerifyAll();
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using OpenFund.Infrastructure.Auth;

namespace OpenFund.UnitTests;

public class YouTubeOAuthServiceTests
{
    private static (
        Mock<UserManager<User>> userManager,
        Mock<IJwtProvider> jwtProvider,
        Mock<ITokenStorageService> tokenStorage,
        Mock<ILogger<YouTubeOAuthService>> logger,
        YouTubeOAuthService sut)
        BuildSut()
    {
        var userStore = new Mock<IUserStore<User>>();
        var options = Microsoft.Extensions.Options.Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var userManagerLogger = new Mock<ILogger<UserManager<User>>>();

        var userManager = new Mock<UserManager<User>>(
            userStore.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            userManagerLogger.Object);

        var jwtProvider = new Mock<IJwtProvider>(MockBehavior.Strict);
        var tokenStorage = new Mock<ITokenStorageService>(MockBehavior.Strict);
        var logger = new Mock<ILogger<YouTubeOAuthService>>();

        var sut = new YouTubeOAuthService(
            userManager.Object,
            jwtProvider.Object,
            tokenStorage.Object,
            logger.Object);

        return (userManager, jwtProvider, tokenStorage, logger, sut);
    }

    private static AuthenticateResult CreateAuthenticateResult(
        string googleId,
        string email,
        string? name = null,
        string? picture = null,
        string? accessToken = null,
        string? refreshToken = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, googleId),
            new(ClaimTypes.Email, email)
        };

        if (name != null) claims.Add(new Claim(ClaimTypes.Name, name));
        if (picture != null) claims.Add(new Claim("picture", picture));

        var identity = new ClaimsIdentity(claims, "Google");
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties();
        var tokens = new List<AuthenticationToken>();
        if (accessToken != null)
            tokens.Add(new AuthenticationToken { Name = "access_token", Value = accessToken });
        if (refreshToken != null)
            tokens.Add(new AuthenticationToken { Name = "refresh_token", Value = refreshToken });

        if (tokens.Count > 0)
            properties.StoreTokens(tokens);

        return AuthenticateResult.Success(new AuthenticationTicket(principal, properties, "Google"));
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenAuthenticationFailed_ReturnsFailureResult()
    {
        var (_, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var result = await sut.HandleCallbackAsync(AuthenticateResult.Fail("Auth failed"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Authentication failed");
        result.User.Should().BeNull();
        result.JwtToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();

        jwtProvider.VerifyNoOtherCalls();
        tokenStorage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenMissingGoogleId_ReturnsFailureResult()
    {
        var (_, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var claims = new List<Claim> { new(ClaimTypes.Email, "test@mail.com") };
        var identity = new ClaimsIdentity(claims, "Google");
        var principal = new ClaimsPrincipal(identity);
        var authResult = AuthenticateResult.Success(
            new AuthenticationTicket(principal, new AuthenticationProperties(), "Google"));

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Missing required claims");
        result.User.Should().BeNull();

        jwtProvider.VerifyNoOtherCalls();
        tokenStorage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenMissingEmail_ReturnsFailureResult()
    {
        var (_, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "google123") };
        var identity = new ClaimsIdentity(claims, "Google");
        var principal = new ClaimsPrincipal(identity);
        var authResult = AuthenticateResult.Success(
            new AuthenticationTicket(principal, new AuthenticationProperties(), "Google"));

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Missing required claims");

        jwtProvider.VerifyNoOtherCalls();
        tokenStorage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenUserNotFound_ReturnsFailureResult()
    {
        var (userManager, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var authResult = CreateAuthenticateResult("google123", "notfound@mail.com", "User");

        userManager.Setup(um => um.FindByEmailAsync("notfound@mail.com"))
            .ReturnsAsync((User?)null);

        userManager.Setup(um => um.CreateAsync(It.Is<User>(u => u.Email == "notfound@mail.com")))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed" }));

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User not found. Please sign in with Google first.");
        result.User.Should().BeNull();

        userManager.VerifyAll();
        jwtProvider.VerifyNoOtherCalls();
        tokenStorage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenNewUser_CreatesUserAsCreatorAndReturnsTokens()
    {
        var (userManager, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var email = "creator@mail.com";
        var googleId = "google123";
        var name = "Creator User";
        var picture = "https://example.com/pic.jpg";
        var accessToken = "yt_access_token";
        var refreshToken = "yt_refresh_token";

        var authResult = CreateAuthenticateResult(googleId, email, name, picture, accessToken, refreshToken);

        userManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        userManager.Setup(um => um.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);

        var jwtToken = "jwt_token_789";
        var jwtRefreshToken = "jwt_refresh_token_012";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        jwtProvider.Setup(j => j.Create(It.IsAny<User>()))
            .Returns((jwtToken, expiresAt));

        jwtProvider.Setup(j => j.GenerateRefreshToken())
            .Returns(jwtRefreshToken);

        tokenStorage.Setup(ts => ts.CreateRefreshTokenAsync(
                It.IsAny<Guid>(),
                jwtRefreshToken,
                null,
                null,
                accessToken,
                refreshToken,
                default))
            .ReturnsAsync(new RefreshToken());

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(email);
        result.User.GoogleId.Should().Be(googleId);
        result.User.DisplayName.Should().Be(name);
        result.User.Avatar.Should().Be(picture);
        result.User.EmailConfirmed.Should().BeTrue();
        result.User.IsCreator.Should().BeTrue();
        result.JwtToken.Should().Be(jwtToken);
        result.RefreshToken.Should().Be(jwtRefreshToken);
        result.ErrorMessage.Should().BeNull();

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(email);
        capturedUser.UserName.Should().Be(email);
        capturedUser.GoogleId.Should().Be(googleId);
        capturedUser.DisplayName.Should().Be(name);
        capturedUser.Avatar.Should().Be(picture);
        capturedUser.EmailConfirmed.Should().BeTrue();
        capturedUser.IsCreator.Should().BeTrue();

        userManager.VerifyAll();
        jwtProvider.VerifyAll();
        tokenStorage.VerifyAll();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenExistingUser_UpdatesUserAndReturnsTokens()
    {
        var (userManager, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var email = "existing@mail.com";
        var googleId = "google123";
        var name = "Updated Name";
        var picture = "https://example.com/new-pic.jpg";
        var accessToken = "new_yt_access_token";
        var refreshToken = "new_yt_refresh_token";

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = "Old Name",
            GoogleId = "old_google_id",
            Avatar = "https://example.com/old-pic.jpg",
            IsCreator = false
        };

        var authResult = CreateAuthenticateResult(googleId, email, name, picture, accessToken, refreshToken);

        userManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        userManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var jwtToken = "jwt_token_789";
        var jwtRefreshToken = "jwt_refresh_token_012";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        jwtProvider.Setup(j => j.Create(existingUser))
            .Returns((jwtToken, expiresAt));

        jwtProvider.Setup(j => j.GenerateRefreshToken())
            .Returns(jwtRefreshToken);

        tokenStorage.Setup(ts => ts.CreateRefreshTokenAsync(
                existingUser.Id,
                jwtRefreshToken,
                null,
                null,
                accessToken,
                refreshToken,
                default))
            .ReturnsAsync(new RefreshToken());

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeTrue();
        result.User.Should().Be(existingUser);
        result.User.GoogleId.Should().Be(googleId);
        result.User.DisplayName.Should().Be(name);
        result.User.Avatar.Should().Be(picture);
        result.JwtToken.Should().Be(jwtToken);
        result.RefreshToken.Should().Be(jwtRefreshToken);

        userManager.VerifyAll();
        jwtProvider.VerifyAll();
        tokenStorage.VerifyAll();
    }

    [Fact]
    public async Task HandleCallbackAsync_WhenExistingUserWithNullName_KeepsOldDisplayName()
    {
        var (userManager, jwtProvider, tokenStorage, _, sut) = BuildSut();

        var email = "existing@mail.com";
        var googleId = "google123";
        var oldDisplayName = "Old Name";

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = oldDisplayName,
            GoogleId = "old_google_id"
        };

        var authResult = CreateAuthenticateResult(googleId, email, null, null, "token", null);

        userManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(existingUser);

        userManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        jwtProvider.Setup(j => j.Create(existingUser))
            .Returns(("jwt", DateTime.UtcNow.AddHours(1)));

        jwtProvider.Setup(j => j.GenerateRefreshToken())
            .Returns("refresh");

        tokenStorage.Setup(ts => ts.CreateRefreshTokenAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                null,
                null,
                It.IsAny<string>(),
                null,
                default))
            .ReturnsAsync(new RefreshToken());

        var result = await sut.HandleCallbackAsync(authResult);

        result.Success.Should().BeTrue();
        result.User!.DisplayName.Should().Be(oldDisplayName);

        userManager.VerifyAll();
    }
}

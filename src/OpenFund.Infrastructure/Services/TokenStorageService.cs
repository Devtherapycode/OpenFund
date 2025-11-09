using System.Security.Cryptography;
using System.Text;

namespace OpenFund.Infrastructure.Services;

internal sealed class TokenStorageService : ITokenStorageService
{
    private readonly OpenFundDbContext _context;
    private readonly TokenEncryptionOptions _options;

    public TokenStorageService(OpenFundDbContext context, IOptions<TokenEncryptionOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(
        Guid userId,
        string jwtRefreshToken,
        string? googleAccessToken = null,
        string? googleRefreshToken = null,
        string? youtubeAccessToken = null,
        string? youtubeRefreshToken = null,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = await EncryptTokenAsync(jwtRefreshToken),
            GoogleAccessToken = googleAccessToken != null ? await EncryptTokenAsync(googleAccessToken) : null,
            GoogleRefreshToken = googleRefreshToken != null ? await EncryptTokenAsync(googleRefreshToken) : null,
            YoutubeAccessToken = youtubeAccessToken != null ? await EncryptTokenAsync(youtubeAccessToken) : null,
            YoutubeRefreshToken = youtubeRefreshToken != null ? await EncryptTokenAsync(youtubeRefreshToken) : null,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var encryptedToken = await EncryptTokenAsync(token);

        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == encryptedToken &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var encryptedToken = await EncryptTokenAsync(token);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == encryptedToken, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<string> EncryptTokenAsync(string token)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_options.EncryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16]; // Using zero IV for simplicity; in production, use proper IV management

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(token);
        }

        return Task.FromResult(Convert.ToBase64String(ms.ToArray()));
    }

    public Task<string> DecryptTokenAsync(string encryptedToken)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_options.EncryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16]; // Using zero IV for simplicity; in production, use proper IV management

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(encryptedToken));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return Task.FromResult(sr.ReadToEnd());
    }
}

public class TokenEncryptionOptions
{
    public const string SectionName = "TokenEncryption";
    public string EncryptionKey { get; set; } = string.Empty;
}

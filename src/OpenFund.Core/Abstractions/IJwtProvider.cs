namespace OpenFund.Core.Abstractions;

public interface IJwtProvider
{
    (string token, DateTime expiresAtUtc) Create(User user);
}

using OpenFund.Core.DTOs;

namespace OpenFund.Core.Interfaces.Managers;

public interface  ITokenManager
{
    Task<AuthTokenDto> CreateAuthenticationTokensAsync(string userId, string email, CancellationToken cancellationToken);
}
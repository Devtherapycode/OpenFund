using OpenFund.Core.DTOs;

namespace OpenFund.Core.Interfaces.Managers;

public interface IExternalAuthManager
{
    string GetInitialGmailAuthenticationLinkAsync(string redirectUri);
    Task<GoogleUserInfoDto> AuthenticateByGmailAsync(string redirectUri, string code);
}
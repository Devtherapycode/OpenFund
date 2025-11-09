namespace OpenFund.Core.Interfaces.Managers;

public interface IExternalAuthManager
{
    string GetInitialGmailAuthenticationLinkAsync(string redirectUri);
    Task<string> AuthenticateByGmailAsync(string redirectUri, string code);
}
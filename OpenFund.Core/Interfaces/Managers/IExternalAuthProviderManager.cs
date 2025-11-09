namespace OpenFund.Core.Interfaces.Managers;

public interface IExternalAuthProviderManager
{
    Task<string> AuthenticateByGmail(string redirectUri, string code);
}
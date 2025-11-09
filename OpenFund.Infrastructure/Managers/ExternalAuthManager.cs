using Microsoft.Extensions.Options;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Managers;

public class ExternalAuthManager : IExternalAuthManager
{
    private readonly HttpClient _client;
    private readonly GoogleOptions _googleOptions;

    
    public ExternalAuthManager(
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleOptions> googleOptions)
    {
        _client = httpClientFactory.CreateClient();
        _googleOptions = googleOptions.Value;
    }
    
    public async Task<string> AuthenticateByGmailAsync(string redirectUri, string code)
    {
        const string grantTypeValue = "authorization_code";

        var data = new Dictionary<string, string>
        {
            { "client_id", _googleOptions.ClientId },
            { "client_secret", _googleOptions.ClientSecret },
            { "code", code },
            { "grant_type", grantTypeValue },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(data);
        var response = await _client.PostAsync(_googleOptions.TokenRetrievalAddress, content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public string GetInitialGmailAuthenticationLinkAsync(string redirectUri)
    {
        const string responseType = "code";
        const string scope = "openid email profile";
        var initialUrl = _googleOptions.InitialAuthAddress;
        return $"{initialUrl}?client_id={_googleOptions.ClientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}";
    }
}
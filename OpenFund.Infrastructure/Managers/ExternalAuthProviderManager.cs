using Microsoft.Extensions.Options;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Managers;

public class ExternalAuthProviderManager : IExternalAuthProviderManager
{
    private readonly HttpClient _client;
    private readonly GoogleOptions _googleOptions;

    
    public ExternalAuthProviderManager(
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleOptions> googleOptions)
    {
        _client = httpClientFactory.CreateClient();
        _googleOptions = googleOptions.Value;
    }
    
    public async Task<string> AuthenticateByGmail(string redirectUri, string code)
    {
        const string GrantTypeValue = "authorization_code";

        var data = new Dictionary<string, string>
        {
            { "client_id", _googleOptions.ClientId },
            { "client_secret", _googleOptions.ClientSecret },
            { "code", code },
            { "grant_type", GrantTypeValue },
            { "redirect_uri", redirectUri }
        };

        var content = new FormUrlEncodedContent(data);
        var response = await _client.PostAsync(_googleOptions.TokenRetrievalAddress, content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

}
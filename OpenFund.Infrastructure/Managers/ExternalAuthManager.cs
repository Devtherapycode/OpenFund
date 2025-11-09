using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenFund.Core.DTOs;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Infrastructure.Models;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Managers;

public class ExternalAuthManager : IExternalAuthManager
{
    private readonly HttpClient _initialGoogleAuthHttpClient;
    private readonly HttpClient _userInfoHttpClient;
    private readonly GoogleOptions _googleOptions;
    public ExternalAuthManager(
        IHttpClientFactory httpClientFactory,
        IOptions<GoogleOptions> googleOptions)
    {
        _initialGoogleAuthHttpClient = httpClientFactory.CreateClient("Google");
        _userInfoHttpClient = httpClientFactory.CreateClient("GoogleUserInfo");
        _googleOptions = googleOptions.Value;
    }
    
    public async Task<GoogleUserInfoDto> AuthenticateByGmailAsync(string redirectUri, string code)
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
        var response = await _initialGoogleAuthHttpClient.PostAsync(_googleOptions.TokenRetrievalAddress, content);
        response.EnsureSuccessStatusCode();
        
        var responseStream = await response.Content.ReadAsStreamAsync();
        var googleAuthCallbackModel = await JsonSerializer.DeserializeAsync<GoogleAuthCallbackModel>(responseStream);

        var googleUserInfoDto = await GetGoogleUserInfo(googleAuthCallbackModel!.AccessToken);
        return googleUserInfoDto;
    }

    public string GetInitialGmailAuthenticationLinkAsync(string redirectUri)
    {
        const string responseType = "code";
        const string scope = "openid email profile";
        var initialUrl = _googleOptions.InitialAuthAddress;
        return $"{initialUrl}?client_id={_googleOptions.ClientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}";
    }

    private async Task<GoogleUserInfoDto> GetGoogleUserInfo(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _googleOptions.UserInfoAddress);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _userInfoHttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var userInfo = await JsonSerializer.DeserializeAsync<GoogleUserInfoDto>(responseStream);
        return userInfo!;
    }
}
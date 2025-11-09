using System.Text.Json.Serialization;

namespace OpenFund.Infrastructure.Models;

public record GoogleAuthCallbackModel(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    
    [property: JsonPropertyName("scope")]
    string Scope,
    
    [property: JsonPropertyName("token_type")]
    string TokenType,
    
    [property: JsonPropertyName("id_token")]
    string IdToken
);
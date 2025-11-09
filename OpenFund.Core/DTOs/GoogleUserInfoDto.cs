using System.Text.Json.Serialization;

namespace OpenFund.Core.DTOs;

public record GoogleUserInfoDto(
    [property: JsonPropertyName("sub")]
    string Sub,
    
    [property: JsonPropertyName("name")]
    string Name,
    
    [property: JsonPropertyName("given_name")]
    string GivenName,
    
    [property: JsonPropertyName("family_name")]
    string FamilyName,
    
    [property: JsonPropertyName("picture")]
    string Picture,
    
    [property: JsonPropertyName("email")]
    string Email,
    
    [property: JsonPropertyName("email_verified")]
    bool EmailVerified
);
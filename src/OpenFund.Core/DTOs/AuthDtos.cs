namespace OpenFund.Core.DTOs;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);
public record MeResponse(Guid Id, string Email, string DisplayName, DateTime CreatedOnUtc);

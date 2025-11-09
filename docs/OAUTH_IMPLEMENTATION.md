# OAuth Implementation Documentation

## Overview

OpenFund uses Google OAuth 2.0 for user authentication with two distinct flows:
1. **Google Sign-In**: Basic user authentication (all users)
2. **YouTube Connection**: Additional OAuth flow for content creators to receive donations

## Architecture

### Authentication Stack
- **Primary Authentication**: JWT Bearer tokens
- **OAuth State Management**: Temporary cookies (15-minute expiration)
- **Token Storage**: Encrypted refresh tokens in PostgreSQL database

### Project Structure

```
src/
├── OpenFund.Core/
│   ├── Abstractions/
│   │   ├── IOAuthService.cs          # OAuth service interface
│   │   ├── IJwtProvider.cs           # JWT token generation
│   │   └── ITokenStorageService.cs   # Encrypted token storage
│   └── Entities/
│       ├── User.cs                   # User entity with OAuth fields
│       └── RefreshToken.cs           # Encrypted OAuth tokens
├── OpenFund.Infrastructure/
│   ├── Auth/
│   │   ├── GoogleOAuthService.cs     # Google sign-in logic
│   │   ├── YouTubeOAuthService.cs    # YouTube connection logic
│   │   ├── JwtProvider.cs            # JWT generation implementation
│   │   └── TokenStorageService.cs    # AES-encrypted token storage
│   └── Persistence/
│       └── OpenFundDbContext.cs      # EF Core configuration
└── OpenFund.Api/
    ├── Endpoints/
    │   └── OAuthEndpoints.cs         # OAuth HTTP endpoints
    └── Program.cs                    # Authentication configuration
```

## User Entity

### Fields

```csharp
public class User : IdentityUser<Guid>
{
    public string? GoogleId { get; set; }        // Google account identifier
    public string? Avatar { get; set; }          // Profile picture URL
    public string? DisplayName { get; set; }     // User display name
    public bool IsCreator { get; set; }          // True if YouTube connected
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

### User Types

1. **Regular User** (`IsCreator = false`)
   - Signed in via Google OAuth
   - Can browse and donate to creators
   - Basic profile scopes only

2. **Creator** (`IsCreator = true`)
   - Regular user + YouTube connection
   - Can receive donations
   - YouTube API access tokens stored encrypted

## OAuth Flows

### Flow 1: Google Sign-In (All Users)

**Endpoint**: `GET /auth/google`

**Process**:
1. User clicks "Sign in with Google"
2. Backend redirects to Google consent screen
3. User grants permissions for: `openid`, `profile`, `email`
4. Google redirects back to `/auth/google/callback`
5. Backend validates OAuth response
6. Creates or updates user account
7. Generates JWT access + refresh tokens
8. Redirects to frontend with tokens in URL

**Scopes Requested**:
- `openid` - OpenID Connect authentication
- `profile` - Basic profile information
- `email` - Email address

**Implementation**: `GoogleOAuthService.cs:17-62`

### Flow 2: YouTube Connection (Creators Only)

**Endpoint**: `GET /auth/youtube`

**Prerequisites**: User must already have a Google account (signed in once)

**Process**:
1. User clicks "Connect YouTube"
2. Backend redirects to Google consent screen
3. User grants YouTube API permissions (currently disabled - requires Google verification)
4. Google redirects back to `/auth/youtube/callback`
5. Backend finds existing user by email
6. Sets `IsCreator = true`
7. Stores encrypted YouTube tokens
8. Generates JWT tokens
9. Redirects to frontend with tokens

**Scopes Requested** (when enabled):
- `https://www.googleapis.com/auth/youtube.readonly` - Read YouTube data
- `https://www.googleapis.com/auth/youtube.upload` - Upload videos

**Current Status**: YouTube scopes temporarily disabled pending Google verification

**Implementation**: `YouTubeOAuthService.cs:17-62`

## Authentication Configuration

### Program.cs Setup

```csharp
// Default: JWT Bearer for all API requests
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(...)           // Primary authentication
.AddCookie("TempOAuth", ...) // Temporary OAuth state (15 min)
.AddGoogle(...)              // Google OAuth provider
```

### Cookie Usage

**Name**: `OpenFund.OAuth.State`
**Purpose**: OAuth state management during redirect flow ONLY
**Lifetime**: 15 minutes
**Settings**:
- `HttpOnly: true`
- `SecurePolicy: SameAsRequest`
- `SameSite: Lax`

**Important**: Cookies are NOT used for API authentication - only JWT tokens.

## Service Layer Architecture

### IOAuthService Interface

```csharp
public interface IOAuthService
{
    Task<OAuthResult> HandleCallbackAsync(
        AuthenticateResult authenticateResult,
        CancellationToken cancellationToken = default);
}

public record OAuthResult(
    bool Success,
    User? User,
    string? JwtToken,
    string? RefreshToken,
    string? ErrorMessage = null
);
```

### GoogleOAuthService

**Responsibility**: Handle Google sign-in OAuth callbacks

**Key Methods**:
- `HandleCallbackAsync()` - Main OAuth callback handler
- `ExtractUserInfo()` - Extract claims from OAuth response
- `FindOrCreateUserAsync()` - Create or update user account

**Location**: `OpenFund.Infrastructure/Auth/GoogleOAuthService.cs`

### YouTubeOAuthService

**Responsibility**: Handle YouTube connection OAuth callbacks

**Key Methods**:
- `HandleCallbackAsync()` - Main OAuth callback handler
- `ExtractUserInfo()` - Extract claims and tokens
- User must exist (no new user creation)

**Location**: `OpenFund.Infrastructure/Auth/YouTubeOAuthService.cs`

## Token Storage

### RefreshToken Entity

```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }                 // Encrypted JWT refresh token
    public string? GoogleAccessToken { get; set; }    // Encrypted OAuth token
    public string? GoogleRefreshToken { get; set; }   // Encrypted OAuth token
    public string? YoutubeAccessToken { get; set; }   // Encrypted OAuth token
    public string? YoutubeRefreshToken { get; set; }  // Encrypted OAuth token
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public User User { get; set; }
}
```

### Encryption

**Algorithm**: AES (Advanced Encryption Standard)
**Key Source**: `TokenEncryption:EncryptionKey` in appsettings
**Implementation**: `TokenStorageService.cs`

**Methods**:
- `EncryptTokenAsync()` - Encrypts tokens before storage
- `DecryptTokenAsync()` - Decrypts tokens when retrieved

## Configuration

### appsettings.Development.json

```json
{
  "GoogleAuth": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret"
  },
  "TokenEncryption": {
    "EncryptionKey": "32-character-minimum-encryption-key"
  },
  "Jwt": {
    "Issuer": "openfund-dev",
    "Audience": "openfund-client-dev",
    "Key": "your-jwt-signing-key-32-chars-min",
    "ExpiryMinutes": 60
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

### Google Cloud Console Setup

1. **Create OAuth 2.0 Credentials**
   - Navigate to APIs & Services → Credentials
   - Create OAuth 2.0 Client ID
   - Application type: Web application

2. **Authorized Redirect URIs**
   ```
   Development:
   - http://localhost:5005/signin-google
   - https://localhost:5005/signin-google

   Production:
   - https://yourdomain.com/signin-google
   ```

3. **OAuth Consent Screen**
   - User type: External
   - Scopes: openid, profile, email
   - Test users: Add your development email

4. **YouTube API** (Optional - for creators)
   - Enable YouTube Data API v3
   - Add scopes: youtube.readonly, youtube.upload
   - Submit for Google verification (required for production)

## Endpoints

### GET /auth/google
**Purpose**: Initiate Google sign-in flow
**Response**: 302 Redirect to Google consent screen
**Logs**: `Program.cs:17`

### GET /auth/google/callback
**Purpose**: Handle Google OAuth callback
**Parameters**: OAuth authorization code (from Google)
**Response**: 302 Redirect to frontend with JWT tokens
**Success**: `{frontend}/auth/success?accessToken={jwt}&refreshToken={refresh}`
**Error**: `{frontend}/auth/error?message={error}`
**Implementation**: `OAuthEndpoints.cs:32-52`

### GET /auth/youtube
**Purpose**: Initiate YouTube connection flow
**Prerequisite**: User must be authenticated
**Response**: 302 Redirect to Google consent screen
**Logs**: `OAuthEndpoints.cs:56`

### GET /auth/youtube/callback
**Purpose**: Handle YouTube OAuth callback
**Response**: 302 Redirect to frontend with JWT tokens
**Sets**: `IsCreator = true`
**Implementation**: `OAuthEndpoints.cs:75-102`

### GET /me
**Purpose**: Get current user profile
**Authentication**: Required (JWT Bearer)
**Response**: User object with OAuth fields

## Database Migrations

### Initial Migration
Created User and RefreshToken tables with OAuth support.

### Migration: UpdateUserRemoveYoutubeChannelIdAddIsCreator
**Changes**:
- Removed `YoutubeChannelId` column from Users table
- Added `IsCreator` boolean column (default: false)
- Added index on `IsCreator` for creator queries

**Run Migration**:
```bash
dotnet ef database update --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

## Security Considerations

### Token Security
1. **JWT Tokens**
   - Signed with HMAC-SHA256
   - Short-lived (60 minutes default)
   - Validated on every request

2. **OAuth Tokens**
   - Encrypted with AES before database storage
   - Never exposed in API responses
   - Refresh tokens allow long-term access

3. **Cookies**
   - HttpOnly (no JavaScript access)
   - Secure flag in production
   - SameSite=Lax (CSRF protection)
   - Short-lived (15 minutes)

### Best Practices
- ✅ OAuth tokens encrypted at rest
- ✅ JWT tokens for stateless authentication
- ✅ Minimal cookie usage (OAuth state only)
- ✅ CORS configured for specific origins
- ✅ Separate scopes for different user types
- ✅ Refresh token rotation

## Frontend Integration

### Authentication Flow

```javascript
// 1. Redirect to OAuth
window.location.href = 'http://localhost:5005/auth/google';

// 2. Handle callback (on /auth/success page)
const urlParams = new URLSearchParams(window.location.search);
const accessToken = urlParams.get('accessToken');
const refreshToken = urlParams.get('refreshToken');

// 3. Store tokens
localStorage.setItem('accessToken', accessToken);
localStorage.setItem('refreshToken', refreshToken);

// 4. Make authenticated requests
fetch('http://localhost:5005/me', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
```

### YouTube Connection Flow

```javascript
// User must be signed in first
// Then redirect to YouTube OAuth
window.location.href = 'http://localhost:5005/auth/youtube';

// Handle callback same as above
// User now has IsCreator = true
```

## Testing

### Test Page
Located at: `test-auth.html`

**Features**:
- Google sign-in button
- YouTube connection button
- Protected endpoint testing
- Token display

**Usage**:
1. Serve via local web server (e.g., IntelliJ built-in server)
2. Click "Sign in with Google"
3. Grant permissions
4. View tokens in callback section
5. Test `/me` endpoint with token

### Manual Testing

```bash
# Test Google sign-in
curl -L http://localhost:5005/auth/google

# Test protected endpoint
curl http://localhost:5005/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Troubleshooting

### Common Issues

1. **redirect_uri_mismatch**
   - Solution: Add `http://localhost:5005/signin-google` to Google Cloud Console authorized redirects
   - Note: ASP.NET Core uses `/signin-google` not `/auth/google/callback`

2. **Error 403: access_denied (YouTube scopes)**
   - Cause: YouTube API scopes require Google verification
   - Solution: Add test users in Google Cloud Console OR remove YouTube scopes temporarily

3. **Cookies not working**
   - Check CORS configuration in `Program.cs:16-32`
   - Ensure `AllowCredentials()` is enabled
   - Verify origin matches exactly

4. **JWT validation failed**
   - Verify `Jwt:Key` in appsettings matches
   - Check token expiration
   - Ensure `Jwt:Issuer` and `Jwt:Audience` match configuration

## Future Enhancements

### Pending Items

1. **YouTube Verification**
   - Submit app to Google for verification
   - Enable YouTube scopes in production
   - Implement YouTube channel ID fetching

2. **Token Refresh**
   - Add `/auth/refresh` endpoint
   - Implement automatic token refresh in frontend
   - Handle expired access tokens gracefully

3. **Revocation**
   - Add `/auth/revoke` endpoint
   - Allow users to disconnect YouTube
   - Implement token cleanup for revoked sessions

4. **Enhanced Security**
   - Add PKCE (Proof Key for Code Exchange)
   - Implement rate limiting
   - Add device fingerprinting

## Change Log

### 2025-11-09
- ✅ Refactored OAuth logic into service layer (GoogleOAuthService, YouTubeOAuthService)
- ✅ Removed YoutubeChannelId from User entity
- ✅ Added IsCreator flag to distinguish creators from regular users
- ✅ Changed default authentication from Cookies to JWT Bearer
- ✅ Minimized cookie usage to OAuth state management only (15-min expiration)
- ✅ Updated frontend redirect to use configurable URL from appsettings
- ✅ Created migration: UpdateUserRemoveYoutubeChannelIdAddIsCreator
- ✅ Temporarily disabled YouTube scopes pending Google verification

### Earlier Changes
- ✅ Implemented Google OAuth 2.0 integration
- ✅ Added JWT authentication
- ✅ Created encrypted token storage
- ✅ Set up PostgreSQL database with Docker
- ✅ Implemented OAuth endpoints
- ✅ Added comprehensive logging
- ✅ Created test HTML page

## References

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [YouTube Data API](https://developers.google.com/youtube/v3)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)

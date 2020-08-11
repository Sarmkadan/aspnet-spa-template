// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# JWT Authentication Implementation Guide

Complete guide to implementing JWT-based authentication in aspnet-spa-template.

## Prerequisites

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

## Step 1: Create JWT Settings Class

**File: `Configuration/JwtSettings.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Configuration
{
    /// JWT configuration settings
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationMinutes { get; set; }
    }
}
```

## Step 2: Add JWT Settings to Configuration

**Update `appsettings.json`:**

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "aspnet-spa-template",
    "Audience": "aspnet-spa-users",
    "ExpirationMinutes": 60
  }
}
```

## Step 3: Create JWT Token Service

**File: `Services/ITokenService.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Services
{
    /// Service for generating JWT tokens
    public interface ITokenService
    {
        string GenerateToken(Guid userId, string email);
        bool ValidateToken(string token);
    }
}
```

**File: `Services/JwtTokenService.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetSpaTemplate.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AspNetSpaTemplate.Services
{
    /// JWT token generation and validation service
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            IOptions<JwtSettings> options,
            ILogger<JwtTokenService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public string GenerateToken(Guid userId, string email)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _settings.ExpirationMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_settings.Secret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

## Step 4: Configure Authentication in Program.cs

```csharp
// Add JWT configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSettings);

// Add token service
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// Configure authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();
```

## Step 5: Create Login Endpoint

**Create: `DTOs/LoginRequest.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs
{
    /// Login request DTO
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
```

**Create: `DTOs/LoginResponse.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs
{
    /// Login response with token
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
    }
}
```

**Update `Controllers/UsersController.cs`:**

```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
    [FromBody] LoginRequest request)
{
    // Validate request
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest(ApiResponse.Error("Email and password required"));
    }

    // Find user
    var user = await _userService.AuthenticateAsync(
        request.Email, request.Password);
    if (user == null)
    {
        return Unauthorized(ApiResponse.Error("Invalid credentials"));
    }

    // Generate token
    var token = _tokenService.GenerateToken(user.Id, user.Email);

    var response = new LoginResponse
    {
        UserId = user.Id,
        Email = user.Email,
        Token = token,
        ExpiresIn = 60 // minutes
    };

    return Ok(ApiResponse.Success(response, "Login successful"));
}
```

## Step 6: Protect Endpoints with [Authorize]

```csharp
[HttpGet("profile")]
[Authorize]
public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
{
    var userId = User.FindFirst(
        ClaimTypes.NameIdentifier)?.Value;
    
    if (!Guid.TryParse(userId, out var id))
        return Unauthorized();

    var user = await _userService.GetByIdAsync(id);
    return Ok(ApiResponse.Success(user));
}
```

## Step 7: Frontend Implementation

**File: `wwwroot/js/auth.js`**

```javascript
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

class AuthService {
  async login(email, password) {
    const response = await fetch('/api/users/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const result = await response.json();
    if (result.success) {
      localStorage.setItem('authToken', result.data.token);
      return result.data;
    } else {
      throw new Error(result.error.message);
    }
  }

  logout() {
    localStorage.removeItem('authToken');
    window.location.href = '/login';
  }

  getToken() {
    return localStorage.getItem('authToken');
  }

  isAuthenticated() {
    return !!this.getToken();
  }

  async getUserProfile() {
    const response = await fetch('/api/users/profile', {
      headers: {
        'Authorization': `Bearer ${this.getToken()}`
      }
    });
    return response.json();
  }
}

const authService = new AuthService();
```

**HTML Login Form:**

```html
<form id="loginForm">
  <input type="email" id="email" placeholder="Email" required>
  <input type="password" id="password" placeholder="Password" required>
  <button type="submit">Login</button>
</form>

<script>
document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  
  try {
    const user = await authService.login(
      document.getElementById('email').value,
      document.getElementById('password').value
    );
    
    // Redirect to dashboard
    window.location.href = '/dashboard';
  } catch (error) {
    alert('Login failed: ' + error.message);
  }
});
</script>
```

## Step 8: Test Authentication

```bash
# Login
curl -X POST https://localhost:7001/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Response:
# {
#   "success": true,
#   "data": {
#     "userId": "550e8400-e29b-41d4-a716-446655440000",
#     "email": "user@example.com",
#     "token": "eyJhbGciOiJIUzI1NiIs...",
#     "expiresIn": 60
#   }
# }

# Use token to access protected endpoint
curl https://localhost:7001/api/users/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

## Security Considerations

1. **Secret Key**: Use strong, random secret key (min 32 characters)
2. **HTTPS Only**: Always use HTTPS in production
3. **Token Expiration**: Keep expiration time short (e.g., 1 hour)
4. **Refresh Tokens**: Implement refresh token rotation for security
5. **Password Hashing**: Hash passwords with bcrypt or similar
6. **CORS**: Restrict CORS to trusted origins
7. **Rate Limiting**: Implement rate limiting on login endpoint

## Production Checklist

- [ ] Strong secret key in production
- [ ] HTTPS enabled
- [ ] Rate limiting on auth endpoints
- [ ] Refresh token mechanism
- [ ] Password reset functionality
- [ ] Two-factor authentication (optional)
- [ ] Audit logging for auth events
- [ ] Token blacklist for logout

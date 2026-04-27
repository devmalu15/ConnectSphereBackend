using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ConnectSphere.Auth.API.DTOs;
using ConnectSphere.Auth.API.Entities;
using ConnectSphere.Auth.API.Repositories;
using ConnectSphere.Contracts.DTOs;
using Google.Apis.Auth;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConnectSphere.Auth.API.Data;

namespace ConnectSphere.Auth.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;
    private readonly IDistributedCache _cache;
    private readonly Cloudinary _cloudinary;
    private readonly AuthDbContext _ctx;
    private readonly PasswordHasher<User> _hasher = new();

    public UserService(IUserRepository repo, IConfiguration config,
        IDistributedCache cache, Cloudinary cloudinary, AuthDbContext ctx)
    {
        _repo = repo; _config = config; _cache = cache;
        _cloudinary = cloudinary; _ctx = ctx;
    }

    public async Task<(string Token, string RefreshToken, int UserId)>
RegisterAsync(RegisterDto dto)
    {
        if (await _repo.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");
        if (await _repo.ExistsByUserNameAsync(dto.UserName))
            throw new InvalidOperationException("Username taken.");

        var user = new User
        {
            UserName = dto.UserName,
            FullName = dto.FullName,
            Email = dto.Email
        };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();
        return (GenerateJwt(user), GenerateRefreshToken(user.UserId), user.UserId);
    }

    public async Task<(string Token, string RefreshToken, int UserId)> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash!,
dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");
        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account suspended.");

        await _ctx.Users.Where(u => u.UserId == user.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt,
DateTime.UtcNow));

        return (GenerateJwt(user), GenerateRefreshToken(user.UserId), user.UserId);
    }

    public async Task<(string Token, string RefreshToken, int UserId)> GoogleOAuthAsync(string
idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        var user = await _repo.GetByOAuthAsync("Google", payload.Subject);
        if (user == null)
        {
            user = new User
            {
                UserName = payload.Email.Split('@')[0] + "_" + new
Random().Next(1000, 9999),
                FullName = payload.Name,
                Email = payload.Email,
                AvatarUrl = payload.Picture,
                OAuthProvider = "Google",
                OAuthProviderId = payload.Subject,
                IsActive = true
            };
            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
        }
        return (GenerateJwt(user), GenerateRefreshToken(user.UserId), user.UserId);
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        // In production: validate refresh token from DB/Redis 
        // Simplified: decode and re-issue 
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(refreshToken);
        var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (userIdClaim == null) throw new SecurityTokenException("Invalid refresh token.");
        var user = await _repo.GetByIdAsync(int.Parse(userIdClaim))
            ?? throw new SecurityTokenException("User not found.");
        return GenerateJwt(user);
    }

    public async Task LogoutAsync(int userId, string token)
    {
        // Blacklist token in Redis for remaining TTL 
        var key = $"blacklist:token:{token}";
        await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                int.Parse(_config["JwtSettings:ExpiryMinutes"]!))
        });
    }

    public async Task<UserDto> GetByIdAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new
KeyNotFoundException("User not found.");
        return ToDto(user);
    }

    public async Task<UserDto> GetByUserNameAsync(string userName)
    {
        var user = await _repo.GetByUserNameAsync(userName) ?? throw new
KeyNotFoundException("User not found.");
        return ToDto(user);
    }

    public async Task<IList<UserDto>> SearchAsync(string query)
    {
        var users = await _repo.SearchAsync(query);
        return users.Select(ToDto).ToList();
    }

    public async Task<IList<UserDto>> GetSuggestedUsersAsync(int userId)
    {
        // Simplified: return top users not already followed 
        var users = await _repo.GetSuggestedUsersAsync(userId, new List<int>());
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new
KeyNotFoundException();
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Bio != null) user.Bio = dto.Bio;

        if (dto.AvatarFile != null)
        {
            if (user.AvatarPublicId != null)
                await _cloudinary.DestroyAsync(new
DeletionParams(user.AvatarPublicId));

            using var stream = dto.AvatarFile.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.AvatarFile.FileName, stream),
                Folder = "connectsphere/avatars"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            user.AvatarUrl = uploadResult.SecureUrl.ToString();
            user.AvatarPublicId = uploadResult.PublicId;
        }

        await _repo.UpdateAsync(user);
        await _repo.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new
KeyNotFoundException();
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash!,
dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Current password incorrect.");
        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
        await _repo.UpdateAsync(user);
        await _repo.SaveChangesAsync();
    }

    public async Task<bool> TogglePrivacyAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new
KeyNotFoundException();
        var newValue = !user.IsPrivate;
        await _ctx.Users.Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsPrivate, newValue));
        return newValue;
    }

    public async Task DeactivateAccountAsync(int userId)
    {
        await _ctx.Users.Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false));
    }

    public async Task UpdateCountersAsync(int userId, int followerDelta, int
followingDelta, int postDelta)
    {
        await _ctx.Users.Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FollowerCount, u => u.FollowerCount +
followerDelta)
                .SetProperty(u => u.FollowingCount, u => u.FollowingCount +
followingDelta)
                .SetProperty(u => u.PostCount, u => u.PostCount + postDelta));
    }

    private string GenerateJwt(User user)
    {
        var key = new
SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("username", user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires:
DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(int userId)
    {
        var key = new
SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: new[] { new Claim("sub", userId.ToString()) },
            expires:
DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!)),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task SetRoleAsync(int userId, string role)
    {
        await _ctx.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, role));
    }

    public async Task ReactivateAccountAsync(int userId)
{
    await _ctx.Users
        .Where(u => u.UserId == userId)
        .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, true));
}

public async Task<int> GetCountAsync()
{
    return await _ctx.Users.CountAsync(u => u.IsActive);
}

    private static UserDto ToDto(User u) => new(
        u.UserId, u.UserName, u.FullName, u.AvatarUrl,
        u.Bio, u.IsPrivate, u.FollowerCount, u.FollowingCount, u.PostCount);
}
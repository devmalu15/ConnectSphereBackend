using ConnectSphere.Auth.API.DTOs; 
using ConnectSphere.Auth.API.Entities; 
using ConnectSphere.Contracts.DTOs; 
  
namespace ConnectSphere.Auth.API.Services; 
  
public interface IUserService 
{ 
    Task<(string Token, string RefreshToken)> RegisterAsync(RegisterDto dto); 
    Task<(string Token, string RefreshToken)> LoginAsync(LoginDto dto); 
    Task<(string Token, string RefreshToken)> GoogleOAuthAsync(string idToken); 
    Task<string> RefreshTokenAsync(string refreshToken); 
    Task LogoutAsync(int userId, string token); 
    Task<UserDto> GetByIdAsync(int userId); 
    Task<UserDto> GetByUserNameAsync(string userName); 
    Task<IList<UserDto>> SearchAsync(string query); 
    Task<IList<UserDto>> GetSuggestedUsersAsync(int userId); 
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto); 
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto); 
    Task<bool> TogglePrivacyAsync(int userId); 
    Task DeactivateAccountAsync(int userId); 
    Task UpdateCountersAsync(int userId, int followerDelta, int followingDelta, int 
postDelta); 
} 
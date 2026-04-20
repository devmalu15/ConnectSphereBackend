using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 
  
public record RegisterDto( 
    [Required][MaxLength(50)] string UserName, 
    [Required][MaxLength(100)] string FullName, 
    [Required][EmailAddress] string Email, 
    [Required][MinLength(8)] string Password 
); 
  
public record LoginDto( 
    [Required] string Email, 
    [Required] string Password 
); 
  
public record UpdateProfileDto( 
    string? FullName, 
    string? Bio, 
    IFormFile? AvatarFile 
); 
  
public record ChangePasswordDto( 
    [Required] string CurrentPassword, 
    [Required][MinLength(8)] string NewPassword 
); 
  
public record GoogleOAuthDto([Required] string IdToken); 
  
public record RefreshTokenDto([Required] string RefreshToken);
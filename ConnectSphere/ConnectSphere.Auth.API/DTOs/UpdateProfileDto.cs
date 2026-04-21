using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 

public record UpdateProfileDto( 
    string? FullName, 
    string? Bio, 
    IFormFile? AvatarFile 
); 
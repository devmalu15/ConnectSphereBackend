using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 

public record RefreshTokenDto([Required] string RefreshToken);
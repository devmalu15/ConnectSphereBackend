using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.DTOs; 

public record GoogleOAuthDto([Required] string IdToken);
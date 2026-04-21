using ConnectSphere.Contracts.Enums;
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Post.API.DTOs; 

public record UpdatePostDto( 
    string? Content, 
    Visibility? Visibility, 
    string? Hashtags 
); 
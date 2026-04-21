using ConnectSphere.Contracts.Enums;
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Post.API.DTOs; 
  
public record CreatePostDto( 
    [Required][MaxLength(2000)] string Content, 
    MediaType MediaType, 
    Visibility Visibility, 
    string? Hashtags, 
    IFormFile? MediaFile 
); 
  

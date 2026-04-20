using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.DTOs; 
  
public record PostDto( 
    int PostId, int UserId, string Content, string? MediaUrl, 
    MediaType MediaType, Visibility Visibility, int LikeCount, 
    int CommentCount, int ShareCount, string? Hashtags, 
    bool IsDeleted, DateTime CreatedAt, DateTime? UpdatedAt); 
using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 
  
public record PostCreatedEvent( 
    int PostId, int UserId, string Content, string? Hashtags, 
    Visibility Visibility, DateTime CreatedAt) : IPostCreatedEvent; 
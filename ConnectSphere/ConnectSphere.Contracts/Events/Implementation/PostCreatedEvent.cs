using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 
  
public record PostCreatedEvent( 
    int PostId, int UserId, string Content, string? Hashtags, 
    Visibility Visibility, DateTime CreatedAt) : IPostCreatedEvent; 
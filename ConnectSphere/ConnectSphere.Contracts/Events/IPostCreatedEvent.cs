using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events; 
  
public interface IPostCreatedEvent 
{ 
    int PostId { get; } 
    int UserId { get; } 
    string Content { get; } 
    string? Hashtags { get; } 
    Visibility Visibility { get; } 
    DateTime CreatedAt { get; } 
}
using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record CommentAddedEvent(int CommentId, int PostId, int UserId, int? 
ParentCommentId, string Content, DateTime CreatedAt) : ICommentAddedEvent; 
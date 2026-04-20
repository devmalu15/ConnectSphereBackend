using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record CommentAddedEvent(int CommentId, int PostId, int UserId, int? 
ParentCommentId, string Content, DateTime CreatedAt) : ICommentAddedEvent; 
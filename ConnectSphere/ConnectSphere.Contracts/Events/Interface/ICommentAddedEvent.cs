namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface ICommentAddedEvent 
{ 
    int CommentId { get; } 
    int PostId { get; } 
    int UserId { get; } 
    int? ParentCommentId { get; } 
    string Content { get; } 
    DateTime CreatedAt { get; } 
} 
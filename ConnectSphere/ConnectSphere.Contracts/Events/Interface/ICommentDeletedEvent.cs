namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface ICommentDeletedEvent 
{ 
    int CommentId { get; } 
    int PostId { get; } 
} 
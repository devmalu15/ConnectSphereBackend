namespace ConnectSphere.Contracts.Events; 
  
public interface ICommentDeletedEvent 
{ 
    int CommentId { get; } 
    int PostId { get; } 
} 
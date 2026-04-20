using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record CommentDeletedEvent(int CommentId, int PostId) : 
ICommentDeletedEvent; 
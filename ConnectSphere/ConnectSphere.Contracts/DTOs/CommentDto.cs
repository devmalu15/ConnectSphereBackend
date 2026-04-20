namespace ConnectSphere.Contracts.DTOs; 
  
public record CommentDto( 
    int CommentId, int PostId, int UserId, int? ParentCommentId, 
    string Content, int LikeCount, int ReplyCount, 
    bool IsEdited, bool IsDeleted, DateTime CreatedAt, DateTime? EditedAt); 
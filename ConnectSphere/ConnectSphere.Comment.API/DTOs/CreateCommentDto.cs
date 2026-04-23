namespace ConnectSphere.Comment.API.DTOs; 

public record CreateCommentDto(int PostId, string Content, int? ParentCommentId);
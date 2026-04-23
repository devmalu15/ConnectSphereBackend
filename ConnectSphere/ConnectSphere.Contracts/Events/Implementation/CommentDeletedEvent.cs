using ConnectSphere.Contracts.Events.Interface;

namespace ConnectSphere.Contracts.Events.Implementation;

public record CommentDeletedEvent(
    int CommentId,
    int PostId,
    int? ParentCommentId,
    int DeletedCount) : ICommentDeletedEvent;
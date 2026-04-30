namespace ConnectSphere.Contracts.Events.Interface;

public interface ICommentDeletedEvent
{
    int CommentId { get; }
    int PostId { get; }
    int? ParentCommentId { get; }
    int DeletedCount { get; } 
}
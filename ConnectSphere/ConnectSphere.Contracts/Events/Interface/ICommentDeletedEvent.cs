namespace ConnectSphere.Contracts.Events.Interface;

public interface ICommentDeletedEvent
{
    int CommentId { get; }
    int PostId { get; }
    int? ParentCommentId { get; }
    int DeletedCount { get; } // total rows deleted: 1 (comment) + N (its replies)
}
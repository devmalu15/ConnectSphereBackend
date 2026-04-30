namespace ConnectSphere.Contracts.Events.Interface;

public interface IPostRepostedEvent
{
    int OriginalPostId { get; }
    int ReposterId { get; }
    int OriginalAuthorId { get; }
    int NewPostId { get; }
    DateTime CreatedAt { get; }
}

using ConnectSphere.Contracts.Events.Interface;

namespace ConnectSphere.Contracts.Events.Implementation;

public record PostRepostedEvent(
    int OriginalPostId,
    int ReposterId,
    int OriginalAuthorId,
    int NewPostId,
    DateTime CreatedAt) : IPostRepostedEvent;

using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Interface;

namespace ConnectSphere.Contracts.Events.Implementation;

public record MentionEvent(
    int ActorId,
    int MentionedUserId,
    int TargetId,
    TargetType TargetType,
    string Content,
    DateTime CreatedAt) : IMentionEvent;

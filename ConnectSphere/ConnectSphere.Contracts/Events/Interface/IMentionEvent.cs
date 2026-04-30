namespace ConnectSphere.Contracts.Events.Interface;

public interface IMentionEvent
{
    int ActorId { get; }
    int MentionedUserId { get; }
    int TargetId { get; }
    ConnectSphere.Contracts.Enums.TargetType TargetType { get; }
    string Content { get; }
    DateTime CreatedAt { get; }
}

namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IFollowRequestedEvent 
{ 
    int FollowId { get; } 
    int FollowerId { get; } 
    int FolloweeId { get; } 
    bool IsPrivate { get; } 
} 
namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IFollowAcceptedEvent 
{ 
    int FollowId { get; } 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
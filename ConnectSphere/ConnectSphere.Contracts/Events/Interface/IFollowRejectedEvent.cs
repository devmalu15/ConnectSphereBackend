namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IFollowRejectedEvent 
{ 
    int FollowId { get; } 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
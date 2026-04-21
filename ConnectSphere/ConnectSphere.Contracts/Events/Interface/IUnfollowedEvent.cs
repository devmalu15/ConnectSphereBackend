namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IUnfollowedEvent 
{ 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
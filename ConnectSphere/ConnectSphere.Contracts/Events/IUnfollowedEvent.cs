namespace ConnectSphere.Contracts.Events; 
  
public interface IUnfollowedEvent 
{ 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
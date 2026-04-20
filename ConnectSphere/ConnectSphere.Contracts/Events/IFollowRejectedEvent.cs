namespace ConnectSphere.Contracts.Events; 
  
public interface IFollowRejectedEvent 
{ 
    int FollowId { get; } 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
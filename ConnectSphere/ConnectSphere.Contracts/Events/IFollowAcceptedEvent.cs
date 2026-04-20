namespace ConnectSphere.Contracts.Events; 
  
public interface IFollowAcceptedEvent 
{ 
    int FollowId { get; } 
    int FollowerId { get; } 
    int FolloweeId { get; } 
} 
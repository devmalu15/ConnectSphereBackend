namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface ICountersUpdatedEvent 
{ 
    int UserId { get; } 
    int FollowerDelta { get; } 
    int FollowingDelta { get; } 
    int PostDelta { get; } 
} 
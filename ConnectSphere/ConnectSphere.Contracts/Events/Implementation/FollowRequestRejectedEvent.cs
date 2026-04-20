using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowRejectedEvent(int FollowId, int FollowerId, int FolloweeId) : 
IFollowRejectedEvent; 
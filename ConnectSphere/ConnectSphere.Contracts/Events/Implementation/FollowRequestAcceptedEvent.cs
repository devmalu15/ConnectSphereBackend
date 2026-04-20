using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowAcceptedEvent(int FollowId, int FollowerId, int FolloweeId) : 
IFollowAcceptedEvent; 

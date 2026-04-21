using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowAcceptedEvent(int FollowId, int FollowerId, int FolloweeId) : 
IFollowAcceptedEvent; 

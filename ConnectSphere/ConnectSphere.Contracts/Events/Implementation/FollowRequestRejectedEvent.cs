using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowRejectedEvent(int FollowId, int FollowerId, int FolloweeId) : 
IFollowRejectedEvent; 
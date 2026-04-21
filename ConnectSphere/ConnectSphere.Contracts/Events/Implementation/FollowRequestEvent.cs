using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowRequestedEvent(int FollowId, int FollowerId, int FolloweeId, 
bool IsPrivate) : IFollowRequestedEvent; 
using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FollowRequestedEvent(int FollowId, int FollowerId, int FolloweeId, 
bool IsPrivate) : IFollowRequestedEvent; 
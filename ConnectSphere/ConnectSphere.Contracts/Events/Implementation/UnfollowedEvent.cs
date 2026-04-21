using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record UnfollowedEvent(int FollowerId, int FolloweeId) : IUnfollowedEvent; 
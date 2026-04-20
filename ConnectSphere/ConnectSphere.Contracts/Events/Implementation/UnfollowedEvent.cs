using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record UnfollowedEvent(int FollowerId, int FolloweeId) : IUnfollowedEvent; 
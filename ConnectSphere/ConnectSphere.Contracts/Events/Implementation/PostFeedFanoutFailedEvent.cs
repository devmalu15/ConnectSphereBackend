using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record PostFeedFanoutFailedEvent(int PostId, string Reason) : 
IPostFeedFanoutFailedEvent; 
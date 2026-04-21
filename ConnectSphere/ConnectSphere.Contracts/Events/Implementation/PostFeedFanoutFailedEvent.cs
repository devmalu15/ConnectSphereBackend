using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record PostFeedFanoutFailedEvent(int PostId, string Reason) : 
IPostFeedFanoutFailedEvent; 
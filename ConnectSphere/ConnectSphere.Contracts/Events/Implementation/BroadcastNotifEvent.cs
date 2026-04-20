using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record BroadcastNotifEvent(string Title, string Message, IList<int> UserIds, 
NotifType Type) : IBroadcastNotifEvent; 
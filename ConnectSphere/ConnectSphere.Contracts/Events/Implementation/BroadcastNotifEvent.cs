using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record BroadcastNotifEvent(string Title, string Message, IList<int> UserIds, 
NotifType Type) : IBroadcastNotifEvent; 
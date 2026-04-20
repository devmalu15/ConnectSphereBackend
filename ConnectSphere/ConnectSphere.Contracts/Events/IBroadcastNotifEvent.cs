using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events; 
  
public interface IBroadcastNotifEvent 
{ 
    string Title { get; } 
    string Message { get; } 
    IList<int> UserIds { get; } 
    NotifType Type { get; } 
} 
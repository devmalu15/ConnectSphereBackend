using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 
  

public record PostDeletedEvent(int PostId, int UserId) : IPostDeletedEvent; 
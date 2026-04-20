using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 
  

public record PostDeletedEvent(int PostId, int UserId) : IPostDeletedEvent; 
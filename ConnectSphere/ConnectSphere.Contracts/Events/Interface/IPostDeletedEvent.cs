namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IPostDeletedEvent 
{ 
    int PostId { get; } 
    int UserId { get; } 
} 
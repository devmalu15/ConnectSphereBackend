namespace ConnectSphere.Contracts.Events; 
  
public interface IPostDeletedEvent 
{ 
    int PostId { get; } 
    int UserId { get; } 
} 
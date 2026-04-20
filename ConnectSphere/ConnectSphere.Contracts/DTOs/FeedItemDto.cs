namespace ConnectSphere.Contracts.DTOs; 
  
public record FeedItemDto(int FeedItemId, int UserId, int PostId, int ActorId, 
decimal Score, DateTime CreatedAt); 
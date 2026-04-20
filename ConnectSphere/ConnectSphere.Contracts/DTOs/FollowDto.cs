using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.DTOs; 
  
public record FollowDto(int FollowId, int FollowerId, int FolloweeId, FollowStatus 
Status, DateTime CreatedAt); 
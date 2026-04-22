using ConnectSphere.Contracts.Enums; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Like.API.Entities; 
  
[Index(nameof(UserId), nameof(TargetId), nameof(TargetType), IsUnique = true)] 
public class Like 
{ 
    public int LikeId { get; set; } 
    public int UserId { get; set; } 
    public int TargetId { get; set; } 
    public TargetType TargetType { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}
using ConnectSphere.Contracts.Enums; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Follow.API.Entities; 
  
[Index(nameof(FollowerId), nameof(FolloweeId), IsUnique = true)] 
[Index(nameof(FolloweeId))] 
public class Follow 
{ 
    public int FollowId { get; set; } 
    public int FollowerId { get; set; } 
    public int FolloweeId { get; set; } 
    public FollowStatus Status { get; set; } = FollowStatus.PENDING; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime? AcceptedAt { get; set; } 
} 
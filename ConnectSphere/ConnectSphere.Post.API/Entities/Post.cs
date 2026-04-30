using ConnectSphere.Contracts.Enums; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Post.API.Entities; 
  
[Index(nameof(UserId))] 
[Index(nameof(CreatedAt))] 
public class Post 
{ 
    public int PostId { get; set; } 
  
    [System.ComponentModel.DataAnnotations.Required] 
    public int UserId { get; set; } 
  
    [System.ComponentModel.DataAnnotations.MaxLength(2000)] 
    public string Content { get; set; } = string.Empty; 
  
    public string? MediaUrl { get; set; } 
    public string? MediaPublicId { get; set; } 
  
    public MediaType MediaType { get; set; } = MediaType.NONE; 
    public Visibility Visibility { get; set; } = Visibility.PUBLIC; 
  
    public string? Hashtags { get; set; } 
  
    public int LikeCount { get; set; } = 0; 
    public int CommentCount { get; set; } = 0; 
    public int ShareCount { get; set; } = 0; 
  
    public bool IsDeleted { get; set; } = false; 
  
    public PostDistribution DistributionStatus { get; set; } = 
PostDistribution.PENDING; 
  
    public int? OriginalPostId { get; set; } 
  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime? UpdatedAt { get; set; } 

    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
}
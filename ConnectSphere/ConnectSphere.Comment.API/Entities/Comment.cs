using Microsoft.EntityFrameworkCore; 
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Comment.API.Entities; 
  
[Index(nameof(PostId))] 
[Index(nameof(PostId), nameof(ParentCommentId))] 
public class Comment 
{ 
    public int CommentId { get; set; } 
    public int PostId { get; set; } 
    public int UserId { get; set; } 
    public int? ParentCommentId { get; set; } 
  
    [MaxLength(1000)] 
    public string Content { get; set; } = string.Empty; 
  
    public int LikeCount { get; set; } = 0; 
    public int ReplyCount { get; set; } = 0; 
    public bool IsDeleted { get; set; } = false; 
    public bool IsEdited { get; set; } = false; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime? EditedAt { get; set; } 
} 
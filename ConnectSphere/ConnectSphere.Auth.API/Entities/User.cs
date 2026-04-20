using Microsoft.EntityFrameworkCore; 
using System.ComponentModel.DataAnnotations; 
  
namespace ConnectSphere.Auth.API.Entities; 
  
[Index(nameof(UserName), IsUnique = true)] 
[Index(nameof(Email), IsUnique = true)] 
public class User 
{ 
    public int UserId { get; set; } 
  
    [MaxLength(50)] 
    public string UserName { get; set; } = string.Empty; 
  
    [MaxLength(100)] 
    public string FullName { get; set; } = string.Empty; 
  
    [MaxLength(255)] 
    public string Email { get; set; } = string.Empty; 
  
    public string? PasswordHash { get; set; } 
  
    [MaxLength(500)] 
    public string? Bio { get; set; } 
  
    public string? AvatarUrl { get; set; } 
    public string? AvatarPublicId { get; set; } 
  
    public bool IsPrivate { get; set; } = false; 
    public bool IsActive { get; set; } = true; 
  
    public int FollowerCount { get; set; } = 0; 
    public int FollowingCount { get; set; } = 0; 
    public int PostCount { get; set; } = 0; 
  
    public string? OAuthProvider { get; set; } 
    public string? OAuthProviderId { get; set; } 
  
    [MaxLength(10)] 
    public string Role { get; set; } = "User"; 
  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime? LastLoginAt { get; set; } 
} 
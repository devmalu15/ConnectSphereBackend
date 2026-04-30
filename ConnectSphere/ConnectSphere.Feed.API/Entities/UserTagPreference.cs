using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Feed.API.Entities; 
  
[Index(nameof(UserId), nameof(Tag))] 
public class UserTagPreference 
{ 
    public int UserTagPreferenceId { get; set; } 
    public int UserId { get; set; } 
  
        public string Tag { get; set; } = string.Empty; 
  
        public double AffinityScore { get; set; } = 1.0; 
  
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow; 
} 

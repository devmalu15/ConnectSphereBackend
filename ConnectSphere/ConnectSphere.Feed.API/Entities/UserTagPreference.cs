using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Feed.API.Entities; 
  
/// <summary> 
/// Tracks which hashtags a user engages with (via likes). 
/// Used by the tag-based feed ranking algorithm. 
/// </summary> 
[Index(nameof(UserId), nameof(Tag))] 
public class UserTagPreference 
{ 
    public int UserTagPreferenceId { get; set; } 
    public int UserId { get; set; } 
  
    /// <summary>Normalised hashtag, e.g. "travel" (without #)</summary> 
    public string Tag { get; set; } = string.Empty; 
  
    /// <summary> 
    /// Affinity score — incremented each time the user likes a post 
    /// containing this tag. Decays over time (optional advanced feature). 
    /// </summary> 
    public double AffinityScore { get; set; } = 1.0; 
  
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow; 
} 

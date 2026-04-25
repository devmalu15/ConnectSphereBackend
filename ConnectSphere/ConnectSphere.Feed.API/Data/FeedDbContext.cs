using ConnectSphere.Feed.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Feed.API.Data; 
  
public class FeedDbContext : DbContext 
{ 
    public FeedDbContext(DbContextOptions<FeedDbContext> options) : base(options) { } 
    
    public DbSet<FeedItem> FeedItems => Set<FeedItem>(); 
    public DbSet<UserTagPreference> UserTagPreferences => Set<UserTagPreference>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        
        modelBuilder.Entity<FeedItem>(entity => 
        {
            entity.HasKey(f => f.FeedItemId);

            // Fixes the truncation warning and ensures ranking accuracy
            entity.Property(f => f.Score)
                  .HasPrecision(18, 4); // Total 18 digits, 4 after the decimal
        });

        modelBuilder.Entity<UserTagPreference>(entity => 
        {
            entity.HasKey(u => u.UserTagPreferenceId);

            // Optional: If UserTagPreference also has a decimal 'Weight' or 'Score'
            // entity.Property(u => u.Weight).HasPrecision(18, 4);
        });
    } 
}
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

            
            entity.Property(f => f.Score)
                  .HasPrecision(18, 4); 
        });

        modelBuilder.Entity<UserTagPreference>(entity => 
        {
            entity.HasKey(u => u.UserTagPreferenceId);

            
            
        });
    } 
}
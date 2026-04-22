using LikeEntity = ConnectSphere.Like.API.Entities.Like; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Like.API.Data; 
  
public class LikeDbContext : DbContext 
{ 
    public LikeDbContext(DbContextOptions<LikeDbContext> options) : base(options) { 
} 
    public DbSet<LikeEntity> Likes => Set<LikeEntity>(); 
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<LikeEntity>(e => e.Property(l => 
l.TargetType).HasConversion<string>()); 
    } 
} 
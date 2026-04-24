using ConnectSphere.Contracts.Enums; 
using FollowEntity = ConnectSphere.Follow.API.Entities.Follow; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Follow.API.Data; 
  
public class FollowDbContext : DbContext 
{ 
    public FollowDbContext(DbContextOptions<FollowDbContext> options) : 
base(options) { } 
  
    public DbSet<FollowEntity> Follows => Set<FollowEntity>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<FollowEntity>(entity => 
        { 
            entity.HasKey(f => f.FollowId); 
            entity.Property(f => f.Status).HasConversion<string>(); 
            entity.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); 
        }); 
    } 
} 
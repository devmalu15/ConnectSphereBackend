using ConnectSphere.Contracts.Enums; 
using PostEntity = ConnectSphere.Post.API.Entities.Post; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Post.API.Data; 
  
public class PostDbContext : DbContext 
{ 
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { 
} 
  
    public DbSet<PostEntity> Posts => Set<PostEntity>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<PostEntity>(entity => 
        { 
            entity.HasKey(p => p.PostId); 
            entity.Property(p => p.MediaType).HasConversion<string>(); 
            entity.Property(p => p.Visibility).HasConversion<string>(); 
            entity.Property(p => p.DistributionStatus).HasConversion<string>(); 
            entity.HasQueryFilter(p => !p.IsDeleted); 
        }); 
    } 
} 
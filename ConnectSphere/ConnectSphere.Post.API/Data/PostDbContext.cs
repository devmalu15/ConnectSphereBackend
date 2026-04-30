using ConnectSphere.Contracts.Enums; 
using PostEntity = ConnectSphere.Post.API.Entities.Post; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Post.API.Data; 
  
public class PostDbContext : DbContext 
{ 
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { 
} 
  
    public DbSet<PostEntity> Posts => Set<PostEntity>(); 
    public DbSet<ConnectSphere.Post.API.Entities.Mention> Mentions => Set<ConnectSphere.Post.API.Entities.Mention>(); 
  
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

            entity.HasMany(p => p.Mentions)
                  .WithOne(m => m.Post)
                  .HasForeignKey(m => m.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        }); 

        modelBuilder.Entity<ConnectSphere.Post.API.Entities.Mention>(entity =>
        {
            entity.HasKey(m => m.MentionId);
            entity.HasIndex(m => m.UserId);
        });
    } 
} 
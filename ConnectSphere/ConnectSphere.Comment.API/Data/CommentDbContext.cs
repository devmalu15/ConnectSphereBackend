using CommentEntity = ConnectSphere.Comment.API.Entities.Comment; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Comment.API.Data; 
  
public class CommentDbContext : DbContext 
{ 
    public CommentDbContext(DbContextOptions<CommentDbContext> options) : 
base(options) { } 
  
    public DbSet<CommentEntity> Comments => Set<CommentEntity>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<CommentEntity>(entity => 
        { 
            entity.HasKey(c => c.CommentId); 
            entity.Property(c => c.Content).IsRequired().HasMaxLength(1000); 
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); 
            // Soft-delete global query filter 
            entity.HasQueryFilter(c => !c.IsDeleted); 
        }); 
    } 
} 
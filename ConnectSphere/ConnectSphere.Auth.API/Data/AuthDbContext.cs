using Microsoft.EntityFrameworkCore; 
using ConnectSphere.Auth.API.Entities;
  
namespace ConnectSphere.Auth.API.Data; 
  
public class AuthDbContext : DbContext 
{ 
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { 
} 
  
    public DbSet<User> Users => Set<User>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
  
        modelBuilder.Entity<User>(entity => 
        { 
            entity.HasKey(u => u.UserId); 
            entity.Property(u => u.UserName).IsRequired().HasMaxLength(50); 
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100); 
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255); 
            entity.Property(u => u.Role).HasDefaultValue("User"); 
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); 
        }); 
    } 
} 
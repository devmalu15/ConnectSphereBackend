using ConnectSphere.Admin.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Admin.API.Data; 
  
public class AdminDbContext : DbContext 
{ 
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) 
{ } 
  
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<AuditLog>(entity => 
        { 
            entity.HasKey(a => a.AuditLogId); 
            entity.Property(a => a.Action).IsRequired().HasMaxLength(100); 
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(50); 
            entity.Property(a => a.EntityId).IsRequired().HasMaxLength(50); 
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); 
        }); 
    } 
} 
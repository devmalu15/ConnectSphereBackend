using ConnectSphere.Notif.API.Entities; 
using Microsoft.EntityFrameworkCore; 
  
namespace ConnectSphere.Notif.API.Data; 
  
public class NotifDbContext : DbContext 
{ 
    public NotifDbContext(DbContextOptions<NotifDbContext> options) : base(options) 
{ } 
  
    public DbSet<Notification> Notifications => Set<Notification>(); 
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder); 
        modelBuilder.Entity<Notification>(entity => 
        { 
            entity.HasKey(n => n.NotificationId); 
            entity.Property(n => n.Type).HasConversion<string>(); 
            entity.Property(n => n.TargetType).HasConversion<string>(); 
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); 
        }); 
    } 
}
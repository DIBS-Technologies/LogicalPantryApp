using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class ApplicationDataContext : DbContext
{
 

    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<TimeSlotSignup> TimeSlotSignups { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserRole composite key
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // User - Tenant relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId);
            //.OnDelete(DeleteBehavior.Cascade);

        // UserRole relationships
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);
        //.OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
            // .OnDelete(DeleteBehavior.Restrict);

        // TimeSlotSignup relationships
        modelBuilder.Entity<TimeSlotSignup>()
            .HasOne(ts => ts.TimeSlot)
            .WithMany(t => t.TimeSlotSignups)
            .HasForeignKey(ts => ts.TimeSlotId);
        //.OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeSlotSignup>()
            .HasOne(ts => ts.User)
            .WithMany(u => u.TimeSlotSignups)
            .HasForeignKey(ts => ts.UserId);
            //.OnDelete(DeleteBehavior.Cascade);

        // TimeSlot relationships
        modelBuilder.Entity<TimeSlot>()
            .HasOne(ts => ts.User)
            .WithMany(u => u.TimeSlots)
            .HasForeignKey(ts => ts.UserId);
           // .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeSlot>()
            .HasOne(ts => ts.Tenant)
            .WithMany(t => t.TimeSlots)
            .HasForeignKey(ts => ts.TenantId);
            //.OnDelete(DeleteBehavior.Cascade);
    }
}

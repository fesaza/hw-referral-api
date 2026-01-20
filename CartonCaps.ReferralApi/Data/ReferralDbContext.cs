using CartonCaps.ReferralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.Data;

/// <summary>
/// Database context using Entity Framework Core.
/// </summary>
public class ReferralDbContext(DbContextOptions<ReferralDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Referral> Referrals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(255);
        });

        // Configure Referral entity
        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .IsRequired();

            entity.Property(e => e.ReferralCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.ReferralCode)
                .IsUnique();

            entity.Property(e => e.ReferrerUserId)
                .IsRequired();

            entity.HasIndex(e => e.ReferrerUserId);

            // Configure relationship: ReferrerUser
            entity.HasOne(e => e.ReferrerUser)
                .WithMany(u => u.ReferralsCreated)
                .HasForeignKey(e => e.ReferrerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.ShareableLink)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.RefereeUserId);

            // Configure relationship: RefereeUser
            entity.HasOne(e => e.RefereeUser)
                .WithMany(u => u.ReferralsReceived)
                .HasForeignKey(e => e.RefereeUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

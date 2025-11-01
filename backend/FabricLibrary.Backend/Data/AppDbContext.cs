using FabricLibrary.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace FabricLibrary.Backend.Data;

/// <summary>
/// Application database context for Fabric Library.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            // GoogleSub must be unique (one Google account = one user)
            entity.HasIndex(e => e.GoogleSub)
                .IsUnique();

            // Email should be indexed for lookups
            entity.HasIndex(e => e.Email);

            // Required fields
            entity.Property(e => e.GoogleSub)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.DisplayName)
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

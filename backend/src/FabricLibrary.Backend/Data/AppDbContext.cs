using Microsoft.EntityFrameworkCore;
using FabricLibrary.Backend.Models;

namespace FabricLibrary.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.GoogleSub).IsUnique();
            b.Property(u => u.Email).IsRequired();
        });
    }
}

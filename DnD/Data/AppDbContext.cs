using Microsoft.EntityFrameworkCore;
using DnD.Models;

namespace DnD.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.Code).IsUnique();
            entity.Property(r => r.Code).HasMaxLength(10).IsRequired();
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.Property(c => c.NamePlayer).HasMaxLength(50).IsRequired();
            entity.Property(c => c.NameCharacter).HasMaxLength(100).IsRequired();
            entity.Property(c => c.ClassCharacter).HasMaxLength(50).IsRequired();
            entity.Property(c => c.RaceCharacter).HasMaxLength(50).IsRequired();

            entity.HasOne(c => c.Room)
                .WithMany(r => r.Characters)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
using DnD.Models;
using Microsoft.EntityFrameworkCore;

namespace DnD.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(e =>
        {
            e.HasKey(r => r.Id);

            // Код комнаты уникален: на это опирается повторная генерация в RoomsController
            e.HasIndex(r => r.Code).IsUnique();
            e.Property(r => r.Code).HasMaxLength(12).IsRequired();
            e.Property(r => r.Name).HasMaxLength(100).IsRequired();

            e.HasMany(r => r.Characters)
                .WithOne(c => c.Room)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.RoomId);

            e.Property(c => c.NamePlayer).HasMaxLength(100).IsRequired();
            e.Property(c => c.NameCharacter).HasMaxLength(100).IsRequired();
            e.Property(c => c.Alignment).HasMaxLength(50);
            e.Property(c => c.ClassId).HasMaxLength(50);
            e.Property(c => c.RaceId).HasMaxLength(50);
            e.Property(c => c.BackgroundId).HasMaxLength(50);
            e.Property(c => c.ArmorId).HasMaxLength(50);

            // Списки владений хранятся как text[] (Npgsql). Значение по умолчанию нужно,
            // чтобы миграция могла заполнить уже существующие строки.
            e.Property(c => c.SaveProficiencies).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.SkillProficiencies).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.SkillExpertise).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.ArmorTraining).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.WeaponProficiencies).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.ToolProficiencies).HasColumnType("text[]").HasDefaultValueSql("'{}'");
            e.Property(c => c.Feats).HasColumnType("text[]").HasDefaultValueSql("'{}'");

            // Вычисляемые свойства (без сеттера) EF и так не маппит; перечислены явно для наглядности.
            e.Ignore(c => c.ProficiencyBonus);
            e.Ignore(c => c.StrengthMod);
            e.Ignore(c => c.DexterityMod);
            e.Ignore(c => c.ConstitutionMod);
            e.Ignore(c => c.IntelligenceMod);
            e.Ignore(c => c.WisdomMod);
            e.Ignore(c => c.CharismaMod);
            e.Ignore(c => c.PassivePerception);
            e.Ignore(c => c.InitiativeBonus);
            e.Ignore(c => c.InitiativeScore);
            e.Ignore(c => c.HitDiceTotal);
            e.Ignore(c => c.HitDiceLeft);
            e.Ignore(c => c.HitDieType);
            e.Ignore(c => c.CarryCapacityLb);
            e.Ignore(c => c.EffectiveSpeed);
            e.Ignore(c => c.SpellSaveDc);
            e.Ignore(c => c.SpellAttackBonus);
            e.Ignore(c => c.XpForNextLevel);
        });
    }
}

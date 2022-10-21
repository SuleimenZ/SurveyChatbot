using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Models;

namespace SurveyChatbot.Database;

public class DatabaseContext : DbContext
{
#pragma warning disable CS8618
    public DbSet<User> Users { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Question> Questions { get; set; }
#pragma warning restore CS8618
    public DatabaseContext() : base() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("users");
        modelBuilder.Entity<Survey>()
            .ToTable("surveys")
            .Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Report>()
            .ToTable("reports")
            .Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Question>()
            .ToTable("questions")
            .Property(e => e.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<Survey>()
            .HasMany(s => s.Questions)
            .WithOne(q => q.Survey);
        modelBuilder.Entity<Survey>()
            .HasMany(s => s.Reports)
            .WithOne(r => r.Survey);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(DatabaseConfiguration.ConnectionString);
    }
}
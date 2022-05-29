using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Models;

namespace SurveyChatbot.Database;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ClosedQuestion> ClosedQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("users");
        modelBuilder.Entity<ClosedQuestion>()
            .ToTable("qClosed")
            .Property(p => p.Id).ValueGeneratedOnAdd();

        SeedTestData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void SeedTestData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClosedQuestion>()
            .HasData(new[]
            {
                new ClosedQuestion(
                    1,
                    "What is your favorite color?",
                    new[] { "Blue", "Red", "Green", "Purple" }),
                new ClosedQuestion(
                    2,
                    "Do you like living in Poland",
                    new[] { "Yes", "No" })
            });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(DatabaseConfiguration.ConnectionString);
    }
}

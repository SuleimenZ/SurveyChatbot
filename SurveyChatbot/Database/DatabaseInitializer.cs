using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Models;

namespace SurveyChatbot.Database;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync()
    {
        var context = new DatabaseContext();
        await InitializeAsync(context);
    }
    public static async Task InitializeAsync(DatabaseContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Surveys.AnyAsync()) return;

        var survey = new Survey(1, "Test", "Survey for testing purposes");

        var questions = new Question[]
        {
            new ClosedQuestion(
                1,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey),
            new ClosedQuestion(
                2,
                "Do you like living in Poland",
                new[] { "Yes", "No" },
                survey)
        };

        await context.Surveys.AddAsync(survey);
        foreach (Question question in questions)
        {
            await context.AddAsync(question);
        }

        await context.SaveChangesAsync();
    }
}
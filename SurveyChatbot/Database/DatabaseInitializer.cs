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
        var survey2 = new Survey(2, "Test2", "Survey for testing purposes");
        var survey3 = new Survey(3, "Test3", "Survey for testing purposes");
        var survey4 = new Survey(4, "Test4", "Survey for testing purposes");
        var survey5 = new Survey(5, "Test5", "Survey for testing purposes");
        var survey6 = new Survey(6, "Test6", "Survey for testing purposes");
        var survey7 = new Survey(7, "Test7", "Survey for testing purposes");

        var questions = new Question[]
        {
            new Question(
                1,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey),
            new Question(
                2,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey)
        };
        var questions2 = new Question[]
        {
            new Question(
                3,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey2),
            new Question(
                4,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey2)
        };
        var questions3 = new Question[]
        {
            new Question(
                5,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey3),
            new Question(
                6,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey3)
        };
        var questions4 = new Question[]
        {
            new Question(
                7,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey4),
            new Question(
                8,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey4)
        };
        var questions5 = new Question[]
{
            new Question(
                9,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey5),
            new Question(
                10,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey5)
};
        var questions6 = new Question[]
        {
            new Question(
                11,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey6),
            new Question(
                12,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey6)
        };
        var questions7 = new Question[]
        {
            new Question(
                13,
                "What is your favorite color?",
                new[] { "Blue", "Red", "Green", "Purple" },
                survey7),
            new Question(
                14,
                "Select your favorite type of sport",
                new[] { "Football", "Basketball", "Volleyball", "Golf" },
                survey7)
        };


        survey.AddQuestions(questions);
        survey2.AddQuestions(questions2);
        survey3.AddQuestions(questions3);
        survey4.AddQuestions(questions4);
        survey5.AddQuestions(questions5);
        survey6.AddQuestions(questions6);
        survey7.AddQuestions(questions7);

        await context.Surveys.AddAsync(survey);
        await context.Surveys.AddAsync(survey2);
        await context.Surveys.AddAsync(survey3);
        await context.Surveys.AddAsync(survey4);
        await context.Surveys.AddAsync(survey5);
        await context.Surveys.AddAsync(survey6);
        await context.Surveys.AddAsync(survey7);

        questions.AsParallel().ForAll(a => context.AddAsync(a));
        questions2.AsParallel().ForAll(a => context.AddAsync(a));
        questions3.AsParallel().ForAll(a => context.AddAsync(a));
        questions4.AsParallel().ForAll(a => context.AddAsync(a));
        questions5.AsParallel().ForAll(a => context.AddAsync(a));
        questions6.AsParallel().ForAll(a => context.AddAsync(a));
        questions7.AsParallel().ForAll(a => context.AddAsync(a));
        //foreach (Question question in questions)
        //{
        //    await context.AddAsync(question);
        //}
        await context.SaveChangesAsync();
    }
}
using System.Text.Json;
using SurveyChatbot.Database;
using SurveyChatbot.TelegramBot;
using SurveyChatbot.Repositories;
using SurveyChatbot.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var context = new DatabaseContext();
var bot = new Bot();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddNpgsql<DatabaseContext>(DatabaseConfiguration.ConnectionString);
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<SurveyRepository>();
builder.Services.AddScoped<QuestionRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.MaxDepth = 10;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapPost("api/surveys/add", (Survey survey) =>
{
    return bot.SurveyRepo.AddAsync(survey);
})
.WithName("AddSurvey");

await bot.Start();
app.Run();

await bot.Stop();
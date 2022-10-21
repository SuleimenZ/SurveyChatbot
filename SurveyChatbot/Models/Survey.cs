using SurveyChatbot.Utility;
using System.Text.Json.Serialization;

namespace SurveyChatbot.Models;

public class Survey
{
    [JsonIgnore]
    public long Id { get; }
    [JsonIgnore]
    public string SearchId { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Question> Questions { get; set; } = new List<Question>();
    [JsonIgnore]
    public List<Report> Reports { get; set; } = new List<Report>();

    public Survey() 
    {
        SearchId = SurveyIdGenerator.Generate();
    }
    public Survey(string? name, string? description) : this()
    {
        Name = name;
        Description = description;
    }
    public Survey(long id, string? name, string? description) : this()
    {
        Name = name;
        Description = description;
    }
    public Survey(string? name, string? description, Question[] questions) : this(name, description)
    {
        Questions = new(questions);
    }
    public Survey(long id, string? name, string? description, Question[] questions) : this(id, name, description)
    {
        Questions = new(questions);
    }

    public void AddQuestions(params Question[] questions)
    {
        Questions.AddRange(questions);
    }
}
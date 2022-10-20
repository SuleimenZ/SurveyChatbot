using SurveyChatbot.Utility;

namespace SurveyChatbot.Models;

public class Survey
{
    public long Id { get; }
    public string SearchId { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Question> Questions { get; private set; } = new List<Question>();
    public List<Report> Reports { get; private set; } = new List<Report>();

    public Survey() 
    {
        SearchId = SurveyIdGenerator.Generate();
    }
    public Survey(long id, string? name, string? description) : this()
    {
        Name = name;
        Description = description;
    }
    public Survey(long id, string? name, string? description, Question[] questions) : this(id, name, description)
    {
        Questions = new List<Question>(questions);
    }

    public void AddQuestions(params Question[] questions)
    {
        Questions.AddRange(questions);
    }
}
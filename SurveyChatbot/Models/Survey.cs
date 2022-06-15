namespace SurveyChatbot.Models;

public class Survey
{
    public long Id { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Question> Questions { get; private set; } = new List<Question>();
    public List<Report> Reports { get; private set; } = new List<Report>();

    private int _currentQuestionId;

    public Survey() {}
    public Survey(long id, string? name, string? description)
    {
        Id = id;
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

    public bool HasNextQuestion()
    {
        return _currentQuestionId < Questions.Count;
    }

    public Question GetNextQuestion()
    {
        if (HasNextQuestion())
        {
            var question =  Questions.ElementAt(_currentQuestionId);
            _currentQuestionId++;
            return question;
        }
        return GetBlankQuestion();
    }

    private Question GetBlankQuestion()
    {
        return new ClosedQuestion(-1, "Blank question", Array.Empty<string>(), this);
    }

    public void ResetQuestionCounter()
    {
        _currentQuestionId = 0;
    }
}
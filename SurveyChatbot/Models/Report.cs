using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyChatbot.Models;

public class Report
{
    public long Id { get; set; }
    public Survey Survey { get; set; }
    public User? User { get; set; }
    public int[] Answers { get; set; }

    public Report() {}

    public Report(Survey survey, User? user)
    {
        Survey = survey;
        User = user;
        Answers = new int[Survey.Questions.Count()];
    }

    public void AddAnswer(int questionId, string answer)
    {
        int answerId = Array.IndexOf(Survey.Questions[questionId].Answers, answer);
        Answers[questionId] += (int)Math.Pow(2, answerId);
    }

    public void RemoveAnswer(int questionId, string answer)
    {
        int answerId = Array.IndexOf(Survey.Questions[questionId].Answers, answer);
        Answers[questionId] -= (int)Math.Pow(2, answerId);
    }
    public string[] GetSelectedAnswers(int questionId)
    {
        return Survey.Questions[questionId].Answers.Where((answer, answerId) => ((Answers[questionId] >> answerId) & 1) != 0).ToArray();
    }
}
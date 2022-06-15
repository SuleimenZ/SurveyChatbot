namespace SurveyChatbot.Models;

public class Report
{
    public long Id { get; set; }
    public Survey Survey { get; set; }
    public User? User { get; set; }
    public string[] Answers { get; set; }
}
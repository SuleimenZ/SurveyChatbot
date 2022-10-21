using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Models;

public class Question
{
    [JsonIgnore]
    public long Id { get; init; }
    public string Text { get; init; }
    [JsonIgnore]
    public Survey Survey { get; set; }
    public string[] Answers { get; init; }
    public Question() {}
    public Question(string text, string[] answers, Survey survey)
    {
        Text = text;
        Survey = survey;
        Answers = answers;
    }

    public Question(long id, string text, string[] answers, Survey survey)
    {
        Id = id;
        Text = text;
        Survey = survey;
        Answers = answers;
    }
}
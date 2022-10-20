using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Models;

public class Question
{
    public long Id { get; init; }
    public string Text { get; init; }
    public Survey Survey { get; set; }
    public string[] Answers { get; init; }
    public Question() {}

    public Question(long id, string text, string[] answers, Survey survey)
    {
        Id = id;
        Text = text;
        Survey = survey;
        Answers = answers;
    }

    public IReplyMarkup GetQuestionMarkup()
    {
        var markupButtons = Answers
            .Select(a => InlineKeyboardButton.WithCallbackData(a, a));
        return new InlineKeyboardMarkup(new[] { markupButtons });
    }

    //public virtual IReplyMarkup GetQuestionMarkup()
    //{
    //    return new InlineKeyboardMarkup(new InlineKeyboardButton[] { });
    //}

    //public virtual string[] HandleResponse(Update update)
    //{
    //    return Array.Empty<string>();
    //}
}
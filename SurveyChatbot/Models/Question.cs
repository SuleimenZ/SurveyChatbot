using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Models;

public abstract class Question
{
    public long Id { get; init; }
    public string Text { get; init; }
    public Survey Survey { get; set; }

#pragma warning disable CS8618
    protected Question() {}
#pragma warning restore CS8618

    protected Question(long id, string text, Survey survey)
    {
        Id = id;
        Text = text;
        Survey = survey;
    }

    public virtual IReplyMarkup GetQuestionMarkup()
    {
        return new InlineKeyboardMarkup(new InlineKeyboardButton[] {});
    }
}
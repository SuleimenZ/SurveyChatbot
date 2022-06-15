using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SurveyChatbot.Models;

public class ClosedQuestion : Question
{
    public string[] Answers { get; init; }

    public ClosedQuestion() : base() { }

    public ClosedQuestion(long id, string text, string[] answers, Survey survey) : base(id, text, survey)
    {
        Answers = answers;
    }

    public override IReplyMarkup GetQuestionMarkup()
    {
        var markupButtons = Answers
            .Select(a => InlineKeyboardButton.WithCallbackData(a, a));
        return new InlineKeyboardMarkup(new[] { markupButtons });
    }
}
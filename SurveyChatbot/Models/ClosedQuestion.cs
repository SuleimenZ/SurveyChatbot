using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SurveyChatbot.Models;

public class ClosedQuestion : IQuestion
{
    public long Id { get; init; }
    public string Text { get; set; }
    public string[] Answers { get; set; }
    public IReplyMarkup GetQuestionMarkup()
    {
        var markupButtons = Answers
            .Select(a => InlineKeyboardButton.WithCallbackData(a, a));
        return new InlineKeyboardMarkup(new[] {markupButtons});
    }

    public ClosedQuestion(string text, string[] answers)
    {
        Text = text;
        Answers = answers;
    }

    public ClosedQuestion(long id, string text, string[] answers) : this(text, answers)
    {
        Id = id;
    }

    //public Task<Message> SendMessageAsync(ITelegramBotClient botClient, Message message)
    //{
    //}
}
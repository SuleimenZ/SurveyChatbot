using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Models;

public interface IQuestion
{
    public long Id { get; init; }
    public string Text { get; set; }
    public IReplyMarkup GetQuestionMarkup();
    //public Task<Message> SendMessageAsync(ITelegramBotClient botClient, Message message);
}
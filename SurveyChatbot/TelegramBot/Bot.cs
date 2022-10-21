using SurveyChatbot.Models;
using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Repositories;
using SurveyChatbot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using User = Telegram.Bot.Types.User;
using SurveyChatbot.Utility;

namespace SurveyChatbot.TelegramBot
{
    public class Bot
    {
        private CancellationTokenSource cts = new();
        private DatabaseContext context = new();

        public SurveyRepository SurveyRepo;
        public UserRepository UserRepo;
        public QuestionRepository QuestionRepo;
        public ReportRepository ReportRepo;
        public async Task Start()
        {
            var bot = new TelegramBotClient(Configuration.BotToken);
            User me = await bot.GetMeAsync();
            Console.Title = me.Username ?? "My awesome Bot";

            SurveyRepo = new(context);
            UserRepo = new(context);
            QuestionRepo = new(context);
            ReportRepo = new(context);
            Handler handler = new(SurveyRepo, UserRepo, QuestionRepo, ReportRepo);

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new();
            bot.StartReceiving(
                handler.HandleUpdateAsync,
                handler.HandleErrorAsync,
                receiverOptions,
                cts.Token);

            Console.WriteLine($"Start listening for @{me.Username ?? "bot"}");
        }

        public async Task Stop()
        {
            // Send cancellation request to stop bot
            await Task.FromResult(() => cts.Cancel());
        }
    }
}

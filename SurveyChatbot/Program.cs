using SurveyChatbot.Models;
using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Repositories;
using SurveyChatbot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using User = Telegram.Bot.Types.User;
using SurveyChatbot.Utility;

var bot = new TelegramBotClient(Configuration.BotToken);
User me = await bot.GetMeAsync();
Console.Title = me.Username ?? "My awesome Bot";

var context = new DatabaseContext();
await DatabaseInitializer.InitializeAsync(context);
SurveyRepository surveyRepo = new(context);
UserRepository userRepo = new(context);
QuestionRepository questionRepo = new(context);
ReportRepository reportRepo = new(context);

Handler handler = new(surveyRepo, userRepo, questionRepo, reportRepo);
using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new();
bot.StartReceiving(
    handler.HandleUpdateAsync,
    handler.HandleErrorAsync,
    receiverOptions,
    cts.Token);

Console.WriteLine($"Start listening for @{me.Username ?? "bot"}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();
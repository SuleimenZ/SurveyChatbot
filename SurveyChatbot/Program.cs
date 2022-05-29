using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace SurveyChatbot;

public static class Program
{
    private static TelegramBotClient? _bot;

    public static async Task Main()
    {
        _bot = new TelegramBotClient(Configuration.BotToken);

        User me = await _bot.GetMeAsync();
        Console.Title = me.Username ?? "My awesome Bot";

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new();
        _bot.StartReceiving(Handlers.HandleUpdateAsync,
                           Handlers.HandleErrorAsync,
                           receiverOptions,
                           cts.Token);

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }
}

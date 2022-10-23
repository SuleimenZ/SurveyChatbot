using SurveyChatbot.Models;
using SurveyChatbot.Repositories;
using SurveyChatbot.Utility;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using User = SurveyChatbot.Models.User;

namespace SurveyChatbot.TelegramBot;

public class Handler
{
    private readonly UserRepository _userRepo;
    private readonly QuestionRepository _questionRepo;
    private readonly ReportRepository _reportRepo;
    private readonly SurveyRepository _surveyRepo;
    private readonly MenuHandler _menuHandler;

    private Message? _lastBotMessage;
    private Message? _lastUserMessage;

    public Handler(SurveyRepository surveyRepo, UserRepository userRepo, QuestionRepository questionRepo, ReportRepository reportRepo)
    {
        _userRepo = userRepo;
        _questionRepo = questionRepo;
        _reportRepo = reportRepo;
        _surveyRepo = surveyRepo;
        _menuHandler = new MenuHandler(surveyRepo, questionRepo, reportRepo, userRepo);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            //UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        _lastUserMessage = message;
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/start" => Welcome(botClient, message),
            "/surveys" => ShowSurveys(botClient, message),
            "/search" => ShowSurveyBySearchId(botClient, message),
            "/results" => SendResults(botClient, message),
            _ => Usage(botClient, message)
        };
        await action;


        async Task<Message> Welcome(ITelegramBotClient botClient, Message message)
        {
            await RegisterUser(message.From);
            var welcome = $"Hello, {message.From!.FirstName}. " +
                          "Type /surveys to show surveys";
            return await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: welcome,
                                    replyMarkup: new ReplyKeyboardRemove());
        }

        async Task<Message> ShowSurveys(ITelegramBotClient botClient, Message message)
        {
            (string text, IReplyMarkup markup) = await _menuHandler.ShowSurveys();

            _lastBotMessage = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: markup,
                parseMode: ParseMode.Html);
            return _lastBotMessage;
        }

        async Task<Message> ShowSurveyBySearchId(ITelegramBotClient botClient, Message message)
        {
            (string text, IReplyMarkup markup) = await _menuHandler.ShowSurveyBySearchId(message);

            _lastBotMessage = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: markup,
                parseMode: ParseMode.Html);
            return _lastBotMessage;
        }

        async Task<Message> SendResults(ITelegramBotClient botClient, Message message)
        {
            (var stream, string fileName) = await _menuHandler.SendResults(message);
            if(stream == null)
            {
                _lastBotMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "No survey found with this id",
                    parseMode: ParseMode.Html);
                return _lastBotMessage;
            }
            _lastBotMessage = await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, fileName));
            return _lastBotMessage;
            }

        async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/surveys - show all surveys\n" +
                                 "/search XXXXXXXX - search survey by id\n" +
                                 "/results XXXXXXXX - get survey results";

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: usage,
                                                        replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private async Task RegisterUser(Telegram.Bot.Types.User telegramUser)
    {
        var appUser = new User(
            telegramUser.Id,
            telegramUser.FirstName,
            telegramUser.LastName,
            telegramUser.Username);

        if (!await _userRepo.ContainsAsync(appUser))
        {
            await _userRepo.AddAsync(appUser);
        }
    }

    private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        (string text, IReplyMarkup markup) = await _menuHandler.HandleUpdate(callbackQuery);

        await botClient.EditMessageTextAsync(
            chatId: _lastBotMessage.Chat.Id,
            messageId: _lastBotMessage.MessageId,
            text: text,
            replyMarkup: (InlineKeyboardMarkup)markup,
            parseMode: ParseMode.Html);
    }

    private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}

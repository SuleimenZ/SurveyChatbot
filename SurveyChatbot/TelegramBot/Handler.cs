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
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
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
        Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text) 
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/start" => Welcome(botClient, message),
            "/inline" => SendInlineKeyboard(botClient, message),
            "/surveys" => ShowSurveys(botClient, message),
            "/keyboard" => SendReplyKeyboard(botClient, message),
            _ => Usage(botClient, message)
        };
        await action;


        async Task<Message> Welcome(ITelegramBotClient botClient, Message message)
        {
            var welcome = $"Hello, {message.From!.FirstName}. " + 
                          "Type /surveys to show surveys";

            await RegisterUser(message.From);
            return await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: welcome,
                                    replyMarkup: new ReplyKeyboardRemove());
        }

        // You can process responses in BotOnCallbackQueryReceived handler
        async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            return _lastBotMessage;
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

        async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
            new[]
            {
                    new KeyboardButton[] { "1.1", "1.2" },
                    new KeyboardButton[] { "2.1", "2.2" },
            })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Choose",
                                                        replyMarkup: replyKeyboardMarkup);
        }

        async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/surveys - show all surveys\n" +
                                 "/testsurvey - start test survey";

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

    private async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                               results: results,
                                               isPersonal: true,
                                               cacheTime: 0);
    }

    private Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
    {
        Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}

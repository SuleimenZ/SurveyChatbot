using SurveyChatbot.Models;
using Microsoft.EntityFrameworkCore;
using SurveyChatbot.Database;
using SurveyChatbot.Repositories;
using SurveyChatbot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using User = Telegram.Bot.Types.User;
using SurveyChatbot.Utility;

var bot = new Bot();
await bot.Start();
Console.ReadLine();
await bot.Stop();
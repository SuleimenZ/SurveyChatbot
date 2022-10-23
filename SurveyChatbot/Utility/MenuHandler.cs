using SurveyChatbot.Models;
using SurveyChatbot.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Utility
{
    public class MenuHandler
    {
        private readonly SurveyRepository _surveyRepo;
        private readonly QuestionRepository _questionRepo;
        private readonly ReportRepository _reportRepo;
        private readonly UserRepository _userRepo;
        private readonly SurveyHandler _surveyHandler;

        private int _currentPageId = 0;
        private bool _surveyMode;

        public long SelectedSurveyId { get; set; } = -1;

        public MenuHandler(SurveyRepository surveyRepo, QuestionRepository questionRepo, ReportRepository reportRepo, UserRepository userRepo)
        {
            _surveyRepo = surveyRepo;
            _questionRepo = questionRepo;
            _reportRepo = reportRepo;
            _userRepo = userRepo;
            _surveyHandler = new SurveyHandler(this, surveyRepo, questionRepo, reportRepo, _userRepo);
        }

        public async Task<(string text, IReplyMarkup markup)> HandleUpdate(CallbackQuery callback)
        {
            return callback!.Data switch
            {
                _ when _surveyMode => await _surveyHandler.HandleUpdate(callback),
                "\u2800" => await ShowSurveys(),                                            //Blank
                "\u25b6" => await GetNextPage(),                                            //Right arrow
                "\u25c0" => await GetPreviousPage(),                                        //Left arrow
                _ => await Update(callback)
            };
        }

        public async Task<(string text, IReplyMarkup markup)> ShowSurveys()
        {
            _surveyMode = false;

            string text = "<b>List of all surveys</b>";
            var surveys = await _surveyRepo.GetAllAsync();

            var chunks = surveys.Chunk(6);
            if(chunks.Count() > 1)
            {
                text += $"\n\nPage {_currentPageId + 1}/{chunks.Count()}";
            }

            InlineKeyboardButton leftNavButton = _currentPageId switch
            {
                0 => InlineKeyboardButton.WithCallbackData("\u2800", "\u2800"),
                _ => InlineKeyboardButton.WithCallbackData("\u25c0", "\u25c0")
            };

            InlineKeyboardButton rightNavButton = _currentPageId switch
            {
                _ when _currentPageId == chunks.Count() - 1 => InlineKeyboardButton.WithCallbackData("\u2800", "\u2800"),
                _ => InlineKeyboardButton.WithCallbackData("\u25b6", "\u25b6")
            };

            //TEMPORARY UGLY SOLUTION

            List<InlineKeyboardButton[]> inlineButtonRowsList = new List<InlineKeyboardButton[]>();
            if (chunks.Count() > 1)
            {
                //Adding navigation row
                inlineButtonRowsList.Add(new InlineKeyboardButton[] { leftNavButton, rightNavButton });
            }
            //Adding survey rows
            inlineButtonRowsList.AddRange(chunks.ElementAt(_currentPageId).Chunk(3).Select((chunkOf3) => chunkOf3.Select(survey => InlineKeyboardButton.WithCallbackData(survey.Name, survey.Id.ToString())).ToArray()));

            //InlineKeyboardMarkup markup = new(new[]
            //{
            //    new InlineKeyboardButton[] { leftNavButton, rightNavButton }
            //    //chunks.ElementAt(_currentPageId).Chunk(3).Select((chunkOf3) => chunkOf3.Select(a => InlineKeyboardButton.WithCallbackData("1")).ToArray())   
            //});
            InlineKeyboardMarkup markup = new(inlineButtonRowsList.ToArray());

            return (text, markup);
        }

        public async Task<(string text, IReplyMarkup markup)> ShowSurveyBySearchId(Message message)
        {
            var split = message.Text!.Split();

            if (split.Length > 1 && isValid(split[1]))
            {
                _surveyMode = true;
                await _surveyHandler.SetupBySearchId(message.From!.Id, split[1]);
                return await _surveyHandler.GetSurveyDetails();
            }

            return await Task.FromResult(("Incorrect search id", InlineKeyboardMarkup.Empty()));
        }

        public async Task<Stream?> SendResults(Message message)
        {
            var split = message.Text!.Split();

            if (split.Length > 1 && isValid(split[1]))
            {
                var survey = await _surveyRepo.GetBySearchIdAsync(split[1]);
                //if (survey == null) { return await Task.FromResult((Nul)); }
                var reports = await _reportRepo.GetAllBySurveyIdAsync(survey.Id);

                ExcelSerializer es = new();

                var stream = es.Serialize(survey, reports);
                //if (stream == null) { return await Task.FromResult((false, "Unexpected error occured")); }
                return await stream;
            }

            //return await Task.FromResult((false, "Incorrect search id"));
            return null;
        }

        private bool isValid(string searchId)
        {
            if(searchId == null) { return false; }
            if(searchId.Length != 8) { return false; }
            if (searchId.Any(c => !char.IsLetterOrDigit(c))) { return false; }

            return true;
        }

        private async Task<(string text, IReplyMarkup markup)> GetNextPage()
        {
            _currentPageId++;
            return await ShowSurveys();
        }
        private async Task<(string text, IReplyMarkup markup)> GetPreviousPage()
        {
            _currentPageId--;
            return await ShowSurveys();
        }

        private async Task<(string text, IReplyMarkup markup)> EndSurvey()
        {
            await _surveyHandler.EndSurvey();
            _surveyMode = false;
            return await ShowSurveys();
        }

        private async Task<(string text, IReplyMarkup markup)> CancelSurvey()
        {
            _surveyMode = false;
            return await ShowSurveys();
        }

        private async Task<(string text, IReplyMarkup markup)> Update(CallbackQuery callback)
        {
            _surveyMode = true;
            await _surveyHandler.Setup(callback.From.Id, long.Parse(callback.Data));
            return await HandleUpdate(callback);
        }
    }
}

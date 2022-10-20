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
                //"\u274C" when _surveyMode => await CancelSurvey(),                          //Cross mark
                //"\u2705" when _surveyMode => await _surveyHandler.HandleUpdate(callback),   //Check mark
                //"\U0001F3C1" when _surveyMode => await EndSurvey(),                         //Chequered flag
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

        public void ResetSurveyId()
        {
            SelectedSurveyId = -1;
        }
    }
}

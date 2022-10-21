using SurveyChatbot.Models;
using SurveyChatbot.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SurveyChatbot.Utility
{
    public class SurveyHandler
    {
        private readonly SurveyRepository _surveyRepo;
        private readonly QuestionRepository _questionRepo;
        private readonly ReportRepository _reportRepo;
        private readonly UserRepository _userRepo;
        private readonly MenuHandler _menuHandler;

        private Report _report = new();
        private Survey _survey = new();

        private int _currentPageId = -2;

        public SurveyHandler(MenuHandler menuHandler, SurveyRepository surveyRepo, QuestionRepository questionRepo, ReportRepository reportRepo, UserRepository userRepo)
        {
            _surveyRepo = surveyRepo;
            _questionRepo = questionRepo;
            _reportRepo = reportRepo;
            _userRepo = userRepo;
            _menuHandler = menuHandler;
        }

        public async Task Setup(long userId, long surveyId)
        {
            _currentPageId = -2;
            _survey = (await _surveyRepo.GetByIdAsync(surveyId))!;
            var user = (await _userRepo.GetByIdAsync(userId));
            _report = new(_survey, user);

            //return await GetSurveyDetails();
        }

        public async Task SetupBySearchId(long userId, string surveySearchId)
        {
            _currentPageId = -2;
            _survey = (await _surveyRepo.GetBySearchIdAsync(surveySearchId))!;
            if (_survey == null) { return; }
            var user = (await _userRepo.GetByIdAsync(userId));
            _report = new(_survey, user);

            //return await GetSurveyDetails();
        }

        public async Task<(string text, IReplyMarkup markup)> HandleUpdate(CallbackQuery callback)
        {
            return callback.Data switch
            {
                _ when _currentPageId == -2 => await GetSurveyDetails(),
                "\U0001F3C1" => await GetConfirmationPage(),            //Chequered flag
                "\u274C" when _currentPageId == _survey.Questions.Count() - 1 => await GetQuestionById(_currentPageId), //Cross mark
                "\u2705" when _currentPageId == _survey.Questions.Count() - 1 => await EndSurvey(),                     //Check mark
                "\u274C" => await _menuHandler.ShowSurveys(),           //Cross mark
                "\u2705" => await GetQuestionById(0),          //Check mark
                "\u2800" => await GetQuestionById(_currentPageId),
                "\u25b6" => await GetNextQuestion(),
                "\u25c0" => await GetPreviousQuestion(),
                _ => await Update(callback.Data)
            };
        }

        private async Task<(string text, IReplyMarkup markup)> Update(string answer)
        {
            int answerId = Array.IndexOf(_survey.Questions[_currentPageId].Answers, answer);
            if (answerIsSelected(answerId))
            {
                _report.RemoveAnswer(_currentPageId, answer);
            }
            else
            {
                _report.AddAnswer(_currentPageId, answer);
            }
            return await GetQuestionById(_currentPageId);
        }

        private async Task<(string text, IReplyMarkup markup)> GetQuestionById(int questionId)
        {
            _currentPageId = questionId;
            Question question = _survey.Questions[questionId];

            string questionText = $"<b>Question {_currentPageId + 1}</b>\n<i>{question.Text}</i>";

            InlineKeyboardButton leftNavButton = _currentPageId switch
            {
                0 => InlineKeyboardButton.WithCallbackData("\u2800", "\u2800"),
                _ => InlineKeyboardButton.WithCallbackData("\u25c0", "\u25c0")
            };

            InlineKeyboardButton rightNavButton = _currentPageId switch
            {
                _ when _currentPageId == _survey.Questions.Count() - 1 => InlineKeyboardButton.WithCallbackData("\U0001F3C1", "\U0001F3C1"),
                _ => InlineKeyboardButton.WithCallbackData("\u25b6", "\u25b6")
            };

            InlineKeyboardMarkup markup = new(new[]
            {
                new InlineKeyboardButton[] { leftNavButton, rightNavButton },
                question.Answers.Select((answer, answerId) => InlineKeyboardButton.WithCallbackData($"{(answerIsSelected(answerId) ? "\u2705 " : "")}" + answer, answer))
            });;
            return await Task.FromResult((questionText, markup));
        }

        private async Task<(string text, IReplyMarkup markup)> GetNextQuestion()
        {
            int id = _currentPageId < _survey.Questions.Count() - 1 ? ++_currentPageId : _currentPageId;
            return await GetQuestionById(id);
        }

        private async Task<(string text, IReplyMarkup markup)> GetPreviousQuestion()
        {
            int id = _currentPageId > 0 ? --_currentPageId : _currentPageId;
            return await GetQuestionById(id);
        }

        public async Task<(string text, IReplyMarkup markup)> GetSurveyDetails()
        {
            if (_survey == null) { return await Task.FromResult(("Survey not found", InlineKeyboardMarkup.Empty())); }
            string text = "<b>Do you want to start this survey?</b>\n\n" +
                         $"<b>{_survey.Name}</b>\n" +
                         $"{_survey.Description}\n\n" +
                         $"Search ID: {_survey.SearchId}";

            InlineKeyboardMarkup markup = new(new[]
            {
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("\u274C", "\u274C"), InlineKeyboardButton.WithCallbackData("\u2705", "\u2705") }
            });

            _currentPageId++;
            return await Task.FromResult((text, markup));
        }

        private async Task<(string text, IReplyMarkup markup)> GetConfirmationPage()
        {
            if(_survey == null)
            {
                return await Task.FromResult(("Survey not found", InlineKeyboardMarkup.Empty()));
            }

            int numOfQuestions = _report.Answers.Count();
            int numOfQuestionsAnswered = _report.Answers.Where(a => a != 0).Count();
            string text = "<b>Do you want to end this survey?</b>\n\n" +
                         $"Questions answered: {numOfQuestionsAnswered}/{numOfQuestions}";

            InlineKeyboardMarkup markup = new(new[]
            {
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("\u274C", "\u274C"), InlineKeyboardButton.WithCallbackData("\u2705", "\u2705") }
            });

            return await Task.FromResult((text, markup));
        }

        public async Task<(string text, IReplyMarkup markup)> EndSurvey()
        {
            await _reportRepo.AddAsync(_report);
            return await _menuHandler.ShowSurveys();
        }

        private bool answerIsSelected(int answerId)
        {
            return ((_report.Answers[_currentPageId] >> answerId) & 1) != 0;
        }
    }
}


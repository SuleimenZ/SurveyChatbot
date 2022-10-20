using shortid;
using shortid.Configuration;

namespace SurveyChatbot.Utility
{
    public static class SurveyIdGenerator
    {
        private static readonly GenerationOptions _options = new GenerationOptions(false,false,8);
        public static string Generate()
        {
            return ShortId.Generate(_options);
        }
    }
}

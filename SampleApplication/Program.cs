using Less.API.NetFramework.KakaoBotAPI.Bot;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            RunSampleChatBot();
            // RunSampleQuizBot();
        }

        static void RunSampleChatBot()
        {
            var bot = new SampleChatBot("@@", ChatBot.Target.Self, "self");
            bot.StartTask();
        }

        static void RunSampleQuizBot()
        {
            var bot = new SampleQuizBot("여기에 채팅방 이름을 입력합니다.", ChatBot.Target.Group, "quiz");
            bot.StartTask();
        }
    }
}

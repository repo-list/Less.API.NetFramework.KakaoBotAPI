using Less.API.NetFramework.KakaoBotAPI.Bot;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string roomName = "나의 채팅방"; // 여기에 채팅방 이름을 입력합니다.
            var roomType = ChatBot.TargetTypeOption.Group; // 여기에 채팅방의 유형을 입력합니다. (Self : 본인과의 1:1 채팅방, Friend : 친구와의 1:1 채팅방, Group : 단톡방)
            string botRunnerName = "나의 닉네임"; // 여기에 당신의 해당 채팅방에서의 닉네임을 입력합니다. 오픈채팅의 경우, 당신의 닉네임이 방마다 다를 수 있으니 실수하지 않도록 주의하세요.
            string identifier = "MyQuiz"; // 여기에 이 봇을 구분하기 위한 특별한 식별자를 입력합니다. 파일 이름 및 봇을 식별하기 위한 여러 장소에서 사용되는 값입니다.
            RunSampleChatBot(roomName, roomType, botRunnerName, identifier);
            // RunSampleQuizBot(roomName, roomType, botRunnerName, identifier);
        }

        static void RunSampleChatBot(string roomName, ChatBot.TargetTypeOption roomType, string botRunnerName, string identifier)
        {
            var bot = new SampleChatBot(roomName, roomType, botRunnerName, identifier);
            bot.Start();
        }

        static void RunSampleQuizBot(string roomName, ChatBot.TargetTypeOption roomType, string botRunnerName, string identifier)
        {
            var bot = new SampleQuizBot(roomName, roomType, botRunnerName, identifier);
            bot.Start();
        }
    }
}

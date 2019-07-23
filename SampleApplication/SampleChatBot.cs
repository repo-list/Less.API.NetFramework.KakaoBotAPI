using System;
using Less.API.NetFramework.KakaoBotAPI.Bot;

namespace SampleApplication
{
    class SampleChatBot : ChatBot
    {
        public SampleChatBot(string roomName, Target type, string identifier) : base(roomName, type, identifier) {}

        protected override string GetDateChangeNotice(string content, DateTime sendTime)
        {
            string notice = content;
            Console.WriteLine(notice);
            return content;
        }

        protected override string GetStartNotice()
        {
            string notice = $"채팅봇이 시작되었습니다. 방 이름은 {RoomName}입니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetStopNotice()
        {
            string notice = $"채팅봇을 종료합니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetUserJoinNotice(string userName, DateTime sendTime)
        {
            string notice = $"{userName}님께서 들어오셨습니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetUserLeaveNotice(string userName, DateTime sendTime)
        {
            string notice = $"{userName}님께서 나가셨습니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override void InitializeBotSettings() {
            // 여기에 초기화 문장들을 작성합니다.
        }

        protected override void ParseMessage(string userName, string content, DateTime sendTime)
        {
            Console.WriteLine($"유저 이름 : {userName}, 내용 : {content}, 전송 시각 : {sendTime.ToString()}");

            string[] words = content.Split(' ');

            if (words[0].Equals("!종료")) StopTask();
        }
    }
}

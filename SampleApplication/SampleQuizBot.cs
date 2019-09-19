using System;
using System.Text;
using System.Threading;
using Less.API.NetFramework.KakaoBotAPI.Bot;
using Less.API.NetFramework.KakaoBotAPI.Model;

namespace SampleApplication
{
    class SampleQuizBot : QuizBot
    {
        const string Version = "v1.0.0";
        const string AlertHeader = "[알림]";

        const string RoomNameAlias = "퀴즈방";

        public SampleQuizBot(string roomName, TargetTypeOption type, string botRunnerName, string identifier) : base(roomName, type, botRunnerName, identifier) {}

        protected override string GetDateChangeNotice(string content, DateTime sendTime)
        {
            string notice = content;
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetStartNotice()
        {
            string notice = $"{AlertHeader}퀴즈봇 {Version} 버전이 실행되었습니다.\n";
            notice += "!명령어 를 입력하여 기능을 확인하세요.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetStopNotice()
        {
            string notice = $"{AlertHeader}퀴즈봇을 종료합니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetUserJoinNotice(string userName, DateTime sendTime)
        {
            string notice = $"{AlertHeader}반갑습니다, {userName}님. {Window.RoomName}에 오신 것을 환영합니다.\n";
            Console.WriteLine(notice);
            return notice;
        }

        protected override string GetUserLeaveNotice(string userName, DateTime sendTime)
        {
            string notice = $"{AlertHeader}{userName} 님이 퇴장하셨습니다.";
            Console.WriteLine(notice);
            return notice;
        }

        protected override void InitializeBotSettings()
        {
            base.InitializeBotSettings(); // QuizBot 객체에서 퀴즈 목록 갱신 작업을 진행하므로, 반드시 base 메서드를 호출해주어야 합니다.

            // 여기에 초기화 문장들을 작성합니다.
        }

        protected override void ParseMessage(string userName, string content, DateTime sendTime)
        {
            Console.WriteLine($"유저 이름 : {userName}, 내용 : {content}, 전송 시각 : {sendTime.ToString()}");

            QuizUser user = FindUserByNickname(userName);

            switch (content)
            {
                case "!명령어": OnRequestingCommands(); break;
                case "!정보": OnRequestingInfo(user); break;
                case "!퀴즈 실행": OnRequestingQuizStart(); break;
                case "!퀴즈 중지": OnRequestingQuizStop(); break;
                case "!종료": OnRequestingQuit(); break;
            }
        }

        private void OnRequestingCommands()
        {
            string message = $"[명령어]\n";
            message += "!정보 : 유저 정보를 확인합니다.\n";
            message += "!퀴즈 실행 : 퀴즈를 실행합니다.\n";
            message += "!퀴즈 중지 : 퀴즈를 중지합니다.\n";
            message += "!종료 : 퀴즈봇을 종료합니다.";

            SendMessage(message);
            Thread.Sleep(SendMessageInterval);
        }

        private void OnRequestingInfo(QuizUser user)
        {
            StringBuilder availableTitles = new StringBuilder();
            foreach (Title title in user.AvailableTitles) availableTitles.Append($"{title.Name}, ");
            availableTitles.Remove(availableTitles.Length - 2, 2);

            string message = $"[유저 정보 - {RoomNameAlias}]\n";
            message += $"닉네임 : {user.Nickname}\n";
            message += $"레벨 : {user.Level}\n";
            message += $"경험치 : {user.Experience}\n";
            message += $"머니 : {user.Money}\n";
            message += $"세대 : {user.Generation}세대\n";
            message += $"타이틀 : {user.CurrentTitle.Name}";

            SendMessage(message);
            Thread.Sleep(SendMessageInterval);
        }

        private void OnRequestingQuizStart()
        {
            SendMessage("속담 퀴즈를 시작합니다.");
            Thread.Sleep(SendMessageInterval);

            var quizType = Quiz.TypeOption.General; // 퀴즈의 유형 (일반 퀴즈)
            string[] subjects = new string[] { "속담" }; // 퀴즈 주제 목록
            int requestQuizCount = 5; // 요청할 퀴즈의 총 개수
            int minQuizCount = 3; // 최소 퀴즈 개수 (만약 데이터 파일에 넣어둔 문제 수가 이 값보다 작으면, 퀴즈가 실행되지 않음)
            int quizTimeLimit = 20; // 퀴즈 문제 하나당 풀이 제한시간 (초 단위)
            int bonusExperience = 10; // 퀴즈 정답 시 추가로 받는 경험치
            int bonusMoney = 50; // 퀴즈 정답 시 추가로 받는 머니
            int idleTimeLimit = 180; // 잠수일 경우, 퀴즈가 자동으로 중단되기까지 걸리는 시간 (초 단위)
            bool showSubject = true; // 퀴즈 출제 시 주제 표시 여부

            StartQuiz(quizType, subjects, requestQuizCount, minQuizCount, quizTimeLimit, bonusExperience, bonusMoney, idleTimeLimit, showSubject);
        }

        private void OnRequestingQuizStop()
        {
            SendMessage("퀴즈를 중지합니다.");
            Thread.Sleep(SendMessageInterval);

            StopQuiz();
        }

        private void OnRequestingQuit()
        {
            SendMessage("종료를 요청하셨습니다.");
            Thread.Sleep(SendMessageInterval);
            this.Stop(); // == StopQuiz() + ChatBot.Stop()
        }

        protected override void UpdateUserProfile(QuizUser user, int bonusExperience, int bonusMoney)
        {
            user.Experience += bonusExperience;
            user.Money += bonusMoney;

            SaveUserData();
        }
    }
}

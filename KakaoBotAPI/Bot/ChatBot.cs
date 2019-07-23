using Less.API.NetFramework.KakaoBotAPI.People;
using Less.API.NetFramework.KakaoBotAPI.Util;
using Less.API.NetFramework.KakaoTalkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Less.API.NetFramework.KakaoBotAPI.Bot
{
    public abstract class ChatBot
    {
        public static int SendMessageIntervalShort = 500;
        public static int SendMessageInterval = 1500;
        public static int SendMessageIntervalLong = 2500;
        public static int GetMessageInterval = 50;
        public static int LoadMessageInterval = 500;

        protected const string DataPath = @"data\";
        protected const string DataExtension = ".dat";

        protected const string ProfilePath = DataPath + @"profile\";
        protected const string ProfileNameHeader = "profile_";
        protected const string ProfileExtension = XmlHelper.FileExtension;

        protected const string ShortcutPath = DataPath + @"shortcut\";
        protected const string ShortcutNameHeader = "shortcut_";
        protected const string ShortcutExtension = DataExtension;
        static string[] ShortcutComments = new string[] { "; 숏컷 명령어 (줄인 명령어 = 실제 명령어)", "; 예시 : !가위바위보 = !게임 가위바위보", "; 영어의 경우 대소문자를 구분하지 않습니다.", "; 세미콜론(;)이 문장 맨 앞에 붙을 경우 주석으로 인식합니다." };

        /// <summary>
        /// 채팅방의 이름
        /// </summary>
        public string RoomName { get; }
        /// <summary>
        /// 채팅방의 유형. 본인과 대화할 경우 Target.Self, 친구 1명과 대화할 경우 Target.Friend, 단톡방에서 대화할 경우 Target.Group 값을 가짐.
        /// </summary>
        public Target Type { get; }
        /// <summary>
        /// 채팅방에 부여할 특별한 식별자.
        /// </summary>
        public string Identifier { get; }
        /// <summary>
        /// GetMessagesUsingClipboard 메서드를 호출한 뒤에 마지막으로 분석한 메시지의 인덱스 값.
        /// </summary>
        public int LastMessageIndex { get; set; }

        protected KakaoTalk.KTChatWindow Window;
        protected bool MinimizeWindow;
        protected Thread Runner;
        protected bool IsRunning;

        protected List<User> Users = new List<User>();

        public enum Target { Self = 1, Friend, Group }

        /// <summary>
        /// 새로운 채팅봇 객체를 생성합니다.
        /// </summary>
        /// <param name="roomName">봇을 가동할 채팅방 이름</param>
        /// <param name="type">채팅방의 유형 (Self : 자기 자신, Friend : 친구, Group : 단톡방)</param>
        /// <param name="identifier">이 유저 또는 그룹에게 부여할 특별한 식별자. 파일 저장이나 쿼리 생성 시 활용하기 위한 값입니다.</param>
        public ChatBot(string roomName, Target type, string identifier)
        {
            RoomName = roomName;
            Type = type;
            Identifier = identifier;
        }

        /// <summary>
        /// 채팅봇을 가동합니다.
        /// </summary>
        /// <param name="minimizeWindow">봇 가동 완료 시 채팅창을 최소화할지 여부</param>
        public void StartTask(bool minimizeWindow = false)
        {
            if (!KakaoTalk.IsInitialized()) KakaoTalk.InitializeManually();
            MinimizeWindow = minimizeWindow;

            if (Type == Target.Self) Window = KakaoTalk.MainWindow.Friends.StartChattingWithMyself(RoomName, minimizeWindow);
            else if (Type == Target.Friend) Window = KakaoTalk.MainWindow.Friends.StartChattingWith(RoomName, minimizeWindow);
            else if (Type == Target.Group) Window = KakaoTalk.MainWindow.Chatting.StartChattingAt(RoomName, minimizeWindow);

            KakaoTalk.Message[] messages;
            while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(KakaoTalk.ProgressCheckInterval);
            Thread.Sleep(LoadMessageInterval);
            messages = Window.GetMessagesUsingClipboard();
            int messageCount = messages.Length;

            string notice = GetStartNotice();
            if (notice != null) Window.SendText(notice);
            Thread.Sleep(SendMessageInterval);

            LastMessageIndex = (messageCount - 1) + 1; // (messageCount - 1) + greetingMessage

            Runner = new Thread(new ThreadStart(Run));
            IsRunning = true;
            Runner.Start();
        }

        /// <summary>
        /// 채팅봇을 중지합니다.
        /// </summary>
        public void StopTask()
        {
            IsRunning = false;

            string notice = GetStopNotice();
            if (notice != null) Window.SendText(notice);
            Thread.Sleep(SendMessageInterval);

            Window.Dispose();
        }

        /// <summary>
        /// 채팅봇 스레드가 수행할 명령을 지정합니다.
        /// 가능하면 이 메서드의 내용을 임의로 변경하지 마십시오. 꼭 필요한 경우에만 오버라이드하는 것을 권장합니다.
        /// </summary>
        protected virtual void Run()
        {
            KakaoTalk.Message[] messages;
            KakaoTalk.MessageType messageType;
            string userName, content;
            DateTime sendTime;

            Users = LoadUserData();
            InitializeBotSettings();

            while (IsRunning)
            {
                // 메시지 목록 얻어오기
                Thread.Sleep(GetMessageInterval);
                messages = Window.GetMessagesUsingClipboard();
                if (messages == null) continue;
                if (messages.Length == LastMessageIndex + 1) continue;

                sendTime = DateTime.Now;

                // 메시지 목록 분석
                for (int i = LastMessageIndex + 1; i < messages.Length; i++)
                {
                    messageType = messages[i].Type;
                    userName = messages[i].UserName;
                    content = messages[i].Content;

                    LastMessageIndex++;
                    if (messageType == KakaoTalk.MessageType.Unknown) continue;
                    else if (messageType == KakaoTalk.MessageType.DateChange) SendDateChangeNotice(content, sendTime);
                    else if (messageType == KakaoTalk.MessageType.UserJoin) SendUserJoinNotice(userName, sendTime);
                    else if (messageType == KakaoTalk.MessageType.UserLeave) SendUserLeaveNotice(userName, sendTime);
                    else if (messageType == KakaoTalk.MessageType.Talk) ProcessUserMessage(userName, content, sendTime);
                }
            }
        }

        /// <summary>
        /// 유저 데이터 세이브 파일의 경로를 가져옵니다.
        /// </summary>
        protected string GetUserDataFilePath()
        {
            return ProfilePath + ProfileNameHeader + Identifier + ProfileExtension;
        }

        /// <summary>
        /// 파일 시스템으로부터 유저 데이터를 불러옵니다.
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 해당 유저 타입을 리턴하고 필요한 노드 값들을 전부 불러올 수 있도록 new 키워드를 통하여 재정의하십시오.
        /// </summary>
        protected virtual List<User> LoadUserData()
        {
            var document = GetUserDataDocument();
            List<User> data = new List<User>();

            string nickname;

            foreach (var node in document.ChildNodes)
            {
                nickname = node.GetValue("nickname");

                data.Add(new User(nickname));
            }

            return data;
        }

        protected XmlHelper.Document GetUserDataDocument()
        {
            string path = GetUserDataFilePath();
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
            var helper = new XmlHelper(path);
            if (!File.Exists(path)) helper.CreateFile("list", new List<XmlHelper.Node>());

            return helper.ReadFile();
        }

        /// <summary>
        /// 파일 시스템에 유저 데이터를 저장합니다.
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 필요한 노드들을 전부 생성하여 저장할 수 있도록 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void SaveUserData()
        {
            string path = GetUserDataFilePath();

            var helper = new XmlHelper(path);
            var nodeList = new List<XmlHelper.Node>();
            XmlHelper.Node node;

            for (int i = 0; i < Users.Count; i++)
            {
                node = helper.GetNewNode("user");
                node.AddValue("nickname", Users[i].Nickname);

                nodeList.Add(node);
            }

            helper.CreateFile("list", nodeList);
        }

        /// <summary>
        /// 닉네임을 통해 유저 정보를 얻어옵니다.
        /// 만약 현재 해당 닉네임을 가진 유저가 기록에 존재하지 않으면 null을 반환합니다.
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 해당 유저 타입을 리턴하도록 new 키워드를 통하여 재정의하십시오.
        /// </summary>
        /// <param name="userName">유저의 닉네임</param>
        protected virtual User FindUserByNickname(string userName)
        {
            foreach (User user in Users) if (user.Nickname.Equals(userName)) return user;

            return null;
        }

        /// <summary>
        /// 해당 닉네임을 가진 유저 정보를 새로 등록합니다.
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 해당 유저 타입을 등록하고 리턴하도록 new 키워드를 통하여 재정의하십시오.
        /// </summary>
        /// <param name="userName">새로 등록될 유저의 닉네임</param>
        protected virtual User AddNewUser(string userName)
        {
            var user = new User(userName);
            Users.Add(user);
            SaveUserData();
            return user;
        }

        /// <summary>
        /// 날짜 변경 메시지가 출력되는 시점에 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="content">바뀐 요일에 대한 정보</param>
        /// <param name="sendTime">요일 정보가 채팅창에 출력된 시각</param>
        protected virtual void SendDateChangeNotice(string content, DateTime sendTime)
        {
            string notice = GetDateChangeNotice(content, sendTime);

            if (notice != null) Window.SendText(notice);
            Thread.Sleep(SendMessageIntervalShort);
        }

        /// <summary>
        /// 유저 입장 시 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="userName">입장한 유저의 닉네임</param>
        /// <param name="sendTime">입장한 시각</param>
        protected virtual void SendUserJoinNotice(string userName, DateTime sendTime)
        {
            string notice = GetUserJoinNotice(userName, sendTime);

            if (notice != null) Window.SendText(notice);
            Thread.Sleep(SendMessageIntervalShort);
        }

        /// <summary>
        /// 유저 퇴장 시 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="userName">퇴장한 유저의 닉네임</param>
        /// <param name="sendTime">퇴장한 시각</param>
        protected virtual void SendUserLeaveNotice(string userName, DateTime sendTime)
        {
            string notice = GetUserLeaveNotice(userName, sendTime);

            if (notice != null) Window.SendText(notice);
            Thread.Sleep(SendMessageIntervalShort);
        }

        /// <summary>
        /// 유저가 메시지를 입력한 경우 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="userName">메시지를 보낸 유저의 닉네임</param>
        /// <param name="content">메시지 내용</param>
        /// <param name="sendTime">메시지를 보낸 시각</param>
        protected virtual void ProcessUserMessage(string userName, string content, DateTime sendTime)
        {
            if (FindUserByNickname(userName) == null) AddNewUser(userName);
            foreach (User user in Users)
            {
                if (user.Nickname.Equals(userName))
                {
                    if (user.IsIgnored) return;
                    else break;
                }
            }

            ParseMessage(userName, content, sendTime);
            Thread.Sleep(SendMessageIntervalShort);
        }

        /// <summary>
        /// 숏컷 명령어 파일의 경로를 가져옵니다.
        /// </summary>
        protected string GetShortcutFilePath()
        {
            return ShortcutPath + ShortcutNameHeader + Identifier + ShortcutExtension;
        }

        /// <summary>
        /// 유저가 보낸 메시지에 숏컷 명령어가 사용되었는지 확인하고, 만약 사용되었다면 원래 명령어로 복구하여 되돌려줍니다.
        /// </summary>
        /// <param name="words">유저가 보낸 메시지의 내용</param>
        protected string GetOriginalCommand(string content)
        {
            string path = GetShortcutFilePath();

            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                File.WriteAllLines(path, ShortcutComments);
                return content;
            }

            string[] shortcutTexts = File.ReadAllText(path).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string text;
            string[] sentWords = content.Split(' ');
            string[] leftWords, rightWords;
            bool isCorrectShortcut;
            string[] resultWords = null;
            var result = new StringBuilder();
            foreach (string s in shortcutTexts)
            {
                text = s.Trim();
                if (text.Length == 0) continue;
                if (text.IndexOf(';') == 0) continue;
                int equalSignIndex = text.IndexOf('=');
                if (equalSignIndex == 0 || equalSignIndex == text.Length - 1) continue;

                leftWords = text.Substring(0, equalSignIndex).TrimEnd().Split(' ');
                isCorrectShortcut = true;
                for (int i = 0; i < leftWords.Length; i++)
                {
                    if (!leftWords[i].Equals(sentWords[i])) { isCorrectShortcut = false; break; }
                }
                if (!isCorrectShortcut) continue;

                rightWords = text.Substring(equalSignIndex + 1).TrimStart().Split(' ');
                resultWords = new string[rightWords.Length + (sentWords.Length - leftWords.Length)];
                for (int i = 0; i < rightWords.Length; i++) resultWords[i] = rightWords[i];
                for (int i = rightWords.Length; i < resultWords.Length; i++) resultWords[i] = sentWords[i - rightWords.Length + leftWords.Length];
                for (int i = 0; i < resultWords.Length; i++) result.Append($"{resultWords[i]} ");
                result.Remove(result.Length - 1, 1);
            }

            if (resultWords == null) return content;
            else return result.ToString();
        }

        /// <summary>
        /// 본격적으로 메시지 분석을 시작하기 전에, 필요한 초기화 작업을 진행할 수 있도록 작성된 메서드입니다.
        /// </summary>
        protected abstract void InitializeBotSettings();

        /// <summary>
        /// 봇이 시작될 때 채팅창에 보낼 안내 메시지를 리턴 값으로 지정합니다. null을 리턴하면 안내 메시지가 전송되지 않습니다.
        /// </summary>
        protected abstract string GetStartNotice();

        /// <summary>
        /// 봇이 종료될 때 채팅창에 보낼 안내 메시지를 리턴 값으로 지정합니다. null을 리턴하면 안내 메시지가 전송되지 않습니다.
        /// </summary>
        protected abstract string GetStopNotice();

        /// <summary>
        /// 날짜 변경 메시지가 출력되는 시점에 채팅창에 보낼 안내 메시지를 리턴 값으로 지정합니다. null을 리턴하면 안내 메시지가 전송되지 않습니다.
        /// </summary>
        /// <param name="content">바뀐 요일에 대한 정보</param>
        /// <param name="sendTime">요일 정보가 채팅창에 출력된 시각</param>
        protected abstract string GetDateChangeNotice(string content, DateTime sendTime);

        /// <summary>
        /// 유저 입장 시 채팅창에 보낼 안내 메시지를 리턴 값으로 지정합니다. null을 리턴하면 안내 메시지가 전송되지 않습니다.
        /// </summary>
        /// <param name="userName">입장한 유저의 닉네임</param>
        /// <param name="sendTime">입장한 시각</param>
        protected abstract string GetUserJoinNotice(string userName, DateTime sendTime);

        /// <summary>
        /// 유저 퇴장 시 채팅창에 보낼 안내 메시지를 리턴 값으로 지정합니다. null을 리턴하면 안내 메시지가 전송되지 않습니다.
        /// </summary>
        /// <param name="userName">퇴장한 유저의 닉네임</param>
        /// <param name="sendTime">퇴장한 시각</param>
        protected abstract string GetUserLeaveNotice(string userName, DateTime sendTime);

        /// <summary>
        /// 본격적인 메시지 분석 작업을 시작합니다.
        /// </summary>
        /// <param name="userName">메시지를 보낸 유저의 닉네임</param>
        /// <param name="content">메시지 내용</param>
        /// <param name="sendTime">메시지를 보낸 시각</param>
        protected abstract void ParseMessage(string userName, string content, DateTime sendTime);
    }
}

using Less.API.NetFramework.KakaoBotAPI.Model;
using Less.API.NetFramework.KakaoBotAPI.Util;
using Less.API.NetFramework.KakaoTalkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Less.API.NetFramework.KakaoBotAPI.Bot
{
    /// <summary>
    /// KakaoBotAPI에서 제공하는 채팅봇 중 가장 기본 형태가 되는 클래스입니다.<para/>
    /// 만약 봇 커스터마이징 시 최고 레벨의 자유도를 원한다면, 이 클래스를 상속하여 사용하십시오.
    /// </summary>
    public abstract class ChatBot : IKakaoBot
    {
        /// <summary>
        /// 채팅창에 메시지나 이미지, 이모티콘 등을 보낸 후 적용할 지연 시간<para/>
        /// 이 값을 너무 적게 설정할 경우, 카카오톡의 매크로 방지에 걸릴 수 있으니 주의하십시오.
        /// </summary>
        public static int SendMessageInterval = 500;

        /// <summary>
        /// 채팅창에서 메시지 로그를 폴링으로 가져오는 데 적용할 지연 시간<para/>
        /// 이 값을 너무 적게 설정할 경우, 시스템에 과부하가 발생할 수 있으니 주의하십시오.
        /// </summary>
        public static int GetMessageInterval = 50;

        /// <summary>
        /// 데이터 파일들이 위치하는 최상단 경로
        /// </summary>
        protected const string DataPath = @"data\";

        /// <summary>
        /// Well-Known 파일 확장자들(XML, INI 등)을 제외한 데이터 파일들의 기본 확장자
        /// </summary>
        protected const string DataFileDefaultExtension = ".dat";

        /// <summary>
        /// 설정 파일들이 위치하는 최상단 경로
        /// </summary>
        protected const string ConfigPath = @"config\";

        /// <summary>
        /// 설정 파일들의 기본 확장자
        /// </summary>
        protected const string ConfigFileDefaultExtension = ".cfg";

        /// <summary>
        /// Profile 파일의 이름
        /// </summary>
        protected const string ProfileFileName = "profile";

        /// <summary>
        /// Profile 파일의 확장자
        /// </summary>
        protected const string ProfileFileExtension = XmlHelper.FileExtension;

        /// <summary>
        /// 바로가기 명령어를 열거한 파일의 이름
        /// </summary>
        protected const string ShortcutFileName = "cmd_shortcuts";

        /// <summary>
        /// 바로가기 명령어를 열거한 파일의 확장자
        /// </summary>
        protected const string ShortcutFileExtension = ConfigFileDefaultExtension;

        /// <summary>
        /// 봇의 금지어-대체어 목록을 열거한 파일의 이름
        /// </summary>
        protected const string BotLimitedWordsFileName = "bot_limitedWords";

        /// <summary>
        /// 봇의 금지어-대체어 목록을 열거한 파일의 확장자
        /// </summary>
        protected const string BotLimitedWordsFileExtension = IniHelper.FileExtension;

        /// <summary>
        /// 텍스트 파일의 확장자
        /// </summary>
        protected const string TextFileExtension = ".txt";

        /// <summary>
        /// 바로가기 명령어 목록을 담고 있는 배열<para/>
        /// GetOriginalCommand 메서드에서 초기화됩니다.
        /// </summary>
        protected string[] ShortcutTexts = null;

        /// <summary>
        /// 봇의 금지어 목록<para/>
        /// RefreshLimitedWordsList 메서드를 통해 데이터가 삽입됩니다.
        /// </summary>
        protected List<string> LimitedWords = new List<string>();

        /// <summary>
        /// 봇의 대체어 목록<para/>
        /// RefreshLimitedWordsList 메서드를 통해 데이터가 삽입됩니다.
        /// </summary>
        protected List<string> ReplacedWords = new List<string>();

        /// <summary>
        /// 자동으로 생성되는 새 User 객체의 채팅을 무시할지에 대한 여부<para/>
        /// true : 채팅을 무시합니다.<para/>
        /// false : 채팅을 무시하지 않습니다.
        /// </summary>
        protected static bool IsNewUserIgnored = false;

        /// <summary>
        /// 채팅방의 이름<para/>
        /// 이 값이 정확하지 않으면 봇이 동작하지 않으므로, 꼭 제대로 확인하십시오.
        /// </summary>
        public string RoomName { get; }

        /// <summary>
        /// 대상 채팅방의 유형<para/>
        /// ChatBot.TargetTypeOption.Self : 본인. 봇을 적용하는 상대가 본인인 경우 이 값으로 설정합니다.<para/>
        /// ChatBot.TargetTypeOption.Friend : 친구. 봇을 적용하는 채팅방이 친구와의 1:1 대화방인 경우 이 값으로 설정합니다.<para/>
        /// ChatBot.TargetTypeOption.Group : 단톡. 봇을 적용하는 채팅방이 단톡방인 경우 이 값으로 설정합니다.
        /// </summary>
        public TargetTypeOption Type { get; }

        /// <summary>
        /// 봇 돌리미의 해당 채팅방에서의 닉네임<para/>
        /// 돌리미에게만 특정 권한을 부여하여 명령어를 제한할 수 있도록 하기 위해 만들어진 값입니다.
        /// </summary>
        public string BotRunnerName { get; }

        /// <summary>
        /// 채팅방에 부여할 특별한 식별자<para/>
        /// 이 값은 각각의 봇 인스턴스를 구별하는 식별자입니다.<para/>
        /// 방 이름을 다르게 하더라도 식별자가 같으면 같은 설정 정보를 이용하도록 설계되었으며, 반대로 방 이름을 같게 하더라도 식별자가 다르면 다른 설정 정보를 이용하도록 설계되었습니다.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// 마지막으로 분석한 메시지의 인덱스 값<para/>
        /// GetMessagesUsingClipboard 메서드를 호출한 뒤에 메시지의 개수가 달라졌다면, 이 값을 증가 또는 감소시키는 원리입니다.
        /// </summary>
        public int LastMessageIndex { get; set; }

        /// <summary>
        /// 봇이 돌아가는 채팅방 인스턴스
        /// </summary>
        protected KakaoTalk.KTChatWindow Window;

        /// <summary>
        /// 봇 인스턴스에 대한 Main Thread
        /// </summary>
        protected Thread MainTaskRunner;

        /// <summary>
        /// 봇의 메인 Thread가 실행 중인지에 대한 여부<para/>
        /// true : StartMainTask 메서드가 호출되었으며, StopMainTask 메서드가 아직 호출되지 않은 상태를 나타냅니다.<para/>
        /// false : StartMainTask 메서드가 호출되지 않았거나, StopMainTask 메서드가 호출되어 봇이 종료된 상태를 나타냅니다.
        /// </summary>
        protected bool IsMainTaskRunning;

        /// <summary>
        /// 봇 인스턴스에 대한 유저 목록
        /// </summary>
        protected List<User> Users = new List<User>();

        /// <summary>
        /// 대상 채팅방의 유형<para/>
        /// ChatBot.TargetTypeOption.Self : 본인. 봇을 적용하는 상대가 본인인 경우 이 값으로 설정합니다.<para/>
        /// ChatBot.TargetTypeOption.Friend : 친구. 봇을 적용하는 채팅방이 친구와의 1:1 대화방인 경우 이 값으로 설정합니다.<para/>
        /// ChatBot.TargetTypeOption.Group : 단톡. 봇을 적용하는 채팅방이 단톡방인 경우 이 값으로 설정합니다.
        /// </summary>
        public enum TargetTypeOption { Self = 1, Friend, Group }

        /// <summary>
        /// 채팅봇 객체를 생성합니다.
        /// </summary>
        /// <param name="roomName">채팅방의 이름</param>
        /// <param name="type">대상 채팅방의 유형</param>
        /// <param name="botRunnerName">봇 돌리미의 해당 채팅방에서의 닉네임</param>
        /// <param name="identifier">채팅방에 부여할 특별한 식별자</param>
        public ChatBot(string roomName, TargetTypeOption type, string botRunnerName, string identifier)
        {
            RoomName = roomName;
            Type = type;
            BotRunnerName = botRunnerName;
            Identifier = identifier;
        }

        /// <summary>
        /// 봇 인스턴스를 시작합니다.<para/>
        /// 하나의 봇 인스턴스에 여러 개의 Thread에 기반한 작업을 수행할 경우, 이 메서드를 오버라이드하여 적절한 조치를 취하십시오.
        /// </summary>
        public virtual void Start()
        {
            StartMainTask();
        }

        /// <summary>
        /// 봇 인스턴스를 중지합니다.<para/>
        /// 하나의 봇 인스턴스에 여러 개의 Thread에 기반한 작업을 수행할 경우, 이 메서드를 오버라이드하여 적절한 조치를 취하십시오.
        /// </summary>
        public virtual void Stop()
        {
            StopMainTask();
        }

        /// <summary>
        /// 봇 객체를 참조하여 메시지를 전송합니다.<para/>
        /// 메시지를 전송하기 전에 금지어 목록에 걸리는 단어가 있는지 확인하고, 있을 경우 대체어로 바꾸어 전송합니다.<para/>
        /// 전송이 성공하려면, IsMainTaskRunning이 true여야 합니다. 
        /// </summary>
        /// <param name="message">전송할 메시지</param>
        /// <returns>전송의 성공 여부</returns>
        public bool SendMessage(string message)
        {
            for (int i = 0; i < LimitedWords.Count; i++)
            {
                if (message.Contains(LimitedWords[i]) && !ReplacedWords[i].Contains(LimitedWords[i])) message = message.Replace(LimitedWords[i], ReplacedWords[i]);
            }
            Window.SendText(message);

            return true;
        }

        /// <summary>
        /// 봇 객체를 참조하여 이미지를 전송합니다.<para/>
        /// 전송이 성공하려면, IsMainTaskRunning이 true여야 합니다.
        /// </summary>
        /// <param name="path">전송할 이미지 파일의 경로</param>
        /// <returns>전송의 성공 여부</returns>
        public bool SendImage(string path)
        {
            Window.SendImageUsingClipboard(path);

            return true;
        }

        /// <summary>
        /// 봇 객체를 참조하여 이모티콘을 전송합니다.<para/>
        /// 전송이 성공하려면, IsMainTaskRunning이 true여야 합니다.
        /// </summary>
        /// <param name="emoticon">전송할 카카오톡 이모티콘</param>
        /// <returns>전송의 성공 여부</returns>
        public bool SendEmoticon(KakaoTalk.Emoticon emoticon)
        {
            Window.SendEmoticon(emoticon);

            return true;
        }

        /// <summary>
        /// 봇 인스턴스의 메인 Thread를 시작합니다.
        /// </summary>
        /// <param name="minimizeWindow">봇 가동 완료 시 채팅창을 최소화할지 여부</param>
        protected void StartMainTask(bool minimizeWindow = false)
        {
            if (!KakaoTalk.IsInitialized()) KakaoTalk.InitializeManually();

            if (Type == TargetTypeOption.Self) Window = KakaoTalk.MainWindow.Friends.StartChattingWithMyself(RoomName, minimizeWindow);
            else if (Type == TargetTypeOption.Friend) Window = KakaoTalk.MainWindow.Friends.StartChattingWith(RoomName, minimizeWindow);
            else if (Type == TargetTypeOption.Group) Window = KakaoTalk.MainWindow.Chatting.StartChattingAt(RoomName, minimizeWindow);

            KakaoTalk.Message[] messages;
            while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);

            SendMainTaskStartNotice();

            while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);
            LastMessageIndex = messages.Length - 1;
            MainTaskRunner = new Thread(new ThreadStart(RunMain));
            MainTaskRunner.Start();
        }

        /// <summary>
        /// 봇 인스턴스의 메인 Thread를 중지합니다.
        /// </summary>
        protected void StopMainTask()
        {
            IsMainTaskRunning = false;
            SendMainTaskStopNotice();
            Window.Dispose();
        }

        /// <summary>
        /// 봇의 메인 Thread가 수행할 명령을 기술합니다.<para/>
        /// 가능하면 이 메서드의 내용을 변경하지 마십시오. 꼭 필요한 경우에만 오버라이드하는 것을 권장합니다.
        /// </summary>
        protected virtual void RunMain()
        {
            IsMainTaskRunning = true;

            RefreshUserData();
            RefreshLimitedWordsList();
            InitializeBotSettings();

            KakaoTalk.Message[] messages;
            KakaoTalk.MessageType messageType;
            string userName, content;
            DateTime sendTime;

            while (IsMainTaskRunning)
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
        /// 파일 시스템으로부터 유저 데이터를 불러옵니다.<para/>
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 필요한 노드 데이터들을 전부 불러올 수 있도록 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <returns>유저 목록</returns>
        protected virtual void RefreshUserData()
        {
            var document = GetUserDataDocument();
            var users = new List<User>();

            string nickname;
            bool isIgnored;
            string value;

            for (int i = 0; i < document.ChildNodes.Count; i++)
            {
                var node = document.ChildNodes[i];

                nickname = node.GetData("nickname");

                value = node.GetData("isIgnored");
                if (value == "true") isIgnored = true;
                else if (value == "false") isIgnored = false;
                else throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 isIgnored 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                users.Add(new User(nickname, isIgnored));
            }

            Users = users;
        }

        /// <summary>
        /// 유저 데이터가 기록된 XML 문서를 가져옵니다.
        /// </summary>
        /// <returns>XML 문서 객체</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected XmlHelper.Document GetUserDataDocument()
        {
            string path = ConfigPath + $"{Identifier}\\" + ProfileFileName + ProfileFileExtension;
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

            var helper = new XmlHelper(path);
            if (!File.Exists(path)) helper.CreateFile("list", new List<XmlHelper.Node>());

            return helper.ReadFile();
        }

        /// <summary>
        /// 파일 시스템에 유저 데이터를 저장합니다.<para/>
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 필요한 노드들을 전부 생성하여 저장할 수 있도록 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void SaveUserData()
        {
            string path = ConfigPath + $"{Identifier}\\" + ProfileFileName + ProfileFileExtension;
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

            var helper = new XmlHelper(path);
            var nodeList = new List<XmlHelper.Node>();
            XmlHelper.Node node;

            for (int i = 0; i < Users.Count; i++)
            {
                node = new XmlHelper.Node("user");
                node.AddData("nickname", Users[i].Nickname);
                node.AddData("isIgnored", Users[i].IsIgnored ? "true" : "false");

                nodeList.Add(node);
            }

            helper.CreateFile("list", nodeList);
        }

        /// <summary>
        /// 닉네임을 통해 유저 정보를 얻어옵니다.<para/>
        /// 만약 현재 해당 닉네임을 가진 유저가 기록에 존재하지 않으면 null을 반환합니다.<para/>
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 해당 유저 타입을 리턴하도록 new 키워드를 통하여 재정의하십시오.
        /// </summary>
        /// <param name="userName">유저의 닉네임</param>
        /// <returns>유저 객체</returns>
        protected virtual User FindUserByNickname(string userName)
        {
            foreach (User user in Users) if (user.Nickname == userName) return user;

            return null;
        }

        /// <summary>
        /// 해당 닉네임을 가진 유저 정보를 새로 등록합니다.<para/>
        /// 만약 이 클래스 상속 시 새로운 유저 클래스를 같이 만든다면, 이 메서드가 해당 유저 타입을 등록하고 리턴하도록 new 키워드를 통하여 재정의하십시오.
        /// </summary>
        /// <param name="userName">새로 등록될 유저의 닉네임</param>
        /// <returns>유저 객체</returns>
        protected virtual User AddNewUser(string userName)
        {
            var user = new User(userName, IsNewUserIgnored);
            Users.Add(user);
            SaveUserData();
            return user;
        }

        /// <summary>
        /// 봇이 본격적으로 가동되기 전 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void SendMainTaskStartNotice()
        {
            Window.SendText("채팅봇을 시작합니다.");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 봇이 종료될 때 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void SendMainTaskStopNotice()
        {
            Window.SendText("채팅봇을 종료합니다.");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 날짜 변경 메시지가 출력되는 시점에 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="content">바뀐 요일에 대한 정보</param>
        /// <param name="sendTime">요일 정보가 채팅창에 출력된 시각</param>
        protected virtual void SendDateChangeNotice(string content, DateTime sendTime)
        {
            Window.SendText($"날짜가 변경되었습니다. ({content})");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 유저 입장 시 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="userName">입장한 유저의 닉네임</param>
        /// <param name="sendTime">입장한 시각</param>
        protected virtual void SendUserJoinNotice(string userName, DateTime sendTime)
        {
            Window.SendText($"{userName}님이 입장하셨습니다.");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 유저 퇴장 시 수행할 행동을 지정합니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="userName">퇴장한 유저의 닉네임</param>
        /// <param name="sendTime">퇴장한 시각</param>
        protected virtual void SendUserLeaveNotice(string userName, DateTime sendTime)
        {
            Window.SendText($"{userName}님이 퇴장하셨습니다.");
            Thread.Sleep(SendMessageInterval);
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
                if (user.Nickname == userName)
                {
                    if (user.IsIgnored) return;
                    else break;
                }
            }

            ParseMessage(userName, GetOriginalCommand(content), sendTime);
        }

        /// <summary>
        /// 유저가 보낸 메시지에 바로가기 명령어가 사용되었는지 확인하고, 만약 사용되었다면 원래 명령어로 복구하여 되돌려줍니다.
        /// </summary>
        /// <param name="words">유저가 보낸 메시지의 내용</param>
        /// <returns>복원된 최종 명령어</returns>
        protected string GetOriginalCommand(string content)
        {
            if (ShortcutTexts == null) ShortcutTexts = ReadShortcutFile();

            string text;
            string[] sentWords = content.Split(' ');
            string[] leftWords, rightWords;
            bool isCorrectShortcut;
            string[] resultWords = null;
            var result = new StringBuilder();
            foreach (string s in ShortcutTexts)
            {
                text = s.Trim();
                if (text.Length == 0) continue;
                if (text.IndexOf(';') == 0) continue;
                int equalSignIndex = text.IndexOf('=');
                if (equalSignIndex == 0 || equalSignIndex == text.Length - 1) continue;

                leftWords = text.Substring(0, equalSignIndex).TrimEnd().Split(' ');
                isCorrectShortcut = leftWords.Length <= sentWords.Length ? true : false;
                if (isCorrectShortcut)
                {
                    for (int i = 0; i < leftWords.Length; i++)
                    {
                        if (leftWords[i].ToLower() != sentWords[i].ToLower()) { isCorrectShortcut = false; break; }
                    }
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
        /// 바로가기 명령어를 열거한 파일의 내용을 가져옵니다.
        /// </summary>
        /// <returns>파일의 내용</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private string[] ReadShortcutFile()
        {
            string path = ConfigPath + $"{Identifier}\\" + ShortcutFileName + ShortcutFileExtension;
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

            if (!File.Exists(path)) GenerateShortcutFile(path);
            return File.ReadAllText(path).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 바로가기 명령어 파일을 생성합니다.
        /// </summary>
        /// <param name="path">바로가기 파일의 경로</param>
        private void GenerateShortcutFile(string path)
        {
            string message = Properties.Resources.cmd_shortcuts;

            File.WriteAllLines(path, message.Split(new string[] { "\r\n" }, StringSplitOptions.None), new UTF8Encoding(false));
        }

        /// <summary>
        /// 금지어-대체어 목록을 갱신합니다.
        /// </summary>
        protected void RefreshLimitedWordsList()
        {
            string[] lines = ReadLimitedWordsFile();
            var limitedWords = new List<string>();
            var replacedWords = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "[Contents]")
                {
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].IndexOf(";") == 0) continue;
                        if (lines[j].Split('=').Length != 2) continue;
                        string[] pair = lines[j].Split('=');
                        string key = pair[0].Trim();
                        string value = pair[1].Trim();
                        if (key.Length == 0 || value.Length == 0) continue;
                        limitedWords.Add(key);
                        replacedWords.Add(value);
                    }
                }
            }

            LimitedWords = limitedWords;
            ReplacedWords = replacedWords;
        }

        /// <summary>
        /// 금지어-대체어 목록을 열거한 파일의 내용을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private string[] ReadLimitedWordsFile()
        {
            string path = ConfigPath + $"{Identifier}\\" + BotLimitedWordsFileName + BotLimitedWordsFileExtension;
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

            if (!File.Exists(path)) GenerateBotLimitedWordsFile(path);

            return File.ReadAllLines(path);
        }

        /// <summary>
        /// 금지어-대체어 목록 파일을 생성합니다.
        /// </summary>
        /// <param name="path">금지어-대체어 목록 파일의 경로</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void GenerateBotLimitedWordsFile(string path)
        {
            string message = Properties.Resources.bot_limitedWords;

            File.WriteAllLines(path, message.Split(new string[] { "\r\n" }, StringSplitOptions.None), Encoding.Unicode);
        }

        /// <summary>
        /// 본격적으로 메시지 분석을 시작하기 전에, 필요한 초기화 작업을 진행할 수 있도록 작성된 메서드입니다.
        /// </summary>
        protected abstract void InitializeBotSettings();

        /// <summary>
        /// 본격적인 메시지 분석 작업을 시작합니다.
        /// </summary>
        /// <param name="userName">메시지를 보낸 유저의 닉네임</param>
        /// <param name="content">메시지 내용</param>
        /// <param name="sendTime">메시지를 보낸 시각</param>
        protected abstract void ParseMessage(string userName, string content, DateTime sendTime);
    }
}

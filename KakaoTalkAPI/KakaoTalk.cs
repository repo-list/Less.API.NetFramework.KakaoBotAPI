
/*
 * < KakaoTalk API >
 * 
 * @ Author : Eric Kim
 * 
 * @ Nickname : Less
 * 
 * @ Email : syusa5537@gmail.com
 * 
 * @ ProductName : Less.API.NetFramework.KakaoTalkAPI
 * 
 * @ Version : 0.3.0
 * 
 * @ License : The Non-Profit Open Software License v3.0 (NPOSL-3.0) (https://opensource.org/licenses/NPOSL-3.0)
 * -> 이 API에는 NPOSL-3.0 오픈소스 라이선스가 적용되며, 사용자는 절대 영리적 목적으로 이 API를 사용해서는 안 됩니다.
 * 
 * @ Description : An automation API which is applicable to the software "KakaoTalk for Windows" created and distributed by Kakao Corp.
 * -> 카카오 회사에서 제작하고 배포하는 "KakaoTalk for Windows" 소프트웨어에 적용 가능한 자동화 API입니다.
 * 
 * @ Other Legal Responsibilities :
 * -> Developers using this automation API should never try to harm or damage servers of "Kakao Corp." by any kinds of approaches.
 * -> 이 자동화 API를 이용하는 개발자들은 절대 어떠한 방법으로도 카카오 회사의 서버에 피해를 입히려는 시도를 해서는 안 됩니다.
 * -> Developers using this automation API should never try to take any undesired actions which are opposite to the Kakao Terms of Service (http://www.kakao.com/policy/terms?type=s).
 * -> 이 자동화 API를 이용하는 개발자들은 절대 카카오 서비스 약관 (http://www.kakao.com/policy/terms?type=s)에 반하는 바람직하지 않은 행동들을 취해서는 안 됩니다.
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Less.API.NetFramework.WindowsAPI;

namespace Less.API.NetFramework.KakaoTalkAPI
{
    /// <summary>
    /// PC 카카오톡용 API 목록을 담고 있는 클래스입니다.
    /// </summary>
    public sealed class KakaoTalk
    {
        // 버전 정보
        private static string FullApiVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 현재 카카오톡 API의 버전 값을 담고 있는 스트링입니다.
        /// </summary>
        public readonly static string ApiVersion = FullApiVersion.Substring(0, FullApiVersion.LastIndexOf('.'));

        // 변경해도 되는 값 목록 (개발 시에 유동적으로 변경해서 쓰도록 함)
        /// <summary>
        /// 카카오톡이 설치된 경로입니다. 만약 설치 경로를 수동으로 지정했을 경우, 이 값을 변경하여 사용하도록 합니다.
        /// </summary>
        public static string InstallPath = @"C:\Program Files (x86)\Kakao\KakaoTalk\KakaoTalk.exe";

        public static int DefaultChattingCheckInterval = 20; // 권장 : 20 ~ 50 (채팅방 Task 검사 스레드의 Sleep 시간)
        public static int ProgressCheckInterval = 20; // 권장 20 ~ 50 (기본적인 폴링 시 전반적으로 사용되는 값)
        public static int UIChangeInterval = 1000; // 권장 : 1000 이상
        public static int MouseClickInterval = 200; // 권장 : 200 이상
        public static int KeyPressInterval = 200; // 권장 : 200 이상
        public static int ButtonActivateInterval = 200; // 권장 : 200 이상
        public static int SendActivateInterval = 100; // 권장 : 100 이상
        public static int ImageCheckInterval = 0; // 권장 : 10 이하, 검사 속도를 빠르게 하는 것이 무엇보다 중요함
        public static int ImageCheckLimit = 500; // 권장 : 1000 이하
        public static int EmoticonCheckInterval1 = 0; // 권장 : 10 이하, 검사 속도를 빠르게 하는 것이 무엇보다 중요함
        public static int EmoticonCheckLimit1 = 500; // 권장 : 1000 이하
        public static int EmoticonCheckInterval2 = 0; // 권장 : 10 이하, 검사 속도를 빠르게 하는 것이 무엇보다 중요함
        public static int EmoticonCheckLimit2 = 500; // 권장 : 1000 이하
        public static int EmoticonCheckInterval3 = 0; // 권장 : 10 이하, 검사 속도를 빠르게 하는 것이 무엇보다 중요함
        public static int EmoticonCheckLimit3 = 500; // 권장 : 1000 이하
        public static int PostDelay = 100; // 권장 : 100 이상
        public static int WindowCloseInterval = 200; // 권장 : 200 이상

        public static int CategoryPossibleMaxCount = 100; // 만약 소유한 이모티콘이 이 수치보다 많으면 계산 시 문제 발생 가능

        public static string DateChangeNotifierName = "System"; // KakaoTalk.Message이 DateChange 타입인 경우 Username 값

        // 변경해선 안 되는 상수값 목록
        const string MainWindowTitle = "카카오톡";
        const string MainWindowClass = "EVA_Window_Dblclk";
        const string ProcessName = "KakaoTalk";

        const string LoginWindowClass = "EVA_Window";
        const string ChatWindowClass = "#32770";

        const string ImageDialogCaption = "";
        const string ImageDialogClass = ChatWindowClass;

        const int EmoticonDialogWidth = 330;
        const int EmoticonDialogHeight = 450;
        const string EmoticonDialogCaption = "";
        const string EmoticonDialogClass = MainWindowClass;
        const string EmoticonFirstChildClass = "EVA_ChildWindow";
        const string EmoticonSecondChildClass1 = "EVA_ChildWindow_Dblclk";
        const string EmoticonSecondChildClass2 = "_EVA_CustomScrollCtrl";
        const string EmoticonSecondChildClass3 = "EVA_VH_ListControl_Dblclk";
        const string EmoticonSecondNextClass = EmoticonSecondChildClass1;

        // 클래스 인스턴스 변수 목록
        public static KTMainWindow MainWindow;
        static List<KTChatWindow> ChatWindows = new List<KTChatWindow>();
        static readonly object Clipboard = new object();

        // 열거형 자료 목록
        public enum MainWindowTab { Friends = 1, Chatting, More }

        // 초기화 여부
        private static bool Initialized = false;

        // KakaoTalk.Message 및 KakaoTalk.Emoticon 구조체에 관한 정의는 맨 아래쪽에 있습니다.

        private KakaoTalk() { }

        /// <summary>
        /// 현재 카카오톡 창이 열려 있는지 확인합니다. 열려 있는 상태이면 true를, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public static bool IsWindowOpen()
        {
            IntPtr hMainWindow = WinAPI.FindWindow(MainWindowClass, MainWindowTitle);
            return hMainWindow != IntPtr.Zero ? true : false;
        }

        /// <summary>
        /// 현재 로그인이 된 상태인지 확인합니다. 로그인된 상태이면 true를, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public static bool IsLoggedIn()
        {
            IntPtr hLoginWindow = WinAPI.FindWindow(LoginWindowClass, MainWindowTitle);
            IntPtr hMainWindow = WinAPI.FindWindow(MainWindowClass, MainWindowTitle);
            IntPtr hAdArea = WinAPI.GetWindow(hMainWindow, WinAPI.GW_CHILD);
            for (int i = 0; i < 2; i++) hAdArea = WinAPI.GetWindow(hAdArea, WinAPI.GW_HWNDNEXT);

            return (IsWindowOpen() && hLoginWindow == IntPtr.Zero && hAdArea != IntPtr.Zero) ? true : false;
        }

        /// <summary>
        /// 로그인 패널이 열려 있는 여부를 반환합니다. 열려 있는 상태이면 true를, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        private static bool IsLoginWindowLoaded()
        {
            IntPtr hLoginWindow = WinAPI.FindWindow(LoginWindowClass, MainWindowTitle);
            return hLoginWindow != IntPtr.Zero ? true : false;
        }

        /// <summary>
        /// 해당 이름을 가진 채팅방이 열려 있는지 검사합니다. 열려 있는 상태이면 true를, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="roomName">검사할 채팅방의 이름</param>
        public static bool IsChatRoomOpen(string roomName)
        {
            IntPtr hChatRoom = WinAPI.FindWindow(ChatWindowClass, roomName);
            return hChatRoom != IntPtr.Zero ? true : false;
        }

        /// <summary>
        /// PC 카카오톡 프로그램을 실행합니다.
        /// </summary>
        public static void Run()
        {
            if (IsWindowOpen()) return;

            Process process = Process.Start(InstallPath);
            while (!IsWindowOpen()) Thread.Sleep(ProgressCheckInterval);
        }

        /// <summary>
        /// 로그인 패널의 핸들값과 이메일, 비밀번호를 입력받아 로그인을 시작합니다.
        /// 반드시 Run 메서드를 실행한 후에 호출해야 하며, 그렇지 않았을 경우 KakaoTalk.NotOpenException 예외가 발생합니다.
        /// 또한 이미 로그인이 되어 있는 경우, KakaoTalk.AlreadyLoggedInException 예외가 발생합니다.
        /// </summary>
        /// <param name="email">사용자의 이메일 (카카오계정)</param>
        /// <param name="password">사용자의 비밀번호</param>
        public static void Login(string email, string password)
        {
            if (!IsWindowOpen()) throw new NotOpenException();
            else if (IsLoggedIn()) throw new AlreadyLoggedInException();

            // 로그인 패널이 정상적으로 열릴 때까지 대기
            while (!IsLoginWindowLoaded()) Thread.Sleep(ProgressCheckInterval);

            IntPtr hLoginWindow = WinAPI.FindWindow(LoginWindowClass, MainWindowTitle);
            IntPtr hEditEmail = WinAPI.GetWindow(hLoginWindow, WinAPI.GW_CHILD);
            IntPtr hEditPassword = WinAPI.GetWindow(hEditEmail, WinAPI.GW_HWNDNEXT);

            Thread.Sleep(UIChangeInterval);

            WinAPI.SetEditText(hEditEmail, email, WinAPI.Encoding.Unicode);
            WinAPI.SetEditText(hEditPassword, password, WinAPI.Encoding.Unicode);
            Thread.Sleep(ButtonActivateInterval);
            WinAPI.PressKeyInBackground(hLoginWindow, WinAPI.KeyCode.VK_ENTER);
            while (!IsLoggedIn()) Thread.Sleep(ProgressCheckInterval);

            InitializeAppSettings();
        }

        public static bool IsInitialized()
        {
            return Initialized;
        }

        private static void InitializeAppSettings()
        {
            if (!IsLoggedIn()) throw new NotLoggedInException();

            IntPtr hMainWindow = WinAPI.FindWindow(MainWindowClass, MainWindowTitle);
            MainWindow = new KTMainWindow(hMainWindow);

            Initialized = true;
        }

        /// <summary>
        /// 앱 세팅을 수동으로 초기화합니다.
        /// Run 및 Login 메서드를 통해 카카오톡을 실행했을 경우에는 따로 호출할 필요가 없으며,
        /// 이미 로그인된 상태에서 Login 메서드를 호출하여 KakaoTalk.AlreadyLoggedInException이 발생한 경우에 호출합니다.
        /// 그렇지 않으면 KakaoTalk.NotLoggedInException 예외가 발생합니다.
        /// </summary>
        public static void InitializeManually()
        {
            InitializeAppSettings();
        }

        /// <summary>
        /// 카카오톡 메인 창을 제어할 수 있도록 만들어진 클래스입니다.
        /// </summary>
        public class KTMainWindow
        {
            // 핸들 목록
            IntPtr RootHandle { get; }
            IntPtr TabAreaHandle { get; }
            IntPtr AdAreaHandle { get; }

            // 클래스 인스턴스 변수 목록
            public FriendsTab Friends { get; }
            public ChattingTab Chatting { get; }
            public MoreTab More { get; }

            internal KTMainWindow(IntPtr hMainWindow)
            {
                RootHandle = hMainWindow;
                TabAreaHandle = WinAPI.GetWindow(hMainWindow, WinAPI.GW_CHILD);
                for (int i = 0; i < 2; i++) AdAreaHandle = WinAPI.GetWindow(TabAreaHandle, WinAPI.GW_HWNDNEXT);

                IntPtr hFriendTab = WinAPI.GetWindow(TabAreaHandle, WinAPI.GW_CHILD);
                IntPtr hChattingTab = WinAPI.GetWindow(hFriendTab, WinAPI.GW_HWNDNEXT);
                IntPtr hMoreTab = WinAPI.GetWindow(hChattingTab, WinAPI.GW_HWNDNEXT);

                Friends = new FriendsTab(hFriendTab);
                Chatting = new ChattingTab(hChattingTab);
                More = new MoreTab(hMoreTab);
            }

            /// <summary>
            /// 카카오톡 메인 창에서 원하는 탭으로 이동합니다.
            /// </summary>
            /// <param name="targetTab">이동할 탭</param>
            public void ChangeTabTo(MainWindowTab targetTab)
            {
                for (int i = 0; i < 2; i++) WinAPI.PressKeyInBackground(Friends.RootHandle, WinAPI.KeyCode.VK_LEFT); // 무조건 현재 탭을 1번으로 만들고 계산
                for (int i = 1; i < (int)targetTab; i++) WinAPI.PressKeyInBackground(Friends.RootHandle, WinAPI.KeyCode.VK_RIGHT);
            }

            public class Tab
            {
                internal IntPtr RootHandle { get; }

                internal Tab(IntPtr rootHandle)
                {
                    RootHandle = rootHandle;
                }
            }

            /// <summary>
            /// 메인 창의 친구 탭을 제어할 수 있도록 만들어진 클래스입니다.
            /// </summary>
            public class FriendsTab : Tab
            {
                // 변경해선 안 되는 상수값 목록
                const short FirstSearchItemX = 86;
                const short FirstSearchItemY = 60;

                // 핸들 목록
                IntPtr SearchHandle { get; }
                IntPtr FriendsListHandle { get; }
                IntPtr SearchResultListHandle { get; }

                internal FriendsTab(IntPtr hFriendTab) : base(hFriendTab)
                {
                    SearchHandle = WinAPI.GetWindow(hFriendTab, WinAPI.GW_CHILD);
                    SearchResultListHandle = WinAPI.GetWindow(SearchHandle, WinAPI.GW_HWNDNEXT);
                    FriendsListHandle = WinAPI.GetWindow(SearchResultListHandle, WinAPI.GW_HWNDNEXT);
                }

                /// <summary>
                /// 기존 친구들 중에서 해당 별명을 가진 유저를 검색합니다.
                /// </summary>
                /// <param name="nickname">검색할 친구의 별명</param>
                public void SearchByNickname(string nickname)
                {
                    WinAPI.SetEditText(SearchHandle, nickname, WinAPI.Encoding.Unicode);
                    Thread.Sleep(UIChangeInterval);
                }

                /// <summary>
                /// 친구 탭의 검색 결과를 초기화합니다.
                /// </summary>
                public void ClearSearchResult()
                {
                    WinAPI.SetEditText(SearchHandle, "", WinAPI.Encoding.Unicode);
                    Thread.Sleep(UIChangeInterval);
                }

                /// <summary>
                /// 해당 별명을 가지고 있는 친구와 대화를 시작합니다. (별명은 정확히 일치해야 합니다)
                /// </summary>
                /// <param name="nickname">검색할 친구의 별명</param>
                /// <param name="minimizeWindow">채팅 시작 후, 바로 채팅창을 최소화할지 여부</param>
                public KTChatWindow StartChattingWith(string nickname, bool minimizeWindow = false)
                {
                    ClearSearchResult();
                    SearchByNickname(nickname);
                    WinAPI.ClickInBackground(SearchResultListHandle, WinAPI.MouseButton.Left, FirstSearchItemX, FirstSearchItemY);
                    Thread.Sleep(MouseClickInterval);
                    KTChatWindow chatRoom;
                    lock (ChatWindows)
                    {
                        WinAPI.PressKeyInBackground(SearchResultListHandle, WinAPI.KeyCode.VK_ENTER);
                        while (!IsChatRoomOpen(nickname)) Thread.Sleep(ProgressCheckInterval);
                        chatRoom = new KTChatWindow(nickname);
                        if (minimizeWindow) chatRoom.Minimize();
                    }
                    ChatWindows.Add(chatRoom);
                    ClearSearchResult();

                    return chatRoom;
                }

                public KTChatWindow StartChattingWithMyself(string myNickname, bool minimizeWindow = false)
                {
                    ClearSearchResult();
                    KTChatWindow chatRoom;
                    lock (ChatWindows)
                    {
                        WinAPI.DoubleClickInBackground(FriendsListHandle, WinAPI.MouseButton.Left, 86, 62);
                        while (!IsChatRoomOpen(myNickname)) Thread.Sleep(ProgressCheckInterval);
                        chatRoom = new KTChatWindow(myNickname);
                        if (minimizeWindow) chatRoom.Minimize();
                    }
                    ChatWindows.Add(chatRoom);

                    return chatRoom;
                }
            }

            /// <summary>
            /// 메인 창의 채팅 탭을 제어할 수 있도록 만들어진 클래스입니다.
            /// </summary>
            public class ChattingTab : Tab
            {
                // 변경해선 안 되는 상수값 목록
                const short FirstSearchItemX = 86;
                const short FirstSearchItemY = 36;

                // 핸들 목록
                IntPtr SearchHandle { get; }
                IntPtr ChatRoomListHandle { get; }
                IntPtr SearchResultListHandle { get; }

                internal ChattingTab(IntPtr hChattingTab) : base(hChattingTab)
                {
                    SearchHandle = WinAPI.GetWindow(hChattingTab, WinAPI.GW_CHILD);
                    ChatRoomListHandle = WinAPI.GetWindow(SearchHandle, WinAPI.GW_HWNDNEXT);
                    SearchResultListHandle = WinAPI.GetWindow(ChatRoomListHandle, WinAPI.GW_HWNDNEXT);
                }

                /// <summary>
                /// 기존 채팅방 중에서 해당 이름을 가진 채팅방을 검색합니다.
                /// </summary>
                /// <param name="roomName">검색할 채팅방 이름</param>
                public void SearchByRoomName(string roomName)
                {
                    WinAPI.SetEditText(SearchHandle, roomName, WinAPI.Encoding.Unicode);
                    Thread.Sleep(UIChangeInterval);
                }

                /// <summary>
                /// 채팅방 탭의 검색 결과를 초기화합니다.
                /// </summary>
                public void ClearSearchResult()
                {
                    WinAPI.SetEditText(SearchHandle, "", WinAPI.Encoding.Unicode);
                    Thread.Sleep(UIChangeInterval);
                }

                /// <summary>
                /// 해당 이름을 가진 채팅방에서 대화를 시작합니다. (채팅방 이름은 정확히 일치해야 합니다)
                /// 만약 내가 만든 오픈채팅방일 경우에는, 해당 방의 메뉴 아이콘 클릭 -> 채팅방 설정 클릭 후 채팅방 이름을 변경해주어야 정상 작동합니다.
                /// </summary>
                /// <param name="roomName">검색할 채팅방 이름</param>
                /// <param name="minimizeWindow">채팅 시작 후, 바로 채팅창을 최소화할지 여부</param>
                public KTChatWindow StartChattingAt(string roomName, bool minimizeWindow = false)
                {
                    return _StartChattingAt(roomName, minimizeWindow, null);
                }

                internal KTChatWindow _StartChattingAt(string roomName, bool minimizeWindow, KTChatWindow previousWindow)
                {
                    ClearSearchResult();
                    SearchByRoomName(roomName);
                    WinAPI.ClickInBackground(SearchResultListHandle, WinAPI.MouseButton.Left, FirstSearchItemX, FirstSearchItemY);
                    Thread.Sleep(MouseClickInterval);
                    KTChatWindow chatRoom = null;
                    lock (ChatWindows)
                    {
                        WinAPI.PressKeyInBackground(SearchResultListHandle, WinAPI.KeyCode.VK_ENTER);
                        while (!IsChatRoomOpen(roomName)) Thread.Sleep(ProgressCheckInterval);
                        if (previousWindow == null)
                        {
                            chatRoom = new KTChatWindow(roomName);
                            if (minimizeWindow) chatRoom.Minimize();
                            ChatWindows.Add(chatRoom);
                        }
                        else chatRoom = new KTChatWindow(roomName, false);
                    }
                    ClearSearchResult();

                    return chatRoom;
                }
            }

            /// <summary>
            /// 메인 창의 더보기 탭을 제어할 수 있도록 만들어진 클래스입니다.
            /// </summary>
            public class MoreTab : Tab
            {
                // TODO : 이 클래스의 내용을 정의해야 합니다.

                internal MoreTab(IntPtr hMoreTab) : base(hMoreTab)
                {

                }
            }
        }

        /// <summary>
        /// 카카오톡 채팅 창을 제어할 수 있도록 만들어진 클래스입니다.
        /// </summary>
        public class KTChatWindow : IDisposable
        {
            /// <summary>
            /// 현재 채팅방의 이름을 담고 있습니다.
            /// </summary>
            public string RoomName { get; }

            /// <summary>
            /// 작업 큐에 남아 있는 작업이 없을 경우 해당 시간만큼 기다렸다가 다시 검사합니다.
            /// </summary>
            public int TaskCheckInterval { get; set; }

            internal IntPtr RootHandle { get; set; }
            internal IntPtr EditMessageHandle { get; set; }
            internal IntPtr SearchWordsHandle { get; set; }
            internal IntPtr ChatListHandle { get; set; }
            Thread Checker = null;
            bool ThreadActivated = true;
            List<Task> Tasks = new List<Task>();
            Task CurrentTask = null;
            Message[] Messages = null;

            internal KTChatWindow(string roomName) : this(roomName, true, DefaultChattingCheckInterval) { }

            internal KTChatWindow(string roomName, int taskCheckInterval) : this(roomName, true, taskCheckInterval) { }

            internal KTChatWindow(string roomName, bool useTaskChecker) : this(roomName, useTaskChecker, DefaultChattingCheckInterval) { }

            internal KTChatWindow(string roomName, bool useTaskChecker, int taskCheckInterval)
            {
                RoomName = roomName;
                TaskCheckInterval = taskCheckInterval;
                RootHandle = WinAPI.FindWindow(ChatWindowClass, roomName);
                EditMessageHandle = WinAPI.GetWindow(RootHandle, WinAPI.GW_CHILD);
                SearchWordsHandle = WinAPI.GetWindow(EditMessageHandle, WinAPI.GW_HWNDNEXT);
                ChatListHandle = WinAPI.GetWindow(SearchWordsHandle, WinAPI.GW_HWNDNEXT);

                if (useTaskChecker)
                {
                    Checker = new Thread(new ThreadStart(RunTasks));
                    Checker.Start();
                }
            }

            /// <summary>
            /// 채팅 창에 텍스트 메시지를 보냅니다. 정상적으로 메시지가 보내졌다면 true를, 만약 채팅 창이 종료된 상태여서 전송 실패 시 false를 반환합니다.
            /// </summary>
            /// <param name="text">보낼 텍스트</param>
            public void SendText(string text)
            {
                Tasks.Add(new Task(TaskType.SendText, text));
            }

            private bool _SendText(string text)
            {
                if (!IsOpen()) return false;

                WinAPI.SetEditText(EditMessageHandle, text, WinAPI.Encoding.Unicode);
                Thread.Sleep(ButtonActivateInterval);
                if (WinAPI.IsIconic(RootHandle))
                {
                    ActivateInput();
                    Thread.Sleep(SendActivateInterval);
                }
                lock (ChatWindows)
                {
                    WinAPI.PressKeyInBackground(EditMessageHandle, WinAPI.KeyCode.VK_ENTER);
                    Thread.Sleep(PostDelay);
                }
                int interval = KeyPressInterval - PostDelay;
                Thread.Sleep(interval > 0 ? interval : 0);

                return true;
            }

            /// <summary>
            /// 클립보드를 활용하여 채팅 창에 이미지를 보냅니다.
            /// 정상적으로 이미지가 보내졌다면 true를, 만약 채팅 창이 종료된 상태이거나 클립보드 작업에 실패 또는 예기치 않은 문제로 인하여 실패 시 false를 반환합니다.
            /// </summary>
            /// <param name="imagePath">보낼 이미지의 경로 (상대 경로 및 절대 경로 모두 가능)</param>
            public void SendImageUsingClipboard(string imagePath)
            {
                Tasks.Add(new Task(TaskType.SendImageUsingClipboard, imagePath));
            }

            private bool _SendImageUsingClipboard(string imagePath, bool backupClipboardData)
            {
                if (!IsOpen()) return false;
                bool result = true;

                try
                {
                    IntPtr hMainWindow = WinAPI.FindWindow(MainWindowClass, MainWindowTitle);
                    lock (Clipboard)
                    {
                        if (backupClipboardData) ClipboardManager.BackupData();
                        ClipboardManager.SetImage(imagePath);
                    }
                    lock (ChatWindows)
                    {
                        WinAPI.PostMessage(EditMessageHandle, 0x7E9, 0xE125, 0); // 클립보드에 있는 내용물 붙여넣기
                        Thread.Sleep(PostDelay);

                        // 퍼포먼스 > 가독성 코딩
                        IntPtr hSendDialog = IntPtr.Zero;
                        long limitInTicks = ImageCheckLimit * 10000;
                        long prevTick = DateTime.Now.Ticks;
                        while (!IsDialogForeground(ref hSendDialog, ImageCheckInterval, ImageDialogClass, ImageDialogCaption)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;

                        if (hSendDialog == IntPtr.Zero)
                        {
                            // Foreground Window 검사를 통해 핸들을 얻어오는 데 실패함.
                            // 하지만 무한 루프에 의해 실패할 경우, 기본적인 경우와 다르게 이미지 전송 다이얼로그가 꺼지지 않고 계속 켜져 있음.
                            // 그 버그를 이용하여 전체 TopMost Window들 중 Class 값이 "#32770"이고 Caption 값이 ""이며 OS 윈도우 스택의 최상단(첫 번째)에 있는 것을 찾는 과정.
                            hSendDialog = WinAPI.GetFirstHwndWithIdentifiers(ImageDialogClass, ImageDialogCaption);
                        }

                        if (hSendDialog != IntPtr.Zero)
                        {
                            WinAPI.PressKeyInBackground(hSendDialog, WinAPI.KeyCode.VK_ENTER);
                            Thread.Sleep(PostDelay);
                        }
                    }
                    if (backupClipboardData) lock (Clipboard) { ClipboardManager.RestoreData(); }
                }
                catch (ClipboardManager.CannotOpenException) { result = false; }

                return result;
            }

            /// <summary>
            /// 채팅 창에 이모티콘을 보냅니다. 정상적으로 이모티콘이 보내졌다면 true를, 만약 채팅 창이 종료된 상태이거나 예기치 않은 문제로 인하여 실패 시 false를 반환합니다.
            /// </summary>
            /// <param name="emoticon">보낼 이모티콘 객체. KakaoTalk.Emoticon 클래스를 통해 생성한 객체를 전달해주어야 합니다.</param>
            public void SendEmoticon(Emoticon emoticon)
            {
                Tasks.Add(new Task(TaskType.SendEmoticon, emoticon));
            }

            private bool _SendEmoticon(Emoticon emoticon)
            {
                if (!IsOpen()) return false;
                long limitInTicks, prevTick;

                lock (ChatWindows)
                {
                    if (emoticon.Category == Emoticon.BasicsCategory)
                    {
                        Emoticon selected = BasicEmoticons[emoticon.Position - 1];
                        string message = "(" + selected.Nickname + ")";
                        _SendText(message);
                    }
                    else
                    {
                        bool wasMinimized = false;
                        if (WinAPI.IsIconic(RootHandle))
                        {
                            wasMinimized = true;
                            lock (ChatWindows) WinAPI.ShowWindow(RootHandle, WinAPI.SW_RESTORE);
                        }
                        var chatWindowRect = WinAPI.GetWindowRect(RootHandle);
                        int height = chatWindowRect.bottom - chatWindowRect.top;
                        WinAPI.ClickInBackground(RootHandle, WinAPI.MouseButton.Left, 21, (short)(height - 21));

                        // 퍼포먼스 > 가독성 코딩 (SendImageUsingClipboard와 기본적으로 동일한 매커니즘)
                        IntPtr hSendDialog = IntPtr.Zero;
                        limitInTicks = EmoticonCheckLimit1 * 10000;
                        prevTick = DateTime.Now.Ticks;
                        while (!IsDialogForeground(ref hSendDialog, EmoticonCheckInterval1, EmoticonDialogClass, EmoticonDialogCaption)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;

                        limitInTicks = EmoticonCheckLimit2 * 10000;
                        prevTick = DateTime.Now.Ticks;
                        while (!IsSendEmoticonDialogReady(ref hSendDialog, EmoticonCheckInterval2)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;

                        IntPtr hFirstChild = WinAPI.GetWindow(hSendDialog, WinAPI.GW_CHILD);
                        IntPtr hSecondChild = WinAPI.GetWindow(hFirstChild, WinAPI.GW_CHILD);
                        while (!WinAPI.GetClassName(hSecondChild).Equals(EmoticonSecondChildClass1))
                        {
                            WinAPI.ClickInBackground(hSendDialog, WinAPI.MouseButton.Left, 58, 56);
                            Thread.Sleep(MouseClickInterval);
                            WinAPI.PressKeyInBackground(hSendDialog, WinAPI.KeyCode.VK_ESC);
                            Thread.Sleep(KeyPressInterval);

                            WinAPI.ClickInBackground(RootHandle, WinAPI.MouseButton.Left, 21, (short)(height - 21));
                            hSendDialog = IntPtr.Zero;
                            prevTick = DateTime.Now.Ticks;
                            while (!IsDialogForeground(ref hSendDialog, EmoticonCheckInterval1, EmoticonDialogClass, EmoticonDialogCaption)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;
                            prevTick = DateTime.Now.Ticks;
                            while (!IsSendEmoticonDialogReady(ref hSendDialog, EmoticonCheckInterval2)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;
                            hFirstChild = WinAPI.GetWindow(hSendDialog, WinAPI.GW_CHILD);
                            hSecondChild = WinAPI.GetWindow(hFirstChild, WinAPI.GW_CHILD);
                        }

                        limitInTicks = EmoticonCheckLimit3 * 10000;
                        prevTick = DateTime.Now.Ticks;
                        while (!IsEmoticonTabReady(hSecondChild, EmoticonCheckInterval3)) if (DateTime.Now.Ticks - prevTick >= limitInTicks) break;
                        IntPtr hSecondNext = WinAPI.GetWindow(hSecondChild, WinAPI.GW_HWNDNEXT);

                        var dialogRect = WinAPI.GetWindowRect(hSendDialog);
                        int dialogWidth = dialogRect.right - dialogRect.left;
                        int dialogHeight = dialogRect.bottom - dialogRect.top;
                        //if (dialogWidth != EmoticonDialogWidth || dialogHeight != EmoticonDialogHeight) ResizeDialog(hSendDialog, EmoticonDialogWidth, EmoticonDialogHeight);

                        // 카테고리 커서 초기화
                        int categorySingleRowCount = 7;
                        int categoryKeyleftLoopCount = categorySingleRowCount - 1;
                        int categoryKeyupLoopCount = CategoryPossibleMaxCount / categorySingleRowCount;
                        for (int i = 0; i < categoryKeyleftLoopCount; i++) WinAPI.PressKeyInBackground(hSecondChild, WinAPI.KeyCode.VK_LEFT);
                        for (int i = 0; i < categoryKeyupLoopCount; i++) WinAPI.PressKeyInBackground(hSecondChild, WinAPI.KeyCode.VK_UP);

                        // 카테고리 위치 계산
                        int categoryKeydownLoopCount = (emoticon.Category - 1) / categorySingleRowCount;
                        int categoryKeyrightLoopCount = (emoticon.Category - 1) % categorySingleRowCount;
                        for (int i = 0; i < categoryKeydownLoopCount; i++) WinAPI.PressKeyInBackground(hSecondChild, WinAPI.KeyCode.VK_DOWN);
                        for (int i = 0; i < categoryKeyrightLoopCount; i++) WinAPI.PressKeyInBackground(hSecondChild, WinAPI.KeyCode.VK_RIGHT);

                        // 이모티콘 쪽 선택
                        WinAPI.PressKeyInBackground(hSecondChild, WinAPI.KeyCode.VK_TAB);

                        // 이모티콘 위치 계산
                        int emoticonSingleRowCount = 3;
                        int emoticonKeydownLoopCount = (emoticon.Position - 1) / emoticonSingleRowCount;
                        int emoticonKeyrightLoopCount = (emoticon.Position - 1) % emoticonSingleRowCount;
                        for (int i = 0; i < emoticonKeydownLoopCount; i++) WinAPI.PressKeyInBackground(hSecondNext, WinAPI.KeyCode.VK_DOWN);
                        for (int i = 0; i < emoticonKeyrightLoopCount; i++) WinAPI.PressKeyInBackground(hSecondNext, WinAPI.KeyCode.VK_RIGHT);

                        // 입력
                        WinAPI.PressKeyInBackground(hSecondNext, WinAPI.KeyCode.VK_ENTER);
                        Thread.Sleep(KeyPressInterval);

                        // 최종 전송
                        WinAPI.PressKeyInBackground(hSendDialog, WinAPI.KeyCode.VK_ESC);
                        WinAPI.PressKeyInBackground(EditMessageHandle, WinAPI.KeyCode.VK_ENTER);
                        Thread.Sleep(KeyPressInterval);

                        if (wasMinimized) lock (ChatWindows) WinAPI.ShowWindow(RootHandle, WinAPI.SW_MINIMIZE);
                    }
                }

                return true;
            }

            /// <summary>
            /// 클립보드를 활용하여 현재 채팅창에 있는 전체 메시지 목록을 가져옵니다. 클립보드 제어 문제로 인하여 목록 가져오기 실패 시 null을 반환합니다.
            /// </summary>
            public Message[] GetMessagesUsingClipboard()
            {
                Tasks.Add(new Task(TaskType.GetMessagesUsingClipboard));
                lock (this) Monitor.Wait(this); // Pulse는 RunTasks 메서드 내에서 이루어짐.
                return Messages;
            }

            private Message[] _GetMessagesUsingClipboard(bool backupClipboardData)
            {
                WinAPI.SendMessage(ChatListHandle, 0x7E9, 0x65, 0); // 메시지 전체 선택
                string messageString = null;
                bool isClipboardAvailable = true;
                Message[] messages = null;

                lock (Clipboard)
                {
                    try
                    {
                        if (backupClipboardData) ClipboardManager.BackupData();
                        WinAPI.SendMessage(ChatListHandle, 0x7E9, 0x64, 0); // 메시지 복사
                        messageString = ClipboardManager.GetText();
                        if (backupClipboardData) ClipboardManager.RestoreData();
                    }
                    catch (ClipboardManager.CannotOpenException) { isClipboardAvailable = false; }
                }

                if (isClipboardAvailable)
                {
                    string[] messageStrings = messageString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    messages = new Message[messageStrings.Length];
                    for (int i = 0; i < messages.Length; i++) messages[i] = new Message(messageStrings[i]);
                }

                return messages;
            }

            /// <summary>
            /// 채팅창을 닫습니다. 이 채팅의 활성화 상태를 완전히 중지하려면 Dispose 메서드를 호출해야 합니다.
            /// </summary>
            public void Close()
            {
                Tasks.Add(new Task(TaskType.Close));
            }

            private void _Close()
            {
                if (!IsOpen()) return;
                lock (ChatWindows)
                {
                    WinAPI.PressKeyInBackground(RootHandle, WinAPI.KeyCode.VK_ESC);
                    Thread.Sleep(PostDelay);
                }
                int interval = WindowCloseInterval - PostDelay;
                Thread.Sleep(interval > 0 ? interval : 0);
            }

            /// <summary>
            /// 채팅창을 닫고 다시 엽니다. 만약 이미 채팅창이 닫힌 상태라면 바로 다시 열립니다.
            /// </summary>
            public void Reopen(bool minimizeWindow = false)
            {
                Tasks.Add(new Task(TaskType.Reopen, minimizeWindow));
                lock (this) Monitor.Wait(this);
            }

            private void _Reopen(bool minimizeWindow)
            {
                if (IsOpen()) _Close();
                MainWindow.ChangeTabTo(MainWindowTab.Chatting);
                KTChatWindow newWindowInfo = MainWindow.Chatting._StartChattingAt(RoomName, minimizeWindow, this);
                RootHandle = newWindowInfo.RootHandle;
                EditMessageHandle = newWindowInfo.EditMessageHandle;
                SearchWordsHandle = newWindowInfo.SearchWordsHandle;
                ChatListHandle = newWindowInfo.ChatListHandle;
            }

            /// <summary>
            /// 현재 채팅을 완전히 종료하고 활성화된 스레드를 중지시킵니다.
            /// </summary>
            public void Dispose()
            {
                Tasks.Add(new Task(TaskType.Dispose));
            }

            private void _Dispose()
            {
                if (IsOpen()) _Close();
                ThreadActivated = false;
                ChatWindows.Remove(this);
            }

            /// <summary>
            /// 현재 채팅창을 최소화합니다.
            /// </summary>
            public void Minimize()
            {
                while (!DoesWindowHaveSize()) Thread.Sleep(ProgressCheckInterval);
                var rect = WinAPI.GetWindowRect(RootHandle);
                WinAPI.ClickInBackground(RootHandle, WinAPI.MouseButton.Left, (short)((rect.right - rect.left) - 63), 18);
            }

            /// <summary>
            /// 현재 채팅창을 최소화 상태에서 원래 상태로 복구합니다.
            /// </summary>
            public void Restore()
            {
                WinAPI.ShowWindow(RootHandle, WinAPI.SW_RESTORE);
                while (!DoesWindowHaveSize()) Thread.Sleep(ProgressCheckInterval);
                WinAPI.SetForegroundWindow(RootHandle);
                WinAPI.BringWindowToTop(RootHandle);
            }

            private bool DoesWindowHaveSize()
            {
                var rect = WinAPI.GetWindowRect(RootHandle);
                return rect.right - rect.left > 0 && rect.bottom - rect.top > 0;
            }

            /// <summary>
            /// 현재 남아 있는 작업이 있는지 여부를 반환합니다.
            /// </summary>
            public bool HasTasks()
            {
                return Tasks.Count > 0 ? true : false;
            }

            /// <summary>
            /// 현재 남아 있는 작업의 개수를 반환합니다.
            /// </summary>
            public int GetTaskCount()
            {
                return Tasks.Count;
            }

            // private 메서드 목록
            private bool IsOpen()
            {
                return WinAPI.FindWindow(ChatWindowClass, RoomName) == RootHandle ? true : false;
            }

            private bool IsDialogForeground(ref IntPtr hWnd, int checkInterval, string className, string caption)
            {
                if (checkInterval > 0) Thread.Sleep(checkInterval);
                IntPtr hWndForeground = WinAPI.GetForegroundWindow();
                if (!WinAPI.GetClassName(hWndForeground).Equals(className)) return false;
                if (!WinAPI.GetWindowText(hWndForeground).Equals(caption)) return false;

                hWnd = hWndForeground;
                return true;
            }

            private bool IsSendEmoticonDialogReady(ref IntPtr hDialog, int checkInterval)
            {
                // 이모티콘 다이얼로그의 실행은 "현재 카톡 창이 ForegroundWindow인가"에 의해 결정됨.
                // 만약 ForegroundWindow라면, 해당 시점에 EmoticonDialogClass와 EmoticonDialogCaption를 가진 첫 번째 다이얼로그이며,
                // 그렇지 않다면, 해당 시점에 EmoticonDialogClass와 EmoticonDialogCaption를 가진 또 다른 다이얼로그가 이모티콘 다이얼로그임.
                // 그래서 해당 식별자들을 가진 Topmost Window Handle 목록을 얻어온 다음 원하는 구조를 가지고 있는 다이얼로그를 찾는 작업이 필요함.

                if (checkInterval > 0) Thread.Sleep(checkInterval);
                IntPtr hWndTemp;
                var hDialogList = WinAPI.GetHwndListWithIdentifiers(EmoticonDialogClass, EmoticonDialogCaption);
                for (int i = 0; i < hDialogList.Count; i++)
                {
                    hDialog = hDialogList[i];
                    hWndTemp = WinAPI.GetWindow(hDialog, WinAPI.GW_CHILD); // First Child => EVA_ChildWindow
                    if (!WinAPI.GetClassName(hWndTemp).Equals(EmoticonFirstChildClass)) continue;
                    hWndTemp = WinAPI.GetWindow(hWndTemp, WinAPI.GW_CHILD); // Second Child => EVA_ChildWindow_Dblclk / _EVA_CustomScrollCtrl / EVA_VH_ListControl_Dblclk 셋 중 하나
                    string secondChildClassName = WinAPI.GetClassName(hWndTemp);
                    if (secondChildClassName.Equals(EmoticonSecondChildClass1) ||
                        secondChildClassName.Equals(EmoticonSecondChildClass2) ||
                        secondChildClassName.Equals(EmoticonSecondChildClass3)) break;
                }

                return true;
            }

            private bool IsEmoticonTabReady(IntPtr hSecondChild, int checkInterval)
            {
                if (checkInterval > 0) Thread.Sleep(checkInterval);

                IntPtr hSecondNext = WinAPI.GetWindow(hSecondChild, WinAPI.GW_HWNDNEXT);
                if (!WinAPI.GetClassName(hSecondNext).Equals(EmoticonSecondNextClass)) return false;

                return true;
            }

            private void ResizeDialog(IntPtr hWnd, int width, int height)
            {
                RECT rect = WinAPI.GetWindowRect(hWnd);
                int prevX = rect.left;
                int prevY = rect.top;
                int prevWidth = rect.right - rect.left;
                int prevHeight = rect.bottom - rect.top;
                WinAPI.ResizeWindow(hWnd, width, height);
                WinAPI.MoveWindow(hWnd, prevX + (prevWidth - width), prevY + (prevHeight - height));
            }

            private void ActivateInput()
            {
                WinAPI.SendMessage(RootHandle, WinAPI.WM_COMMAND, (0x400 * 0x10000) | (1006 & 0xFFFF), (int)RootHandle);
            }

            private void RunTasks()
            {
                Console.WriteLine($"Thread 실행 ({RoomName})");
                while (ThreadActivated)
                {
                    if (Tasks.Count > 0)
                    {
                        CurrentTask = Tasks[0];
                        if (CurrentTask != null)
                        {
                            switch (CurrentTask.Type)
                            {
                                case TaskType.SendText:
                                    _SendText((string)CurrentTask.Parameter);
                                    break;
                                case TaskType.SendImageUsingClipboard:
                                    _SendImageUsingClipboard((string)CurrentTask.Parameter, false);
                                    break;
                                case TaskType.SendEmoticon:
                                    _SendEmoticon((Emoticon)CurrentTask.Parameter);
                                    break;
                                case TaskType.GetMessagesUsingClipboard:
                                    Messages = _GetMessagesUsingClipboard(false);
                                    lock (this) Monitor.Pulse(this);
                                    break;
                                case TaskType.Close:
                                    _Close();
                                    break;
                                case TaskType.Reopen:
                                    _Reopen((bool)CurrentTask.Parameter);
                                    lock (this) Monitor.Pulse(this);
                                    break;
                                case TaskType.Dispose:
                                    _Dispose();
                                    break;
                            }
                            Tasks.RemoveAt(0);
                            CurrentTask = null;
                        }
                    }
                    Thread.Sleep(TaskCheckInterval);
                }
                Console.WriteLine($"Thread 종료 ({RoomName})");
            }

            enum TaskType { SendText = 1, SendImageUsingClipboard, SendEmoticon, GetMessagesUsingClipboard, Close, Reopen, Dispose }

            class Task
            {
                internal TaskType Type { get; }
                internal object Parameter { get; }

                internal Task(TaskType type, object parameter = null)
                {
                    Type = type;
                    Parameter = parameter;
                }
            }
        }

        public class NotOpenException : Exception
        {
            internal NotOpenException() : base("카카오톡이 열려 있지 않습니다.") { }
        }
        public class NotLoggedInException : Exception
        {
            internal NotLoggedInException() : base("로그인되지 않은 상태입니다.") { }
        }
        public class AlreadyLoggedInException : Exception
        {
            internal AlreadyLoggedInException() : base("이미 로그인된 상태입니다.") { }
        }

        public enum MessageType { Unknown, DateChange, UserJoin, UserLeave, Talk }

        /// <summary>
        /// 카카오톡 메시지에 대한 데이터를 담고 있는 구조체입니다.
        /// </summary>
        public struct Message
        {
            /// <summary>
            /// 해당 메시지의 타입. 타입은 KakaoTalk.MessageType 을 참고하세요.
            /// </summary>
            public MessageType Type { get; }
            /// <summary>
            /// 해당 메시지를 보낸 유저의 이름
            /// </summary>
            public string UserName { get; }
            /// <summary>
            /// 해당 메시지의 내용
            /// </summary>
            public string Content { get; }

            public Message(string fullContent)
            {
                Type = GetMessageType(fullContent);
                UserName = GetUserName(fullContent, Type);
                Content = GetContent(fullContent, Type, UserName);
            }

            public override string ToString()
            {
                string type = "";
                switch (Type)
                {
                    case MessageType.DateChange:
                        type = "DateChange";
                        break;
                    case MessageType.UserJoin:
                        type = "UserJoin";
                        break;
                    case MessageType.UserLeave:
                        type = "UserLeave";
                        break;
                    case MessageType.Talk:
                        type = "Talk";
                        break;
                }

                return $"Type : {type}, Username : {UserName}, Content : {Content}";
            }

            private static MessageType GetMessageType(string fullContent)
            {
                // ????년 ?월 ?일 ?요일 => MessageType.DateTime
                // 홍길동님이 들어왔습니다. => MessageType.UserJoin
                // 홍길동나갔습니다. => MessageType.UserLeave
                // [홍길동] [오? ?(?):??] 입력문장 => MessageType.Talk

                if (fullContent.IndexOf("20") == 0)
                {
                    if (fullContent.IndexOf("년 ") == 4)
                    {
                        if (fullContent.IndexOf("월 ") == 7 || fullContent.IndexOf("월 ") == 8)
                        {
                            if (fullContent.IndexOf("일 ") == 10 ||
                                fullContent.IndexOf("일 ") == 11 ||
                                fullContent.IndexOf("일 ") == 12)
                            {
                                if ((fullContent.IndexOf("요일") == 13 && fullContent.Length == 15) ||
                                    (fullContent.IndexOf("요일") == 14 && fullContent.Length == 16) ||
                                    (fullContent.IndexOf("요일") == 15 && fullContent.Length == 17))
                                {
                                    return MessageType.DateChange;
                                }
                            }
                        }
                    }
                }

                if (fullContent.Contains("님이 들어왔습니다.") && fullContent.IndexOf("님이 들어왔습니다.") == fullContent.Length - 10)
                {
                    if (!(fullContent.IndexOf("[") == 0 && fullContent.Contains("] [오") && fullContent.Contains("] ")))
                    {
                        return MessageType.UserJoin;
                    }
                }

                if (fullContent.Contains("나갔습니다.") && fullContent.IndexOf("나갔습니다.") == fullContent.Length - 6)
                {
                    if (!(fullContent.IndexOf("[") == 0 && fullContent.Contains("] [오") && fullContent.Contains("] ")))
                    {
                        return MessageType.UserLeave;
                    }
                }

                if (fullContent.IndexOf("[") == 0)
                {
                    if (fullContent.Contains("] [오"))
                    {
                        if (fullContent.Contains("] "))
                        {
                            return MessageType.Talk;
                        }
                    }
                }

                return MessageType.Unknown;
            }

            private static string GetUserName(string fullContent, MessageType type)
            {
                switch (type)
                {
                    case MessageType.UserJoin:
                        return fullContent.Substring(0, fullContent.LastIndexOf("님이 들어왔습니다."));
                    case MessageType.UserLeave:
                        return fullContent.Substring(0, fullContent.LastIndexOf("나갔습니다."));
                    case MessageType.Talk:
                        return fullContent.Substring(1, fullContent.IndexOf("] [오") - 1);
                    case MessageType.DateChange:
                        return DateChangeNotifierName;
                    default:
                        return null;
                }
            }

            private static string GetContent(string fullContent, MessageType type, string Username)
            {
                switch (type)
                {
                    case MessageType.DateChange:
                        return string.Format("오늘은 {0}입니다.", fullContent);
                    case MessageType.UserJoin:
                        return Username + "님이 들어왔습니다.";
                    case MessageType.UserLeave:
                        return Username + "님이 나갔습니다.";
                    case MessageType.Talk:
                        string temp = fullContent.Substring(Username.Length + 2);
                        return temp.Substring(temp.IndexOf("] ") + 2);
                    default:
                        return null;
                }
            }
        }

        // 이모티콘 관련
        /// <summary>
        /// 카테고리 번호와 이모티콘의 위치값(position)를 통해 이모티콘을 찾도록 설계된 구조체입니다.
        /// </summary>
        public struct Emoticon
        {
            public const int BasicsCategory = 0;
            public const int FavoritesCategory = 1;

            public string Nickname { get; set; }
            public int Category { get; }
            public int Position { get; }

            /// <summary>
            /// 카카오톡 이모티콘을 생성합니다.
            /// </summary>
            /// <param name="nickname">해당 이모티콘에 부여할 별명</param>
            /// <param name="category">이모티콘이 등록된 카테고리 (카테고리 값은 1부터 시작하며, 1은 즐겨찾기입니다)</param>
            /// <param name="position">해당 카테고리 내의 이모티콘의 위치 (위치 값은 1부터 시작합니다)</param>
            public Emoticon(string nickname, int category, int position)
            {
                Nickname = nickname;
                Category = category;
                Position = position;
            }

            public struct BasicsPosition
            {
                public const int 하트뿅 = 1;
                public const int 하하 = 2;
                public const int 우와 = 3;
                public const int 심각 = 4;
                public const int 힘듦 = 5;
                public const int 흑흑 = 6;

                public const int 아잉 = 7;
                public const int 찡긋 = 8;
                public const int 뿌듯 = 9;
                public const int 깜짝 = 10;
                public const int 빠직 = 11;
                public const int 짜증 = 12;

                public const int 제발 = 13;
                public const int 씨익 = 14;
                public const int 신나 = 15;
                public const int 헉 = 16;
                public const int 열받아 = 17;
                public const int 흥 = 18;

                public const int 감동 = 19;
                public const int 뽀뽀 = 20;
                public const int 멘붕 = 21;
                public const int 정색 = 22;
                public const int 쑥스 = 23;
                public const int 꺄아 = 24;

                public const int 좋아 = 25;
                public const int 굿 = 26;
                public const int 훌쩍 = 27;
                public const int 허걱 = 28;
                public const int 부르르 = 29;
                public const int 푸하하 = 30;

                public const int 발그레 = 31;
                public const int 수줍 = 32;
                public const int 컴온 = 33;
                public const int 졸려 = 34;
                public const int 미소 = 35;
                public const int 윙크 = 36;

                public const int 방긋 = 37;
                public const int 반함 = 38;
                public const int 눈물 = 39;
                public const int 절규 = 40;
                public const int 크크 = 41;
                public const int 메롱 = 42;

                public const int 잘자 = 43;
                public const int 잘난척 = 44;
                public const int 헤롱 = 45;
                public const int 놀람 = 46;
                public const int 아픔 = 47;
                public const int 당황 = 48;

                public const int 풍선껌 = 49;
                public const int 버럭 = 50;
                public const int 부끄 = 51;
                public const int 궁금 = 52;
                public const int 흡족 = 53;
                public const int 깜찍 = 54;

                public const int 으으 = 55;
                public const int 민망 = 56;
                public const int 곤란 = 57;
                public const int 잠 = 58;
                public const int 행복 = 59;
                public const int 안도 = 60;

                public const int 우웩 = 61;
                public const int 외계인 = 62;
                public const int 외계인녀 = 63;
                public const int 공포 = 64;
                public const int 근심 = 65;
                public const int 악마 = 66;

                public const int 썩소 = 67;
                public const int 쳇 = 68;
                public const int 야호 = 69;
                public const int 좌절 = 70;
                public const int 삐침 = 71;
                public const int 하트 = 72;

                public const int 실연 = 73;
                public const int 별 = 74;
                public const int 브이 = 75;
                public const int 오케이 = 76;
                public const int 최고 = 77;
                public const int 최악 = 78;

                public const int 그만 = 79;
                public const int 땀 = 80;
                public const int 알약 = 81;
                public const int 밥 = 82;
                public const int 커피 = 83;
                public const int 맥주 = 84;

                public const int 소주 = 85;
                public const int 와인 = 86;
                public const int 치킨 = 87;
                public const int 축하 = 88;
                public const int 음표 = 89;
                public const int 선물 = 90;

                public const int 케이크 = 91;
                public const int 촛불 = 92;
                public const int 컵케이크a = 93;
                public const int 컵케이크b = 94;
                public const int 해 = 95;
                public const int 구름 = 96;

                public const int 비 = 97;
                public const int 눈 = 98;
                public const int 똥 = 99;
                public const int 근조 = 100;
                public const int 딸기 = 101;
                public const int 호박 = 102;

                public const int 입술 = 103;
                public const int 야옹 = 104;
                public const int 돈 = 105;
                public const int 담배 = 106;
                public const int 축구 = 107;
                public const int 야구 = 108;

                public const int 농구 = 109;
                public const int 당구 = 110;
                public const int 골프 = 111;
                public const int 카톡 = 112;
                public const int 꽃 = 113;
                public const int 총 = 114;

                public const int 크리스마스 = 115;
                public const int 콜 = 116;
            }
        }

        internal static Emoticon[] BasicEmoticons = {
            new Emoticon("하트뿅", Emoticon.BasicsCategory, Emoticon.BasicsPosition.하트뿅),
            new Emoticon("하하", Emoticon.BasicsCategory, Emoticon.BasicsPosition.하하),
            new Emoticon("우와", Emoticon.BasicsCategory, Emoticon.BasicsPosition.우와),
            new Emoticon("심각", Emoticon.BasicsCategory, Emoticon.BasicsPosition.심각),
            new Emoticon("힘듦", Emoticon.BasicsCategory, Emoticon.BasicsPosition.힘듦),
            new Emoticon("흑흑", Emoticon.BasicsCategory, Emoticon.BasicsPosition.흑흑),
            new Emoticon("아잉", Emoticon.BasicsCategory, Emoticon.BasicsPosition.아잉),
            new Emoticon("찡긋", Emoticon.BasicsCategory, Emoticon.BasicsPosition.찡긋),
            new Emoticon("뿌듯", Emoticon.BasicsCategory, Emoticon.BasicsPosition.뿌듯),
            new Emoticon("깜짝", Emoticon.BasicsCategory, Emoticon.BasicsPosition.깜짝),
            new Emoticon("빠직", Emoticon.BasicsCategory, Emoticon.BasicsPosition.빠직),
            new Emoticon("짜증", Emoticon.BasicsCategory, Emoticon.BasicsPosition.짜증),
            new Emoticon("제발", Emoticon.BasicsCategory, Emoticon.BasicsPosition.제발),
            new Emoticon("씨익", Emoticon.BasicsCategory, Emoticon.BasicsPosition.씨익),
            new Emoticon("신나", Emoticon.BasicsCategory, Emoticon.BasicsPosition.신나),
            new Emoticon("헉", Emoticon.BasicsCategory, Emoticon.BasicsPosition.헉),
            new Emoticon("열받아", Emoticon.BasicsCategory, Emoticon.BasicsPosition.열받아),
            new Emoticon("흥", Emoticon.BasicsCategory, Emoticon.BasicsPosition.흥),
            new Emoticon("감동", Emoticon.BasicsCategory, Emoticon.BasicsPosition.감동),
            new Emoticon("뽀뽀", Emoticon.BasicsCategory, Emoticon.BasicsPosition.뽀뽀),
            new Emoticon("멘붕", Emoticon.BasicsCategory, Emoticon.BasicsPosition.멘붕),
            new Emoticon("정색", Emoticon.BasicsCategory, Emoticon.BasicsPosition.정색),
            new Emoticon("쑥스", Emoticon.BasicsCategory, Emoticon.BasicsPosition.쑥스),
            new Emoticon("꺄아", Emoticon.BasicsCategory, Emoticon.BasicsPosition.꺄아),
            new Emoticon("좋아", Emoticon.BasicsCategory, Emoticon.BasicsPosition.좋아),
            new Emoticon("굿", Emoticon.BasicsCategory, Emoticon.BasicsPosition.굿),
            new Emoticon("훌쩍", Emoticon.BasicsCategory, Emoticon.BasicsPosition.훌쩍),
            new Emoticon("허걱", Emoticon.BasicsCategory, Emoticon.BasicsPosition.허걱),
            new Emoticon("부르르", Emoticon.BasicsCategory, Emoticon.BasicsPosition.부르르),
            new Emoticon("푸하하", Emoticon.BasicsCategory, Emoticon.BasicsPosition.푸하하),
            new Emoticon("발그레", Emoticon.BasicsCategory, Emoticon.BasicsPosition.발그레),
            new Emoticon("수줍", Emoticon.BasicsCategory, Emoticon.BasicsPosition.수줍),
            new Emoticon("컴온", Emoticon.BasicsCategory, Emoticon.BasicsPosition.컴온),
            new Emoticon("졸려", Emoticon.BasicsCategory, Emoticon.BasicsPosition.졸려),
            new Emoticon("미소", Emoticon.BasicsCategory, Emoticon.BasicsPosition.미소),
            new Emoticon("윙크", Emoticon.BasicsCategory, Emoticon.BasicsPosition.윙크),
            new Emoticon("방긋", Emoticon.BasicsCategory, Emoticon.BasicsPosition.방긋),
            new Emoticon("반함", Emoticon.BasicsCategory, Emoticon.BasicsPosition.반함),
            new Emoticon("눈물", Emoticon.BasicsCategory, Emoticon.BasicsPosition.눈물),
            new Emoticon("절규", Emoticon.BasicsCategory, Emoticon.BasicsPosition.절규),
            new Emoticon("크크", Emoticon.BasicsCategory, Emoticon.BasicsPosition.크크),
            new Emoticon("메롱", Emoticon.BasicsCategory, Emoticon.BasicsPosition.메롱),
            new Emoticon("잘자", Emoticon.BasicsCategory, Emoticon.BasicsPosition.잘자),
            new Emoticon("잘난척", Emoticon.BasicsCategory, Emoticon.BasicsPosition.잘난척),
            new Emoticon("헤롱", Emoticon.BasicsCategory, Emoticon.BasicsPosition.헤롱),
            new Emoticon("놀람", Emoticon.BasicsCategory, Emoticon.BasicsPosition.놀람),
            new Emoticon("아픔", Emoticon.BasicsCategory, Emoticon.BasicsPosition.아픔),
            new Emoticon("당황", Emoticon.BasicsCategory, Emoticon.BasicsPosition.당황),
            new Emoticon("풍선껌", Emoticon.BasicsCategory, Emoticon.BasicsPosition.풍선껌),
            new Emoticon("버럭", Emoticon.BasicsCategory, Emoticon.BasicsPosition.버럭),
            new Emoticon("부끄", Emoticon.BasicsCategory, Emoticon.BasicsPosition.부끄),
            new Emoticon("궁금", Emoticon.BasicsCategory, Emoticon.BasicsPosition.궁금),
            new Emoticon("흡족", Emoticon.BasicsCategory, Emoticon.BasicsPosition.흡족),
            new Emoticon("깜찍", Emoticon.BasicsCategory, Emoticon.BasicsPosition.깜찍),
            new Emoticon("으으", Emoticon.BasicsCategory, Emoticon.BasicsPosition.으으),
            new Emoticon("민망", Emoticon.BasicsCategory, Emoticon.BasicsPosition.민망),
            new Emoticon("곤란", Emoticon.BasicsCategory, Emoticon.BasicsPosition.곤란),
            new Emoticon("잠", Emoticon.BasicsCategory, Emoticon.BasicsPosition.잠),
            new Emoticon("행복", Emoticon.BasicsCategory, Emoticon.BasicsPosition.행복),
            new Emoticon("안도", Emoticon.BasicsCategory, Emoticon.BasicsPosition.안도),
            new Emoticon("우웩", Emoticon.BasicsCategory, Emoticon.BasicsPosition.우웩),
            new Emoticon("외계인", Emoticon.BasicsCategory, Emoticon.BasicsPosition.외계인),
            new Emoticon("외계인녀", Emoticon.BasicsCategory, Emoticon.BasicsPosition.외계인녀),
            new Emoticon("공포", Emoticon.BasicsCategory, Emoticon.BasicsPosition.공포),
            new Emoticon("근심", Emoticon.BasicsCategory, Emoticon.BasicsPosition.근심),
            new Emoticon("악마", Emoticon.BasicsCategory, Emoticon.BasicsPosition.악마),
            new Emoticon("썩소", Emoticon.BasicsCategory, Emoticon.BasicsPosition.썩소),
            new Emoticon("쳇", Emoticon.BasicsCategory, Emoticon.BasicsPosition.쳇),
            new Emoticon("야호", Emoticon.BasicsCategory, Emoticon.BasicsPosition.야호),
            new Emoticon("좌절", Emoticon.BasicsCategory, Emoticon.BasicsPosition.좌절),
            new Emoticon("삐침", Emoticon.BasicsCategory, Emoticon.BasicsPosition.삐침),
            new Emoticon("하트", Emoticon.BasicsCategory, Emoticon.BasicsPosition.하트),
            new Emoticon("실연", Emoticon.BasicsCategory, Emoticon.BasicsPosition.실연),
            new Emoticon("별", Emoticon.BasicsCategory, Emoticon.BasicsPosition.별),
            new Emoticon("브이", Emoticon.BasicsCategory, Emoticon.BasicsPosition.브이),
            new Emoticon("오케이", Emoticon.BasicsCategory, Emoticon.BasicsPosition.오케이),
            new Emoticon("최고", Emoticon.BasicsCategory, Emoticon.BasicsPosition.최고),
            new Emoticon("최악", Emoticon.BasicsCategory, Emoticon.BasicsPosition.최악),
            new Emoticon("그만", Emoticon.BasicsCategory, Emoticon.BasicsPosition.그만),
            new Emoticon("땀", Emoticon.BasicsCategory, Emoticon.BasicsPosition.땀),
            new Emoticon("알약", Emoticon.BasicsCategory, Emoticon.BasicsPosition.알약),
            new Emoticon("밥", Emoticon.BasicsCategory, Emoticon.BasicsPosition.밥),
            new Emoticon("커피", Emoticon.BasicsCategory, Emoticon.BasicsPosition.커피),
            new Emoticon("맥주", Emoticon.BasicsCategory, Emoticon.BasicsPosition.맥주),
            new Emoticon("소주", Emoticon.BasicsCategory, Emoticon.BasicsPosition.소주),
            new Emoticon("와인", Emoticon.BasicsCategory, Emoticon.BasicsPosition.와인),
            new Emoticon("치킨", Emoticon.BasicsCategory, Emoticon.BasicsPosition.치킨),
            new Emoticon("축하", Emoticon.BasicsCategory, Emoticon.BasicsPosition.축하),
            new Emoticon("음표", Emoticon.BasicsCategory, Emoticon.BasicsPosition.음표),
            new Emoticon("선물", Emoticon.BasicsCategory, Emoticon.BasicsPosition.선물),
            new Emoticon("케이크", Emoticon.BasicsCategory, Emoticon.BasicsPosition.케이크),
            new Emoticon("촛불", Emoticon.BasicsCategory, Emoticon.BasicsPosition.촛불),
            new Emoticon("컵케이크a", Emoticon.BasicsCategory, Emoticon.BasicsPosition.컵케이크a),
            new Emoticon("컵케이크b", Emoticon.BasicsCategory, Emoticon.BasicsPosition.컵케이크b),
            new Emoticon("해", Emoticon.BasicsCategory, Emoticon.BasicsPosition.해),
            new Emoticon("구름", Emoticon.BasicsCategory, Emoticon.BasicsPosition.구름),
            new Emoticon("비", Emoticon.BasicsCategory, Emoticon.BasicsPosition.비),
            new Emoticon("눈", Emoticon.BasicsCategory, Emoticon.BasicsPosition.눈),
            new Emoticon("똥", Emoticon.BasicsCategory, Emoticon.BasicsPosition.똥),
            new Emoticon("근조", Emoticon.BasicsCategory, Emoticon.BasicsPosition.근조),
            new Emoticon("딸기", Emoticon.BasicsCategory, Emoticon.BasicsPosition.딸기),
            new Emoticon("호박", Emoticon.BasicsCategory, Emoticon.BasicsPosition.호박),
            new Emoticon("입술", Emoticon.BasicsCategory, Emoticon.BasicsPosition.입술),
            new Emoticon("야옹", Emoticon.BasicsCategory, Emoticon.BasicsPosition.야옹),
            new Emoticon("돈", Emoticon.BasicsCategory, Emoticon.BasicsPosition.돈),
            new Emoticon("담배", Emoticon.BasicsCategory, Emoticon.BasicsPosition.담배),
            new Emoticon("축구", Emoticon.BasicsCategory, Emoticon.BasicsPosition.축구),
            new Emoticon("야구", Emoticon.BasicsCategory, Emoticon.BasicsPosition.야구),
            new Emoticon("농구", Emoticon.BasicsCategory, Emoticon.BasicsPosition.농구),
            new Emoticon("당구", Emoticon.BasicsCategory, Emoticon.BasicsPosition.당구),
            new Emoticon("골프", Emoticon.BasicsCategory, Emoticon.BasicsPosition.골프),
            new Emoticon("카톡", Emoticon.BasicsCategory, Emoticon.BasicsPosition.카톡),
            new Emoticon("꽃", Emoticon.BasicsCategory, Emoticon.BasicsPosition.꽃),
            new Emoticon("총", Emoticon.BasicsCategory, Emoticon.BasicsPosition.총),
            new Emoticon("크리스마스", Emoticon.BasicsCategory, Emoticon.BasicsPosition.크리스마스),
            new Emoticon("콜", Emoticon.BasicsCategory, Emoticon.BasicsPosition.콜)
        };
    }
}
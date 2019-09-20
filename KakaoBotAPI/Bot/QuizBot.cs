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
    public abstract class QuizBot : ChatBot
    {
        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체의 경험치
        /// </summary>
        protected static int NewUserExperience = 0;

        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체의 레벨
        /// </summary>
        protected static int NewUserLevel = 1;

        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체의 머니
        /// </summary>
        protected static int NewUserMoney = 0;

        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체의 세대
        /// </summary>
        protected static int NewUserGeneration = 1;

        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체에 적용되는 타이틀
        /// </summary>
        protected static Title NewUserCurrentTitle = new Title("새로운타이틀");

        /// <summary>
        /// 자동으로 생성되는 새 QuizUser 객체가 적용 가능한 타이틀 목록
        /// </summary>
        protected static List<Title> NewUserAvailableTitles = new List<Title>() { NewUserCurrentTitle };

        /// <summary>
        /// 퀴즈 데이터가 위치하는 최상위 경로
        /// </summary>
        protected const string QuizPath = DataPath + @"quiz\";

        /// <summary>
        /// 퀴즈 설정 파일의 이름
        /// </summary>
        protected const string QuizSettingsFileName = "settings";

        /// <summary>
        /// 퀴즈 설정 파일의 확장자
        /// </summary>
        protected const string QuizSettingsFileExtension = IniHelper.FileExtension;

        /// <summary>
        /// 퀴즈 설정 파일의 Section 이름
        /// </summary>
        protected const string QuizSettingsSection = "Settings";

        /// <summary>
        /// 퀴즈 데이터 파일의 이름
        /// </summary>
        protected const string QuizDataFileName = "data";

        /// <summary>
        /// 퀴즈 데이터 파일의 확장자
        /// </summary>
        protected const string QuizDataFileExtension = XmlHelper.FileExtension;

        /// <summary>
        /// 퀴즈 목록
        /// </summary>
        protected List<Quiz> QuizList = new List<Quiz>();

        /// <summary>
        /// 현재 퀴즈가 진행 중인지 여부
        /// true : 퀴즈가 진행 중입니다.
        /// false : 퀴즈가 진행 중이 아닙니다.
        /// </summary>
        protected bool IsQuizTaskRunning = false;

        /// <summary>
        /// 퀴즈 시작 시 나오는 공지 목록 파일의 이름
        /// </summary>
        protected const string QuizNoticeFileName = "quiz_notice";

        /// <summary>
        /// 퀴즈 시작 시 나오는 공지 목록 파일의 확장자
        /// </summary>
        protected const string QuizNoticeFileExtension = ConfigFileDefaultExtension;

        /// <summary>
        /// 퀴즈 Thread의 메시지 폴링 간격
        /// </summary>
        protected static int QuizScanInterval = 200;

        /// <summary>
        /// 봇 인스턴스에 대한 Quiz Thread
        /// </summary>
        protected Thread QuizTaskRunner;

        /// <summary>
        /// 봇 인스턴스 내부에서 사용되는 랜덤 객체
        /// </summary>
        protected Random BotRandom = new Random();

        /// <summary>
        /// 퀴즈 주제 추가 방법을 설명하는 파일의 이름
        /// </summary>
        protected const string QuizAddSubjectName = "퀴즈 주제 추가 방법";

        /// <summary>
        /// 퀴즈 주제 추가 방법을 설명하는 파일의 확장자
        /// </summary>
        protected const string QuizAddSubjectExtension = TextFileExtension;

        /// <summary>
        /// 퀴즈봇 객체를 생성합니다.
        /// </summary>
        /// <param name="roomName">채팅방의 이름</param>
        /// <param name="type">대상 채팅방의 유형</param>
        /// <param name="botRunnerName">봇 돌리미의 해당 채팅방에서의 닉네임</param>
        /// <param name="identifier">채팅방에 부여할 특별한 식별자</param>
        public QuizBot(string roomName, TargetTypeOption type, string botRunnerName, string identifier) : base(roomName, type, botRunnerName, identifier) { }

        /// <summary>
        /// 퀴즈봇을 시작합니다.
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// 퀴즈봇을 중지합니다.
        /// </summary>
        public override void Stop()
        {
            StopQuiz();
            base.Stop();
        }

        /// <summary>
        /// 본격적으로 메시지 분석을 시작하기 전에, 필요한 초기화 작업을 진행합니다.
        /// </summary>
        protected override void InitializeBotSettings()
        {
            RefreshQuizList();

            string path = QuizAddSubjectName + QuizAddSubjectExtension;
            if (!File.Exists(path)) GenerateHowToAddQuizSubjectFile(path);
        }

        /// <summary>
        /// 유저가 메시지를 입력한 경우 수행할 행동을 지정합니다.
        /// </summary>
        /// <param name="userName">메시지를 보낸 유저의 닉네임</param>
        /// <param name="content">메시지 내용</param>
        /// <param name="sendTime">메시지를 보낸 시각</param>
        protected override void ProcessUserMessage(string userName, string content, DateTime sendTime)
        {
            if (FindUserByNickname(userName) == null) AddNewUser(userName);
            foreach (QuizUser user in Users)
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
        /// 퀴즈를 시작합니다.
        /// </summary>
        /// <param name="quizType">퀴즈의 유형</param>
        /// <param name="subjects">주제 목록</param>
        /// <param name="requestQuizCount">요청하는 퀴즈의 총 개수</param>
        /// <param name="minQuizCount">퀴즈 최소 개수</param>
        /// <param name="quizTimeLimit">퀴즈의 제한시간</param>
        /// <param name="bonusExperience">퀴즈 정답 시 획득 경험치</param>
        /// <param name="bonusMoney">퀴즈 정답 시 획득 머니</param>
        /// <param name="idleTimeLimit">퀴즈의 잠수 제한시간</param>
        /// <param name="showSubject">주제 표시 여부</param>
        protected void StartQuiz(Quiz.TypeOption quizType, string[] subjects, int requestQuizCount, int minQuizCount, int quizTimeLimit, int bonusExperience, int bonusMoney, int idleTimeLimit = 180, bool showSubject = true)
        {
            IsQuizTaskRunning = true;
            Thread.Sleep(1000);

            List<Quiz.Data> quizDataList = GetQuizDataList(quizType, subjects, requestQuizCount);
            if (quizDataList.Count < requestQuizCount) requestQuizCount = quizDataList.Count;
            if (requestQuizCount < minQuizCount)
            {
                Thread.Sleep(SendMessageInterval);
                OnQuizCountInvalid(minQuizCount, requestQuizCount);
                IsQuizTaskRunning = false;
                return;
            }

            string[] notices = GetQuizNoticesFromFile();
            Thread.Sleep(SendMessageInterval);
            SendMessage($"{notices[BotRandom.Next(notices.Length)]}");
            Thread.Sleep(2000);

            bool isRandom = subjects.Length > 1 ? true : false;

            OnQuizReady();
            RunQuiz(quizType, subjects, requestQuizCount, quizTimeLimit, bonusExperience, bonusMoney, idleTimeLimit, showSubject, isRandom, quizDataList);

            if (IsQuizTaskRunning) IsQuizTaskRunning = false;
            Thread.Sleep(2000);
            KakaoTalk.Message[] messages;
            if (IsMainTaskRunning)
            {
                while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);
                LastMessageIndex = (messages.Length - 1) + 1;
                OnQuizFinish();
            }
        }

        /// <summary>
        /// 퀴즈를 중지합니다.
        /// </summary>
        protected void StopQuiz()
        {
            IsQuizTaskRunning = false;
        }

        /// <summary>
        /// 퀴즈 리스트를 갱신합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void RefreshQuizList()
        {
            /* 퀴즈 리스트 */
            var newQuizList = new List<Quiz>();

            /* 설정 변수 */
            Quiz.TypeOption quizType;
            string mainSubject;
            List<string> childSubjects;
            bool isCaseSensitive;
            bool useMultiChoice;
            Quiz.ChoiceExtractMethodOption choiceExtractMethod;
            int choiceCount;

            /* 데이터 변수 */
            string question;
            string answer;
            string explanation;
            string type;
            string beforeImagePath;
            string afterImagePath;
            string childSubject;
            string regDateStr;
            DateTime regDate;
            List<Quiz.Data> dataList;

            /* 임시 변수 */
            string value;

            Directory.CreateDirectory(QuizPath);
            string[] directories = Directory.GetDirectories(QuizPath);

            for (int i = 0; i < directories.Length; i++)
            {
                /* 퀴즈 설정 값 로드 */
                string path = directories[i] + "\\" + QuizSettingsFileName + QuizSettingsFileExtension;
                if (!File.Exists(path)) throw new ArgumentException($"{path} 파일이 누락되었습니다.");
                var iniHelper = new IniHelper(path);

                value = iniHelper.Read("quizType", QuizSettingsSection);
                if (value == "일반") quizType = Quiz.TypeOption.General;
                else if (value == "초성") quizType = Quiz.TypeOption.Chosung;
                else throw new ArgumentException($"지원되지 않는 퀴즈 유형입니다. (\"{value}\" << {path})");

                mainSubject = iniHelper.Read("mainSubject", QuizSettingsSection);

                childSubjects = new List<string>();
                value = iniHelper.Read("childSubjects", QuizSettingsSection);
                string[] tempArr = null;
                if (value != "")
                {
                    tempArr = value.Split(',');
                    for (int j = 0; j < tempArr.Length; j++) childSubjects.Add(tempArr[j].Trim());
                }

                value = iniHelper.Read("isCaseSensitive", QuizSettingsSection);
                if (value == "true") isCaseSensitive = true;
                else if (value == "false") isCaseSensitive = false;
                else throw new ArgumentException($"대소문자 구분 정답 처리 여부에 잘못된 값이 설정되었습니다. (\"{value}\" << {path})");

                value = iniHelper.Read("useMultiChoice", QuizSettingsSection);
                if (value == "true") useMultiChoice = true;
                else if (value == "false") useMultiChoice = false;
                else throw new ArgumentException($"객관식 여부에 잘못된 값이 설정되었습니다. (\"{value}\" << {path})");

                value = iniHelper.Read("choiceExtractMethod", QuizSettingsSection);
                if (value == "") choiceExtractMethod = Quiz.ChoiceExtractMethodOption.None;
                else if (value == "RICS") choiceExtractMethod = Quiz.ChoiceExtractMethodOption.RICS;
                else if (value == "RAPT") choiceExtractMethod = Quiz.ChoiceExtractMethodOption.RAPT;
                else throw new ArgumentException($"객관식 선택지 추출 방식에 잘못된 값이 설정되었습니다. (\"{value}\" << {path})");

                value = iniHelper.Read("choiceCount", QuizSettingsSection);
                if (value == "") value = "0";
                try { choiceCount = int.Parse(value); }
                catch (Exception) { throw new ArgumentException($"객관식 선택지 개수에 잘못된 값이 설정되었습니다. (\"{value}\" << {path})"); }

                /* 퀴즈 데이터 로드 */
                path = directories[i] + "\\" + QuizDataFileName + QuizDataFileExtension;
                if (!File.Exists(path)) throw new ArgumentException($"{path} 파일이 누락되었습니다.");
                var xmlHelper = new XmlHelper(path);

                question = null;
                answer = null;
                explanation = null;
                type = null;
                beforeImagePath = null;
                afterImagePath = null;
                childSubject = null;
                regDateStr = null;
                dataList = new List<Quiz.Data>();
                var document = xmlHelper.ReadFile();
                if (document.RootElementName != "list") throw new ArgumentException($"{QuizDataFileName + QuizDataFileExtension} 파일의 Parent Element의 이름은 \"list\"여야 합니다. (현재: {document.RootElementName})");
                for (int j = 0; j < document.ChildNodes.Count; j++)
                {
                    var node = document.ChildNodes[j];
                    if (node.Name != "data") throw new ArgumentException($"{QuizDataFileName + QuizDataFileExtension} 파일의 Child Element의 이름은 \"data\"여야 합니다. ({j + 1}번째 Child: {node.Name})");

                    foreach (XmlHelper.NodeData nodeData in node.DataList)
                    {
                        switch (nodeData.Key)
                        {
                            case "question": question = nodeData.Value; break;
                            case "answer": answer = nodeData.Value; break;
                            case "explanation": explanation = nodeData.Value; break;
                            case "type": type = nodeData.Value; break;
                            case "beforeImagePath": beforeImagePath = nodeData.Value; break;
                            case "afterImagePath": afterImagePath = nodeData.Value; break;
                            case "childSubject": childSubject = nodeData.Value; break;
                            case "regDate": regDateStr = nodeData.Value; break;
                        }
                    }
                    if (question == null) throw new ArgumentException($"{QuizDataFileName + QuizDataFileExtension} 파일의 {j + 1}번째 data에 question 요소가 누락되었습니다.");
                    if (answer == null) throw new ArgumentException($"{QuizDataFileName + QuizDataFileExtension} 파일의 {j + 1}번째 data에 answer 요소가 누락되었습니다.");
                    if (regDateStr == null) throw new ArgumentException($"{QuizDataFileName + QuizDataFileExtension} 파일의 {j + 1}번째 data에 regDate 요소가 누락되었습니다.");
                    else
                    {
                        int year = int.Parse(regDateStr.Substring(0, 4));
                        int month = int.Parse(regDateStr.Substring(5, 2));
                        int day = int.Parse(regDateStr.Substring(8, 2));
                        int hour = int.Parse(regDateStr.Substring(11, 2));
                        int minute = int.Parse(regDateStr.Substring(14, 2));
                        int second = int.Parse(regDateStr.Substring(17, 2));
                        regDate = new DateTime(year, month, day, hour, minute, second);
                    }

                    dataList.Add(new Quiz.Data(mainSubject, question, answer, explanation, type, null, beforeImagePath, afterImagePath, childSubject, isCaseSensitive, regDate)); // TODO : choices를 data.xml 파일에서 입력받을 수도 있도록 처리
                }

                newQuizList.Add(new Quiz(quizType, mainSubject, childSubjects, isCaseSensitive, useMultiChoice, choiceExtractMethod, choiceCount, dataList));
            }

            QuizList = newQuizList;
        }

        /// <summary>
        /// 파일 시스템으로부터 유저 데이터를 불러옵니다.
        /// </summary>
        /// <returns>퀴즈유저 목록</returns>
        protected override void RefreshUserData()
        {
            var document = GetUserDataDocument();
            var users = new List<User>();

            string nickname;
            bool isIgnored;
            int experience, level, money, generation;
            Title currentTitle;
            List<Title> availableTitles;
            string[] tempArray;
            string value;

            for (int i = 0; i < document.ChildNodes.Count; i++)
            {
                var node = document.ChildNodes[i];

                nickname = node.GetData("nickname");

                value = node.GetData("isIgnored");
                if (value == "true") isIgnored = true;
                else if (value == "false") isIgnored = false;
                else throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 isIgnored 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                if (node.GetData("experience") == null) experience = NewUserExperience;
                else if (!int.TryParse(node.GetData("experience"), out experience)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 experience 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                if (node.GetData("level") == null) level = NewUserLevel;
                else if (!int.TryParse(node.GetData("level"), out level)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 level 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                if (node.GetData("money") == null) money = NewUserMoney;
                else if (!int.TryParse(node.GetData("money"), out money)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 money 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                if (node.GetData("generation") == null) generation = NewUserGeneration;
                else if (!int.TryParse(node.GetData("generation"), out generation)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 generation 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                if (node.GetData("currentTitle") == null) currentTitle = NewUserCurrentTitle;
                else currentTitle = new Title(node.GetData("currentTitle"));

                availableTitles = new List<Title>();
                if (node.GetData("availableTitles") == null) availableTitles = NewUserAvailableTitles;
                else
                {
                    tempArray = node.GetData("availableTitles").Split(',');
                    if (tempArray.Length == 0 || (tempArray.Length == 1 && tempArray[0] == "")) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 availableTitles 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");
                    else
                    {
                        for (int j = 0; j < tempArray.Length; j++)
                        {
                            if (tempArray[j] == currentTitle.Name) break;
                            else if (j == tempArray.Length - 1) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 availableTitles 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");
                        }
                    }
                    for (int j = 0; j < tempArray.Length; j++) availableTitles.Add(new Title(tempArray[j]));
                }

                users.Add(new QuizUser(nickname, isIgnored, experience, level, money, generation, currentTitle, availableTitles));
            }

            Users = users;
            SaveUserData();
        }


        /// <summary>
        /// 파일 시스템에 유저 데이터를 저장합니다.
        /// </summary>
        protected override void SaveUserData()
        {
            string path = ConfigPath + $"{Identifier}\\" + ProfileFileName + ProfileFileExtension;

            var helper = new XmlHelper(path);
            var nodeList = new List<XmlHelper.Node>();
            XmlHelper.Node node;
            string temp;

            for (int i = 0; i < Users.Count; i++)
            {
                node = new XmlHelper.Node("user");
                node.AddData("nickname", Users[i].Nickname);
                node.AddData("isIgnored", Users[i].IsIgnored ? "true" : "false");
                node.AddData("experience", (Users[i] as QuizUser).Experience);
                node.AddData("level", (Users[i] as QuizUser).Level);
                node.AddData("money", (Users[i] as QuizUser).Money);
                node.AddData("generation", (Users[i] as QuizUser).Generation);
                node.AddData("currentTitle", (Users[i] as QuizUser).CurrentTitle.Name);
                temp = "";
                for (int j = 0; j < (Users[i] as QuizUser).AvailableTitles.Count; j++) temp += (Users[i] as QuizUser).AvailableTitles[j].Name + ",";
                node.AddData("availableTitles", temp.Substring(0, temp.Length - 1));

                nodeList.Add(node);
            }

            helper.CreateFile("list", nodeList);
        }

        /// <summary>
        /// 해당 닉네임을 가진 유저 정보를 새로 등록합니다.
        /// </summary>
        /// <param name="userName">새로 등록될 유저의 닉네임</param>
        /// <returns>퀴즈유저 객체</returns>
        protected new QuizUser AddNewUser(string userName)
        {
            var user = new QuizUser(userName, IsNewUserIgnored, NewUserExperience, NewUserLevel, NewUserMoney, NewUserGeneration, NewUserCurrentTitle, NewUserAvailableTitles);
            Users.Add(user);
            SaveUserData();
            return user;
        }

        /// <summary>
        /// 닉네임을 통해 유저 정보를 얻어옵니다.<para/>
        /// 만약 현재 해당 닉네임을 가진 유저가 기록에 존재하지 않으면 null을 반환합니다.
        /// </summary>
        /// <param name="userName">유저의 닉네임</param>
        /// <returns>퀴즈유저 객체</returns>
        protected new QuizUser FindUserByNickname(string userName)
        {
            return (QuizUser)base.FindUserByNickname(userName);
        }

        /// <summary>
        /// 파일에서 퀴즈 출제 전에 표시하는 공지사항을 가져옵니다.
        /// </summary>
        /// <returns>공지사항 스트링 배열</returns>
        protected string[] GetQuizNoticesFromFile()
        {
            string[] tempArr = ReadQuizNoticeFile();
            var notices = new List<string>();
            for (int i = 0; i < tempArr.Length; i++)
            {
                string line = tempArr[i].Trim();
                if (line == "" || line.IndexOf(";") == 0) continue;
                notices.Add(line);
            }

            return notices.ToArray();
        }

        /// <summary>
        /// 퀴즈 공지사항 파일을 읽어들입니다.
        /// </summary>
        /// <returns>전체 파일 내용 배열</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private string[] ReadQuizNoticeFile()
        {
            string path = ConfigPath + $"{Identifier}\\" + QuizNoticeFileName + QuizNoticeFileExtension;
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

            if (!File.Exists(path)) GenerateQuizNoticeFile(path);

            return File.ReadAllLines(path);
        }

        /// <summary>
        /// 퀴즈 공지사항 파일을 생성합니다.
        /// </summary>
        /// <param name="path">퀴즈 공지사항 파일 경로</param>
        private void GenerateQuizNoticeFile(string path)
        {
            string message = Properties.Resources.quiz_notice;

            File.WriteAllLines(path, message.Split(new string[] { "\r\n" }, StringSplitOptions.None), new UTF8Encoding(false));
        }

        /// <summary>
        /// 퀴즈 데이터 목록을 얻어옵니다.
        /// </summary>
        /// <param name="requestedQuizType">요청된 퀴즈 유형</param>
        /// <param name="subjects">주제 목록</param>
        /// <param name="quizCount">퀴즈 개수</param>
        /// <returns>퀴즈 데이터 목록</returns>
        protected List<Quiz.Data> GetQuizDataList(Quiz.TypeOption requestedQuizType, string[] subjects, int quizCount)
        {
            var data2dList = new List<List<Quiz.Data>>();
            List<Quiz.Data> tempList;
            bool matchesChildSubject;

            /* resultDataList 초기화 */
            for (int i = 0; i < subjects.Length; i++)
            {
                foreach (Quiz quiz in QuizList)
                {
                    if (quiz.Type == requestedQuizType)
                    {
                        string mainSubject = quiz.MainSubject;
                        if (mainSubject == subjects[i])
                        {
                            tempList = new List<Quiz.Data>();
                            for (int j = 0; j < quiz.DataList.Count; j++)
                            {
                                var data = quiz.DataList[j];
                                if (quiz.UseMultiChoice == true) data.Choices = GetChoiceList(quiz, data);
                                tempList.Add(data);
                            }
                            data2dList.Add(tempList);
                            break;
                        }
                        else
                        {
                            matchesChildSubject = false;
                            foreach (string childSubject in quiz.ChildSubjects)
                            {
                                if ($"{mainSubject}-{childSubject}" == subjects[i])
                                {
                                    tempList = new List<Quiz.Data>();
                                    for (int j = 0; j < quiz.DataList.Count; j++)
                                    {
                                        var data = quiz.DataList[j];
                                        if (data.ChildSubject == childSubject)
                                        {
                                            if (quiz.UseMultiChoice == true) data.Choices = GetChoiceList(quiz, data);
                                            tempList.Add(data);
                                        }
                                    }
                                    data2dList.Add(tempList);
                                    matchesChildSubject = true;
                                }
                            }
                            if (matchesChildSubject) break;
                        }
                    }
                }
            }

            /* data2dList Shuffle */
            int shuffleCount = 3;
            for (int i = 0; i < data2dList.Count; i++)
            {
                for (int j = 0; j < shuffleCount; j++)
                {
                    ListUtil.Shuffle(data2dList[i]);
                }
            }

            /* 배열 리스트, 문항 수 초기화 */
            List<Quiz.Data[]> dataArrList = new List<Quiz.Data[]>();
            int totalCount = 0;
            for (int i = 0; i < data2dList.Count; i++)
            {
                dataArrList.Add(new Quiz.Data[data2dList[i].Count]);
                totalCount += data2dList[i].Count;
            }
            if (totalCount > quizCount) totalCount = quizCount;

            /* 문제 선택 및 결과 리스트로 이동 */
            var resultDataList = new List<Quiz.Data>();
            for (int i = 0; i < totalCount; i++)
            {
                int randomValue = BotRandom.Next(dataArrList.Count);
                for (int j = 0; j < dataArrList[randomValue].Length; j++)
                {
                    if (dataArrList[randomValue][j] == null)
                    {
                        dataArrList[randomValue][j] = data2dList[randomValue][j];
                        resultDataList.Add(dataArrList[randomValue][j]);
                        break;
                    }
                    else if (j == dataArrList[randomValue].Length - 1) { i--; }
                }
            }

            /* 결과 리스트 shuffle (리스트 내에서 다시 셔플) */
            for (int i = 0; i < shuffleCount; i++) ListUtil.Shuffle(resultDataList);

            return resultDataList;
        }

        /// <summary>
        /// 퀴즈 선택지 목록을 가져옵니다.
        /// </summary>
        /// <param name="quiz">퀴즈 객체</param>
        /// <param name="data">퀴즈 데이터 객체</param>
        /// <returns></returns>
        protected List<string> GetChoiceList(Quiz quiz, Quiz.Data data)
        {
            var choiceList = new List<string>();
            var choiceCandidates = new List<string>();
            bool shouldRecalc;

            if (quiz.ChoiceCount > quiz.DataList.Count) throw new ArgumentException($"객관식에서 퀴즈 선택지 수가 문항 수보다 많습니다. ({quiz.Type}-{quiz.MainSubject})");

            choiceList.Add(data.Answer);
            if (quiz.ChoiceExtractMethod == Quiz.ChoiceExtractMethodOption.RICS)
            {
                for (int i = 0; i < quiz.ChoiceCount - 1; i++)
                {
                    string value = quiz.DataList[BotRandom.Next(quiz.DataList.Count)].Answer;
                    shouldRecalc = false;
                    for (int j = 0; j < choiceList.Count; j++)
                    {
                        if (choiceList[j] == value) { shouldRecalc = true; break; }
                    }
                    if (shouldRecalc) { i--; continue; }
                    choiceList.Add(value);
                }
            }
            else if (quiz.ChoiceExtractMethod == Quiz.ChoiceExtractMethodOption.RAPT)
            {
                for (int i = 0; i < quiz.DataList.Count; i++) if (quiz.DataList[i].Type == data.Type) choiceCandidates.Add(quiz.DataList[i].Answer);
                if (quiz.ChoiceCount > choiceCandidates.Count) throw new ArgumentException($"RAPT 객관식에서 퀴즈 선택지 수가 가능한 선택지 수보다 많습니다. ({quiz.Type}-{quiz.MainSubject})");

                for (int i = 0; i < quiz.ChoiceCount - 1; i++)
                {
                    string value = choiceCandidates[BotRandom.Next(choiceCandidates.Count)];
                    shouldRecalc = false;
                    for (int j = 0; j < choiceList.Count; j++)
                    {
                        if (choiceList[j] == value) { shouldRecalc = true; break; }
                    }
                    if (shouldRecalc) { i--; continue; }
                    choiceList.Add(value);
                }
            }
            for (int i = 0; i < 3; i++) ListUtil.Shuffle(choiceList);

            return choiceList;
        }

        /// <summary>
        /// 퀴즈 실행부입니다.
        /// </summary>
        /// <param name="quizType">퀴즈의 유형</param>
        /// <param name="subjects">주제 목록</param>
        /// <param name="requestQuizCount">요청하는 퀴즈의 총 개수</param>
        /// <param name="quizTimeLimit">퀴즈의 제한시간</param>
        /// <param name="bonusExperience">퀴즈 정답 시 획득 경험치</param>
        /// <param name="bonusMoney">퀴즈 정답 시 획득 머니</param>
        /// <param name="idleTimeLimit">퀴즈의 잠수 제한시간</param>
        /// <param name="showSubject">주제 표시 여부</param>
        /// <param name="isRandom">주제 랜덤 여부</param>
        /// <param name="quizDataList">퀴즈 데이터 목록</param>
        protected void RunQuiz(Quiz.TypeOption quizType, string[] subjects, int requestQuizCount, int quizTimeLimit, int bonusExperience, int bonusMoney, int idleTimeLimit, bool showSubject, bool isRandom, List<Quiz.Data> quizDataList)
        {
            KakaoTalk.Message[] messages;
            KakaoTalk.MessageType messageType;
            string userName;
            string content;
            DateTime sendTime;
            QuizUser user;
            int lastInputTick = Environment.TickCount;

            for (int i = 0; i < requestQuizCount; i++) // == quizDataList.Count
            {
                int currentQuiz = i + 1;

                var quizData = quizDataList[i];
                string subject = quizData.MainSubject;
                if (quizData.ChildSubject != null) subject += $"-{quizData.ChildSubject}";
                string question = quizData.Question;
                string answer = quizData.Answer;
                string explanation = quizData.Explanation;
                string beforeImagePath = quizData.BeforeImagePath;
                string afterImagePath = quizData.AfterImagePath;
                bool isCaseSensitive = quizData.IsCaseSensitive;
                
                while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);
                LastMessageIndex = (messages.Length - 1) + 1; // 뒤에 바로 SendMessage를 하므로, +1 해서 초기화
                OnQuizQuestionSend(isRandom, showSubject, subject, currentQuiz, requestQuizCount, question);

                if (beforeImagePath != null) OnQuizBeforeImageSend(beforeImagePath);

                if (quizData.Choices != null) OnQuizChoicesSend(quizData.Choices);

                int beginTick = Environment.TickCount;
                bool shouldContinue = true;
                while (IsQuizTaskRunning && shouldContinue)
                {
                    Thread.Sleep(QuizScanInterval);
                    if (Environment.TickCount > lastInputTick + (idleTimeLimit * 1000))
                    {
                        IsQuizTaskRunning = false;
                        OnQuizIdleLimitExceed();
                        break;
                    }
                    else if (Environment.TickCount > beginTick + (quizTimeLimit * 1000)) // 시간 제한 초과
                    {
                        OnQuizTimeLimitExceed(answer);
                        shouldContinue = false;
                    }
                    else
                    {
                        while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);
                        sendTime = DateTime.Now;
                        for (int j = LastMessageIndex; j < messages.Length; j++)
                        {
                            messageType = messages[j].Type;
                            userName = messages[j].UserName;
                            content = messages[j].Content;

                            LastMessageIndex++;
                            user = FindUserByNickname(userName);
                            if (user == null)
                            {
                                AddNewUser(userName);
                                user = FindUserByNickname(userName);
                            }

                            if (!isCaseSensitive) { content = content.ToLower(); answer = answer.ToLower(); }

                            if (IsQuizTaskRunning) // 다른 곳에서 StopQuiz 요청에 의해 IsQuizTaskRunning은 false가 될 수 있음. 따라서 검사 시마다 확인.
                            {
                                if (messageType == KakaoTalk.MessageType.Unknown) continue;
                                else if (messageType == KakaoTalk.MessageType.DateChange) SendDateChangeNotice(content, sendTime);
                                else if (messageType == KakaoTalk.MessageType.UserJoin) SendUserJoinNotice(userName, sendTime);
                                else if (messageType == KakaoTalk.MessageType.UserLeave) SendUserLeaveNotice(userName, sendTime);
                                else if (messageType == KakaoTalk.MessageType.Talk)
                                {
                                    if (content == answer)
                                    {
                                        lastInputTick = Environment.TickCount;
                                        OnQuizAnswerCorrect(answer, user, bonusExperience, bonusMoney);
                                        shouldContinue = false;
                                        break;
                                    }
                                    else ProcessUserMessage(userName, content, sendTime);
                                }
                            }
                            else break;
                        }
                    }
                    if (IsQuizTaskRunning && !shouldContinue)
                    {
                        if (afterImagePath != null) OnQuizAfterImageSend(afterImagePath);
                        if (explanation != null) OnQuizExplanationSend(explanation);
                        else Thread.Sleep(1500);
                    }
                }
                if (!IsQuizTaskRunning) break;
                else if (i == requestQuizCount - 1)
                {
                    OnQuizAllCompleted();
                    IsQuizTaskRunning = false;
                }
            }
        }

        /// <summary>
        /// 유저의 Profile을 업데이트하는 메서드입니다.<para/>
        /// 이 메서드는 유저가 퀴즈 정답을 맞혔을 경우에 자동으로 호출되므로, 정답 시마다 특정 액션을 취하고 싶다면 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="user">유저 객체</param>
        /// <param name="bonusExperience">추가 경험치</param>
        /// <param name="bonusMoney">추가 머니</param>
        protected abstract void UpdateUserProfile(QuizUser user, int bonusExperience, int bonusMoney);

        /// <summary>
        /// 퀴즈 주제 추가 방법을 설명하는 파일을 생성합니다.
        /// </summary>
        /// <param name="path">주제 추가 방법 파일 경로</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void GenerateHowToAddQuizSubjectFile(string path)
        {
            string message = Properties.Resources.how_to_add_quiz_subjects;

            File.WriteAllLines(path, message.Split(new string[] { "\r\n" }, StringSplitOptions.None), Encoding.Unicode);
        }

        /// <summary>
        /// 요청한 퀴즈 개수가 부족할 경우 추가적으로 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 퀴즈 개수 부족 알림"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="minQuizCount">퀴즈 최소 개수</param>
        /// <param name="requestQuizCount">요청하는 퀴즈의 총 개수</param>
        protected virtual void OnQuizCountInvalid(int minQuizCount, int requestQuizCount)
        {
            SendMessage($"퀴즈 문항 수가 최솟값보다 작습니다. (최소: {minQuizCount}개, 현재: {requestQuizCount}개)");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 퀴즈의 실행 준비가 완료된 시점에 추가적으로 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 퀴즈 시작 알림"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void OnQuizReady()
        {
            SendMessage("퀴즈를 시작합니다.");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 퀴즈가 전부 끝난 시점에 추가적으로 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 퀴즈 종료 알림"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void OnQuizFinish()
        {
            SendMessage("퀴즈가 종료되었습니다.");
            Thread.Sleep(SendMessageInterval);
        }

        /// <summary>
        /// 퀴즈의 문제를 전송하는 시점에 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 퀴즈 주제 및 문제 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="isRandom">주제 랜덤 여부</param>
        /// <param name="showSubject">주제 표시 여부</param>
        /// <param name="subject">현재 문항의 주제</param>
        /// <param name="currentQuiz">현재 문항 번호</param>
        /// <param name="requestQuizCount">요청하는 퀴즈의 총 개수</param>
        /// <param name="question">현재 문항의 문제</param>
        protected virtual void OnQuizQuestionSend(bool isRandom, bool showSubject, string subject, int currentQuiz, int requestQuizCount, string question)
        {
            string randomText = isRandom ? "랜덤 " : "";
            SendMessage($"[{randomText}" + (showSubject ? subject : "") + $" {currentQuiz}/{requestQuizCount}]{question}");
        }

        /// <summary>
        /// 퀴즈의 문제를 풀기 전 이미지를 전송하는 시점에 할 행동을 지정합니다. 기본 설정은 "SendImage 메서드를 통한 이미지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="beforeImagePath">문제 풀이 전 전송하는 이미지</param>
        protected virtual void OnQuizBeforeImageSend(string beforeImagePath)
        {
            Thread.Sleep(1500);
            SendImage(beforeImagePath);
        }

        /// <summary>
        /// 퀴즈의 선택지를 전송하는 시점에 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 선택지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="choices"></param>
        protected virtual void OnQuizChoicesSend(List<string> choices)
        {
            string content = "";
            for (int j = 0; j < choices.Count; j++) content += $"{j + 1}. {choices[j]}\n";
            content = content.Substring(0, content.Length - 1);

            Thread.Sleep(2000);
            SendMessage(content);
        }

        /// <summary>
        /// 퀴즈의 잠수 제한 시간이 초과되었을 경우 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 퀴즈 중단 메시지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void OnQuizIdleLimitExceed()
        {
            SendMessage("장시간 유효한 입력이 발생하지 않아 문제 풀이를 중단합니다. 잠시만 기다려주세요...");
        }

        /// <summary>
        /// 퀴즈 풀이의 제한 시간이 초과되었을 경우 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 안내 문구 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="answer">퀴즈의 정답</param>
        protected virtual void OnQuizTimeLimitExceed(string answer)
        {
            SendMessage($"정답자가 없어서 다음 문제로 넘어갑니다. 정답: {answer}");
            Thread.Sleep(1500);
        }

        /// <summary>
        /// 퀴즈의 정답을 맞힌 시점에 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 안내 메시지 전송 및 유저 Profile 업데이트"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="answer">퀴즈의 정답</param>
        /// <param name="answeredUser">정답을 맞힌 유저</param>
        /// <param name="bonusExperience">퀴즈 정답 시 획득 경험치</param>
        /// <param name="bonusMoney">퀴즈 정답 시 획득 머니</param>
        protected virtual void OnQuizAnswerCorrect(string answer, QuizUser answeredUser, int bonusExperience, int bonusMoney)
        {
            SendMessage($"정답: {answer}, 정답자: [{answeredUser.CurrentTitle.Name}]{answeredUser.Nickname} (Lv. {answeredUser.Level}), 경험치: {answeredUser.Experience + bonusExperience}(+{bonusExperience}), 머니: {answeredUser.Money + bonusMoney}(+{bonusMoney})");
            UpdateUserProfile(answeredUser, bonusExperience, bonusMoney);
            Thread.Sleep(1500);
        }

        /// <summary>
        /// 퀴즈의 문제를 푼 후 이미지를 전송하는 시점에 할 행동을 지정합니다. 기본 설정은 "SendImage 메서드를 통한 이미지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="afterImagePath">문제 풀이 후 전송하는 이미지</param>
        protected virtual void OnQuizAfterImageSend(string afterImagePath)
        {
            SendImage(afterImagePath);
            Thread.Sleep(1500);
        }

        /// <summary>
        /// 퀴즈의 해설을 전송하는 시점에 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 해설 메시지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        /// <param name="explanation">퀴즈의 설명</param>
        protected virtual void OnQuizExplanationSend(string explanation)
        {
            SendMessage($"[해설]{explanation}");
            Thread.Sleep(3500);
        }

        /// <summary>
        /// 퀴즈 문제를 모두 푼 시점에 할 행동을 지정합니다. 기본 설정은 "SendMessage 메서드를 통한 안내 메시지 전송"입니다. 필요할 경우 이 메서드를 오버라이드하여 사용하십시오.
        /// </summary>
        protected virtual void OnQuizAllCompleted()
        {
            SendMessage("문제를 다 풀었습니다. 잠시만 기다려주세요...");
        }
    }
}

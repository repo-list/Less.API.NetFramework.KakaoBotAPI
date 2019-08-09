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
        protected const string QuizSettingsName = "settings";

        /// <summary>
        /// 퀴즈 설정 파일의 확장자
        /// </summary>
        protected const string QuizSettingsExtension = IniHelper.FileExtension;

        /// <summary>
        /// 퀴즈 설정 파일의 Section 이름
        /// </summary>
        protected const string QuizSettingsSection = "Settings";

        /// <summary>
        /// 퀴즈 데이터 파일의 이름
        /// </summary>
        protected const string QuizDataName = "data";

        /// <summary>
        /// 퀴즈 데이터 파일의 확장자
        /// </summary>
        protected const string QuizDataExtension = XmlHelper.FileExtension;

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
        protected const string MessageQuizNoticeName = "quiz_notice";

        /// <summary>
        /// 퀴즈 시작 시 나오는 공지 목록 파일의 확장자
        /// </summary>
        protected const string MessageQuizNoticeExtension = ".dat";

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
        protected const string QuizAddSubjectName = "주제 추가 방법";

        /// <summary>
        /// 퀴즈 주제 추가 방법을 설명하는 파일의 확장자
        /// </summary>
        protected const string QuizAddSubjectExtension = ".txt";

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

            string path = QuizPath + QuizAddSubjectName + QuizAddSubjectExtension;
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
        /// <param name="requestQuizCount">요청하는 퀴즈 개수</param>
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
                SendMessage($"퀴즈 문항 수가 최솟값보다 작습니다. (최소: {minQuizCount}개, 현재: {requestQuizCount}개)");
                Thread.Sleep(SendMessageInterval);
                IsQuizTaskRunning = false;
                return;
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add("quizType", quizType);
            parameters.Add("subjects", subjects);
            parameters.Add("requestQuizCount", requestQuizCount);
            parameters.Add("quizTimeLimit", quizTimeLimit);
            parameters.Add("bonusExperience", bonusExperience);
            parameters.Add("bonusMoney", bonusMoney);
            parameters.Add("idleTimeLimit", idleTimeLimit);
            parameters.Add("showSubject", showSubject);
            parameters.Add("quizDataList", quizDataList);

            string[] notices = GetQuizNoticesFromFile();
            Thread.Sleep(SendMessageInterval);
            SendMessage($"{notices[BotRandom.Next(notices.Length)]}");
            Thread.Sleep(2000);

            SendMessage("퀴즈를 시작합니다.");
            Thread.Sleep(SendMessageInterval);
            QuizTaskRunner = new Thread(new ParameterizedThreadStart(RunQuiz));
            QuizTaskRunner.Start(parameters);
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
                string path = directories[i] + "\\" + QuizSettingsName + QuizSettingsExtension;
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
                path = directories[i] + "\\" + QuizDataName + QuizDataExtension;
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
                if (document.RootElementName != "list") throw new ArgumentException($"{QuizDataName + QuizDataExtension} 파일의 Parent Element의 이름은 \"list\"여야 합니다. (현재: {document.RootElementName})");
                for (int j = 0; j < document.ChildNodes.Count; j++)
                {
                    var node = document.ChildNodes[j];
                    if (node.Name != "data") throw new ArgumentException($"{QuizDataName + QuizDataExtension} 파일의 Child Element의 이름은 \"data\"여야 합니다. ({j + 1}번째 Child: {node.Name})");

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
                    if (question == null) throw new ArgumentException($"{QuizDataName + QuizDataExtension} 파일의 {j + 1}번째 data에 question 요소가 누락되었습니다.");
                    if (answer == null) throw new ArgumentException($"{QuizDataName + QuizDataExtension} 파일의 {j + 1}번째 data에 answer 요소가 누락되었습니다.");
                    if (regDateStr == null) throw new ArgumentException($"{QuizDataName + QuizDataExtension} 파일의 {j + 1}번째 data에 regDate 요소가 누락되었습니다.");
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

                if (!int.TryParse(node.GetData("experience"), out experience)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 experience 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");
                if (!int.TryParse(node.GetData("level"), out level)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 level 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");
                if (!int.TryParse(node.GetData("money"), out money)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 money 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");
                if (!int.TryParse(node.GetData("generation"), out generation)) throw new ArgumentException($"식별자 {Identifier} 봇의 유저 데이터 파일에 generation 값이 잘못 설정되었습니다. ({i + 1}번째 항목)");

                currentTitle = new Title(node.GetData("currentTitle"));

                availableTitles = new List<Title>();
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

                users.Add(new QuizUser(nickname, isIgnored, experience, level, money, generation, currentTitle, availableTitles));
            }

            Users = users;
        }


        /// <summary>
        /// 파일 시스템에 유저 데이터를 저장합니다.
        /// </summary>
        protected override void SaveUserData()
        {
            string path = ProfilePath + ProfileNameHeader + Identifier + ProfileExtension;

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
            var user = new QuizUser(userName, NewUserIsIgnored, NewUserExperience, NewUserLevel, NewUserMoney, NewUserGeneration, NewUserCurrentTitle, NewUserAvailableTitles);
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
            Directory.CreateDirectory(MessagePath);
            string noticeFilePath = MessagePath + MessageQuizNoticeName + MessageQuizNoticeExtension;
            if (!File.Exists(noticeFilePath)) GenerateQuizNoticeFile(noticeFilePath);

            return File.ReadAllLines(noticeFilePath);
        }

        /// <summary>
        /// 퀴즈 공지사항 파일을 생성합니다.
        /// </summary>
        /// <param name="path">퀴즈 공지사항 파일 경로</param>
        private void GenerateQuizNoticeFile(string path)
        {
            var message = new StringBuilder();
            message.Append("; 여기에는 퀴즈 시작 전에 나오는 알림 메시지를 작성합니다.\n");
            message.Append("; 세미콜론(;)으로 시작하는 문장은 주석으로 인식합니다.\n");
            message.Append("공지 파일을 수정하여 유저들과 최신 소식을 공유해보세요.");

            File.WriteAllLines(path, message.ToString().Split('\n'), new UTF8Encoding(false));
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
        /// 퀴즈 Thread의 실행부입니다.
        /// </summary>
        /// <param name="data">실행에 필요한 매개변수 목록</param>
        protected void RunQuiz(object data)
        {
            var parameters = (Dictionary<string, object>)data;
            var quizType = (Quiz.TypeOption)parameters["quizType"];
            string[] subjects = (string[])parameters["subjects"];
            int requestQuizCount = (int)parameters["requestQuizCount"];
            int quizTimeLimit = (int)parameters["quizTimeLimit"];
            int bonusExperience = (int)parameters["bonusExperience"];
            int bonusMoney = (int)parameters["bonusMoney"];
            int idleTimeLimit = (int)parameters["idleTimeLimit"];
            bool showSubject = (bool)parameters["showSubject"];
            bool isRandom = subjects.Length > 1 ? true : false;
            List<Quiz.Data> quizDataList = (List<Quiz.Data>)parameters["quizDataList"];

            KakaoTalk.Message[] messages;
            KakaoTalk.MessageType messageType;
            string userName;
            string content;
            QuizUser user;
            int quizMessageIndex;
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
                quizMessageIndex = (messages.Length - 1) + 1; // 뒤에 바로 SendMessage를 하므로, +1 해서 초기화
                string randomText = isRandom ? "랜덤 " : "";
                SendMessage($"[{randomText}" + (showSubject ? subject : "") + $" {currentQuiz}/{requestQuizCount}]{question}");

                if (beforeImagePath != null)
                {
                    Thread.Sleep(1500);
                    SendImage(beforeImagePath);
                }

                if (quizData.Choices != null)
                {
                    string choices = "";
                    for (int j = 0; j < quizData.Choices.Count; j++) choices += $"{j + 1}. {quizData.Choices[j]}\n";
                    choices = choices.Substring(0, choices.Length - 1);
                    Thread.Sleep(2000);
                    SendMessage(choices);
                }

                int beginTick = Environment.TickCount;
                bool shouldContinue = true;
                while (IsQuizTaskRunning && shouldContinue)
                {
                    Thread.Sleep(QuizScanInterval);
                    while ((messages = Window.GetMessagesUsingClipboard()) == null) Thread.Sleep(GetMessageInterval);
                    for (int j = quizMessageIndex; j < messages.Length; j++)
                    {
                        messageType = messages[j].Type;
                        userName = messages[j].UserName;
                        content = messages[j].Content;
                        user = FindUserByNickname(userName);
                        if (user == null) AddNewUser(userName);

                        if (!isCaseSensitive) { content = content.ToLower(); answer = answer.ToLower(); }

                        if (IsQuizTaskRunning) // 다른 곳에서 StopQuiz 요청에 의해 IsQuizTaskRunning은 false가 될 수 있음. 따라서 검사 시마다 확인.
                        {
                            if (messageType == KakaoTalk.MessageType.Talk && content == answer)
                            {
                                lastInputTick = Environment.TickCount;
                                SendMessage($"정답: {answer}, 정답자: [{user.CurrentTitle.Name}]{user.Nickname} (Lv. {user.Level}), 경험치: {user.Experience + bonusExperience}(+{bonusExperience}), 머니: {user.Money + bonusMoney}(+{bonusMoney})");
                                UpdateUserProfile(user, bonusExperience, bonusMoney);
                                Thread.Sleep(1500);
                                shouldContinue = false;
                                break;
                            }
                            else if (Environment.TickCount > lastInputTick + (idleTimeLimit * 1000))
                            {
                                SendMessage("장시간 유효한 입력이 발생하지 않아 문제 풀이를 중단합니다. 잠시만 기다려주세요...");
                                IsQuizTaskRunning = false;
                                break;
                            }
                            else if (Environment.TickCount > beginTick + (quizTimeLimit * 1000)) // 시간 제한 초과
                            {
                                SendMessage($"정답자가 없어서 다음 문제로 넘어갑니다. 정답: {answer}");
                                Thread.Sleep(1500);
                                shouldContinue = false;
                                break;
                            }
                        }
                        else break;
                    }
                    if (IsQuizTaskRunning && !shouldContinue)
                    {
                        if (afterImagePath != null)
                        {
                            SendImage(afterImagePath);
                            Thread.Sleep(1500);
                        }
                        if (explanation != null)
                        {
                            SendMessage($"[해설]{explanation}");
                            Thread.Sleep(3500);
                        }
                        else Thread.Sleep(1500);
                    }
                }
                if (!IsQuizTaskRunning) break;
                else if (i == requestQuizCount - 1)
                {
                    SendMessage("문제를 다 풀었습니다. 잠시만 기다려주세요...");
                    IsQuizTaskRunning = false;
                }
            }
            if (IsQuizTaskRunning) IsQuizTaskRunning = false;
            Thread.Sleep(2000);
            SendMessage("퀴즈가 종료되었습니다. \"!명령어\"를 통하여 기능을 확인하세요.");
            Thread.Sleep(SendMessageInterval);
            ReleaseMainThread();
        }

        /// <summary>
        /// 다중 Thread 처리 시 발생할 수 있는 DeadLock 문제를 해결하기 위한 메서드입니다.
        /// </summary>
        protected void ReleaseMainThread()
        {
            lock (Window) Monitor.PulseAll(Window);
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
            var message = new StringBuilder();
            message.Append("퀴즈봇을 돌리기 위해서는, 반드시 해당 주제에 대한 퀴즈 데이터 폴더 및 내부 파일이 존재해야 합니다.\n");
            message.Append("폴더 및 파일을 추가하는 방법은 아래와 같습니다.\n\n");

            message.Append("◎ 폴더 생성\n");
            message.Append("- data/quiz 경로에 폴더를 하나 생성합니다.\n");
            message.Append("(폴더 이름은 아무렇게나 해도 됩니다)\n\n");

            message.Append("◎ settings.ini 파일 추가\n");
            message.Append("생성한 폴더 내에 settings.ini라는 이름으로 파일을 생성합니다. 인코딩은 유니코드(= UTF-16)로 합니다.\n\n");

            message.Append("파일 최상단에 \"[Settings]\" 입력 (ini 파일의 섹션 값)\n\n");

            message.Append("- \"quizType = \" 뒤에 퀴즈 유형 입력 (\"일반\" 또는 \"초성\")\n\n");

            message.Append("- \"mainSubject = \" 뒤에 주제 입력\n\n");

            message.Append("- \"childSubjects = \" 뒤에 하위주제의 목록을 쉼표(,)로 구분하여 입력합니다. (하위주제가 없으면 입력하지 않아도 됩니다.)\n\n");

            message.Append("- \"isCaseSensitive = \" 뒤에 true 또는 false 입력\n");
            message.Append("(만약 정답에 알파벳이 포함되어 있는 경우, 대소문자를 구분해서 정답 처리하고 싶다면 true로, 그렇지 않다면 false를 입력하면 됩니다.)\n\n");

            message.Append("- \"useMultiChoice = \" 뒤에 true 또는 false 입력\n");
            message.Append("(만약 이 주제의 문제들을 객관식으로 하고 싶다면 true를, 아니라면 false를 입력하면 됩니다.)\n\n");

            message.Append("- \"choiceExtractMethod = \" 뒤에 RICS 또는 RAPT 입력\n");
            message.Append("(choiceExtractMethod는 선택지 추출 방식을 의미합니다.)\n");
            message.Append("(RICS : Random In Current Subject, 선택된 주제 내에서 랜덤으로 선택지를 추출하는 방식)\n");
            message.Append("(RAPT : Random According to Predefined Types, 각 답마다 타입을 지정해 놓고, 같은 타입 내에서만 랜덤으로 선택지를 추출하는 방식)\n\n");

            message.Append("- \"choiceCount = \" 뒤에 원하는 객관식 선택지 개수를 입력합니다.\n");
            message.Append("(객관식이 아니라도 0을 입력해야 합니다.)\n\n");

            message.Append("◎ settings.ini 파일 내용 예시\n");
            message.Append("[Settings]\n");
            message.Append("quizType = 일반\n");
            message.Append("mainSubject = 속담\n");
            message.Append("childSubjects = \n");
            message.Append("isCaseSensitive = false\n");
            message.Append("useMultiChoice = false\n");
            message.Append("choiceExtractMethod = \n");
            message.Append("choiceCount = \n\n");

            message.Append("◎ data.xml 파일 추가\n");
            message.Append("- settings.ini 파일과 같은 폴더에 data.xml이라는 이름으로 파일을 생성합니다. 인코딩은 UTF-8 without BOM으로 합니다.\n\n");

            message.Append("- list -> data 엘리먼트 안에 원하는 내용을 작성합니다.\n");
            message.Append("가능한 요소는 question, answer, explanation, type, beforeImagePath, afterImagePath, childSubject, regDate이며, 반드시 필요한 요소는 question, answer, regDate입니다.\n\n");

            message.Append("- data.xml 파일 내용 예시\n");
            message.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>\n");
            message.Append("<list>\n");
            message.Append("  <data>\n");
            message.Append("    <question>ㄱㅂㄹㄴ</question>\n");
            message.Append("    <answer>가보로네</answer>\n");
            message.Append("    <regDate>2019-08-04 17:40:48</regDate>\n");
            message.Append("  </data>\n");
            message.Append("</list>\n\n");

            message.Append("- 각 요소에 대한 설명\n");
            message.Append("question : 퀴즈에서 사용될 질문 내용 (필수)\n");
            message.Append("answer : 퀴즈에서 사용될 정답 내용 (필수)\n");
            message.Append("explanation : 퀴즈에서 사용될 설명 내용 (선택)\n");
            message.Append("type : 퀴즈에서 객관식 - RAPT 방식 사용 시 필요한 유형 값 (선택)\n");
            message.Append("beforeImagePath : 퀴즈에서 문제 출제 전에 보여줄 이미지 파일의 경로 (선택, 에시 : res/image/pokemon/피카츄.png)\n");
            message.Append("afterImagePath : 퀴즈에서 문제 출제 후에 보여줄 이미지 파일의 경로 (선택, 예시 : res/image/pokemon/꼬부기.png)\n");
            message.Append("childSubject : 퀴즈의 하위주제 값 (선택)\n");
            message.Append("regDate : 이 퀴즈 항목을 등록한 시각 (필수, 형식 : yyyy-mm-dd hh:mm:ss)\n\n");

            File.WriteAllLines(path, message.ToString().Split('\n'), Encoding.Unicode);
        }
    }
}

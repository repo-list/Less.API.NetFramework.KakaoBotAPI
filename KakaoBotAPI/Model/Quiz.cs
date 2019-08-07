using System;
using System.Collections.Generic;

namespace Less.API.NetFramework.KakaoBotAPI.Model
{
    /// <summary>
    /// QuizBot에서 기본적으로 사용되는 퀴즈에 관한 모델 클래스입니다.
    /// </summary>
    public class Quiz
    {
        /// <summary>
        /// 퀴즈의 유형<para/>
        /// Quiz.TypeOption.Gerneral : 일반 퀴즈<para/>
        /// Quiz.TypeOption.Chosung : 초성 퀴즈
        /// </summary>
        public TypeOption Type { get; }

        /// <summary>
        /// 퀴즈의 메인 주제<para/>
        /// ChildSubject(= 하위 주제)와 구분되는 개념입니다.
        /// </summary>
        public string MainSubject { get; }

        /// <summary>
        /// 퀴즈의 하위 주제 목록<para/>
        /// 하나의 메인 주제당 여러 개의 하위 주제를 가질 수 있습니다.
        /// </summary>
        public List<string> ChildSubjects { get; }

        /// <summary>
        /// 퀴즈 정답의 알파벳 대소문자 구분 여부<para/>
        /// true : 대소문자를 구분합니다.<para/>
        /// false : 대소문자를 구분하지 않습니다.
        /// </summary>
        public bool IsCaseSensitive { get; }

        /// <summary>
        /// 퀴즈를 객관식으로 처리할지 여부<para/>
        /// true : 객관식<para/>
        /// false : 주관식
        /// </summary>
        public bool UseMultiChoice { get; }

        /// <summary>
        /// 퀴즈가 객관식일 경우의 선택지를 추출하는 방식<para/>
        /// Quiz.ChoiceExtractMethodOption.None : 객관식이 아닐 때 사용합니다.<para/>
        /// Quiz.ChoiceExtractMethodOption.RICS : 해당 주제 내에서 랜덤으로 선택합니다. (Random In Current Subject)<para/>
        /// Quiz.ChoiceExtractMethodOption.RAPT : 미리 정의된 유형(= Quiz.Data.Type)에 준하여 랜덤으로 선택합니다.
        /// </summary>
        public ChoiceExtractMethodOption ChoiceExtractMethod { get; }

        /// <summary>
        /// 퀴즈가 객관식일 경우의 선택지 개수<para/>
        /// 객관식일 경우 1 이상을, 그렇지 않다면 아무 값이나 입력합니다. (권장 : 0)
        /// </summary>
        public int ChoiceCount { get; }

        /// <summary>
        /// 퀴즈에 대한 실제 데이터의 목록<para/>
        /// 하나의 인스턴스에 대하여 Quiz.Data 객체의 목록을 받아와서 일단 저장한 후에 이용하는 방식입니다.
        /// </summary>
        public List<Data> DataList { get; }

        /// <summary>
        /// 퀴즈 유형에 대한 옵션
        /// Quiz.TypeOption.General : 일반 퀴즈<para/>
        /// Quiz.TypeOption.Chosung : 초성 퀴즈
        /// </summary>
        public enum TypeOption { General, Chosung };

        /// <summary>
        /// 객관식 선택지 추출 방식에 대한 옵션<para/>
        /// Quiz.ChoiceExtractMethodOption.None : 객관식이 아닐 때 사용합니다.<para/>
        /// Quiz.ChoiceExtractMethodOption.RICS : 해당 주제 내에서 랜덤으로 선택합니다. (Random In Current Subject)<para/>
        /// Quiz.ChoiceExtractMethodOption.RAPT : 미리 정의된 유형(= Quiz.Data.Type)에 준하여 랜덤으로 선택합니다.
        /// </summary>
        public enum ChoiceExtractMethodOption { None, RICS, RAPT };

        /// <summary>
        /// 퀴즈 객체를 생성합니다.
        /// </summary>
        /// <param name="type">퀴즈의 유형</param>
        /// <param name="mainSubject">퀴즈의 메인 주제</param>
        /// <param name="childSubjects">퀴즈의 하위 주제 목록</param>
        /// <param name="isCaseSensitive">퀴즈 정답의 알파벳 대소문자 구분 여부</param>
        /// <param name="useMultiChoice">퀴즈를 객관식으로 처리할지 여부</param>
        /// <param name="choiceExtractMethod">퀴즈가 객관식일 경우의 선택지를 추출하는 방식</param>
        /// <param name="choiceCount">퀴즈가 객관식일 경우의 선택지 개수</param>
        /// <param name="dataList">퀴즈에 대한 실제 데이터의 목록</param>
        public Quiz(TypeOption type, string mainSubject, List<string> childSubjects, bool isCaseSensitive, bool useMultiChoice, ChoiceExtractMethodOption choiceExtractMethod, int choiceCount, List<Data> dataList)
        {
            Type = type;
            MainSubject = mainSubject;
            ChildSubjects = childSubjects;
            IsCaseSensitive = isCaseSensitive;
            UseMultiChoice = useMultiChoice;
            ChoiceExtractMethod = choiceExtractMethod;
            ChoiceCount = choiceCount;
            DataList = dataList;
        }

        /// <summary>
        /// QuizBot에서 기본적으로 사용되는 퀴즈 데이터에 관한 모델 클래스입니다.
        /// </summary>
        public class Data
        {
            /// <summary>
            /// 퀴즈 데이터의 메인 주제
            /// </summary>
            public string MainSubject { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 질문
            /// </summary>
            public string Question { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 정답
            /// </summary>
            public string Answer { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 설명<para/>
            /// 만약 별도의 설명이 존재하지 않는다면 null로 설정하십시오.
            /// </summary>
            public string Explanation { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 유형<para/>
            /// 퀴즈의 객관식 선택지 추출 방식을 Quiz.ChoiceExtractMethodOption.RAPT로 설정할 경우에 이 값이 이용됩니다.<para/>
            /// 객관식이 아니고, 별도로 필요하지 않을 경우 null로 설정하십시오.
            /// </summary>
            public string Type { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 선택지 목록<para/>
            /// 객관식이 아니라면 null로 설정하십시오.
            /// </summary>
            public List<string> Choices { get; set; }

            /// <summary>
            /// 해당 퀴즈 데이터의 선택지 노출 전에 전송되는 이미지의 경로<para/>
            /// 이미지를 전송하지 않는다면 null로 설정하십시오.
            /// </summary>
            public string BeforeImagePath { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 정답 공개 후에 전송되는 이미지의 경로<para/>
            /// 이미지를 전송하지 않는다면 null로 설정하십시오.
            /// </summary>
            public string AfterImagePath { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 하위 주제
            /// </summary>
            public string ChildSubject { get; }

            /// <summary>
            /// 해당 퀴즈 데이터의 정답 처리 시 알파벳 대소문자 구분 여부
            /// </summary>
            public bool IsCaseSensitive { get; }

            /// <summary>
            /// 해당 퀴즈 데이터가 등록된 일시<para/>
            /// 데이터 정렬 등에 활용하기 위해 부여된 값입니다.
            /// </summary>
            public DateTime RegDate { get; }

            /// <summary>
            /// 퀴즈 데이터 객체를 생성합니다.
            /// </summary>
            /// <param name="mainSubject">퀴즈 데이터의 메인 주제</param>
            /// <param name="question">해당 퀴즈 데이터의 질문</param>
            /// <param name="answer">해당 퀴즈 데이터의 정답</param>
            /// <param name="explanation">해당 퀴즈 데이터의 설명</param>
            /// <param name="type">해당 퀴즈 데이터의 유형</param>
            /// <param name="choices">해당 퀴즈 데이터의 선택지 목록</param>
            /// <param name="beforeImagePath">해당 퀴즈 데이터의 선택지 노출 전에 전송되는 이미지의 경로</param>
            /// <param name="afterImagePath">해당 퀴즈 데이터의 정답 공개 후에 전송되는 이미지의 경로</param>
            /// <param name="childSubject">해당 퀴즈 데이터의 하위 주제</param>
            /// <param name="isCaseSensitive">해당 퀴즈 데이터의 정답 처리 시 알파벳 대소문자 구분 여부</param>
            /// <param name="regDate">해당 퀴즈 데이터가 등록된 일시</param>
            public Data(string mainSubject, string question, string answer, string explanation, string type, List<string> choices, string beforeImagePath, string afterImagePath, string childSubject, bool isCaseSensitive, DateTime regDate)
            {
                MainSubject = mainSubject;
                Question = question;
                Answer = answer;
                Explanation = explanation;
                Type = type;
                Choices = choices;
                BeforeImagePath = beforeImagePath;
                AfterImagePath = afterImagePath;
                ChildSubject = childSubject;
                IsCaseSensitive = isCaseSensitive;
                RegDate = regDate;
            }
        }
    }
}

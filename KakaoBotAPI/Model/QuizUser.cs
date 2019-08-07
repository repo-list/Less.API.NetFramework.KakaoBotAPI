using System.Collections.Generic;

namespace Less.API.NetFramework.KakaoBotAPI.Model
{
    /// <summary>
    /// QuizBot에서 기본적으로 사용되는 유저에 대한 모델 클래스입니다.
    /// </summary>
    public class QuizUser : User
    {
        /// <summary>
        /// 유저의 경험치
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// 유저의 레벨
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 유저의 머니
        /// </summary>
        public int Money { get; set; }

        /// <summary>
        /// 유저의 세대<para/>
        /// 올드 유저들을 구분하여, 추가적인 혜택 등을 부여할 수 있도록 하기 위한 개념입니다.
        /// </summary>
        public int Generation { get; }

        /// <summary>
        /// 유저에게 현재 적용된 타이틀
        /// </summary>
        public Title CurrentTitle { get; set; }

        /// <summary>
        /// 유저가 적용 가능한 타이틀 목록
        /// </summary>
        public List<Title> AvailableTitles { get; }

        /// <summary>
        /// 퀴즈 유저 객체를 생성합니다.
        /// </summary>
        /// <param name="nickname">유저의 닉네임</param>
        /// <param name="isIgnored">유저의 채팅을 무시할지에 대한 여부</param>
        /// <param name="experience">유저의 경험치</param>
        /// <param name="level">유저의 레벨</param>
        /// <param name="money">유저의 머니</param>
        /// <param name="generation">유저의 세대</param>
        /// <param name="currentTitle">유저에게 현재 적용된 타이틀</param>
        /// <param name="availableTitles">유저가 적용 가능한 타이틀 목록</param>
        public QuizUser(string nickname, bool isIgnored, int experience, int level, int money, int generation, Title currentTitle, List<Title> availableTitles) : base(nickname, isIgnored)
        {
            Experience = experience;
            Level = level;
            Money = money;
            Generation = generation;
            CurrentTitle = currentTitle;
            AvailableTitles = availableTitles;
        }
    }
}

using System.Collections.Generic;

namespace Less.API.NetFramework.KakaoBotAPI.People
{
    public class QuizUser : User
    {
        public const int NewExperience = 0;
        public const int NewLevel = 1;
        public const int NewMoney = 0;
        public const int NewGeneration = 2;
        public static Title NewCurrentTitle = new Title(Title.BaseType.Level, Title.LevelBased.신입생);
        public static List<Title> NewAvailableTitles = new List<Title>() { NewCurrentTitle };

        public int Experience { get; set; }
        public int Level { get; set; }
        public int Money { get; set; }
        public int Generation { get; } // 올드 유저들을 구분하기 위한 세대 개념
        public Title CurrentTitle { get; set; }
        public List<Title> AvailableTitles { get; }

        public QuizUser(string nickname, int experience, int level, int money, int generation, Title currentTitle, List<Title> availableTitles) : base(nickname)
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

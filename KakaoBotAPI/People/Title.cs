namespace Less.API.NetFramework.KakaoBotAPI.People
{
    public struct Title
    {
        public enum BaseType { Custom, Level, Money, Quest, Event }

        public BaseType Type { get; }
        public string Name { get; }

        public Title(string name) : this(GetBaseType(name), name) { }

        public Title(BaseType type, string name)
        {
            Type = type;
            Name = name;
        }

        // 타이틀 이름은 그 어떤 경우에도 중복이 안 되어야 함. 주의할 것.
        private static BaseType GetBaseType(string name)
        {
            switch (name)
            {
                case LevelBased.신입생:
                case LevelBased.열공러:
                case LevelBased.노력파:
                case LevelBased.기대주:
                case LevelBased.신인왕:
                case LevelBased.고수:
                case LevelBased.초고수:
                case LevelBased.준프로:
                case LevelBased.프로:
                case LevelBased.퀴즈봇:
                case LevelBased.인공지능:
                case LevelBased.퀴즈신:
                    return BaseType.Level;
                case MoneyBased.저축왕:
                case MoneyBased.알바생:
                case MoneyBased.금수저:
                case MoneyBased.지점장:
                case MoneyBased.갑부:
                case MoneyBased.거상:
                    return BaseType.Money;
                default:
                    return BaseType.Custom;
            }
        }

        public struct LevelBased
        {
            public const string 신입생 = "신입생";
            public const string 열공러 = "열공러";
            public const string 노력파 = "노력파";
            public const string 기대주 = "기대주";
            public const string 신인왕 = "신인왕";
            public const string 고수 = "고수";
            public const string 초고수 = "초고수";
            public const string 준프로 = "준프로";
            public const string 프로 = "프로";
            public const string 퀴즈봇 = "퀴즈봇";
            public const string 인공지능 = "인공지능";
            public const string 퀴즈신 = "퀴즈신";
        }

        public struct MoneyBased
        {
            public const string 저축왕 = "저축왕";
            public const string 알바생 = "알바생";
            public const string 금수저 = "금수저";
            public const string 지점장 = "지점장";
            public const string 갑부 = "갑부";
            public const string 거상 = "거상";
        }

        public struct QuestBased { }
        public struct EventBased { }
    }
}

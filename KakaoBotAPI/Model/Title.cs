namespace Less.API.NetFramework.KakaoBotAPI.Model
{
    /// <summary>
    /// QuizUser가 가지는 속성 중 하나로, 닉네임 외에 부가적으로 사용하는 칭호를 나타내는 모델 클래스입니다.
    /// </summary>
    public class Title
    {
        /// <summary>
        /// 타이틀의 이름
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 타이틀 객체를 생성합니다.
        /// </summary>
        /// <param name="name">타이틀의 이름</param>
        public Title(string name)
        {
            Name = name;
        }
    }
}

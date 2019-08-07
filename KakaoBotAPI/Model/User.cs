namespace Less.API.NetFramework.KakaoBotAPI.Model
{
    /// <summary>
    /// ChatBot에서 기본적으로 사용되는 유저에 대한 모델 클래스입니다.
    /// </summary>
    public class User
    {
        /// <summary>
        /// 유저의 닉네임
        /// </summary>
        public string Nickname { get; }

        /// <summary>
        /// 유저의 채팅을 무시할지에 대한 여부<para/>
        /// true : 채팅을 무시합니다.<para/>
        /// false : 채팅을 무시하지 않습니다.
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        /// 유저 객체를 생성합니다.
        /// </summary>
        /// <param name="nickname">유저의 닉네임</param>
        /// <param name="isIgnored">유저의 채팅을 무시할지에 대한 여부</param>
        public User(string nickname, bool isIgnored)
        {
            Nickname = nickname;
            IsIgnored = isIgnored;
        }
    }
}

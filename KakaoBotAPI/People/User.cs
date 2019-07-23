namespace Less.API.NetFramework.KakaoBotAPI.People
{
    /// <summary>
    /// 채팅봇에서 기본적으로 사용되는 유저 클래스입니다.
    /// 채팅봇 클래스 상속 시 유저에 대한 재정의가 필요하다면 이 클래스를 상속하여 사용해야 합니다.
    /// </summary>
    public class User
    {
        public string Nickname { get; }
        public bool IsIgnored { get; set; }

        public User(string nickname)
        {
            Nickname = nickname;
            IsIgnored = false;
        }
    }
}

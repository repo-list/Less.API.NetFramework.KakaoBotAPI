using Less.API.NetFramework.KakaoTalkAPI;

namespace Less.API.NetFramework.KakaoBotAPI.Bot
{
    /// <summary>
    /// 모든 카카오봇 클래스의 모태가 되는 인터페이스
    /// </summary>
    interface IKakaoBot
    {
        /// <summary>
        /// 봇 인스턴스를 시작합니다.
        /// </summary>
        void Start();

        /// <summary>
        /// 봇 인스턴스를 중지합니다.
        /// </summary>
        void Stop();

        /// <summary>
        /// 봇 객체를 참조하여 메시지를 전송합니다.
        /// </summary>
        /// <param name="message">전송할 메시지</param>
        /// <returns>전송의 성공 여부</returns>
        bool SendMessage(string message);

        /// <summary>
        /// 봇 객체를 참조하여 이미지를 전송합니다.
        /// </summary>
        /// <param name="path">전송할 이미지 파일의 경로</param>
        /// <returns>전송의 성공 여부</returns>
        bool SendImage(string path);

        /// <summary>
        /// 봇 객체를 참조하여 이모티콘을 전송합니다.
        /// </summary>
        /// <param name="emoticon">전송할 카카오톡 이모티콘</param>
        /// <returns>전송의 성공 여부</returns>
        bool SendEmoticon(KakaoTalk.Emoticon emoticon);
    }
}

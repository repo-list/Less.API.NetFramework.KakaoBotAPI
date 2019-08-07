// Reference : https://stackoverflow.com/questions/217902/reading-writing-an-ini-file - Answer by Danny Beckett

using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Less.API.NetFramework.KakaoBotAPI.Util
{
    /// <summary>
    /// INI 파일을 처리하는 데 도움을 주는 유틸리티 클래스입니다.
    /// </summary>
    public class IniHelper
    {
        /// <summary>
        /// INI 파일의 기본 확장자
        /// </summary>
        public const string FileExtension = ".ini";

        /// <summary>
        /// INI 파일의 경로<para/>
        /// 생성자 내부에서 변형된 값이 저장되므로, getter만을 허용합니다.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// INI 헬퍼 객체를 생성합니다.
        /// </summary>
        /// <param name="path">INI 파일의 경로</param>
        public IniHelper(string path)
        {
            Path = new FileInfo(path.Contains(FileExtension) && path.IndexOf(FileExtension) == path.Length - 4 ? path : path + FileExtension).FullName;
        }

        /// <summary>
        /// INI 파일의 해당 section에서 특정 key를 가진 값을 읽어들입니다.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="section">Section</param>
        /// <returns>Value</returns>
        public string Read(string key, string section)
        {
            var value = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", value, 255, Path);
            return value.ToString();
        }

        /// <summary>
        /// INI 파일의 특정 section에 key-value pair를 작성합니다.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="section">Section</param>
        public void Write(string key, string value, string section)
        {
            WritePrivateProfileString(section, key, value, Path);
        }


        /* INI 처리용 Windows API 함수 목록 */

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultStr, StringBuilder retVal, int size, string filePath);
    }
}

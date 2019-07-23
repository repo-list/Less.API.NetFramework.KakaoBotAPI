// Reference : https://stackoverflow.com/questions/217902/reading-writing-an-ini-file - Answer by Danny Beckett

using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Less.API.NetFramework.KakaoBotAPI.Util
{
    public class IniHelper
    {
        public const string Extension = ".ini";

        public string Path { get; }

        public IniHelper(string path)
        {
            Path = new FileInfo(path.Contains(Extension) && path.IndexOf(Extension) == path.Length - 4 ? path : path + Extension).FullName;
        }

        public IniHelper CreateFile()
        {
            File.Create(Path).Close();
            return this;
        }

        public string Read(string key, string section)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retVal, 255, Path);
            return retVal.ToString();
        }

        public IniHelper Write(string key, string value, string section)
        {
            WritePrivateProfileString(section, key, value, Path);
            return this;
        }

        public IniHelper Update(string key, string value, string section)
        {
            DeleteKey(key, section);
            Write(key, value, section);
            return this;
        }

        public bool KeyExists(string key, string section)
        {
            return Read(key, section).Length > 0;
        }

        public IniHelper DeleteKey(string key, string section)
        {
            Write(key, null, section);
            return this;
        }

        public IniHelper DeleteSection(string section)
        {
            Write(null, null, section);
            return this;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultStr, StringBuilder retVal, int size, string filePath);
    }
}

using Less.API.NetFramework.WindowsAPI;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

// .Net Framework에서 기본 제공하는 Clipboard 클래스는 불안정하기 때문에, 전부 Native API로 처리해야 함.

namespace Less.API.NetFramework.KakaoTalkAPI
{
    internal sealed class ClipboardManager
    {
        public static bool HasDataToRestore = false;
        static uint Format;
        static object Data;
        static readonly IntPtr ClipboardOwner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        static IntPtr MemoryHandle = IntPtr.Zero;

        /// <summary>
        /// 현재 클립보드에 저장되어 있는 데이터를 백업합니다. 클립보드 열기 요청 실패 시 ClipboardManager.CannotOpenException 예외가 발생합니다.
        /// </summary>
        public static void BackupData()
        {
            Format = 0;

            bool isClipboardOpen = WinAPI.OpenClipboard(ClipboardOwner);
            if (!isClipboardOpen) throw new CannotOpenException();
            do { Format = WinAPI.EnumClipboardFormats(Format); }
            while (Format >= 0x200 || Format == 0);

            IntPtr pointer = WinAPI.GetClipboardData(Format);
            switch (Format)
            {
                case WinAPI.CF_TEXT:
                    Data = Marshal.PtrToStringAnsi(pointer);
                    MemoryHandle = Marshal.StringToHGlobalAnsi((string)Data);
                    break;
                case WinAPI.CF_UNICODETEXT:
                    Data = Marshal.PtrToStringUni(pointer);
                    MemoryHandle = Marshal.StringToHGlobalUni((string)Data);
                    break;
                case WinAPI.CF_BITMAP:
                    Data = Image.FromHbitmap(pointer);
                    MemoryHandle = ((Bitmap)Data).GetHbitmap();
                    break;
            }
            WinAPI.CloseClipboard();

            HasDataToRestore = true;
        }

        /// <summary>
        /// 백업했던 클립보드 데이터를 복구합니다. 현재 텍스트와 이미지만 복구 기능을 지원하며, 클립보드 열기 요청 실패 시 ClipboardManager.CannotOpenException 예외가 발생합니다.
        /// </summary>
        public static void RestoreData()
        {
            if (!HasDataToRestore) return;

            if (Format == WinAPI.CF_TEXT || Format == WinAPI.CF_UNICODETEXT)
            {
                bool isClipboardOpen = WinAPI.OpenClipboard(ClipboardOwner);
                if (!isClipboardOpen) throw new CannotOpenException();
            }

            switch (Format)
            {
                case WinAPI.CF_TEXT:
                    WinAPI.SetClipboardData(Format, MemoryHandle);
                    break;
                case WinAPI.CF_UNICODETEXT:
                    WinAPI.SetClipboardData(Format, MemoryHandle);
                    break;
                case WinAPI.CF_BITMAP:
                case WinAPI.CF_DIB:
                    Format = WinAPI.CF_BITMAP;
                    SetImage(MemoryHandle);
                    (Data as Bitmap).Dispose();
                    break;
            }
            if (Format == WinAPI.CF_TEXT || Format == WinAPI.CF_UNICODETEXT) WinAPI.CloseClipboard();

            WinAPI.DeleteObject(MemoryHandle);
            Data = null;
            MemoryHandle = IntPtr.Zero;
            HasDataToRestore = false;
        }

        /// <summary>
        /// 클립보드에서 텍스트를 가져옵니다. 클립보드 열기 요청 실패 시 ClipboardManager.CannotOpenException 예외가 발생하고, 만약 텍스트가 존재하지 않을 경우 null을 반환합니다.
        /// </summary>
        public static string GetText()
        {
            string text = null;

            bool isClipboardOpen = WinAPI.OpenClipboard(ClipboardOwner);
            if (!isClipboardOpen) throw new CannotOpenException();
            IntPtr pointer = WinAPI.GetClipboardData(WinAPI.CF_UNICODETEXT);
            if (pointer == IntPtr.Zero)
            {
                pointer = WinAPI.GetClipboardData(WinAPI.CF_TEXT);
                if (pointer != IntPtr.Zero) text = Marshal.PtrToStringAnsi(pointer);
            }
            else text = Marshal.PtrToStringUni(pointer);
            WinAPI.CloseClipboard();

            return text;
        }

        /// <summary>
        /// 클립보드에 텍스트를 저장합니다. 클립보드 열기 요청 실패 시 ClipboardManager.CannotOpenException 예외가 발생합니다.
        /// </summary>
        /// <param name="text">저장할 텍스트</param>
        public static void SetText(string text)
        {
            bool isClipboardOpen = WinAPI.OpenClipboard(ClipboardOwner);
            if (!isClipboardOpen) throw new CannotOpenException();
            WinAPI.EmptyClipboard();
            WinAPI.SetClipboardData(WinAPI.CF_TEXT, Marshal.StringToHGlobalAnsi(text));
            WinAPI.SetClipboardData(WinAPI.CF_UNICODETEXT, Marshal.StringToHGlobalUni(text));
            WinAPI.CloseClipboard();
        }

        /// <summary>
        /// 클립보드에 이미지를 저장합니다. 클립보드 열기 요청 실패 시 ClipboardManager.CannotOpenException 예외가 발생합니다.
        /// 또한 이 메서드를 짧은 시간 간격을 두고 주기적으로 호출할 경우 ExternalException 및 ContextSwitchDeadLock 현상이 발생할 수 있습니다.
        /// 따라서 이 메서드를 반복문 내에서 사용할 때는 주의가 필요합니다.
        /// </summary>
        /// <param name="imagePath">저장할 이미지의 원본 파일 경로</param>
        public static void SetImage(string imagePath)
        {
            using (Bitmap image = (Bitmap)Image.FromFile(imagePath)) _SetImage(image);
        }

        public static void SetImage(IntPtr hBitmap)
        {
            using (Bitmap image = Image.FromHbitmap(hBitmap)) _SetImage(image);
        }

        private static void _SetImage(Bitmap image)
        {
            Bitmap tempImage = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(tempImage))
            {
                IntPtr hScreenDC = WinAPI.GetWindowDC(IntPtr.Zero); // 기본적인 Device Context의 속성들을 카피하기 위한 작업
                IntPtr hDestDC = WinAPI.CreateCompatibleDC(hScreenDC);
                IntPtr hDestBitmap = WinAPI.CreateCompatibleBitmap(hScreenDC, image.Width, image.Height); // destDC와 destBitmap 모두 반드시 screenDC의 속성들을 기반으로 해야 함.
                IntPtr hPrevDestObject = WinAPI.SelectObject(hDestDC, hDestBitmap);

                IntPtr hSourceDC = graphics.GetHdc();
                IntPtr hSourceBitmap = image.GetHbitmap();
                IntPtr hPrevSourceObject = WinAPI.SelectObject(hSourceDC, hSourceBitmap);

                WinAPI.BitBlt(hDestDC, 0, 0, image.Width, image.Height, hSourceDC, 0, 0, WinAPI.SRCCOPY);

                WinAPI.DeleteObject(WinAPI.SelectObject(hSourceDC, hPrevSourceObject));
                WinAPI.SelectObject(hDestDC, hPrevDestObject); // 리턴값 : hDestBitmap
                graphics.ReleaseHdc(hSourceDC);
                WinAPI.DeleteDC(hDestDC);

                bool isClipboardOpen = WinAPI.OpenClipboard(ClipboardOwner);
                if (!isClipboardOpen)
                {
                    WinAPI.DeleteObject(hDestBitmap);
                    WinAPI.DeleteObject(hSourceDC);
                    WinAPI.DeleteObject(hSourceBitmap);
                    throw new CannotOpenException();
                }
                WinAPI.EmptyClipboard();
                WinAPI.SetClipboardData(WinAPI.CF_BITMAP, hDestBitmap);
                WinAPI.CloseClipboard();

                WinAPI.DeleteObject(hDestBitmap);
                WinAPI.DeleteObject(hSourceDC);
                WinAPI.DeleteObject(hSourceBitmap);
            }
            tempImage.Dispose();
        }

        public class CannotOpenException : Exception
        {
            internal CannotOpenException() : base("클립보드가 다른 프로그램에 의해 이미 사용되고 있습니다.") { }
        }

        public class InvalidFormatRequestException : Exception
        {
            internal InvalidFormatRequestException() : base("잘못된 클립보드 포맷 요청입니다.") { }
        }
    }
}

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Less.API.NetFramework.WindowsAPI
{
    /// <summary>
    /// Windows API의 메서드 목록을 담고 있는 클래스입니다.
    /// </summary>
    public sealed class WinAPI
    {
        // 버전 정보
        private static string FullApiVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public readonly static string ApiVersion = FullApiVersion.Substring(0, FullApiVersion.LastIndexOf('.'));

        // hWnd 관련 상수값 목록
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;

        // Send / Post Message 문자열 처리
        public const int WM_SETTEXT = 0xC;
        public const int WM_GETTEXT = 0xD;
        public const int WM_GETTEXTLENGTH = 0xE;

        // Key Press
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;

        // Command
        public const int WM_COMMAND = 0x0111;

        // Mouse Click
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;

        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MBUTTONDBLCLK = 0x209;

        public const int HTCLIENT = 1;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_MOUSEMOVE = 0x200;

        // GDI
        public const int SRCCOPY = 0xCC0020;

        // Clipboard
        public const int CF_TEXT = 1;
        public const int CF_BITMAP = 2;
        public const int CF_DIB = 8;
        public const int CF_UNICODETEXT = 13;

        // Hooking
        public const int WH_GETMESSAGE = 3;
        public const int WH_CALLWNDPROC = 4;
        public const int WH_KEYBOARD_LL = 13;

        // ShowWindow
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string className, string wndTitle);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

        [DllImport("User32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // 메시지 핸들링
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hwnd, uint msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        public static extern int PostMessage(IntPtr hwnd, uint msg, int wParam, int lParam);

        [DllImport("User32.dll", CharSet = CharSet.Ansi)]
        public static extern int SendMessageA(IntPtr hWnd, uint msg, int wParam, string lParam);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int SendMessageW(IntPtr hWnd, uint msg, int wParam, string lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessageGetTextLen(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessageGetTextW(IntPtr hWnd, uint msg, int wParam, [Out] StringBuilder lParam);

        // GDI 함수
        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGdiObject);

        [DllImport("Gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("Gdi32.dll")]
        public static extern bool BitBlt(IntPtr hDestDC, int destX, int destY, int width, int height, IntPtr hSourceDC, int sourceX, int sourceY, int rasterOperationType);

        [DllImport("Gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        // 클립보드 처리
        [DllImport("User32.dll")]
        public static extern bool OpenClipboard(IntPtr hNewOwner);

        [DllImport("User32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("User32.dll")]
        public static extern IntPtr SetClipboardData(uint format, IntPtr hMemory);

        [DllImport("User32.dll")]
        public static extern IntPtr GetClipboardData(uint format);

        [DllImport("User32.dll")]
        public static extern uint EnumClipboardFormats(uint format);

        [DllImport("User32.dll")]
        public static extern bool CloseClipboard();

        // 창 다루기
        [DllImport("User32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder buffer, int maxLength);

        [DllImport("User32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder buffer, int maxLength);

        [DllImport("User32.dll")]
        public static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO windowInfo);

        [DllImport("User32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("User32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int command);

        [DllImport("User32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        // 커스텀 API 목록

        // 커스텀 상수
        public enum Encoding { Ansi = 1, Unicode }

        public static string GetEditText(IntPtr hWnd)
        {
            int capacity = SendMessage(hWnd, WM_GETTEXTLENGTH, 0, 0);
            var buffer = new StringBuilder(capacity);
            SendMessageGetTextW(hWnd, WM_GETTEXT, capacity + 1, buffer);

            return buffer.ToString();
        }

        public static void SetEditText(IntPtr hWnd, string text, Encoding encoding)
        {
            if (encoding == Encoding.Ansi) SendMessageA(hWnd, WM_SETTEXT, 0, text);
            else if (encoding == Encoding.Unicode) SendMessageW(hWnd, WM_SETTEXT, 0, text);
        }

        public static void PressKeyInBackground(IntPtr hWnd, int keyCode)
        {
            PostMessage(hWnd, WM_KEYDOWN, keyCode, 0x1);
            PostMessage(hWnd, WM_KEYUP, keyCode, (int)(0x100000000 - 0xC0000001));
        }

        public static void ClickInBackground(IntPtr hWnd, MouseButton button, short x, short y)
        {
            int message1, message2;

            if (button == MouseButton.Left)
            {
                message1 = WM_LBUTTONDOWN;
                message2 = WM_LBUTTONUP;
            }
            else if (button == MouseButton.Right)
            {
                message1 = WM_RBUTTONDOWN;
                message2 = WM_RBUTTONUP;
            }
            else if (button == MouseButton.Middle)
            {
                message1 = WM_MBUTTONDOWN;
                message2 = WM_MBUTTONUP;
            }
            else throw new NoSuchButtonException();

            PostMessage(hWnd, (uint)message1, 0, (y * 0x10000) | (x & 0xFFFF));
            PostMessage(hWnd, (uint)message2, 0, (y * 0x10000) | (x & 0xFFFF));
        }

        public static void DoubleClickInBackground(IntPtr hWnd, MouseButton button, short x, short y)
        {
            int message;

            if (button == MouseButton.Left) message = WM_LBUTTONDBLCLK;
            else if (button == MouseButton.Right) message = WM_RBUTTONDBLCLK;
            else if (button == MouseButton.Middle) message = WM_MBUTTONDBLCLK;
            else throw new NoSuchButtonException();

            PostMessage(hWnd, (uint)message, 0, (y * 0x10000) | (x & 0xFFFF));
        }

        public static IntPtr GetFirstHwndWithIdentifiers(string className, string caption)
        {
            IntPtr hWndNew = IntPtr.Zero;

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (className == null || GetClassName(hWnd).Equals(className))
                {
                    if (caption == null || GetWindowText(hWnd).Equals(caption))
                    {
                        hWndNew = hWnd;
                        return false;
                    }
                }

                return true;
            }, IntPtr.Zero);

            return hWndNew;
        }

        public static List<IntPtr> GetHwndListWithIdentifiers(string className, string caption)
        {
            List<IntPtr> hWndList = new List<IntPtr>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (className == null || GetClassName(hWnd).Equals(className))
                {
                    if (caption == null || GetWindowText(hWnd).Equals(caption)) hWndList.Add(hWnd);
                }

                return true;
            }, IntPtr.Zero);

            return hWndList;
        }

        public static string GetWindowInfo(IntPtr hWnd)
        {
            int resultBufferCapacity = 512;
            int tempBufferCapacity = 256;
            StringBuilder resultBuffer = new StringBuilder(resultBufferCapacity);

            resultBuffer.Append("hWnd : 0x" + hWnd.ToString("X") + "\n");

            StringBuilder tempBuffer = new StringBuilder(tempBufferCapacity);
            int length = GetWindowText(hWnd, tempBuffer, tempBufferCapacity);
            resultBuffer.Append("Caption : " + tempBuffer.ToString() + "\n");
            resultBuffer.Append("Caption Length : " + length + "\n");

            tempBuffer.Clear();
            length = GetClassName(hWnd, tempBuffer, tempBufferCapacity);
            resultBuffer.Append("Class : " + tempBuffer.ToString() + "\n");
            resultBuffer.Append("Class Length : " + length + "\n");
            resultBuffer.Append("\n");

            var windowInfo = new WINDOWINFO();
            windowInfo.cbSize = (uint)Marshal.SizeOf(windowInfo);
            GetWindowInfo(hWnd, ref windowInfo);

            resultBuffer.Append("rcWindow.Left : " + windowInfo.rcWindow.left + "\n");
            resultBuffer.Append("rcWindow.Top : " + windowInfo.rcWindow.top + "\n");
            resultBuffer.Append("rcWindow.Right : " + windowInfo.rcWindow.right + "\n");
            resultBuffer.Append("rcWindow.Bottom : " + windowInfo.rcWindow.bottom + "\n");
            resultBuffer.Append("\n");

            resultBuffer.Append("rcClient.Left : " + windowInfo.rcClient.left + "\n");
            resultBuffer.Append("rcClient.Top : " + windowInfo.rcClient.top + "\n");
            resultBuffer.Append("rcClient.Right : " + windowInfo.rcClient.right + "\n");
            resultBuffer.Append("rcClient.Bottom : " + windowInfo.rcClient.bottom + "\n");
            resultBuffer.Append("\n");

            return resultBuffer.ToString();
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            var rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            return rect;
        }

        public static string GetClassName(IntPtr hWnd)
        {
            int capacity = 256;
            StringBuilder buffer = new StringBuilder(capacity);
            GetClassName(hWnd, buffer, capacity);

            return buffer.ToString();
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            int capacity = 256;
            StringBuilder buffer = new StringBuilder(capacity);
            GetWindowText(hWnd, buffer, capacity);

            return buffer.ToString();
        }

        public static void ResizeWindow(IntPtr hWnd, int width, int height)
        {
            RECT rect = GetWindowRect(hWnd);
            MoveWindow(hWnd, rect.left, rect.top, width, height, true);
        }

        public static void MoveWindow(IntPtr hWnd, int x, int y)
        {
            RECT rect = GetWindowRect(hWnd);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;
            MoveWindow(hWnd, x, y, width, height, false);
        }

        public struct KeyCode
        {
            public static int VK_LBUTTON = 0x01; // Left Mouse Button
            public static int VK_RBUTTON = 0x02; // Right Mouse Button
            public static int VK_MBUTTON = 0x04; // Middle Mouse Button
            public static int VK_TAB = 0x09;
            public static int VK_RETURN = 0x0D; // == Enter Key
            public static int VK_ENTER = VK_RETURN;
            public static int VK_SHIFT = 0x10;
            public static int VK_CONTROL = 0x11;
            public static int VK_MENU = 0x12; // Alt Key
            public static int VK_CAPITAL = 0x14; // Caps Lock Key
            public static int VK_ESCAPE = 0x1B; // == ESC key
            public static int VK_ESC = VK_ESCAPE;
            public static int VK_LEFT = 0x25;
            public static int VK_UP = 0x26;
            public static int VK_RIGHT = 0x27;
            public static int VK_DOWN = 0x28;
        }

        public enum MouseButton { Left = 1, Right, Middle }

        public class NoSuchButtonException : Exception
        {
            internal NoSuchButtonException() : base("존재하지 않는 버튼 값입니다.") { }
        }
    }

    /// <summary>
    /// x,y 좌표값에 대한 정보를 갖는 구조체입니다.
    /// </summary>
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// 상,하,좌,우 좌표값에 대한 정보를 갖는 구조체입니다.
    /// </summary>
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// 창에 대한 정보를 갖는 구조체입니다.
    /// </summary>
    public struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrossBorderBan.Common
{
    /// <summary>
    /// Start and stop of the cross-border ban.
    /// </summary>
    public static class RangeMouse
    {

        /// <summary>
        /// Hook Id.
        /// </summary>
        private static IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// Target screen.
        /// </summary>
        private static Screen _targetScreen;

        /// <summary>
        /// Clipping mouse range rectangle.
        /// </summary>
        private static Rectangle _mouseRangeRectangle;

        /// <summary>
        /// Callback mothod.
        /// </summary>
        private static readonly NativeMethods.LowLevelMouseProc HookCallbackProc = HookCallback;

        /// <summary>
        /// Setting Grobal Hook;
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private static IntPtr SetGrobalHook(NativeMethods.LowLevelMouseProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {

                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);

            }
        }

        /// <summary>
        /// Unhook mouse.
        /// </summary>
        /// <param name="hookId"></param>
        private static bool Unhook(IntPtr hookId)
        {
            return NativeMethods.UnhookWindowsHookEx(hookId);

        }

        /// <summary>
        /// Getting mouse range rectangle
        /// </summary>
        /// <returns></returns>
        private static Rectangle GetMouseRangeRectangle(Screen screen)
        {
            return screen.Bounds;
        }

        /// <summary>
        /// Callback mouse hook.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        internal static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.MouseMessages.WM_MOUSEMOVE)
            {
                var lparam =
               ((NativeMethods.MouseHookStruct)
                   Marshal.PtrToStructure(lParam, typeof(NativeMethods.MouseHookStruct)));
                var point = lparam.pt;


                if (!(point.x >= _mouseRangeRectangle.Location.X &&
                      point.x <= _mouseRangeRectangle.Location.X + _mouseRangeRectangle.Size.Width &&
                      point.y >= _mouseRangeRectangle.Location.Y &&
                      point.y <= _mouseRangeRectangle.Location.Y + _mouseRangeRectangle.Size.Height))
                {
                    Cursor.Clip = _mouseRangeRectangle;
                }
                else
                {
                    Cursor.Clip = Rectangle.Empty;
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Initialize and start.
        /// </summary>
        public static void Start(Screen screen)
        {
            if (_hookId != IntPtr.Zero)
            {
                if (_targetScreen != null && !_targetScreen.Equals(screen))
                {
                    End();
                    _targetScreen = null;
                }
                else
                {
                    return;
                }
            }

            _mouseRangeRectangle = GetMouseRangeRectangle(screen);

            Cursor.Clip = _mouseRangeRectangle;
            _hookId = SetGrobalHook(HookCallbackProc);
        }


        /// <summary>
        /// stop and post-processing.
        /// </summary>
        public static void End()
        {
            Cursor.Clip = Rectangle.Empty;
            if (Unhook(_hookId))
            {
                _hookId = IntPtr.Zero;
            }
#if DEBUG
            if (_hookId != IntPtr.Zero)
                Console.WriteLine("failed unhook！");
#endif
        }
    }
}

using Digimezzo.Utilities.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Digimezzo.Utilities.Utils
{
    public class WindowUtils
    {
        #region Public
        public static void EnableBlur(Window win)
        {
            var windowHelper = new WindowInteropHelper(win);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public static void MoveWindow(Window win)
        {
            const int WM_NCLBUTTONDOWN = 0xa1;
            const int HT_CAPTION = 0x2;

            Point point = Mouse.GetPosition(win);

            // This handles DragMove and restoring from snap in Windows
            Point wpfPoint = win.PointToScreen(point);
            int x = Convert.ToInt16(wpfPoint.X);
            int y = Convert.ToInt16(wpfPoint.Y);
            int lParam = Convert.ToInt32(Convert.ToInt32(x)) | (y << 16);

            IntPtr windowHandle = new WindowInteropHelper(win).Handle;
            NativeMethods.SendMessage(windowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, lParam);
        }

        public static void CenterWindow(Window win)
        {
            // This is a hack to get the Dialog to center on the application's main window
            try
            {
                if (Application.Current.MainWindow.IsVisible)
                {
                    win.Owner = Application.Current.MainWindow;
                    win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    // If the main window is not visible (like when the OOBE screen is visible),
                    // center the Dialog on the screen
                    win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                // The try catch should not be necessary. But added just in case.
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        /// <summary>
        /// Hides the given window from the Windows ALT-TAB menu. Must be called AFTER the window is shown. 
        /// It doesn't work when called from the window's constructor. Instead, overload the Show() 
        /// function and call it after MyBase.Show(), or call it in the Loaded() function.
        /// </summary>
        /// <param name="win"></param>
        /// <remarks></remarks>
        public static void HideWindowFromAltTab(Window win)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(win);

            int exStyle = Convert.ToInt32(NativeMethods.GetWindowLong(wndHelper.Handle, Convert.ToInt32(GWL.EXSTYLE)));
            exStyle = exStyle | Convert.ToInt32(WSEX.TOOLWINDOW);

            NativeMethods.SetWindowLongPtr(wndHelper.Handle, Convert.ToInt32(GWL.EXSTYLE), (IntPtr)exStyle);
        }

        public static void ShowWindowInAltTab(Window win)
        {
            var wndHelper = new WindowInteropHelper(win);

            int normalEWS = 262400; // 'exStyle' cannot be restored by 'OR' operation. Use this 'magic' number instead.
            NativeMethods.SetWindowLongPtr(wndHelper.Handle, Convert.ToInt32(GWL.EXSTYLE), (IntPtr)normalEWS);
        }

        public static void RemoveWindowCaption(Window win)
        {
            var hwnd = new WindowInteropHelper(win).Handle;
            NativeMethods.SetWindowLongPtr(hwnd, Convert.ToInt32(GWL.STYLE), (IntPtr)(NativeMethods.GetWindowLong(hwnd, Convert.ToInt32(GWL.STYLE)) & ~Convert.ToInt32(WS.SYSMENU)));
        }
        #endregion
    }
}

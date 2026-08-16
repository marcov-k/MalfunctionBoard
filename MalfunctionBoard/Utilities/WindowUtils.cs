using System.Runtime.InteropServices;

namespace MalfunctionBoard.Utilities
{
    public static class WindowUtils
    {
        public static void MakeModalWindow(Window subWindow, Window mainWindow)
        {
            #if WINDOWS
            var main = mainWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var sub = subWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;

            if (main != null && sub != null)
            {
                IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(main);
                IntPtr subHwnd = WinRT.Interop.WindowNative.GetWindowHandle(sub);

                const int GWL_HWNDPARENT = -8;
                if (IntPtr.Size == 8)
                {
                    SetWindowLongPtr64(subHwnd, GWL_HWNDPARENT, mainHwnd);
                }
                else
                {
                    SetWindowLong32(subHwnd, GWL_HWNDPARENT, mainHwnd.ToInt32());
                }

                var subAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(subHwnd));
                var presenter = Microsoft.UI.Windowing.OverlappedPresenter.CreateForDialog();
                presenter.IsModal = true;
                subAppWindow.SetPresenter(presenter);

                sub.Closed += (_, _) => SetForegroundWindow(mainHwnd);
            }
            #endif
        }

        #if WINDOWS
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endif
    }
}

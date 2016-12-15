﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FastTab {

    public class WindowFinder {

        public IList<IntPtr> getOpenWindows () {
            IList<IntPtr> windows = new List<IntPtr>();
            EnumWindows((hWnd, lParam) => {
                    if( IsWindowVisible(hWnd) && (GetWindowTextLength(hWnd) > 0) ) {
                        windows.Add(hWnd);
                    }
                    return true;
                },
                0);
            windows.Remove(GetShellWindow());
            return windows;
        }

        public string getWindowText (IntPtr hWnd) {
            int length = GetWindowTextLength(hWnd);
            StringBuilder builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);
            return builder.ToString();
        }

        [DllImport ("user32.dll")]
        private static extern bool EnumWindows (EnumWindowsProc enumFunc, int lParam);

        [DllImport ("user32.dll")]
        private static extern int GetWindowText (IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport ("user32.dll")]
        private static extern int GetWindowTextLength (IntPtr hWnd);

        [DllImport ("user32.dll")]
        private static extern bool IsWindowVisible (IntPtr hWnd);

        [DllImport ("user32.dll")]
        private static extern IntPtr GetShellWindow ();

        private delegate bool EnumWindowsProc (IntPtr hWnd, int lParam);

    }

}

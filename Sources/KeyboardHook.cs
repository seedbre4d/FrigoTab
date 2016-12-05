﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace FastTab {

    public class KeyboardHook : IDisposable {

        public struct LPARAM {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public delegate bool KeyCallback(int wParam, LPARAM lParam);

        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, ref LPARAM lParam);

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private KeyCallback callback;
        private KeyboardProc hookProc;
        private IntPtr hookId;

        public KeyboardHook(KeyCallback callback) {
            this.callback = callback;
            hookProc = new KeyboardProc(HookProc);
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                hookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, ref LPARAM lParam) {
            bool callNext = true;
            if (nCode >= 0) {
                int w = (int)wParam;
                if (w == WM_KEYDOWN || w == WM_KEYUP || w == WM_SYSKEYDOWN || w == WM_SYSKEYUP) {
                    callNext &= callback((int)wParam, lParam);
                }
            }
            return callNext ? CallNextHookEx(hookId, nCode, wParam, ref lParam) : (IntPtr)1;
        }

        public void Dispose() {
            UnhookWindowsHookEx(hookId);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref LPARAM lParam);

    }

}

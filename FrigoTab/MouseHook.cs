﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace FrigoTab {

    public class MouseHookEventArgs {

        public readonly Point Point;
        public readonly bool Click;

        public MouseHookEventArgs (Point point, bool click) {
            Point = point;
            Click = click;
        }

    }

    public class MouseHook : IDisposable {

        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly LowLevelMouseProc _hookProc;

        private readonly IntPtr _hookId;
        private bool _disposed;

        public event Action<MouseHookEventArgs> MouseEvent;

        public MouseHook () {
            _hookProc = HookProc;
            using( Process curProcess = Process.GetCurrentProcess() ) {
                using( ProcessModule curModule = curProcess.MainModule ) {
                    _hookId = SetWindowsHookEx(14, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        ~MouseHook () {
            Dispose();
        }

        public void Dispose () {
            if( _disposed ) {
                return;
            }
            UnhookWindowsHookEx(_hookId);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private IntPtr HookProc (int nCode, IntPtr wParam, ref LowLevelMouseStruct lParam) {
            HookProcInner(nCode, (WindowMessages) wParam, ref lParam);
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);
        }

        private void HookProcInner (int nCode, WindowMessages wParam, ref LowLevelMouseStruct lParam) {
            if( nCode < 0 ) {
                return;
            }
            Point point = lParam.Point;
            WindowMessages[] clickMessages = {
                WindowMessages.LeftDown,
                WindowMessages.LeftUp,
                WindowMessages.RightDown,
                WindowMessages.RightUp
            };
            bool click = clickMessages.Contains(wParam);
            MouseHookEventArgs e = new MouseHookEventArgs(point, click);
            MouseEvent?.Invoke(e);
        }

        private struct LowLevelMouseStruct {

            public Point Point;
            public int MouseData;
            public int Flags;
            public int Time;
            public IntPtr DwExtraInfo;

        }

        private delegate IntPtr LowLevelMouseProc (int nCode, IntPtr wParam, ref LowLevelMouseStruct lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle (string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx (int idHook, LowLevelMouseProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx (IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx (IntPtr hhk,
            int nCode,
            IntPtr wParam,
            ref LowLevelMouseStruct lParam);

    }

}
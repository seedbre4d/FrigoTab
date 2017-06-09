﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FrigoTab {

    public class KeyHookEventArgs {

        public readonly Keys Key;
        public bool Handled;

        public KeyHookEventArgs (Keys key) {
            Key = key;
        }

    }

    public class KeyHook : IDisposable {

        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly LowLevelKeyProc _hookProc;

        private readonly IntPtr _hookId;
        private bool _disposed;

        public event Action<KeyHookEventArgs> KeyEvent;

        public KeyHook () {
            _hookProc = HookProc;
            using( Process curProcess = Process.GetCurrentProcess() ) {
                using( ProcessModule curModule = curProcess.MainModule ) {
                    _hookId = SetWindowsHookEx(13, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        ~KeyHook () {
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

        private IntPtr HookProc (int nCode, IntPtr wParam, ref LowLevelKeyStruct lParam) {
            if( HookProcInner(nCode, (Wm) wParam, ref lParam) ) {
                return (IntPtr) 1;
            }
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);
        }

        private bool HookProcInner (int nCode, Wm wParam, ref LowLevelKeyStruct lParam) {
            if( nCode < 0 ) {
                return false;
            }
            if( !Enum.IsDefined(typeof(Wm), wParam) ) {
                return false;
            }
            if( lParam.Flags.HasFlag(LowLevelKeyFlags.Injected) ) {
                return false;
            }

            bool alt = lParam.Flags.HasFlag(LowLevelKeyFlags.AltDown);
            Keys key = alt ? lParam.VkCode | Keys.Alt : lParam.VkCode;
            KeyHookEventArgs e = new KeyHookEventArgs(key);
            KeyEvent?.Invoke(e);
            return e.Handled;
        }

        private struct LowLevelKeyStruct {

            public Keys VkCode;
            public int ScanCode;
            public LowLevelKeyFlags Flags;
            public int Time;
            public int DwExtraInfo;

        }

        [Flags]
        private enum LowLevelKeyFlags {

            Injected = 16,
            AltDown = 32

        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum Wm {

            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105

        }

        private delegate IntPtr LowLevelKeyProc (int nCode, IntPtr wParam, ref LowLevelKeyStruct lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle (string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx (int idHook, LowLevelKeyProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx (IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx (IntPtr hhk,
            int nCode,
            IntPtr wParam,
            ref LowLevelKeyStruct lParam);

    }

}

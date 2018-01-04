﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FrigoTab {

    public class BackgroundWindows : IDisposable {

        private readonly IList<BackgroundWindow> backgrounds = new List<BackgroundWindow>();

        public BackgroundWindows (Form owner, WindowFinder finder) {
            foreach( WindowHandle window in finder.ToolWindows.Reverse() ) {
                backgrounds.Add(new BackgroundWindow(owner, window));
            }
        }

        ~BackgroundWindows () {
            Dispose();
        }

        public void Dispose () {
            foreach( BackgroundWindow window in backgrounds ) {
                window.Dispose();
            }
            backgrounds.Clear();
        }

    }

}

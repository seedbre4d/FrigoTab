﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace FrigoTab {

    public class ApplicationWindow : FrigoForm, IDisposable {

        private readonly WindowHandle _application;
        private readonly int _index;
        private readonly Thumbnail _thumbnail;
        private Icon _appIcon;
        private bool _selected;

        public ApplicationWindow (Session session, WindowHandle application, int index) {
            Owner = session;
            ExStyle |= WindowExStyles.Transparent | WindowExStyles.Layered;
            _application = application;
            _index = index;
            _thumbnail = new Thumbnail(application, session.Handle);
            _appIcon = _application.IconFromGetClassLongPtr() ?? Program.Icon;
            IconManager.RegisterIconCallback(_application, SetIcon);
        }

        public new Rectangle Bounds {
            get { return base.Bounds; }
            set {
                base.Bounds = value;
                _thumbnail.Update(new ScreenRect(value));
                RenderOverlay();
            }
        }

        public bool Selected {
            private get { return _selected; }
            set {
                if( _selected == value ) {
                    return;
                }
                _selected = value;
                RenderOverlay();
            }
        }

        public new void Dispose () {
            Close();
            _thumbnail.Dispose();
        }

        public Screen GetScreen () {
            return Screen.FromHandle(_application);
        }

        public void SetForeground () {
            _application.SetForeground();
        }

        public Size GetSourceSize () {
            return _thumbnail.GetSourceSize();
        }

        private void SetIcon (Icon icon) {
            _appIcon = icon;
            RenderOverlay();
        }

        private void RenderOverlay () {
            LayerUpdater.Update(this, RenderOverlay);
        }

        private void RenderOverlay (Graphics graphics) {
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RenderFrame(graphics);
            RenderTitle(graphics);
            RenderNumber(graphics);
        }

        private void RenderFrame (Graphics graphics) {
            if( Selected ) {
                FillRectangle(graphics, graphics.VisibleClipBounds, Color.FromArgb(128, 0, 0, 255));
            }
        }

        private void RenderTitle (Graphics graphics) {
            const int pad = 8;

            Icon icon = _appIcon;
            string text = _application.GetWindowText();

            Font font = new Font("Segoe UI", 11f);
            SizeF textSize = graphics.MeasureString(text, font);

            float width = pad + icon.Width + pad + textSize.Width + pad;
            float height = pad + Math.Max(icon.Height, textSize.Height) + pad;

            RectangleF background = new RectangleF(graphics.VisibleClipBounds.Location, new SizeF(width, height));
            FillRectangle(graphics, background, Color.Black);

            {
                float x = background.X + pad;
                float y = Center(icon.Size, background).Y;
                graphics.DrawIcon(icon, (int) x, (int) y);
            }

            using( Brush brush = new SolidBrush(Color.White) ) {
                float x = background.X + pad + icon.Width + pad;
                float y = Center(textSize, background).Y;
                graphics.DrawString(text, font, brush, x, y);
            }
        }

        private void RenderNumber (Graphics graphics) {
            string text = (_index + 1).ToString();

            Font font = new Font("Segoe UI", 72f, FontStyle.Bold);
            SizeF textSize = graphics.MeasureString(text, font);

            RectangleF background = Center(textSize, graphics.VisibleClipBounds);
            FillRectangle(graphics, background, Color.Black);

            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using( Brush brush = new SolidBrush(Color.White) ) {
                graphics.DrawString(text, font, brush, background);
            }
        }

        private static void FillRectangle (Graphics graphics, RectangleF bounds, Color color) {
            PointF[] points = new PointF[5];
            points[0] = new PointF(bounds.Left, bounds.Top);
            points[1] = new PointF(bounds.Left, bounds.Top);
            points[2] = new PointF(bounds.Right, bounds.Top);
            points[3] = new PointF(bounds.Right, bounds.Bottom);
            points[4] = new PointF(bounds.Left, bounds.Bottom);
            using( Brush brush = new SolidBrush(color) ) {
                graphics.FillPolygon(brush, points);
            }
        }

        private static RectangleF Center (SizeF rect, RectangleF bounds) {
            SizeF margins = bounds.Size - rect;
            PointF location = new PointF(bounds.X + margins.Width / 2, bounds.Y + margins.Height / 2);
            return new RectangleF(location, rect);
        }

    }

}

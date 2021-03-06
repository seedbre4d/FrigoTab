﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace FrigoTab {

    public class ApplicationWindow : FrigoForm {

        public readonly WindowHandle Application;
        public Property<bool> Selected;
        private readonly int index;
        private readonly Thumbnail thumbnail;
        private readonly WindowIcon windowIcon;
        private readonly LayerUpdater layerUpdater;

        public ApplicationWindow (FrigoForm owner, WindowHandle application, int index, Rectangle bounds) {
            Bounds = bounds;
            Owner = owner;
            ExStyle |= WindowExStyles.Transparent | WindowExStyles.Layered;
            Application = application;
            Selected.Changed += (x, y) => RenderOverlay();
            this.index = index;
            thumbnail = new Thumbnail(application, owner.WindowHandle);
            thumbnail.SetDestinationRect(new Rect(Bounds).ScreenToClient(owner.WindowHandle));
            layerUpdater = new LayerUpdater(this);
            windowIcon = new WindowIcon(application);
            windowIcon.Changed += RenderOverlay;
            RenderOverlay();
        }

        protected override void Dispose (bool disposing) {
            layerUpdater.Dispose();
            thumbnail.Dispose();
            base.Dispose(disposing);
        }

        private void RenderOverlay () => layerUpdater.Update(RenderOverlay);

        private void RenderOverlay (Graphics graphics) {
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RenderFrame(graphics);
            RenderTitle(graphics);
            RenderNumber(graphics);
        }

        private void RenderFrame (Graphics graphics) {
            if( Selected.Value ) {
                FillRectangle(graphics, graphics.VisibleClipBounds, Color.FromArgb(128, 0, 0, 255));
            }
        }

        private void RenderTitle (Graphics graphics) {
            const int Pad = 8;

            Icon icon = windowIcon.Icon;
            string text = Application.GetWindowText();

            Font font = new Font("Segoe UI", 11f);
            SizeF textSize = graphics.MeasureString(text, font);

            float width = Pad + icon.Width + Pad + textSize.Width + Pad;
            float height = Pad + Math.Max(icon.Height, textSize.Height) + Pad;

            RectangleF background = new RectangleF(graphics.VisibleClipBounds.Location, new SizeF(width, height));
            FillRectangle(graphics, background, Color.Black);

            {
                float x = background.X + Pad;
                float y = Center(icon.Size, background).Y;
                graphics.DrawIcon(icon, (int) x, (int) y);
            }

            using( Brush brush = new SolidBrush(Color.White) ) {
                float x = background.X + Pad + icon.Width + Pad;
                float y = Center(textSize, background).Y;
                graphics.DrawString(text, font, brush, x, y);
            }
        }

        private void RenderNumber (Graphics graphics) {
            string text = (index + 1).ToString();

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

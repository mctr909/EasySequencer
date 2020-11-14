using System;
using System.Drawing;
using System.Windows.Forms;

namespace Player {
    public class DoubleBuffer : IDisposable {
        private Image mBackGround;
        private Rectangle mBackGroundRect;
        private BufferedGraphics mBuffer;

        public DoubleBuffer(Control control) {
            Dispose();
            var currentContext = BufferedGraphicsManager.Current;
            mBuffer = currentContext.Allocate(control.CreateGraphics(), control.DisplayRectangle);
        }

        public DoubleBuffer(Control control, Image backGround) {
            Dispose();
            var currentContext = BufferedGraphicsManager.Current;
            mBackGround = backGround;
            mBackGroundRect = new Rectangle(0, 0, mBackGround.Width, mBackGround.Height);
            mBuffer = currentContext.Allocate(control.CreateGraphics(), control.DisplayRectangle);
        }

        ~DoubleBuffer() {
            Dispose();
        }

        public void Dispose() {
            if (null != mBuffer) {
                mBuffer.Dispose();
                mBuffer = null;
            }
        }

        public void Render() {
            if (null != mBuffer) {
                mBuffer.Render();
            }
        }

        public Graphics Graphics {
            get {
                mBuffer.Graphics.Clear(Color.Transparent);
                if (null != mBackGround) {
                    mBuffer.Graphics.DrawImage(mBackGround, mBackGroundRect);
                }
                return mBuffer.Graphics;
            }
        }
    }
}

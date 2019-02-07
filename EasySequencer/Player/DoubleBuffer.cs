using System;
using System.Drawing;
using System.Windows.Forms;

namespace Player {
    public class DoubleBuffer : IDisposable {
        private BufferedGraphics mBuffer;
        private Image mBackGround;

        public DoubleBuffer(Control control, Image backGround) {
            Dispose();

            var currentContext = BufferedGraphicsManager.Current;
            mBackGround = backGround;
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
                    mBuffer.Graphics.DrawImage(mBackGround, 0, 0);
                }
                return mBuffer.Graphics;
            }
        }
    }
}

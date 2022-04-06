using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasySequencer {
    public partial class StatusWindow : Form {
        private int mMaxPos;
        private IntPtr mpPos;

        public StatusWindow(int maxPos, IntPtr timePtr) {
            InitializeComponent();
            mMaxPos = maxPos;
            mpPos = timePtr;
        }

        private void StatusWindow_Load(object sender, EventArgs e) {
            progressBar1.Maximum = mMaxPos;
            timer1.Interval = 100;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            var pos = Marshal.PtrToStructure<int>(mpPos);
            if (pos < mMaxPos) {
                progressBar1.Value = pos;
            }
            Text = string.Format("wavファイル出力中({0}%)", (100.0 * pos / mMaxPos).ToString("0.0"));
            if (1.0 <= (double)pos / mMaxPos) {
                timer1.Stop();
                Close();
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasySequencer {
    public partial class StatusWindow : Form {
        private int mMaxTime;
        private IntPtr mpTime;

        public StatusWindow(int maxTime, IntPtr timePtr) {
            InitializeComponent();
            mMaxTime = maxTime;
            mpTime = timePtr;
        }

        private void StatusWindow_Load(object sender, EventArgs e) {
            progressBar1.Maximum = mMaxTime;
            timer1.Interval = 100;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            var time = Marshal.PtrToStructure<int>(mpTime);
            progressBar1.Value = time;
            Text = string.Format("wavファイル出力中({0}%)", (100.0 * time / mMaxTime).ToString("0.0"));
            if (1.0 <= (double)time / mMaxTime) {
                timer1.Stop();
                Close();
            }
        }
    }
}

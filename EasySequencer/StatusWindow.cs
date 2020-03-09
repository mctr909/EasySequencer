using System.Windows.Forms;

namespace EasySequencer {
    unsafe public partial class StatusWindow : Form {
        private int mMaxTime;
        private int* mTime;

        public StatusWindow(int maxTime, int* timePtr) {
            InitializeComponent();
            mMaxTime = maxTime;
            mTime = timePtr;
        }

        private void StatusWindow_Load(object sender, System.EventArgs e) {
            progressBar1.Maximum = mMaxTime;
            timer1.Interval = 100;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void timer1_Tick(object sender, System.EventArgs e) {
            progressBar1.Value = *mTime;
            Text = string.Format("wavファイル出力中({0}%)", (100.0 * *mTime / mMaxTime).ToString("0.0"));
            if (1.0 <= *mTime / mMaxTime) {
                timer1.Stop();
                Close();
            }
        }
    }
}

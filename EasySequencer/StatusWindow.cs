using System.Windows.Forms;
using System.Threading.Tasks;

namespace EasySequencer {
    unsafe public partial class StatusWindow : Form {
        public int TimeMax;
        public int* Time;

        public StatusWindow() {
            InitializeComponent();
        }

        private void StatusWindow_Load(System.Object sender, System.EventArgs e) {
            timer1.Interval = 100;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void timer1_Tick(System.Object sender, System.EventArgs e) {
            progressBar1.Maximum = TimeMax;
            progressBar1.Value = *Time;

            Text = string.Format("wavファイル出力中({0}%)", (100 * *Time / TimeMax).ToString());

            if (1.0 <= *Time / TimeMax) {
                timer1.Stop();
                Close();
            }
        }
    }
}

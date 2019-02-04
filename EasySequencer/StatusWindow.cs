using System.Windows.Forms;
using System.Threading.Tasks;

namespace EasySequencer {
	unsafe public partial class StatusWindow : Form {
		public int ProgressMax;
		public int* Progress;
		public double* Time;
		public Task Task;

		public StatusWindow() {
			InitializeComponent();
		}

		private void StatusWindow_Load(System.Object sender, System.EventArgs e) {
			timer1.Interval = 100;
			timer1.Enabled = true;
			timer1.Start();
		}

		private void timer1_Tick(System.Object sender, System.EventArgs e) {
			progressBar1.Maximum = ProgressMax;
			progressBar1.Value = *Progress;

			var timeCurMin = ((int)*Time) / 60;
			var timeCurSec = ((int)*Time) % 60;
			Text = string.Format("wavファイル出力中({0}:{1})", timeCurMin.ToString("00"), timeCurSec.ToString("00"));

			if (null != Task && Task.IsCompleted) {
				timer1.Stop();
				Close();
			}
		}
	}
}

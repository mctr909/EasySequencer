using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Player;

namespace EasySequencer {
    public partial class InstList : Form {
        private Sender mSender;
        private int mChNum;
        private Dictionary<string , Dictionary<INST_ID, string>> mInstList;

        public InstList(Sender sender, int chNum) {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            mInstList = new Dictionary<string, Dictionary<INST_ID, string>>();
            mSender = sender;
            mChNum = chNum;

            var selectedCategory = "";
            var selectedInst = 0;
            for (int i = 0; i < mSender.InstCount; i++) {
                var inst = mSender.Instruments(i);
                var nam = string.Format("{0} {1}", inst.id.progNum, inst.Name);
                var cat = inst.Category;
                if (!mInstList.ContainsKey(cat)) {
                    mInstList.Add(cat, new Dictionary<INST_ID, string>());
                    cmbCategory.Items.Add(cat);
                }
                if (!mInstList[cat].ContainsKey(inst.id)) {
                    mInstList[cat].Add(inst.id, nam);
                }
                var chParam = mSender.Channel(mChNum);
                if (chParam.InstId.isDrum == inst.id.isDrum &&
                    chParam.InstId.progNum == inst.id.progNum &&
                    chParam.InstId.bankMSB == inst.id.bankMSB &&
                    chParam.InstId.bankLSB == inst.id.bankLSB
                ) {
                    selectedCategory = cat;
                    selectedInst = mInstList[cat].Count - 1;
                }
            }
            if (mInstList.ContainsKey(selectedCategory)) {
                cmbCategory.SelectedItem = selectedCategory;
                lstInst.Items.Clear();
                foreach (var inst in mInstList[selectedCategory]) {
                    lstInst.Items.Add(inst.Value);
                }
                lstInst.SelectedIndex = selectedInst;
            }
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e) {
            if(!mInstList.ContainsKey((string)cmbCategory.SelectedItem)) {
                return;
            }

            lstInst.Items.Clear();
            foreach (var inst in mInstList[(string)cmbCategory.SelectedItem]) {
                lstInst.Items.Add(inst.Value);
            }
        }

        private void lstInst_SelectedIndexChanged(object sender, EventArgs e) {
            if (null == cmbCategory.SelectedItem || !mInstList.ContainsKey((string)cmbCategory.SelectedItem)) {
                return;
            }
            var list = mInstList[(string)cmbCategory.SelectedItem].ToArray();
            var inst = list[(lstInst.SelectedIndex < list.Count()) ? lstInst.SelectedIndex : list.Count() - 1];
            mSender.DrumChannel(mChNum, inst.Key.isDrum != 0);
            mSender.Send(new Event(mChNum, E_CONTROL.BANK_MSB, inst.Key.bankMSB));
            mSender.Send(new Event(mChNum, E_CONTROL.BANK_LSB, inst.Key.bankLSB));
            mSender.Send(new Event(mChNum, E_STATUS.PROGRAM, inst.Key.progNum));
        }

        private void btnCommit_Click(object sender, EventArgs e) {
            Close();
        }
    }
}

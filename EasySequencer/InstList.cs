using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using MIDI;
using WaveOut;

namespace EasySequencer {
    public partial class InstList : Form {
        private Channel mChannel;
        private Dictionary<string , Dictionary<INST_ID, string>> mInstList;

        public InstList(Channel channel) {
            InitializeComponent();
            mInstList = new Dictionary<string, Dictionary<INST_ID, string>>();
            mChannel = channel;

            var selectedCategory = "";
            var selectedInst = 0;

            foreach (var inst in mChannel.InstList) {
                var cat = inst.Value.catgory;
                var nam = inst.Value.name;
                if (!mInstList.ContainsKey(cat)) {
                    mInstList.Add(cat, new Dictionary<INST_ID, string>());
                    cmbCategory.Items.Add(cat);
                }

                mInstList[cat].Add(inst.Key, nam);
                if (mChannel.InstId.isDrum == inst.Key.isDrum
                    && mChannel.InstId.programNo == inst.Key.programNo
                    && mChannel.InstId.bankMSB == inst.Key.bankMSB
                    && mChannel.InstId.bankLSB == inst.Key.bankLSB) {
                    selectedCategory = cat;
                    selectedInst = mInstList[cat].Count - 1;
                }
            }

            cmbCategory.SelectedItem = selectedCategory;
            lstInst.Items.Clear();
            foreach (var inst in mInstList[selectedCategory]) {
                lstInst.Items.Add(inst.Value);
            }
            lstInst.SelectedIndex = selectedInst;
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

            mChannel.CtrlChange(CTRL_TYPE.BANK_MSB, inst.Key.bankMSB);
            mChannel.CtrlChange(CTRL_TYPE.BANK_LSB, inst.Key.bankLSB);
            mChannel.ProgramChange(inst.Key.programNo, inst.Key.isDrum == 0x80);
        }

        private void btnCommit_Click(object sender, EventArgs e) {
            Close();
        }
    }
}

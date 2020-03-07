using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using MIDI;
using Player;

namespace EasySequencer {
    unsafe public partial class InstList : Form {
        private Sender mSender;
        private int mChNum;
        private Dictionary<string , Dictionary<INST_ID, string>> mInstList;

        public InstList(Sender sender, int chNum) {
            InitializeComponent();
            mInstList = new Dictionary<string, Dictionary<INST_ID, string>>();
            mSender = sender;
            mChNum = chNum;

            var selectedCategory = "";
            var selectedInst = 0;

            for (int i = 0; i < mSender.InstList->instCount; i++) {
                var pInst = mSender.InstList->ppInst[i];
                var nam = Marshal.PtrToStringAuto((IntPtr)pInst->pName);
                var cat = Marshal.PtrToStringAuto((IntPtr)pInst->pCategory);
                if (!mInstList.ContainsKey(cat)) {
                    mInstList.Add(cat, new Dictionary<INST_ID, string>());
                    cmbCategory.Items.Add(cat);
                }
                mInstList[cat].Add(pInst->id, nam);
                if (mSender.Channel[mChNum]->InstId.isDrum == pInst->id.isDrum &&
                    mSender.Channel[mChNum]->InstId.programNo == pInst->id.programNo &&
                    mSender.Channel[mChNum]->InstId.bankMSB == pInst->id.bankMSB &&
                    mSender.Channel[mChNum]->InstId.bankLSB == pInst->id.bankLSB
                ) {
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

            mSender.Channel[mChNum]->InstId.isDrum = inst.Key.isDrum;
            mSender.Send(new Event(E_CTRL_TYPE.BANK_MSB, (byte)mChNum, inst.Key.bankMSB));
            mSender.Send(new Event(E_CTRL_TYPE.BANK_LSB, (byte)mChNum, inst.Key.bankLSB));
            mSender.Send(new Event(E_EVENT_TYPE.PROG_CHG, (byte)mChNum, inst.Key.programNo));
        }

        private void btnCommit_Click(object sender, EventArgs e) {
            Close();
        }
    }
}

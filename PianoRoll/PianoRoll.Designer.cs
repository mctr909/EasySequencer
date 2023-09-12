using System.Windows.Forms;

namespace PianoRoll {
    partial class PianoRoll {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PianoRoll));
            this.pnlRoll = new System.Windows.Forms.Panel();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.picRoll = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsdDisp = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmScrollNext = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmScrollPrev = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmScrollDown = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmScrollUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmTimeZoomout = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTimeZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmToneZoomout = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmToneZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbWrite = new System.Windows.Forms.ToolStripButton();
            this.tsbSelect = new System.Windows.Forms.ToolStripButton();
            this.tsbMultiSelect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsdEditMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmEditModeNote = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeAccent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeExp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModePitch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmEditModeInst = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModePreset = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmEditModeAttack = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeRelease = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmEditModeFc = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeFq = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeVol = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModePan = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmEditModeVib = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeVibDep = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeVibRate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeVibDelay = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeDel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeDelDep = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeDelTime = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeRev = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeCho = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmEditModeTempo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmEditModeMeasure = new System.Windows.Forms.ToolStripMenuItem();
            this.tsdTimeDiv = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmTick480 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick240 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick120 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick060 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmTick320 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick160 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick080 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick040 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmTick192 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick096 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick048 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmTick024 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.tslKey = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.tslDegree = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.tslChord = new System.Windows.Forms.ToolStripLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pnlRoll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRoll)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRoll
            // 
            this.pnlRoll.BackColor = System.Drawing.Color.White;
            this.pnlRoll.Controls.Add(this.vScroll);
            this.pnlRoll.Controls.Add(this.hScroll);
            this.pnlRoll.Controls.Add(this.picRoll);
            this.pnlRoll.Location = new System.Drawing.Point(7, 20);
            this.pnlRoll.Name = "pnlRoll";
            this.pnlRoll.Size = new System.Drawing.Size(258, 190);
            this.pnlRoll.TabIndex = 1;
            // 
            // vScroll
            // 
            this.vScroll.Location = new System.Drawing.Point(241, 3);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(17, 167);
            this.vScroll.TabIndex = 2;
            // 
            // hScroll
            // 
            this.hScroll.Location = new System.Drawing.Point(3, 173);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(234, 17);
            this.hScroll.TabIndex = 1;
            // 
            // picRoll
            // 
            this.picRoll.Location = new System.Drawing.Point(3, 3);
            this.picRoll.Name = "picRoll";
            this.picRoll.Size = new System.Drawing.Size(234, 167);
            this.picRoll.TabIndex = 0;
            this.picRoll.TabStop = false;
            this.picRoll.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picRoll_MouseDown);
            this.picRoll.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picRoll_MouseMove);
            this.picRoll.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picRoll_MouseUp);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsdDisp,
            this.toolStripSeparator2,
            this.tsbWrite,
            this.tsbSelect,
            this.tsbMultiSelect,
            this.toolStripSeparator3,
            this.tsdEditMode,
            this.tsdTimeDiv,
            this.toolStripSeparator11,
            this.tslKey,
            this.toolStripSeparator12,
            this.tslDegree,
            this.toolStripSeparator15,
            this.tslChord});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(886, 31);
            this.toolStrip1.TabIndex = 2;
            // 
            // tsdDisp
            // 
            this.tsdDisp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsdDisp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmScrollNext,
            this.tsmScrollPrev,
            this.toolStripSeparator14,
            this.tsmScrollDown,
            this.tsmScrollUp,
            this.toolStripSeparator7,
            this.tsmTimeZoomout,
            this.tsmTimeZoom,
            this.toolStripSeparator10,
            this.tsmToneZoomout,
            this.tsmToneZoom});
            this.tsdDisp.Image = global::PianoRoll.Properties.Resources.disp;
            this.tsdDisp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsdDisp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsdDisp.Name = "tsdDisp";
            this.tsdDisp.Size = new System.Drawing.Size(37, 28);
            this.tsdDisp.Text = "表示";
            // 
            // tsmScrollNext
            // 
            this.tsmScrollNext.Image = global::PianoRoll.Properties.Resources.scroll_next;
            this.tsmScrollNext.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmScrollNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmScrollNext.Name = "tsmScrollNext";
            this.tsmScrollNext.ShortcutKeyDisplayString = "Ctrl + マウスホイール↓";
            this.tsmScrollNext.Size = new System.Drawing.Size(277, 30);
            this.tsmScrollNext.Text = "次の小節へ移動";
            this.tsmScrollNext.Click += new System.EventHandler(this.tsmScrollNext_Click);
            // 
            // tsmScrollPrev
            // 
            this.tsmScrollPrev.Image = global::PianoRoll.Properties.Resources.scroll_prev;
            this.tsmScrollPrev.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmScrollPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmScrollPrev.Name = "tsmScrollPrev";
            this.tsmScrollPrev.ShortcutKeyDisplayString = "Ctrl + マウスホイール↑";
            this.tsmScrollPrev.Size = new System.Drawing.Size(277, 30);
            this.tsmScrollPrev.Text = "前の小節へ移動";
            this.tsmScrollPrev.Click += new System.EventHandler(this.tsmScrollPrev_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(274, 6);
            // 
            // tsmScrollDown
            // 
            this.tsmScrollDown.Image = global::PianoRoll.Properties.Resources.scroll_down;
            this.tsmScrollDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmScrollDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmScrollDown.Name = "tsmScrollDown";
            this.tsmScrollDown.ShortcutKeyDisplayString = "マウスホイール↓";
            this.tsmScrollDown.Size = new System.Drawing.Size(277, 30);
            this.tsmScrollDown.Text = "下にスクロール";
            this.tsmScrollDown.Click += new System.EventHandler(this.tsmScrollDown_Click);
            // 
            // tsmScrollUp
            // 
            this.tsmScrollUp.Image = global::PianoRoll.Properties.Resources.scroll_up;
            this.tsmScrollUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmScrollUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmScrollUp.Name = "tsmScrollUp";
            this.tsmScrollUp.ShortcutKeyDisplayString = "マウスホイール↑";
            this.tsmScrollUp.Size = new System.Drawing.Size(277, 30);
            this.tsmScrollUp.Text = "上にスクロール";
            this.tsmScrollUp.Click += new System.EventHandler(this.tsmScrollUp_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(274, 6);
            // 
            // tsmTimeZoomout
            // 
            this.tsmTimeZoomout.Image = global::PianoRoll.Properties.Resources.time_zoomout;
            this.tsmTimeZoomout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTimeZoomout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmTimeZoomout.Name = "tsmTimeZoomout";
            this.tsmTimeZoomout.ShortcutKeyDisplayString = "Shift + マウスホイール↓";
            this.tsmTimeZoomout.Size = new System.Drawing.Size(277, 30);
            this.tsmTimeZoomout.Text = "時間方向縮小";
            this.tsmTimeZoomout.Click += new System.EventHandler(this.tsmTimeZoomout_Click);
            // 
            // tsmTimeZoom
            // 
            this.tsmTimeZoom.Image = global::PianoRoll.Properties.Resources.time_zoom;
            this.tsmTimeZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTimeZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmTimeZoom.Name = "tsmTimeZoom";
            this.tsmTimeZoom.ShortcutKeyDisplayString = "Shift + マウスホイール↑";
            this.tsmTimeZoom.Size = new System.Drawing.Size(277, 30);
            this.tsmTimeZoom.Text = "時間方向拡大";
            this.tsmTimeZoom.Click += new System.EventHandler(this.tsmTimeZoom_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(274, 6);
            // 
            // tsmToneZoomout
            // 
            this.tsmToneZoomout.Image = global::PianoRoll.Properties.Resources.tone_zoomout;
            this.tsmToneZoomout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmToneZoomout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmToneZoomout.Name = "tsmToneZoomout";
            this.tsmToneZoomout.ShortcutKeyDisplayString = "Alt + マウスホイール↓";
            this.tsmToneZoomout.Size = new System.Drawing.Size(277, 30);
            this.tsmToneZoomout.Text = "音程方向縮小";
            this.tsmToneZoomout.Click += new System.EventHandler(this.tsmToneZoomout_Click);
            // 
            // tsmToneZoom
            // 
            this.tsmToneZoom.Image = global::PianoRoll.Properties.Resources.tone_zoom;
            this.tsmToneZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmToneZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsmToneZoom.Name = "tsmToneZoom";
            this.tsmToneZoom.ShortcutKeyDisplayString = "Alt + マウスホイール↑";
            this.tsmToneZoom.Size = new System.Drawing.Size(277, 30);
            this.tsmToneZoom.Text = "音程方向拡大";
            this.tsmToneZoom.Click += new System.EventHandler(this.tsmToneZoom_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbWrite
            // 
            this.tsbWrite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbWrite.Image = global::PianoRoll.Properties.Resources.write;
            this.tsbWrite.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbWrite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWrite.Name = "tsbWrite";
            this.tsbWrite.Size = new System.Drawing.Size(28, 28);
            this.tsbWrite.Text = "書き込みモード\r\nF1";
            this.tsbWrite.Click += new System.EventHandler(this.tsbWriteSelect_Click);
            // 
            // tsbSelect
            // 
            this.tsbSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSelect.Image = global::PianoRoll.Properties.Resources.select;
            this.tsbSelect.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSelect.Name = "tsbSelect";
            this.tsbSelect.Size = new System.Drawing.Size(29, 28);
            this.tsbSelect.Text = "1トラック選択モード\r\nF2";
            this.tsbSelect.Click += new System.EventHandler(this.tsbWriteSelect_Click);
            // 
            // tsbMultiSelect
            // 
            this.tsbMultiSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbMultiSelect.Image = ((System.Drawing.Image)(resources.GetObject("tsbMultiSelect.Image")));
            this.tsbMultiSelect.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbMultiSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMultiSelect.Name = "tsbMultiSelect";
            this.tsbMultiSelect.Size = new System.Drawing.Size(29, 28);
            this.tsbMultiSelect.Text = "複数トラック選択モード\r\nF3";
            this.tsbMultiSelect.Click += new System.EventHandler(this.tsbWriteSelect_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // tsdEditMode
            // 
            this.tsdEditMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsdEditMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditModeNote,
            this.tsmEditModeAccent,
            this.tsmEditModeExp,
            this.tsmEditModePitch,
            this.toolStripSeparator8,
            this.tsmEditModeInst,
            this.tsmEditModeVol,
            this.tsmEditModePan,
            this.toolStripSeparator9,
            this.tsmEditModeVib,
            this.tsmEditModeDel,
            this.tsmEditModeRev,
            this.tsmEditModeCho,
            this.toolStripSeparator13,
            this.tsmEditModeTempo,
            this.tsmEditModeMeasure});
            this.tsdEditMode.Image = global::PianoRoll.Properties.Resources.edit_note;
            this.tsdEditMode.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsdEditMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsdEditMode.Name = "tsdEditMode";
            this.tsdEditMode.Size = new System.Drawing.Size(37, 28);
            this.tsdEditMode.Text = "入力種別";
            // 
            // tsmEditModeNote
            // 
            this.tsmEditModeNote.Image = global::PianoRoll.Properties.Resources.edit_note;
            this.tsmEditModeNote.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeNote.Name = "tsmEditModeNote";
            this.tsmEditModeNote.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.tsmEditModeNote.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeNote.Text = "音符";
            this.tsmEditModeNote.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeAccent
            // 
            this.tsmEditModeAccent.Image = ((System.Drawing.Image)(resources.GetObject("tsmEditModeAccent.Image")));
            this.tsmEditModeAccent.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeAccent.Name = "tsmEditModeAccent";
            this.tsmEditModeAccent.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.tsmEditModeAccent.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeAccent.Text = "アクセント";
            this.tsmEditModeAccent.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeExp
            // 
            this.tsmEditModeExp.Image = global::PianoRoll.Properties.Resources.edit_exp;
            this.tsmEditModeExp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeExp.Name = "tsmEditModeExp";
            this.tsmEditModeExp.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.tsmEditModeExp.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeExp.Text = "強弱";
            this.tsmEditModeExp.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModePitch
            // 
            this.tsmEditModePitch.Image = global::PianoRoll.Properties.Resources.edit_pitch;
            this.tsmEditModePitch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModePitch.Name = "tsmEditModePitch";
            this.tsmEditModePitch.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.tsmEditModePitch.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModePitch.Text = "ピッチ";
            this.tsmEditModePitch.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(143, 6);
            // 
            // tsmEditModeInst
            // 
            this.tsmEditModeInst.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditModePreset,
            this.toolStripSeparator1,
            this.tsmEditModeAttack,
            this.tsmEditModeRelease,
            this.toolStripSeparator6,
            this.tsmEditModeFc,
            this.tsmEditModeFq});
            this.tsmEditModeInst.Image = global::PianoRoll.Properties.Resources.edit_inst;
            this.tsmEditModeInst.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeInst.Name = "tsmEditModeInst";
            this.tsmEditModeInst.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeInst.Text = "音色";
            // 
            // tsmEditModePreset
            // 
            this.tsmEditModePreset.Image = global::PianoRoll.Properties.Resources.edit_inst;
            this.tsmEditModePreset.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModePreset.Name = "tsmEditModePreset";
            this.tsmEditModePreset.Size = new System.Drawing.Size(160, 30);
            this.tsmEditModePreset.Text = "音色プリセット";
            this.tsmEditModePreset.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmEditModeAttack
            // 
            this.tsmEditModeAttack.Image = global::PianoRoll.Properties.Resources.edit_attack;
            this.tsmEditModeAttack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeAttack.Name = "tsmEditModeAttack";
            this.tsmEditModeAttack.Size = new System.Drawing.Size(160, 30);
            this.tsmEditModeAttack.Text = "立ち上がり時間";
            this.tsmEditModeAttack.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeRelease
            // 
            this.tsmEditModeRelease.Image = global::PianoRoll.Properties.Resources.edit_release;
            this.tsmEditModeRelease.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeRelease.Name = "tsmEditModeRelease";
            this.tsmEditModeRelease.Size = new System.Drawing.Size(160, 30);
            this.tsmEditModeRelease.Text = "持続時間";
            this.tsmEditModeRelease.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmEditModeFc
            // 
            this.tsmEditModeFc.Image = global::PianoRoll.Properties.Resources.edit_fc;
            this.tsmEditModeFc.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeFc.Name = "tsmEditModeFc";
            this.tsmEditModeFc.Size = new System.Drawing.Size(160, 30);
            this.tsmEditModeFc.Text = "カットオフ周波数";
            this.tsmEditModeFc.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeFq
            // 
            this.tsmEditModeFq.Image = global::PianoRoll.Properties.Resources.edit_fq;
            this.tsmEditModeFq.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeFq.Name = "tsmEditModeFq";
            this.tsmEditModeFq.Size = new System.Drawing.Size(160, 30);
            this.tsmEditModeFq.Text = "レゾナンス";
            this.tsmEditModeFq.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeVol
            // 
            this.tsmEditModeVol.Image = global::PianoRoll.Properties.Resources.edit_vol;
            this.tsmEditModeVol.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeVol.Name = "tsmEditModeVol";
            this.tsmEditModeVol.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeVol.Text = "音量";
            this.tsmEditModeVol.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModePan
            // 
            this.tsmEditModePan.Image = global::PianoRoll.Properties.Resources.edit_pan;
            this.tsmEditModePan.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModePan.Name = "tsmEditModePan";
            this.tsmEditModePan.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModePan.Text = "定位";
            this.tsmEditModePan.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(143, 6);
            // 
            // tsmEditModeVib
            // 
            this.tsmEditModeVib.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditModeVibDep,
            this.tsmEditModeVibRate,
            this.tsmEditModeVibDelay});
            this.tsmEditModeVib.Image = global::PianoRoll.Properties.Resources.edit_vib;
            this.tsmEditModeVib.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeVib.Name = "tsmEditModeVib";
            this.tsmEditModeVib.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeVib.Text = "ビブラート";
            // 
            // tsmEditModeVibDep
            // 
            this.tsmEditModeVibDep.Image = global::PianoRoll.Properties.Resources.edit_vib_dep;
            this.tsmEditModeVibDep.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeVibDep.Name = "tsmEditModeVibDep";
            this.tsmEditModeVibDep.Size = new System.Drawing.Size(106, 30);
            this.tsmEditModeVibDep.Text = "深さ";
            this.tsmEditModeVibDep.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeVibRate
            // 
            this.tsmEditModeVibRate.Image = global::PianoRoll.Properties.Resources.edit_vib_rate;
            this.tsmEditModeVibRate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeVibRate.Name = "tsmEditModeVibRate";
            this.tsmEditModeVibRate.Size = new System.Drawing.Size(106, 30);
            this.tsmEditModeVibRate.Text = "速さ";
            this.tsmEditModeVibRate.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeVibDelay
            // 
            this.tsmEditModeVibDelay.Image = global::PianoRoll.Properties.Resources.edit_vib_delay;
            this.tsmEditModeVibDelay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeVibDelay.Name = "tsmEditModeVibDelay";
            this.tsmEditModeVibDelay.Size = new System.Drawing.Size(106, 30);
            this.tsmEditModeVibDelay.Text = "遅延";
            this.tsmEditModeVibDelay.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeDel
            // 
            this.tsmEditModeDel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditModeDelDep,
            this.tsmEditModeDelTime});
            this.tsmEditModeDel.Image = global::PianoRoll.Properties.Resources.edit_del;
            this.tsmEditModeDel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeDel.Name = "tsmEditModeDel";
            this.tsmEditModeDel.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeDel.Text = "ディレイ";
            // 
            // tsmEditModeDelDep
            // 
            this.tsmEditModeDelDep.Image = global::PianoRoll.Properties.Resources.edit_del;
            this.tsmEditModeDelDep.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeDelDep.Name = "tsmEditModeDelDep";
            this.tsmEditModeDelDep.Size = new System.Drawing.Size(106, 30);
            this.tsmEditModeDelDep.Text = "深さ";
            this.tsmEditModeDelDep.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeDelTime
            // 
            this.tsmEditModeDelTime.Image = global::PianoRoll.Properties.Resources.edit_del_time;
            this.tsmEditModeDelTime.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeDelTime.Name = "tsmEditModeDelTime";
            this.tsmEditModeDelTime.Size = new System.Drawing.Size(106, 30);
            this.tsmEditModeDelTime.Text = "間隔";
            this.tsmEditModeDelTime.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeRev
            // 
            this.tsmEditModeRev.Image = global::PianoRoll.Properties.Resources.edit_rev;
            this.tsmEditModeRev.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeRev.Name = "tsmEditModeRev";
            this.tsmEditModeRev.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeRev.Text = "リバーブ";
            this.tsmEditModeRev.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeCho
            // 
            this.tsmEditModeCho.Image = global::PianoRoll.Properties.Resources.edit_cho;
            this.tsmEditModeCho.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeCho.Name = "tsmEditModeCho";
            this.tsmEditModeCho.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeCho.Text = "コーラス";
            this.tsmEditModeCho.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(143, 6);
            // 
            // tsmEditModeTempo
            // 
            this.tsmEditModeTempo.Image = global::PianoRoll.Properties.Resources.edit_tempo;
            this.tsmEditModeTempo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeTempo.Name = "tsmEditModeTempo";
            this.tsmEditModeTempo.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeTempo.Text = "テンポ";
            this.tsmEditModeTempo.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsmEditModeMeasure
            // 
            this.tsmEditModeMeasure.Image = global::PianoRoll.Properties.Resources.edit_measure;
            this.tsmEditModeMeasure.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmEditModeMeasure.Name = "tsmEditModeMeasure";
            this.tsmEditModeMeasure.Size = new System.Drawing.Size(146, 30);
            this.tsmEditModeMeasure.Text = "拍子";
            this.tsmEditModeMeasure.Click += new System.EventHandler(this.tsmEditMode_Click);
            // 
            // tsdTimeDiv
            // 
            this.tsdTimeDiv.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsdTimeDiv.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmTick480,
            this.tsmTick240,
            this.tsmTick120,
            this.tsmTick060,
            this.toolStripSeparator4,
            this.tsmTick320,
            this.tsmTick160,
            this.tsmTick080,
            this.tsmTick040,
            this.toolStripSeparator5,
            this.tsmTick192,
            this.tsmTick096,
            this.tsmTick048,
            this.tsmTick024});
            this.tsdTimeDiv.Image = global::PianoRoll.Properties.Resources.tick240;
            this.tsdTimeDiv.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsdTimeDiv.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsdTimeDiv.Name = "tsdTimeDiv";
            this.tsdTimeDiv.Size = new System.Drawing.Size(29, 28);
            this.tsdTimeDiv.Text = "入力単位";
            // 
            // tsmTick480
            // 
            this.tsmTick480.Image = global::PianoRoll.Properties.Resources.tick480;
            this.tsmTick480.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick480.Name = "tsmTick480";
            this.tsmTick480.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.tsmTick480.Size = new System.Drawing.Size(167, 30);
            this.tsmTick480.Text = "8分";
            this.tsmTick480.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick240
            // 
            this.tsmTick240.Checked = true;
            this.tsmTick240.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmTick240.Image = global::PianoRoll.Properties.Resources.tick240;
            this.tsmTick240.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick240.Name = "tsmTick240";
            this.tsmTick240.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.tsmTick240.Size = new System.Drawing.Size(167, 30);
            this.tsmTick240.Text = "16分";
            this.tsmTick240.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick120
            // 
            this.tsmTick120.Image = global::PianoRoll.Properties.Resources.tick120;
            this.tsmTick120.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick120.Name = "tsmTick120";
            this.tsmTick120.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.tsmTick120.Size = new System.Drawing.Size(167, 30);
            this.tsmTick120.Text = "32分";
            this.tsmTick120.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick060
            // 
            this.tsmTick060.Image = global::PianoRoll.Properties.Resources.tick060;
            this.tsmTick060.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick060.Name = "tsmTick060";
            this.tsmTick060.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.tsmTick060.Size = new System.Drawing.Size(167, 30);
            this.tsmTick060.Text = "64分";
            this.tsmTick060.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(164, 6);
            // 
            // tsmTick320
            // 
            this.tsmTick320.Image = global::PianoRoll.Properties.Resources.tick320;
            this.tsmTick320.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick320.Name = "tsmTick320";
            this.tsmTick320.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F9)));
            this.tsmTick320.Size = new System.Drawing.Size(167, 30);
            this.tsmTick320.Text = "3連8分";
            this.tsmTick320.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick160
            // 
            this.tsmTick160.Image = global::PianoRoll.Properties.Resources.tick160;
            this.tsmTick160.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick160.Name = "tsmTick160";
            this.tsmTick160.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F10)));
            this.tsmTick160.Size = new System.Drawing.Size(167, 30);
            this.tsmTick160.Text = "3連16分";
            this.tsmTick160.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick080
            // 
            this.tsmTick080.Image = global::PianoRoll.Properties.Resources.tick080;
            this.tsmTick080.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick080.Name = "tsmTick080";
            this.tsmTick080.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F11)));
            this.tsmTick080.Size = new System.Drawing.Size(167, 30);
            this.tsmTick080.Text = "3連32分";
            this.tsmTick080.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick040
            // 
            this.tsmTick040.Image = global::PianoRoll.Properties.Resources.tick040;
            this.tsmTick040.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick040.Name = "tsmTick040";
            this.tsmTick040.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F12)));
            this.tsmTick040.Size = new System.Drawing.Size(167, 30);
            this.tsmTick040.Text = "3連64分";
            this.tsmTick040.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(164, 6);
            // 
            // tsmTick192
            // 
            this.tsmTick192.Image = global::PianoRoll.Properties.Resources.tick192;
            this.tsmTick192.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick192.Name = "tsmTick192";
            this.tsmTick192.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F9)));
            this.tsmTick192.Size = new System.Drawing.Size(167, 30);
            this.tsmTick192.Text = "5連8分";
            this.tsmTick192.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick096
            // 
            this.tsmTick096.Image = global::PianoRoll.Properties.Resources.tick096;
            this.tsmTick096.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick096.Name = "tsmTick096";
            this.tsmTick096.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F10)));
            this.tsmTick096.Size = new System.Drawing.Size(167, 30);
            this.tsmTick096.Text = "5連16分";
            this.tsmTick096.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick048
            // 
            this.tsmTick048.Image = global::PianoRoll.Properties.Resources.tick048;
            this.tsmTick048.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick048.Name = "tsmTick048";
            this.tsmTick048.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F11)));
            this.tsmTick048.Size = new System.Drawing.Size(167, 30);
            this.tsmTick048.Text = "5連32分";
            this.tsmTick048.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // tsmTick024
            // 
            this.tsmTick024.Image = global::PianoRoll.Properties.Resources.tick024;
            this.tsmTick024.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmTick024.Name = "tsmTick024";
            this.tsmTick024.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F12)));
            this.tsmTick024.Size = new System.Drawing.Size(167, 30);
            this.tsmTick024.Text = "5連64分";
            this.tsmTick024.Click += new System.EventHandler(this.tsmTick_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 31);
            // 
            // tslKey
            // 
            this.tslKey.AutoSize = false;
            this.tslKey.Font = new System.Drawing.Font("Meiryo UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tslKey.Name = "tslKey";
            this.tslKey.Size = new System.Drawing.Size(80, 28);
            this.tslKey.Text = "Cb/Abm";
            this.tslKey.ToolTipText = "メジャーキー/マイナーキー";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 31);
            // 
            // tslDegree
            // 
            this.tslDegree.AutoSize = false;
            this.tslDegree.Font = new System.Drawing.Font("メイリオ", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tslDegree.Name = "tslDegree";
            this.tslDegree.Size = new System.Drawing.Size(50, 28);
            this.tslDegree.Text = "♭Ⅶ";
            this.tslDegree.ToolTipText = "ディグリーネーム";
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 31);
            // 
            // tslChord
            // 
            this.tslChord.AutoSize = false;
            this.tslChord.Font = new System.Drawing.Font("Meiryo UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tslChord.Name = "tslChord";
            this.tslChord.Size = new System.Drawing.Size(200, 28);
            this.tslChord.Text = "Ebm";
            this.tslChord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tslChord.ToolTipText = "コードネーム";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PianoRoll
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(886, 464);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pnlRoll);
            this.Name = "PianoRoll";
            this.Text = "Form1";
            this.SizeChanged += new System.EventHandler(this.PianoRoll_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PianoRoll_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PianoRoll_KeyUp);
            this.Leave += new System.EventHandler(this.PianoRoll_Leave);
            this.pnlRoll.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picRoll)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picRoll;
        private System.Windows.Forms.Panel pnlRoll;
        private System.Windows.Forms.VScrollBar vScroll;
        private System.Windows.Forms.HScrollBar hScroll;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbWrite;
        private System.Windows.Forms.ToolStripButton tsbSelect;
        private System.Windows.Forms.ToolStripButton tsbMultiSelect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton tsdTimeDiv;
        private System.Windows.Forms.ToolStripMenuItem tsmTick480;
        private System.Windows.Forms.ToolStripMenuItem tsmTick240;
        private System.Windows.Forms.ToolStripMenuItem tsmTick120;
        private System.Windows.Forms.ToolStripMenuItem tsmTick060;
        private System.Windows.Forms.ToolStripMenuItem tsmTick320;
        private System.Windows.Forms.ToolStripMenuItem tsmTick160;
        private System.Windows.Forms.ToolStripMenuItem tsmTick080;
        private System.Windows.Forms.ToolStripMenuItem tsmTick040;
        private System.Windows.Forms.ToolStripMenuItem tsmTick192;
        private System.Windows.Forms.ToolStripMenuItem tsmTick096;
        private System.Windows.Forms.ToolStripMenuItem tsmTick048;
        private System.Windows.Forms.ToolStripMenuItem tsmTick024;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripDropDownButton tsdEditMode;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeNote;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeVol;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeExp;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModePan;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModePitch;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeVib;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeRev;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeDel;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeCho;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeTempo;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeVibDep;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeVibRate;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeVibDelay;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeDelDep;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeDelTime;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeInst;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeAccent;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeFc;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeFq;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeAttack;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeRelease;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModePreset;
        private System.Windows.Forms.ToolStripDropDownButton tsdDisp;
        private System.Windows.Forms.ToolStripMenuItem tsmTimeZoom;
        private System.Windows.Forms.ToolStripMenuItem tsmTimeZoomout;
        private System.Windows.Forms.ToolStripMenuItem tsmToneZoom;
        private System.Windows.Forms.ToolStripMenuItem tsmToneZoomout;
        private System.Windows.Forms.ToolStripMenuItem tsmScrollNext;
        private System.Windows.Forms.ToolStripMenuItem tsmScrollPrev;
        private System.Windows.Forms.ToolStripMenuItem tsmScrollUp;
        private System.Windows.Forms.ToolStripMenuItem tsmScrollDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModeMeasure;
        private System.Windows.Forms.ToolStripLabel tslChord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripLabel tslKey;
        private System.Windows.Forms.ToolStripLabel tslDegree;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private ToolStripSeparator toolStripSeparator15;
    }
}


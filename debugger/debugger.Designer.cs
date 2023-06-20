namespace debugger
{
    partial class debugger
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.setup_btn = new System.Windows.Forms.Button();
            this.goto_btn = new System.Windows.Forms.Button();
            this.data_panel = new System.Windows.Forms.Panel();
            this.addr_txt = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.byte_mode_btn = new System.Windows.Forms.Button();
            this.hword_mode_btn = new System.Windows.Forms.Button();
            this.word_mode_btn = new System.Windows.Forms.Button();
            this.rst_btn = new System.Windows.Forms.Button();
            this.dwnld_btn = new System.Windows.Forms.Button();
            this.formstate_lbl = new System.Windows.Forms.Label();
            this.dbgapb_req_btn = new System.Windows.Forms.Button();
            this.dbg_info_sel = new System.Windows.Forms.ComboBox();
            this.trace_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // setup_btn
            // 
            this.setup_btn.Location = new System.Drawing.Point(21, 21);
            this.setup_btn.Name = "setup_btn";
            this.setup_btn.Size = new System.Drawing.Size(61, 52);
            this.setup_btn.TabIndex = 0;
            this.setup_btn.Text = "Setup";
            this.setup_btn.UseVisualStyleBackColor = true;
            this.setup_btn.Click += new System.EventHandler(this.setup_btn_Click);
            // 
            // goto_btn
            // 
            this.goto_btn.Enabled = false;
            this.goto_btn.Location = new System.Drawing.Point(690, 19);
            this.goto_btn.Name = "goto_btn";
            this.goto_btn.Size = new System.Drawing.Size(75, 23);
            this.goto_btn.TabIndex = 4;
            this.goto_btn.Text = "Goto";
            this.goto_btn.UseVisualStyleBackColor = true;
            this.goto_btn.Click += new System.EventHandler(this.goto_btn_Click);
            // 
            // data_panel
            // 
            this.data_panel.BackColor = System.Drawing.Color.White;
            this.data_panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.data_panel.Location = new System.Drawing.Point(22, 86);
            this.data_panel.Name = "data_panel";
            this.data_panel.Size = new System.Drawing.Size(742, 326);
            this.data_panel.TabIndex = 5;
            // 
            // addr_txt
            // 
            this.addr_txt.Location = new System.Drawing.Point(573, 21);
            this.addr_txt.Name = "addr_txt";
            this.addr_txt.Size = new System.Drawing.Size(111, 22);
            this.addr_txt.TabIndex = 6;
            this.addr_txt.Text = "0";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Location = new System.Drawing.Point(746, 88);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 324);
            this.vScrollBar1.TabIndex = 0;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // byte_mode_btn
            // 
            this.byte_mode_btn.Enabled = false;
            this.byte_mode_btn.Location = new System.Drawing.Point(573, 48);
            this.byte_mode_btn.Name = "byte_mode_btn";
            this.byte_mode_btn.Size = new System.Drawing.Size(60, 23);
            this.byte_mode_btn.TabIndex = 10;
            this.byte_mode_btn.Text = "BYTE";
            this.byte_mode_btn.UseVisualStyleBackColor = true;
            this.byte_mode_btn.Click += new System.EventHandler(this.byte_mode_btn_Click);
            // 
            // hword_mode_btn
            // 
            this.hword_mode_btn.Enabled = false;
            this.hword_mode_btn.Location = new System.Drawing.Point(639, 48);
            this.hword_mode_btn.Name = "hword_mode_btn";
            this.hword_mode_btn.Size = new System.Drawing.Size(60, 23);
            this.hword_mode_btn.TabIndex = 11;
            this.hword_mode_btn.Text = "HWORD";
            this.hword_mode_btn.UseVisualStyleBackColor = true;
            this.hword_mode_btn.Click += new System.EventHandler(this.hword_mode_btn_Click);
            // 
            // word_mode_btn
            // 
            this.word_mode_btn.Enabled = false;
            this.word_mode_btn.Location = new System.Drawing.Point(705, 48);
            this.word_mode_btn.Name = "word_mode_btn";
            this.word_mode_btn.Size = new System.Drawing.Size(60, 23);
            this.word_mode_btn.TabIndex = 12;
            this.word_mode_btn.Text = "WORD";
            this.word_mode_btn.UseVisualStyleBackColor = true;
            this.word_mode_btn.Click += new System.EventHandler(this.word_mode_btn_Click);
            // 
            // rst_btn
            // 
            this.rst_btn.Location = new System.Drawing.Point(88, 21);
            this.rst_btn.Name = "rst_btn";
            this.rst_btn.Size = new System.Drawing.Size(61, 52);
            this.rst_btn.TabIndex = 13;
            this.rst_btn.Text = "Reset";
            this.rst_btn.UseVisualStyleBackColor = true;
            this.rst_btn.Click += new System.EventHandler(this.rst_btn_Click);
            // 
            // dwnld_btn
            // 
            this.dwnld_btn.Location = new System.Drawing.Point(155, 21);
            this.dwnld_btn.Name = "dwnld_btn";
            this.dwnld_btn.Size = new System.Drawing.Size(61, 52);
            this.dwnld_btn.TabIndex = 14;
            this.dwnld_btn.Text = "Download ROM";
            this.dwnld_btn.UseVisualStyleBackColor = true;
            this.dwnld_btn.Click += new System.EventHandler(this.dwnld_btn_Click);
            // 
            // formstate_lbl
            // 
            this.formstate_lbl.BackColor = System.Drawing.Color.Silver;
            this.formstate_lbl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.formstate_lbl.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.formstate_lbl.Location = new System.Drawing.Point(289, 21);
            this.formstate_lbl.Name = "formstate_lbl";
            this.formstate_lbl.Size = new System.Drawing.Size(112, 52);
            this.formstate_lbl.TabIndex = 15;
            this.formstate_lbl.Text = "No work";
            this.formstate_lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dbgapb_req_btn
            // 
            this.dbgapb_req_btn.Location = new System.Drawing.Point(407, 50);
            this.dbgapb_req_btn.Name = "dbgapb_req_btn";
            this.dbgapb_req_btn.Size = new System.Drawing.Size(149, 23);
            this.dbgapb_req_btn.TabIndex = 16;
            this.dbgapb_req_btn.Text = "Read DBGAPB";
            this.dbgapb_req_btn.UseVisualStyleBackColor = true;
            this.dbgapb_req_btn.Click += new System.EventHandler(this.dbgapb_req_btn_Click);
            // 
            // dbg_info_sel
            // 
            this.dbg_info_sel.FormattingEnabled = true;
            this.dbg_info_sel.Location = new System.Drawing.Point(407, 22);
            this.dbg_info_sel.Name = "dbg_info_sel";
            this.dbg_info_sel.Size = new System.Drawing.Size(149, 20);
            this.dbg_info_sel.TabIndex = 17;
            // 
            // trace_btn
            // 
            this.trace_btn.Location = new System.Drawing.Point(222, 23);
            this.trace_btn.Name = "trace_btn";
            this.trace_btn.Size = new System.Drawing.Size(61, 49);
            this.trace_btn.TabIndex = 18;
            this.trace_btn.Text = "Dump Trace Data";
            this.trace_btn.UseVisualStyleBackColor = true;
            this.trace_btn.Click += new System.EventHandler(this.trace_btn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(783, 463);
            this.Controls.Add(this.trace_btn);
            this.Controls.Add(this.dbg_info_sel);
            this.Controls.Add(this.dbgapb_req_btn);
            this.Controls.Add(this.formstate_lbl);
            this.Controls.Add(this.dwnld_btn);
            this.Controls.Add(this.rst_btn);
            this.Controls.Add(this.word_mode_btn);
            this.Controls.Add(this.hword_mode_btn);
            this.Controls.Add(this.byte_mode_btn);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.addr_txt);
            this.Controls.Add(this.data_panel);
            this.Controls.Add(this.goto_btn);
            this.Controls.Add(this.setup_btn);
            this.Name = "Form1";
            this.Text = "Debugger";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button setup_btn;
        private System.Windows.Forms.Button goto_btn;
        private System.Windows.Forms.Panel data_panel;
        private System.Windows.Forms.TextBox addr_txt;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Button byte_mode_btn;
        private System.Windows.Forms.Button hword_mode_btn;
        private System.Windows.Forms.Button word_mode_btn;
        private System.Windows.Forms.Button rst_btn;
        private System.Windows.Forms.Button dwnld_btn;
        private System.Windows.Forms.Label formstate_lbl;
        private System.Windows.Forms.Button dbgapb_req_btn;
        private System.Windows.Forms.ComboBox dbg_info_sel;
        private System.Windows.Forms.Button trace_btn;
    }
}


namespace water
{
    partial class frmPayView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPayView));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_prnpayview = new System.Windows.Forms.Button();
            this.btn_payview = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_Agent = new System.Windows.Forms.ComboBox();
            this.gv_payview = new System.Windows.Forms.DataGridView();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.payUk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PayCommon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buhOK = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.OperOk = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_payview)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btn_prnpayview);
            this.panel1.Controls.Add(this.btn_payview);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.dateTimePicker1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cmb_Agent);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1028, 141);
            this.panel1.TabIndex = 5;
            // 
            // btn_prnpayview
            // 
            this.btn_prnpayview.Location = new System.Drawing.Point(369, 88);
            this.btn_prnpayview.Name = "btn_prnpayview";
            this.btn_prnpayview.Size = new System.Drawing.Size(278, 31);
            this.btn_prnpayview.TabIndex = 7;
            this.btn_prnpayview.Text = "ПЕЧАТЬ";
            this.btn_prnpayview.UseVisualStyleBackColor = true;
            this.btn_prnpayview.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_payview
            // 
            this.btn_payview.Location = new System.Drawing.Point(369, 35);
            this.btn_payview.Name = "btn_payview";
            this.btn_payview.Size = new System.Drawing.Size(278, 31);
            this.btn_payview.TabIndex = 9;
            this.btn_payview.Text = "ВЫВЕСТИ ОТЧЕТ";
            this.btn_payview.UseVisualStyleBackColor = true;
            this.btn_payview.Click += new System.EventHandler(this.btn_payview_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Выберите период";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(16, 94);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(287, 24);
            this.dateTimePicker1.TabIndex = 7;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Выберите агента по приему платежей";
            // 
            // cmb_Agent
            // 
            this.cmb_Agent.FormattingEnabled = true;
            this.cmb_Agent.Location = new System.Drawing.Point(16, 35);
            this.cmb_Agent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmb_Agent.Name = "cmb_Agent";
            this.cmb_Agent.Size = new System.Drawing.Size(287, 24);
            this.cmb_Agent.TabIndex = 5;
            // 
            // gv_payview
            // 
            this.gv_payview.AllowUserToAddRows = false;
            this.gv_payview.AllowUserToDeleteRows = false;
            this.gv_payview.AllowUserToOrderColumns = true;
            this.gv_payview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_payview.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Date,
            this.payUk,
            this.cu,
            this.pay,
            this.cg,
            this.PayCommon,
            this.c,
            this.buhOK,
            this.OperOk});
            this.gv_payview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_payview.Location = new System.Drawing.Point(0, 141);
            this.gv_payview.Name = "gv_payview";
            this.gv_payview.ReadOnly = true;
            this.gv_payview.RowHeadersVisible = false;
            this.gv_payview.Size = new System.Drawing.Size(1028, 425);
            this.gv_payview.TabIndex = 6;
            this.gv_payview.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gv_payview_CellClick);
            // 
            // Date
            // 
            this.Date.Frozen = true;
            this.Date.HeaderText = "Дата";
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 75;
            // 
            // payUk
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "N2";
            dataGridViewCellStyle1.NullValue = null;
            this.payUk.DefaultCellStyle = dataGridViewCellStyle1;
            this.payUk.Frozen = true;
            this.payUk.HeaderText = "Поступление УК";
            this.payUk.Name = "payUk";
            this.payUk.ReadOnly = true;
            this.payUk.Width = 125;
            // 
            // cu
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.cu.DefaultCellStyle = dataGridViewCellStyle2;
            this.cu.Frozen = true;
            this.cu.HeaderText = "Платежей";
            this.cu.Name = "cu";
            this.cu.ReadOnly = true;
            // 
            // pay
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N2";
            this.pay.DefaultCellStyle = dataGridViewCellStyle3;
            this.pay.Frozen = true;
            this.pay.HeaderText = "Поступление";
            this.pay.Name = "pay";
            this.pay.ReadOnly = true;
            this.pay.Width = 125;
            // 
            // cg
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.cg.DefaultCellStyle = dataGridViewCellStyle4;
            this.cg.Frozen = true;
            this.cg.HeaderText = "Платежей";
            this.cg.Name = "cg";
            this.cg.ReadOnly = true;
            // 
            // PayCommon
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N2";
            this.PayCommon.DefaultCellStyle = dataGridViewCellStyle5;
            this.PayCommon.Frozen = true;
            this.PayCommon.HeaderText = "Общее поступление";
            this.PayCommon.Name = "PayCommon";
            this.PayCommon.ReadOnly = true;
            this.PayCommon.Width = 150;
            // 
            // c
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.c.DefaultCellStyle = dataGridViewCellStyle6;
            this.c.Frozen = true;
            this.c.HeaderText = "Платежей";
            this.c.Name = "c";
            this.c.ReadOnly = true;
            // 
            // buhOK
            // 
            this.buhOK.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.buhOK.Frozen = true;
            this.buhOK.HeaderText = "Сверка бух-я";
            this.buhOK.Name = "buhOK";
            this.buhOK.ReadOnly = true;
            this.buhOK.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.buhOK.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.buhOK.Width = 119;
            // 
            // OperOk
            // 
            this.OperOk.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.OperOk.Frozen = true;
            this.OperOk.HeaderText = "Сверка опер-р";
            this.OperOk.Name = "OperOk";
            this.OperOk.ReadOnly = true;
            this.OperOk.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.OperOk.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.OperOk.Width = 128;
            // 
            // printDocument1
            // 
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
            // 
            // printDialog1
            // 
            this.printDialog1.AllowPrintToFile = false;
            this.printDialog1.Document = this.printDocument1;
            this.printDialog1.UseEXDialog = true;
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Document = this.printDocument1;
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(671, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(230, 31);
            this.button1.TabIndex = 10;
            this.button1.Text = "Выгрузить в EXCEL";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // frmPayView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 566);
            this.Controls.Add(this.gv_payview);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmPayView";
            this.Text = "Отчет поступлений";
            this.Load += new System.EventHandler(this.frmPayView_Load);
            this.Shown += new System.EventHandler(this.frmPayView_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_payview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_payview;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_Agent;
        private System.Windows.Forms.DataGridView gv_payview;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.Button btn_prnpayview;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.DataGridViewTextBoxColumn payUk;
        private System.Windows.Forms.DataGridViewTextBoxColumn cu;
        private System.Windows.Forms.DataGridViewTextBoxColumn pay;
        private System.Windows.Forms.DataGridViewTextBoxColumn cg;
        private System.Windows.Forms.DataGridViewTextBoxColumn PayCommon;
        private System.Windows.Forms.DataGridViewTextBoxColumn c;
        private System.Windows.Forms.DataGridViewCheckBoxColumn buhOK;
        private System.Windows.Forms.DataGridViewCheckBoxColumn OperOk;
        private System.Windows.Forms.Button button1;

    }
}
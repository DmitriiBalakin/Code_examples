namespace water
{
    partial class frmBadUsluga
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbl_op = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.cmb_client = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmb_reason = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.pnl_house = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmb_house = new System.Windows.Forms.ComboBox();
            this.cmb_street = new System.Windows.Forms.ComboBox();
            this.pnl_street = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.cmb_street_ = new System.Windows.Forms.ComboBox();
            this.maskedTextBox2 = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnl_vedom = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.cmb_vedom = new System.Windows.Forms.ComboBox();
            this.lbl_k = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pnl_lic = new System.Windows.Forms.Panel();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gv_nu = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.K = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Lic = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.street = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.house = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vedom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnl_house.SuspendLayout();
            this.pnl_street.SuspendLayout();
            this.pnl_vedom.SuspendLayout();
            this.pnl_lic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_nu)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl_op);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.cmb_client);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cmb_reason);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(846, 132);
            this.panel1.TabIndex = 4;
            // 
            // lbl_op
            // 
            this.lbl_op.AutoSize = true;
            this.lbl_op.Location = new System.Drawing.Point(411, 69);
            this.lbl_op.Name = "lbl_op";
            this.lbl_op.Size = new System.Drawing.Size(0, 17);
            this.lbl_op.TabIndex = 10;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(405, 89);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(427, 23);
            this.progressBar1.TabIndex = 9;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(535, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 41);
            this.button2.TabIndex = 8;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // cmb_client
            // 
            this.cmb_client.FormattingEnabled = true;
            this.cmb_client.Location = new System.Drawing.Point(15, 89);
            this.cmb_client.Name = "cmb_client";
            this.cmb_client.Size = new System.Drawing.Size(355, 24);
            this.cmb_client.TabIndex = 7;
            this.cmb_client.SelectedIndexChanged += new System.EventHandler(this.cmb_client_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(341, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Выберите для кого производится перерасчет";
            // 
            // cmb_reason
            // 
            this.cmb_reason.FormattingEnabled = true;
            this.cmb_reason.Location = new System.Drawing.Point(15, 29);
            this.cmb_reason.Name = "cmb_reason";
            this.cmb_reason.Size = new System.Drawing.Size(355, 24);
            this.cmb_reason.TabIndex = 5;
            this.cmb_reason.SelectedIndexChanged += new System.EventHandler(this.cmb_reason_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(245, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Причина некачественной услуги";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.progressBar2);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.dateTimePicker1);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.pnl_house);
            this.panel2.Controls.Add(this.pnl_street);
            this.panel2.Controls.Add(this.maskedTextBox2);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.pnl_vedom);
            this.panel2.Controls.Add(this.lbl_k);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.pnl_lic);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 132);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(370, 466);
            this.panel2.TabIndex = 5;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(15, 411);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(338, 23);
            this.progressBar2.TabIndex = 16;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label11.Location = new System.Drawing.Point(12, 327);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(335, 64);
            this.label11.TabIndex = 15;
            this.label11.Text = "* За текущий период переразсчет будет\r\nпроизведен при закрытии периода\r\nЗа предыд" +
    "ущие пириоды перерасчет будет\r\nпроизведен сразу в виде сторно";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 216);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(137, 17);
            this.label10.TabIndex = 14;
            this.label10.Text = "Выберите период";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "MMMM yyyy";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(15, 236);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.ShowUpDown = true;
            this.dateTimePicker1.Size = new System.Drawing.Size(338, 24);
            this.dateTimePicker1.TabIndex = 13;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 283);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(338, 30);
            this.button1.TabIndex = 12;
            this.button1.Text = "Добавить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pnl_house
            // 
            this.pnl_house.Controls.Add(this.label7);
            this.pnl_house.Controls.Add(this.label6);
            this.pnl_house.Controls.Add(this.cmb_house);
            this.pnl_house.Controls.Add(this.cmb_street);
            this.pnl_house.Location = new System.Drawing.Point(3, 73);
            this.pnl_house.Name = "pnl_house";
            this.pnl_house.Size = new System.Drawing.Size(353, 127);
            this.pnl_house.TabIndex = 11;
            this.pnl_house.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 17);
            this.label7.TabIndex = 3;
            this.label7.Text = "Выберите дом";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Выберите улицу";
            // 
            // cmb_house
            // 
            this.cmb_house.FormattingEnabled = true;
            this.cmb_house.Location = new System.Drawing.Point(12, 89);
            this.cmb_house.Name = "cmb_house";
            this.cmb_house.Size = new System.Drawing.Size(341, 24);
            this.cmb_house.TabIndex = 1;
            // 
            // cmb_street
            // 
            this.cmb_street.FormattingEnabled = true;
            this.cmb_street.Location = new System.Drawing.Point(12, 32);
            this.cmb_street.Name = "cmb_street";
            this.cmb_street.Size = new System.Drawing.Size(341, 24);
            this.cmb_street.TabIndex = 0;
            this.cmb_street.SelectedIndexChanged += new System.EventHandler(this.cmb_street_SelectedIndexChanged);
            // 
            // pnl_street
            // 
            this.pnl_street.Controls.Add(this.label8);
            this.pnl_street.Controls.Add(this.cmb_street_);
            this.pnl_street.Location = new System.Drawing.Point(3, 72);
            this.pnl_street.Name = "pnl_street";
            this.pnl_street.Size = new System.Drawing.Size(353, 66);
            this.pnl_street.TabIndex = 9;
            this.pnl_street.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(128, 17);
            this.label8.TabIndex = 4;
            this.label8.Text = "Выберите улицу";
            // 
            // cmb_street_
            // 
            this.cmb_street_.FormattingEnabled = true;
            this.cmb_street_.Location = new System.Drawing.Point(12, 32);
            this.cmb_street_.Name = "cmb_street_";
            this.cmb_street_.Size = new System.Drawing.Size(341, 24);
            this.cmb_street_.TabIndex = 3;
            // 
            // maskedTextBox2
            // 
            this.maskedTextBox2.BackColor = System.Drawing.SystemColors.Control;
            this.maskedTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox2.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.maskedTextBox2.ForeColor = System.Drawing.Color.Blue;
            this.maskedTextBox2.Location = new System.Drawing.Point(197, 39);
            this.maskedTextBox2.Mask = "00";
            this.maskedTextBox2.Name = "maskedTextBox2";
            this.maskedTextBox2.Size = new System.Drawing.Size(38, 17);
            this.maskedTextBox2.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Количество дней";
            // 
            // pnl_vedom
            // 
            this.pnl_vedom.Controls.Add(this.label9);
            this.pnl_vedom.Controls.Add(this.cmb_vedom);
            this.pnl_vedom.Location = new System.Drawing.Point(3, 72);
            this.pnl_vedom.Name = "pnl_vedom";
            this.pnl_vedom.Size = new System.Drawing.Size(353, 64);
            this.pnl_vedom.TabIndex = 6;
            this.pnl_vedom.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(160, 17);
            this.label9.TabIndex = 6;
            this.label9.Text = "Выберите ведомство";
            // 
            // cmb_vedom
            // 
            this.cmb_vedom.FormattingEnabled = true;
            this.cmb_vedom.Location = new System.Drawing.Point(12, 31);
            this.cmb_vedom.Name = "cmb_vedom";
            this.cmb_vedom.Size = new System.Drawing.Size(341, 24);
            this.cmb_vedom.TabIndex = 5;
            // 
            // lbl_k
            // 
            this.lbl_k.AutoSize = true;
            this.lbl_k.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbl_k.ForeColor = System.Drawing.Color.Blue;
            this.lbl_k.Location = new System.Drawing.Point(194, 12);
            this.lbl_k.Name = "lbl_k";
            this.lbl_k.Size = new System.Drawing.Size(18, 17);
            this.lbl_k.TabIndex = 3;
            this.lbl_k.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(176, 17);
            this.label5.TabIndex = 2;
            this.label5.Text = "Коэфициент пересчета";
            // 
            // pnl_lic
            // 
            this.pnl_lic.Controls.Add(this.maskedTextBox1);
            this.pnl_lic.Controls.Add(this.label3);
            this.pnl_lic.Location = new System.Drawing.Point(3, 72);
            this.pnl_lic.Name = "pnl_lic";
            this.pnl_lic.Size = new System.Drawing.Size(364, 44);
            this.pnl_lic.TabIndex = 1;
            this.pnl_lic.Visible = false;
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.BackColor = System.Drawing.SystemColors.Control;
            this.maskedTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.maskedTextBox1.ForeColor = System.Drawing.Color.Blue;
            this.maskedTextBox1.Location = new System.Drawing.Point(96, 9);
            this.maskedTextBox1.Mask = "0000000000";
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(95, 17);
            this.maskedTextBox1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Номер л/с";
            // 
            // gv_nu
            // 
            this.gv_nu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_nu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.K,
            this.Lic,
            this.street,
            this.house,
            this.vedom});
            this.gv_nu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_nu.Location = new System.Drawing.Point(370, 132);
            this.gv_nu.Name = "gv_nu";
            this.gv_nu.RowHeadersVisible = false;
            this.gv_nu.Size = new System.Drawing.Size(476, 466);
            this.gv_nu.TabIndex = 6;
            // 
            // id
            // 
            this.id.Frozen = true;
            this.id.HeaderText = "id";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            // 
            // K
            // 
            this.K.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.K.Frozen = true;
            this.K.HeaderText = "Коэфициент";
            this.K.Name = "K";
            this.K.ReadOnly = true;
            this.K.Width = 123;
            // 
            // Lic
            // 
            this.Lic.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Lic.Frozen = true;
            this.Lic.HeaderText = "Лицевой счет";
            this.Lic.Name = "Lic";
            this.Lic.ReadOnly = true;
            this.Lic.Width = 121;
            // 
            // street
            // 
            this.street.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.street.Frozen = true;
            this.street.HeaderText = "Улица";
            this.street.Name = "street";
            this.street.ReadOnly = true;
            this.street.Width = 78;
            // 
            // house
            // 
            this.house.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.house.Frozen = true;
            this.house.HeaderText = "Дом";
            this.house.Name = "house";
            this.house.ReadOnly = true;
            this.house.Width = 62;
            // 
            // vedom
            // 
            this.vedom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.vedom.Frozen = true;
            this.vedom.HeaderText = "Ведомство";
            this.vedom.Name = "vedom";
            this.vedom.ReadOnly = true;
            this.vedom.Width = 111;
            // 
            // frmBadUsluga
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 598);
            this.Controls.Add(this.gv_nu);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmBadUsluga";
            this.Text = "Некачественная услуга";
            this.Shown += new System.EventHandler(this.frmBadUsluga_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnl_house.ResumeLayout(false);
            this.pnl_house.PerformLayout();
            this.pnl_street.ResumeLayout(false);
            this.pnl_street.PerformLayout();
            this.pnl_vedom.ResumeLayout(false);
            this.pnl_vedom.PerformLayout();
            this.pnl_lic.ResumeLayout(false);
            this.pnl_lic.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_nu)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmb_client;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmb_reason;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnl_lic;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_k;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel pnl_vedom;
        private System.Windows.Forms.MaskedTextBox maskedTextBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmb_vedom;
        private System.Windows.Forms.Panel pnl_street;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmb_street_;
        private System.Windows.Forms.Panel pnl_house;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmb_house;
        private System.Windows.Forms.ComboBox cmb_street;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView gv_nu;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn K;
        private System.Windows.Forms.DataGridViewTextBoxColumn Lic;
        private System.Windows.Forms.DataGridViewTextBoxColumn street;
        private System.Windows.Forms.DataGridViewTextBoxColumn house;
        private System.Windows.Forms.DataGridViewTextBoxColumn vedom;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lbl_op;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ProgressBar progressBar2;
    }
}
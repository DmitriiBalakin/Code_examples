namespace water
{
    partial class frmPayGet
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_Refresh = new System.Windows.Forms.Button();
            this.btn_payget = new System.Windows.Forms.Button();
            this.chk_top = new System.Windows.Forms.CheckBox();
            this.chk_delete = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.btn_stop = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lbl_count = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_file = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.gv_payfiles = new System.Windows.Forms.DataGridView();
            this.get = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.file = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bric = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.summ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sys_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pach = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_payfiles)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(999, 47);
            this.panel1.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(199, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(203, 29);
            this.button2.TabIndex = 3;
            this.button2.Text = "Принятые файлы";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Файлы на прием";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.btn_Refresh);
            this.panel2.Controls.Add(this.btn_payget);
            this.panel2.Controls.Add(this.chk_top);
            this.panel2.Controls.Add(this.chk_delete);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 635);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(999, 93);
            this.panel2.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LightGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(12, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(160, 29);
            this.button1.TabIndex = 4;
            this.button1.Text = "ВЫДЕЛИТЬ ВСЕ";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_Refresh
            // 
            this.btn_Refresh.BackColor = System.Drawing.Color.LightGreen;
            this.btn_Refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Refresh.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_Refresh.Location = new System.Drawing.Point(12, 52);
            this.btn_Refresh.Name = "btn_Refresh";
            this.btn_Refresh.Size = new System.Drawing.Size(160, 29);
            this.btn_Refresh.TabIndex = 3;
            this.btn_Refresh.Text = "ОБНОВИТЬ";
            this.btn_Refresh.UseVisualStyleBackColor = false;
            this.btn_Refresh.Click += new System.EventHandler(this.btn_Refresh_Click);
            // 
            // btn_payget
            // 
            this.btn_payget.BackColor = System.Drawing.Color.LightGreen;
            this.btn_payget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_payget.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_payget.Location = new System.Drawing.Point(618, 20);
            this.btn_payget.Name = "btn_payget";
            this.btn_payget.Size = new System.Drawing.Size(153, 29);
            this.btn_payget.TabIndex = 2;
            this.btn_payget.Text = "ПРИНЯТЬ";
            this.btn_payget.UseVisualStyleBackColor = false;
            this.btn_payget.Click += new System.EventHandler(this.btn_payget_Click);
            // 
            // chk_top
            // 
            this.chk_top.AutoSize = true;
            this.chk_top.Location = new System.Drawing.Point(272, 20);
            this.chk_top.Name = "chk_top";
            this.chk_top.Size = new System.Drawing.Size(204, 21);
            this.chk_top.TabIndex = 1;
            this.chk_top.Text = "принять без перезаписи";
            this.chk_top.UseVisualStyleBackColor = true;
            this.chk_top.CheckStateChanged += new System.EventHandler(this.chk_top_CheckStateChanged);
            // 
            // chk_delete
            // 
            this.chk_delete.AutoSize = true;
            this.chk_delete.Location = new System.Drawing.Point(272, 47);
            this.chk_delete.Name = "chk_delete";
            this.chk_delete.Size = new System.Drawing.Size(219, 21);
            this.chk_delete.TabIndex = 0;
            this.chk_delete.Text = "удалить принятые данные";
            this.chk_delete.UseVisualStyleBackColor = true;
            this.chk_delete.CheckStateChanged += new System.EventHandler(this.chk_repeat_CheckStateChanged);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.progressBar2);
            this.panel3.Controls.Add(this.btn_stop);
            this.panel3.Controls.Add(this.progressBar1);
            this.panel3.Controls.Add(this.lbl_count);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.lbl_file);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Location = new System.Drawing.Point(272, 28);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(470, 289);
            this.panel3.TabIndex = 5;
            this.panel3.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 204);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(15, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(15, 17);
            this.label7.TabIndex = 10;
            this.label7.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(412, 204);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label6.Size = new System.Drawing.Size(15, 17);
            this.label6.TabIndex = 9;
            this.label6.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(412, 115);
            this.label5.Name = "label5";
            this.label5.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label5.Size = new System.Drawing.Size(15, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(12, 142);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(167, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Общее выполнение";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(12, 168);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(448, 33);
            this.progressBar2.Step = 1;
            this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar2.TabIndex = 6;
            // 
            // btn_stop
            // 
            this.btn_stop.BackColor = System.Drawing.Color.LightGreen;
            this.btn_stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_stop.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_stop.ForeColor = System.Drawing.Color.Red;
            this.btn_stop.Location = new System.Drawing.Point(12, 248);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(261, 36);
            this.btn_stop.TabIndex = 5;
            this.btn_stop.Text = "ОСТАНОВИТЬ ПРИЕМ";
            this.btn_stop.UseVisualStyleBackColor = false;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 79);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(448, 33);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 4;
            // 
            // lbl_count
            // 
            this.lbl_count.AutoSize = true;
            this.lbl_count.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbl_count.ForeColor = System.Drawing.Color.Blue;
            this.lbl_count.Location = new System.Drawing.Point(219, 45);
            this.lbl_count.Name = "lbl_count";
            this.lbl_count.Size = new System.Drawing.Size(15, 17);
            this.lbl_count.TabIndex = 3;
            this.lbl_count.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(12, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(186, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Количество платежей";
            // 
            // lbl_file
            // 
            this.lbl_file.AutoSize = true;
            this.lbl_file.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbl_file.ForeColor = System.Drawing.Color.Blue;
            this.lbl_file.Location = new System.Drawing.Point(219, 14);
            this.lbl_file.Name = "lbl_file";
            this.lbl_file.Size = new System.Drawing.Size(15, 17);
            this.lbl_file.TabIndex = 1;
            this.lbl_file.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(12, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Принимаемый файл";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.gv_payfiles);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 47);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(999, 588);
            this.panel4.TabIndex = 6;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.label9);
            this.panel5.Location = new System.Drawing.Point(111, 222);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(430, 227);
            this.panel5.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.ForeColor = System.Drawing.Color.Blue;
            this.label9.Location = new System.Drawing.Point(132, 97);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(165, 26);
            this.label9.TabIndex = 0;
            this.label9.Text = "ЗАГРУЗКА...";
            // 
            // gv_payfiles
            // 
            this.gv_payfiles.AllowUserToAddRows = false;
            this.gv_payfiles.AllowUserToDeleteRows = false;
            this.gv_payfiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_payfiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.get,
            this.file,
            this.bric,
            this.count,
            this.summ,
            this.sys_name,
            this.path,
            this.pach});
            this.gv_payfiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_payfiles.Location = new System.Drawing.Point(0, 0);
            this.gv_payfiles.Name = "gv_payfiles";
            this.gv_payfiles.ReadOnly = true;
            this.gv_payfiles.RowHeadersVisible = false;
            this.gv_payfiles.Size = new System.Drawing.Size(999, 588);
            this.gv_payfiles.TabIndex = 4;
            this.gv_payfiles.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gv_payfiles_CellContentClick);
            // 
            // get
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.PaleGreen;
            dataGridViewCellStyle1.NullValue = false;
            this.get.DefaultCellStyle = dataGridViewCellStyle1;
            this.get.FalseValue = "False";
            this.get.Frozen = true;
            this.get.HeaderText = "Прием";
            this.get.IndeterminateValue = "False";
            this.get.Name = "get";
            this.get.ReadOnly = true;
            this.get.TrueValue = "True";
            // 
            // file
            // 
            this.file.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.PaleGreen;
            this.file.DefaultCellStyle = dataGridViewCellStyle2;
            this.file.DividerWidth = 5;
            this.file.Frozen = true;
            this.file.HeaderText = "Файл";
            this.file.Name = "file";
            this.file.ReadOnly = true;
            this.file.Width = 75;
            // 
            // bric
            // 
            this.bric.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.bric.DefaultCellStyle = dataGridViewCellStyle3;
            this.bric.DividerWidth = 5;
            this.bric.Frozen = true;
            this.bric.HeaderText = "Брикет";
            this.bric.Name = "bric";
            this.bric.ReadOnly = true;
            this.bric.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.bric.Width = 89;
            // 
            // count
            // 
            this.count.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.count.DividerWidth = 5;
            this.count.Frozen = true;
            this.count.HeaderText = "Кол-во плат.";
            this.count.MinimumWidth = 140;
            this.count.Name = "count";
            this.count.ReadOnly = true;
            this.count.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.count.Width = 140;
            // 
            // summ
            // 
            this.summ.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle4.Format = "N2";
            dataGridViewCellStyle4.NullValue = null;
            this.summ.DefaultCellStyle = dataGridViewCellStyle4;
            this.summ.DividerWidth = 5;
            this.summ.Frozen = true;
            this.summ.HeaderText = "Сумма";
            this.summ.Name = "summ";
            this.summ.ReadOnly = true;
            this.summ.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.summ.Width = 84;
            // 
            // sys_name
            // 
            this.sys_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.sys_name.DefaultCellStyle = dataGridViewCellStyle5;
            this.sys_name.DividerWidth = 5;
            this.sys_name.Frozen = true;
            this.sys_name.HeaderText = "Плат. система";
            this.sys_name.MinimumWidth = 140;
            this.sys_name.Name = "sys_name";
            this.sys_name.ReadOnly = true;
            this.sys_name.Width = 140;
            // 
            // path
            // 
            this.path.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.path.DefaultCellStyle = dataGridViewCellStyle6;
            this.path.DividerWidth = 5;
            this.path.Frozen = true;
            this.path.HeaderText = "Путь к файлу";
            this.path.Name = "path";
            this.path.ReadOnly = true;
            this.path.Width = 123;
            // 
            // pach
            // 
            this.pach.Frozen = true;
            this.pach.HeaderText = "Пачка";
            this.pach.Name = "pach";
            this.pach.ReadOnly = true;
            this.pach.Visible = false;
            // 
            // frmPayGet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(999, 728);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmPayGet";
            this.ShowIcon = false;
            this.Text = "Прием платежей";
            this.Shown += new System.EventHandler(this.frmPayGet_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_payfiles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_payget;
        private System.Windows.Forms.CheckBox chk_top;
        private System.Windows.Forms.CheckBox chk_delete;
        private System.Windows.Forms.Button btn_Refresh;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbl_count;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_file;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataGridView gv_payfiles;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridViewCheckBoxColumn get;
        private System.Windows.Forms.DataGridViewTextBoxColumn file;
        private System.Windows.Forms.DataGridViewTextBoxColumn bric;
        private System.Windows.Forms.DataGridViewTextBoxColumn count;
        private System.Windows.Forms.DataGridViewTextBoxColumn summ;
        private System.Windows.Forms.DataGridViewTextBoxColumn sys_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn path;
        private System.Windows.Forms.DataGridViewTextBoxColumn pach;
        private System.Windows.Forms.Button button2;

    }
}
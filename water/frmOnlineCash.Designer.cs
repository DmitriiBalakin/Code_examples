namespace water
{
    partial class frmOnlineCash
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
            this.label5 = new System.Windows.Forms.Label();
            this.brik_edit = new System.Windows.Forms.ComboBox();
            this.date_edit = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pach_edit = new System.Windows.Forms.MaskedTextBox();
            this.pref_edit = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.brik_list = new System.Windows.Forms.DataGridView();
            this.Briket = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Prefix = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pachka = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DocDate = new CalendarColumn();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WorkPeriod = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Counts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Summa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.peredit = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.brik_list)).BeginInit();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(173, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "Выберите источник поступлений";
            // 
            // brik_edit
            // 
            this.brik_edit.FormattingEnabled = true;
            this.brik_edit.Items.AddRange(new object[] {
            "000 - Единое окно (ПГУК)",
            "001 - Поступления на р/с физ. лиц (ИП)",
            "276 - Поступления на р/с",
            "918 - Физлица на счет",
            "921 - Сбербанк",
            "922 - Сбербанк (ЕПД)",
            "955 - Московский индустриальный банк (терминал)",
            "956 - Московский индустриальный банк",
            "_21 - Сбербанк (с файла)",
            "_22 - Сбербанк (ЕПД) (с файла)",
            "_55 - Московский индустриальный банк (терминал) (с файла)",
            "_56 - Московский индустриальный банк (с файла)",
            "_76 - Поступления на р/с (с файла)"});
            this.brik_edit.Location = new System.Drawing.Point(195, 18);
            this.brik_edit.Name = "brik_edit";
            this.brik_edit.Size = new System.Drawing.Size(499, 21);
            this.brik_edit.TabIndex = 27;
            // 
            // date_edit
            // 
            this.date_edit.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.date_edit.Location = new System.Drawing.Point(195, 45);
            this.date_edit.Name = "date_edit";
            this.date_edit.Size = new System.Drawing.Size(122, 20);
            this.date_edit.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Дата поступления денег на счет";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(463, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Пачка";
            // 
            // pach_edit
            // 
            this.pach_edit.Location = new System.Drawing.Point(508, 45);
            this.pach_edit.Name = "pach_edit";
            this.pach_edit.Size = new System.Drawing.Size(72, 20);
            this.pach_edit.TabIndex = 39;
            // 
            // pref_edit
            // 
            this.pref_edit.Location = new System.Drawing.Point(388, 45);
            this.pref_edit.Name = "pref_edit";
            this.pref_edit.Size = new System.Drawing.Size(65, 20);
            this.pref_edit.TabIndex = 34;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(329, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "Префикс";
            // 
            // brik_list
            // 
            this.brik_list.AllowUserToAddRows = false;
            this.brik_list.AllowUserToDeleteRows = false;
            this.brik_list.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.brik_list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Briket,
            this.Prefix,
            this.Pachka,
            this.DocDate,
            this.FileName,
            this.WorkPeriod,
            this.Counts,
            this.Summa});
            this.brik_list.Location = new System.Drawing.Point(12, 73);
            this.brik_list.MultiSelect = false;
            this.brik_list.Name = "brik_list";
            this.brik_list.ReadOnly = true;
            this.brik_list.RowHeadersVisible = false;
            this.brik_list.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.brik_list.Size = new System.Drawing.Size(879, 370);
            this.brik_list.TabIndex = 50;
            // 
            // Briket
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Briket.DefaultCellStyle = dataGridViewCellStyle1;
            this.Briket.HeaderText = "Брикет";
            this.Briket.Name = "Briket";
            this.Briket.ReadOnly = true;
            // 
            // Prefix
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Prefix.DefaultCellStyle = dataGridViewCellStyle2;
            this.Prefix.HeaderText = "Префикс (для ПГУК)";
            this.Prefix.Name = "Prefix";
            this.Prefix.ReadOnly = true;
            // 
            // Pachka
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Pachka.DefaultCellStyle = dataGridViewCellStyle3;
            this.Pachka.HeaderText = "Пачка";
            this.Pachka.Name = "Pachka";
            this.Pachka.ReadOnly = true;
            // 
            // DocDate
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.DocDate.DefaultCellStyle = dataGridViewCellStyle4;
            this.DocDate.HeaderText = "Дата поступления";
            this.DocDate.Name = "DocDate";
            this.DocDate.ReadOnly = true;
            // 
            // FileName
            // 
            this.FileName.HeaderText = "Файл выгрузки";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 150;
            // 
            // WorkPeriod
            // 
            this.WorkPeriod.HeaderText = "Рабочий период";
            this.WorkPeriod.Name = "WorkPeriod";
            this.WorkPeriod.ReadOnly = true;
            // 
            // Counts
            // 
            this.Counts.HeaderText = "Количество";
            this.Counts.Name = "Counts";
            this.Counts.ReadOnly = true;
            // 
            // Summa
            // 
            this.Summa.HeaderText = "Сумма";
            this.Summa.Name = "Summa";
            this.Summa.ReadOnly = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 463);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 49);
            this.button1.TabIndex = 51;
            this.button1.Text = "Выгрузить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(711, 16);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(179, 49);
            this.button3.TabIndex = 53;
            this.button3.Text = "Добавить в список";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // peredit
            // 
            this.peredit.Location = new System.Drawing.Point(630, 45);
            this.peredit.Name = "peredit";
            this.peredit.Size = new System.Drawing.Size(65, 20);
            this.peredit.TabIndex = 55;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(586, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 54;
            this.label4.Text = "Период";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(157, 463);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(128, 49);
            this.button2.TabIndex = 56;
            this.button2.Text = "Удалить";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(762, 463);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(128, 49);
            this.button4.TabIndex = 57;
            this.button4.Text = "Подготовить к выгрузке";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(589, 463);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(157, 49);
            this.button6.TabIndex = 59;
            this.button6.Text = "Распределить поступления по услугам";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // frmOnlineCash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(902, 522);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.peredit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.brik_list);
            this.Controls.Add(this.pref_edit);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pach_edit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.date_edit);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.brik_edit);
            this.Name = "frmOnlineCash";
            this.Text = "frmOnlineCash";
            ((System.ComponentModel.ISupportInitialize)(this.brik_list)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox brik_edit;
        private System.Windows.Forms.DateTimePicker date_edit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox pach_edit;
        private System.Windows.Forms.MaskedTextBox pref_edit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView brik_list;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.MaskedTextBox peredit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Briket;
        private System.Windows.Forms.DataGridViewTextBoxColumn Prefix;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pachka;
        private CalendarColumn DocDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn WorkPeriod;
        private System.Windows.Forms.DataGridViewTextBoxColumn Counts;
        private System.Windows.Forms.DataGridViewTextBoxColumn Summa;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button6;
    }
}
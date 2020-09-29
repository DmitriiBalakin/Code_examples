namespace water
{
    partial class frm3group
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_street = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmb_house = new System.Windows.Forms.ComboBox();
            this.lbl_vedomstvo = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmb_newvedomstvo = new System.Windows.Forms.ComboBox();
            this.btn_3group = new System.Windows.Forms.Button();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_buk = new System.Windows.Forms.Label();
            this.cmb_flat = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Выберите улицу";
            // 
            // cmb_street
            // 
            this.cmb_street.FormattingEnabled = true;
            this.cmb_street.Location = new System.Drawing.Point(12, 39);
            this.cmb_street.Name = "cmb_street";
            this.cmb_street.Size = new System.Drawing.Size(185, 24);
            this.cmb_street.TabIndex = 1;
            this.cmb_street.SelectedIndexChanged += new System.EventHandler(this.cmb_street_SelectedIndexChanged);
            this.cmb_street.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmb_street_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Выберите дом";
            // 
            // cmb_house
            // 
            this.cmb_house.FormattingEnabled = true;
            this.cmb_house.Location = new System.Drawing.Point(12, 95);
            this.cmb_house.Name = "cmb_house";
            this.cmb_house.Size = new System.Drawing.Size(185, 24);
            this.cmb_house.TabIndex = 3;
            this.cmb_house.SelectedIndexChanged += new System.EventHandler(this.cmb_house_SelectedIndexChanged);
            this.cmb_house.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmb_house_KeyPress);
            // 
            // lbl_vedomstvo
            // 
            this.lbl_vedomstvo.AutoSize = true;
            this.lbl_vedomstvo.ForeColor = System.Drawing.Color.Blue;
            this.lbl_vedomstvo.Location = new System.Drawing.Point(9, 267);
            this.lbl_vedomstvo.Name = "lbl_vedomstvo";
            this.lbl_vedomstvo.Size = new System.Drawing.Size(15, 17);
            this.lbl_vedomstvo.TabIndex = 4;
            this.lbl_vedomstvo.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 232);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Текущее ведомство";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 425);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Новое ведомство";
            // 
            // cmb_newvedomstvo
            // 
            this.cmb_newvedomstvo.FormattingEnabled = true;
            this.cmb_newvedomstvo.Location = new System.Drawing.Point(9, 457);
            this.cmb_newvedomstvo.Name = "cmb_newvedomstvo";
            this.cmb_newvedomstvo.Size = new System.Drawing.Size(182, 24);
            this.cmb_newvedomstvo.TabIndex = 7;
            this.cmb_newvedomstvo.SelectedIndexChanged += new System.EventHandler(this.cmb_newvedomstvo_SelectedIndexChanged);
            // 
            // btn_3group
            // 
            this.btn_3group.Location = new System.Drawing.Point(25, 487);
            this.btn_3group.Name = "btn_3group";
            this.btn_3group.Size = new System.Drawing.Size(131, 23);
            this.btn_3group.TabIndex = 8;
            this.btn_3group.Text = "Перевести";
            this.btn_3group.UseVisualStyleBackColor = true;
            this.btn_3group.Click += new System.EventHandler(this.btn_3group_Click);
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(9, 329);
            this.maskedTextBox1.Mask = "00000";
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(182, 24);
            this.maskedTextBox1.TabIndex = 9;
            this.maskedTextBox1.Text = "01111";
            this.maskedTextBox1.ValidatingType = typeof(int);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 295);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(139, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "Номер договора";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 367);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = "label6";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Red;
            this.label7.Location = new System.Drawing.Point(6, 525);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 17);
            this.label7.TabIndex = 12;
            // 
            // lbl_buk
            // 
            this.lbl_buk.AutoSize = true;
            this.lbl_buk.ForeColor = System.Drawing.Color.Blue;
            this.lbl_buk.Location = new System.Drawing.Point(197, 461);
            this.lbl_buk.Name = "lbl_buk";
            this.lbl_buk.Size = new System.Drawing.Size(0, 17);
            this.lbl_buk.TabIndex = 13;
            // 
            // cmb_flat
            // 
            this.cmb_flat.FormattingEnabled = true;
            this.cmb_flat.Location = new System.Drawing.Point(12, 163);
            this.cmb_flat.Name = "cmb_flat";
            this.cmb_flat.Size = new System.Drawing.Size(185, 24);
            this.cmb_flat.TabIndex = 14;
            this.cmb_flat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmb_flat_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(165, 17);
            this.label8.TabIndex = 15;
            this.label8.Text = "Выберите квартиру";
            // 
            // frm3group
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 560);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.cmb_flat);
            this.Controls.Add(this.lbl_buk);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.maskedTextBox1);
            this.Controls.Add(this.btn_3group);
            this.Controls.Add(this.cmb_newvedomstvo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbl_vedomstvo);
            this.Controls.Add(this.cmb_house);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmb_street);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frm3group";
            this.Text = "Перевод дома в 3 группу";
            this.Shown += new System.EventHandler(this.frm3group_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_street;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmb_house;
        private System.Windows.Forms.Label lbl_vedomstvo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmb_newvedomstvo;
        private System.Windows.Forms.Button btn_3group;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_buk;
        private System.Windows.Forms.ComboBox cmb_flat;
        private System.Windows.Forms.Label label8;
    }
}
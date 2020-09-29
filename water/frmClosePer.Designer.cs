namespace water
{
    partial class frmClosePer
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
            this.gv_close = new System.Windows.Forms.DataGridView();
            this.chk = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.state = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.gv_create = new System.Windows.Forms.DataGridView();
            this.chk_create = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.operation_create = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.state_create = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gv_new = new System.Windows.Forms.DataGridView();
            this.button3 = new System.Windows.Forms.Button();
            this.chk_new = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.operation_new = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.state_new = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gv_close)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_create)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_new)).BeginInit();
            this.SuspendLayout();
            // 
            // gv_close
            // 
            this.gv_close.AllowUserToAddRows = false;
            this.gv_close.AllowUserToDeleteRows = false;
            this.gv_close.AllowUserToResizeColumns = false;
            this.gv_close.AllowUserToResizeRows = false;
            this.gv_close.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.gv_close.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gv_close.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_close.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk,
            this.operation,
            this.state});
            this.gv_close.Location = new System.Drawing.Point(12, 28);
            this.gv_close.MultiSelect = false;
            this.gv_close.Name = "gv_close";
            this.gv_close.ReadOnly = true;
            this.gv_close.RowHeadersVisible = false;
            this.gv_close.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.gv_close.Size = new System.Drawing.Size(829, 82);
            this.gv_close.TabIndex = 3;
            // 
            // chk
            // 
            this.chk.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.chk.HeaderText = "";
            this.chk.Name = "chk";
            this.chk.ReadOnly = true;
            this.chk.Width = 5;
            // 
            // operation
            // 
            this.operation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.operation.HeaderText = "Операция";
            this.operation.Name = "operation";
            this.operation.ReadOnly = true;
            this.operation.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.operation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.operation.Width = 86;
            // 
            // state
            // 
            this.state.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.state.HeaderText = "Статус";
            this.state.Name = "state";
            this.state.ReadOnly = true;
            this.state.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.state.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.state.Width = 62;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(224, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Операции в текущем месяце";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LightGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(12, 116);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(285, 44);
            this.button1.TabIndex = 5;
            this.button1.Text = "Выполнить";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LightGreen;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(12, 227);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(285, 44);
            this.button2.TabIndex = 6;
            this.button2.Text = "Создать новый период";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // gv_create
            // 
            this.gv_create.AllowUserToAddRows = false;
            this.gv_create.AllowUserToDeleteRows = false;
            this.gv_create.AllowUserToResizeColumns = false;
            this.gv_create.AllowUserToResizeRows = false;
            this.gv_create.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.gv_create.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gv_create.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_create.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_create,
            this.operation_create,
            this.state_create});
            this.gv_create.Location = new System.Drawing.Point(12, 167);
            this.gv_create.Name = "gv_create";
            this.gv_create.ReadOnly = true;
            this.gv_create.RowHeadersVisible = false;
            this.gv_create.Size = new System.Drawing.Size(829, 44);
            this.gv_create.TabIndex = 7;
            // 
            // chk_create
            // 
            this.chk_create.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.chk_create.HeaderText = "";
            this.chk_create.Name = "chk_create";
            this.chk_create.ReadOnly = true;
            this.chk_create.Width = 5;
            // 
            // operation_create
            // 
            this.operation_create.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.operation_create.HeaderText = "Операция";
            this.operation_create.Name = "operation_create";
            this.operation_create.ReadOnly = true;
            this.operation_create.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.operation_create.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.operation_create.Width = 86;
            // 
            // state_create
            // 
            this.state_create.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.state_create.HeaderText = "Статус";
            this.state_create.Name = "state_create";
            this.state_create.ReadOnly = true;
            this.state_create.Width = 81;
            // 
            // gv_new
            // 
            this.gv_new.AllowUserToAddRows = false;
            this.gv_new.AllowUserToDeleteRows = false;
            this.gv_new.AllowUserToResizeColumns = false;
            this.gv_new.AllowUserToResizeRows = false;
            this.gv_new.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.gv_new.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gv_new.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_new.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chk_new,
            this.operation_new,
            this.state_new});
            this.gv_new.Location = new System.Drawing.Point(12, 277);
            this.gv_new.Name = "gv_new";
            this.gv_new.ReadOnly = true;
            this.gv_new.RowHeadersVisible = false;
            this.gv_new.Size = new System.Drawing.Size(829, 66);
            this.gv_new.TabIndex = 8;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.LightGreen;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(12, 359);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(285, 44);
            this.button3.TabIndex = 9;
            this.button3.Text = "Выполнить";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // chk_new
            // 
            this.chk_new.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.chk_new.HeaderText = "";
            this.chk_new.Name = "chk_new";
            this.chk_new.ReadOnly = true;
            this.chk_new.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.chk_new.Width = 5;
            // 
            // operation_new
            // 
            this.operation_new.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.operation_new.HeaderText = "Операция";
            this.operation_new.Name = "operation_new";
            this.operation_new.ReadOnly = true;
            this.operation_new.Width = 105;
            // 
            // state_new
            // 
            this.state_new.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.state_new.HeaderText = "Статус";
            this.state_new.Name = "state_new";
            this.state_new.ReadOnly = true;
            this.state_new.Width = 81;
            // 
            // frmClosePer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1143, 654);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.gv_new);
            this.Controls.Add(this.gv_create);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gv_close);
            this.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "frmClosePer";
            this.Text = "Закрытие периода";
            this.Shown += new System.EventHandler(this.frmClosePer_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.gv_close)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_create)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_new)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gv_close;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chk;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation;
        private System.Windows.Forms.DataGridViewTextBoxColumn state;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView gv_create;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chk_create;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation_create;
        private System.Windows.Forms.DataGridViewTextBoxColumn state_create;
        private System.Windows.Forms.DataGridView gv_new;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chk_new;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation_new;
        private System.Windows.Forms.DataGridViewTextBoxColumn state_new;
    }
}
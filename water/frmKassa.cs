using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zen.Barcode;


namespace water
{
    public partial class frmKassa : Form
    {
        //Code128BarcodeDraw bar = BarcodeDrawFactory.Code128WithChecksum;
        SqlConnection db_con = new SqlConnection();
        SqlCommand db_com = new SqlCommand();

        public frmKassa()
        {
            InitializeComponent();
            db_con.ConnectionString = "Data Source=SERVERAB;Initial Catalog=Abon;Integrated Security=True;Persist Security Info=False;User ID=SqlAbon;Connect Timeout=0;TrustServerCertificate=False";
            db_com.Connection = db_con;
            db_com.CommandTimeout = 0;
        }

        private bool findlic(string lic = "", string str = "", string kv = "")
        {
            
            ///pictureBox1.Image = bar.Draw("1313245400201509000000004025", 30);
            Boolean result = false;
            db_com.CommandType = CommandType.Text;
            if (lic.Length > 0 && str.Length == 0 && kv.Length == 0)
            {
                if (checkBox1.CheckState == CheckState.Checked)
                    db_com.CommandText = "select a.lic,a.vvodomer from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk=1 where lic='2'+@lic" +
                                         " union all" +
                                         " select a.lic,a.vvodomer from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk=0 where lic='1'+@lic";
                else
                {
                    db_com.CommandText = "select a.lic,a.vvodomer from "+(lic.Substring(0,1)=="1"?"Abon":"Abonuk")+".dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id = a.kodvedom where lic='"+lic.Substring(0,1)+"'+@lic";
                }
                db_com.Parameters.AddWithValue("@lic", lic.Substring(1,9));
                try
                {
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            db_read.Read();
                            maskedTextBox1.Text = db_read["lic"].ToString();
                            db_com.Parameters.Clear();
                            label2.Text = (maskedTextBox1.Text.Substring(0, 1) == "1") ? "Жилье" : "Жилье УК";
                            result = true;
                        }
                        else
                        {
                            db_com.Parameters.Clear();
                            label2.Text = "Л/сч не найден";
                        }
                    }
                }
                catch { db_com.Parameters.Clear(); }
            }

            return result;
        }

        private void maskedTextBox1_KeyUp(object sender, KeyEventArgs e = null)
        {
            if (maskedTextBox1.Text.Replace("_", "").Length == 10)
            {
                if (findlic(maskedTextBox1.Text)) panel3.Visible = true; else panel3.Visible = false;
            }
            else
            {
                label2.Text = "";
                panel3.Visible = false;
            }
        }

        private void frmKassa_Shown(object sender, EventArgs e)
        {
            try
            {
                db_con.Open();
                comboBox1.Items.Add(new SelectData("1","Поступление"));
                comboBox1.Items.Add(new SelectData("2", "Гос. пошлина"));
                comboBox1.Items.Add(new SelectData("3", "Возврат по кассе"));
                //comboBox1.Items.Add(new SelectData("3", "Группа 3"));
                //comboBox1.Items.Add(new SelectData("4", "Долг"));
                //comboBox1.Items.Add(new SelectData("5", "Доплата"));
                //comboBox1.Items.Add(new SelectData("6", "Комиссионный сбор"));
                //comboBox1.Items.Add(new SelectData("7", "Корректировка"));
                //comboBox1.Items.Add(new SelectData("8", "Неопознанный"));
                //comboBox1.Items.Add(new SelectData("9", "Оплата без показаний"));
                //comboBox1.Items.Add(new SelectData("10", "Оплата по счетчику"));
                comboBox1.Items.Add(new SelectData("11", "Переброска"));
                //comboBox1.Items.Add(new SelectData("12", "Переплата"));
                //comboBox1.Items.Add(new SelectData("13", "Полив"));
                //comboBox1.Items.Add(new SelectData("14", "Проводка"));
                comboBox1.Items.Add(new SelectData("3", "Суд"));
                comboBox1.Items.Add(new SelectData("21", "Уличком I квартал"));
                comboBox1.Items.Add(new SelectData("22", "Уличком II квартал"));
                comboBox1.Items.Add(new SelectData("23", "Уличком III квартал"));
                comboBox1.Items.Add(new SelectData("24", "Уличком IV квартал"));
                //comboBox1.Items.Add(new SelectData("20", "Чужие"));
                comboBox1.Text = "Выберите основание";
            }
            catch
            {
                MessageBox.Show("Невозможно соединиться с базой данных.\nОбратитесь в отдел АСУ.","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void frmKassa_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (db_con.State == ConnectionState.Open) db_con.Close();
            }
            catch { }
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            comboBox1.DroppedDown = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
                maskedTextBox1_KeyUp(this);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Size ns = new Size(10, 10);
            ns = panel4.Size; ns.Height = 0; 
            panel4.Size = ns;
            panel5.Size = ns;
            panel6.Size = ns;
            panel7.Size = ns;

            int var = 100; if (comboBox1.SelectedIndex >= 0) var = Convert.ToInt16(((SelectData)this.comboBox1.SelectedItem).Value);
            if (var == 100) comboBox1.Text = "Выберите основание";
            if (var < 10)
            {
                if (var == 1) panel4.Size = new Size(12, 100);
                if (var == 2) panel5.Size = new Size(12, 100);
                if (var == 3) panel6.Size = new Size(12, 100);
            }
            else if (var < 20)
            {
                if (var == 3) panel7.Size = new Size(12, 100);
            }
            else if (var < 30)
            {
            }
        }

        private void panel3_VisibleChanged(object sender, EventArgs e)
        {
            ///panel4 h = ...
            if (panel3.Visible == false)
            {
                comboBox1.SelectedIndex = -1;
                Size ns = new Size(10, 10);
                ns = panel4.Size; ns.Height = 0; panel4.Size = ns; panel5.Size = ns; panel6.Size = ns; panel7.Size = ns;
            }
            
        }

        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {
            maskedTextBox1.BorderStyle = BorderStyle.FixedSingle;
            maskedTextBox1.BackColor = Color.Yellow;
            maskedTextBox1.SelectionStart = maskedTextBox1.Text.Length;
        }

        private void maskedTextBox1_Leave(object sender, EventArgs e)
        {
            maskedTextBox1.BorderStyle = BorderStyle.None;
            maskedTextBox1.BackColor = SystemColors.Window;
        }

        private void maskedTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            {
                string cur = maskedTextBox1.Text.Replace("_", "").Trim();
                if (cur == "") { maskedTextBox1.SelectionStart = 0;}
                if (cur.Length < 10) { maskedTextBox1.Text = cur; }
                label2.Text = "";
                panel3.Visible = false;
            }
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            maskedTextBox1_KeyUp(this);
        }
    }
}

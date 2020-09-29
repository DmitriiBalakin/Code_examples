using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace water
{
    public partial class frmMoveLic : Form
    {
        SqlConnection con = new SqlConnection();

        public frmMoveLic()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e = null)
        {
            if (textBox1.Text.Trim().Length == 10)
            {
                label2.Text = "";
                try
                {
                    con.Open();
                    gv_lic.Rows.Clear();

                    if (Convert.ToInt64(textBox1.Text.Trim()) > 1)
                    {
                        SqlCommand com = new SqlCommand();
                        com.Connection = con;
                        com.CommandText = "select * from abonuk.dbo.movelic where lic=@lic";
                        com.Parameters.AddWithValue("@lic", textBox1.Text.Trim());
                        using (SqlDataReader r = com.ExecuteReader())
                        {
                            if (r.HasRows)
                            {
                                while (r.Read())
                                {
                                    string[] row = { "", "", "", "", "" };
                                    row[0] = r["id"].ToString();
                                    row[1] = r["per"].ToString();
                                    row[2] = r["old"].ToString();
                                    row[3] = r["new"].ToString();
                                    row[4] = r["FIO_uk"].ToString().Trim();
                                    gv_lic.Rows.Add(row);
                                }
                            }
                            else MessageBox.Show("Лицевой счет не найден", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    con.Close();
                }
                catch
                {
                    MessageBox.Show("Лицевой счет не найден", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (textBox1.Text.Trim().Length > 10) label2.Text = "Лицевой счет не может быть больше 10 знаков";
            else if (textBox1.Text.Trim().Length < 10) label2.Text = "";
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 10) textBox1_KeyUp(this);
        }
    }
}

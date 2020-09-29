using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace water
{

    public partial class frmCHouse : Form
    {
        SqlCommand db_com = new SqlCommand();
        bool can_close = true;

        public frmCHouse()
        {
            InitializeComponent();
            db_com.Connection = frmMain.db_con;
            db_com.CommandType = CommandType.Text;
            db_com.CommandTimeout = 0;
        }

        private void frmCHouse_Shown(object sender, EventArgs e)
        {
                        if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                    db_com.CommandText = "select cod_yl, yl_name from Abon.dbo.Street where cod_yl <> 'тмп' order by yl_name";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            while (db_read.Read())
                            {
                                string result = Convert.ToString(db_read["yl_name"]);
                                result = result.Trim();
                                //result = result + " " + Convert.ToString(db_read["SOCR"]);
                                //result = result.Trim();
                                this.cmb_street.Items.Add(new SelectData(Convert.ToString(db_read["cod_yl"]), result));
                            }
                            db_read.Close();
                            if (this.cmb_street.Items.Count > 0)
                            {
                                this.cmb_street.SelectedIndex = 0;
                            }
                        }
                    }

                    ///update_house(((SelectData)this.cmb_street.SelectedItem).Value);
                }
                catch
                {
                }
            }
            else
            {
                MessageBox.Show("Соединение с базой данных не установлено.\nПытаюсь восстановление подключение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    frmMain.db_con.Open();
                }
                catch
                {
                    MessageBox.Show("Не удалось восстановить подключение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_house.Items.Clear();
            cmb_house.Text = "";
            string street = ((SelectData)this.cmb_street.SelectedItem).Value;
            try
            {
                ///db_com.CommandText = "select S, lit from "+db_base+".dbo.streetuch where cod_yl = '" + street + "'";

                db_com.CommandText = "select dom, SUM(v) as ved from (" +
                    "select dom, 2 as v from Abonuk.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' group by dom " +
                    "union all " +
                    "select dom, 1 as v from Abon.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' group by dom" +
                    ") d group by dom order by dom";

                using (SqlDataReader db_read_house = db_com.ExecuteReader())
                {
                    if (db_read_house.HasRows)
                    {
                        while (db_read_house.Read())
                        {
                            string result = db_read_house["dom"].ToString().Trim();
                            cmb_house.Items.Add(new SelectData("", result, Convert.ToInt16(db_read_house["ved"].ToString())));
                        }
                        db_read_house.Close();
                        if (this.cmb_house.Items.Count > 0)
                        {
                            cmb_house.SelectedIndex = 0;
                        }
                    }

                }

            }
            catch (Exception error)
            {
                MessageBox.Show("Ошибка получения списка домов.\n" + error.Message, "Ошибка");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
            button1.Enabled = false;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 0;
            progressBar1.Value = 0;
            List<string> lic = new List<string>();
            DialogResult res;
            CalculateWater.MainForm Calc = new CalculateWater.MainForm();
            try
            {
                can_close = false;
                int uk = 0, g = 0;
                byte base_ = 0;
                db_com.CommandText = "select lic from abon.dbo.abonent" + frmMain.CurPer + " a inner join abon.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 0 and v.bpaketc = 1 where a.str_code='" + ((SelectData)cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "'" +
                    "union all select lic from abonuk.dbo.abonent" + frmMain.CurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 1 and v.bpaketc = 1 where a.str_code='" + ((SelectData)cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "'";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            lic.Add(db_read["lic"].ToString());
                            if (db_read["lic"].ToString().Substring(0, 1) == "1") g++;
                            if (db_read["lic"].ToString().Substring(0, 1) == "2") uk++;
                        }
                    }
                }
                res = MessageBox.Show("Выбрано " + lic.Count.ToString() + " л/с\n" + g.ToString() + " из Жилья\n" + uk.ToString() + " из Жилья УК\nНачислить?", "Начисление", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    progressBar1.Maximum = lic.Count;
                    for (int i = 0; i < lic.Count; i++)
                    {
                        if (lic.ElementAt(i).Substring(0, 1) == "1") base_ = 0;
                        if (lic.ElementAt(i).Substring(0, 1) == "2") base_ = 1;
                        Calc.Query(base_, frmMain.CurPer, lic.ElementAt(i).ToString());
                        progressBar1.Value++;
                        Application.DoEvents();
                    }
                    MessageBox.Show("Начисление выполнено!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    progressBar1.Value = 0;
                    progressBar1.Maximum = 0;
                }
                progressBar1.Visible = false;
                button1.Enabled = true;
                can_close = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar1.Visible = false;
                button1.Enabled = true;
                can_close = true;
            }
          }

        private void frmCHouse_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!can_close) { e.Cancel = true; MessageBox.Show("Дождитесь завершения расчета", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }


    }
}

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
    public partial class frmGorPer : Form
    {
        SqlConnection con = new SqlConnection();

        public frmGorPer()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch
            {
            }
        }

        private void frmGorPer_Shown(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            if (con.State == ConnectionState.Open)
            {
                try
                {
                    com.CommandText = "select v.str_code, s.yl_name, v.dom from (select a.str_code, a.dom from abon.dbo.abonent"+frmMain.MaxCurPer+" a inner join abon.dbo.oplata"+frmMain.MaxCurPer+" o on o.lic=a.lic where o.prim='пересчет канализации при отключении ГОР водоснабжения'"+
                        " union all "+
                        " select a.str_code, a.dom from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.oplata" + frmMain.MaxCurPer + " o on o.lic=a.lic where o.prim='пересчет канализации при отключении ГОР водоснабжения') v"+
                        " inner join abon.dbo.street s on s.cod_yl=v.str_code group by s.yl_name, v.dom, v.str_code";
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            string[] row = { "", "", "", "-", "-", "-", "true" };
                            while (r.Read())
                            {
                                row[0] = r["str_code"].ToString().Trim();
                                row[1] = r["yl_name"].ToString().Trim();
                                row[3] = r["dom"].ToString().Trim();
                                gv_gp.Rows.Add(row);
                            }
                        }
                    }

                    com.CommandText = "select cod_yl, yl_name from abon.dbo.Street where cod_yl <> 'тмп' order by yl_name";
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            while (r.Read())
                            {
                                string result = Convert.ToString(r["yl_name"]);
                                result = result.Trim();
                                //result = result + " " + Convert.ToString(db_read["SOCR"]);
                                //result = result.Trim();
                                cmbStreet.Items.Add(new SelectData(Convert.ToString(r["cod_yl"]), result));
                            }
                        }
                    }
                    if (this.cmbStreet.Items.Count > 0)
                    {
                        this.cmbStreet.SelectedIndex = 0;
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка получения списка улиц.\n", "Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }

        }

        private void cmbStreet_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmbStreet.DroppedDown = true;
        }

        private void cmbHouse_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmbHouse.DroppedDown = true;
        }

        private void cmbStreet_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            cmbHouse.Items.Clear();
            cmbHouse.Text = "";
            string street = ((SelectData)this.cmbStreet.SelectedItem).Value;
            try
            {
                ///db_com.CommandText = "select S, lit from "+db_base+".dbo.streetuch where cod_yl = '" + street + "'";

                com.CommandText = "select dom, SUM(v) as ved from (" +
                    "select dom, 2 as v from Abonuk.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' group by dom " +
                    "union all " +
                    "select dom, 1 as v from Abon.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' group by dom" +
                    ") d group by dom order by dom";

                using (SqlDataReader db_read_house = com.ExecuteReader())
                {
                    if (db_read_house.HasRows)
                    {
                        while (db_read_house.Read())
                        {
                            string result = db_read_house["dom"].ToString().Trim();
                            cmbHouse.Items.Add(new SelectData("", result, Convert.ToInt16(db_read_house["ved"].ToString())));
                        }
                    }

                }
                if (this.cmbHouse.Items.Count > 0)
                {
                    cmbHouse.SelectedIndex = 0;
                }

            }
            catch (Exception error)
            {
                MessageBox.Show("Ошибка получения списка домов.\n", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] row = { "", "", "", "", "", "", "" };
                row[0] = ((SelectData)this.cmbStreet.SelectedItem).Value.ToString();
                row[1] = ((SelectData)this.cmbStreet.SelectedItem).Text;
                row[2] = cmbHouse.Text.Trim();
                row[3] = Convert.ToDouble(maskedTextBox1.Text.Replace(" ","").Trim()).ToString();
                row[4] = Convert.ToDouble(maskedTextBox2.Text.Replace(" ", "").Trim()).ToString();
                row[5] = Convert.ToDouble(maskedTextBox3.Text.Replace(" ", "").Trim()).ToString();
                row[6] = "false";
                gv_gp.Rows.Add(row);
            }
            catch
            {
                MessageBox.Show("Ошибка при добавлении дома.\n", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = gv_gp.Rows.Count;
            progressBar1.Value = 0;
            for (int i = 0; i < gv_gp.Rows.Count; i++)
            {
                if (gv_gp.Rows[i].Cells["done"].Value != "true")
                {
                    try
                    {
                        List<string> lic = new List<string>();
                        com.CommandText = "select a.lic from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=0 and a.str_code=@str and a.dom=@dom and a.gruppa3=0" +
                            " union all " +
                            " select a.lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=1 and a.str_code=@str and a.dom=@dom and a.gruppa3=0";
                        com.Parameters.AddWithValue("@str", gv_gp.Rows[i].Cells["str"].Value);
                        com.Parameters.AddWithValue("@dom", gv_gp.Rows[i].Cells["house"].Value);
                        using (SqlDataReader r = com.ExecuteReader())
                        {
                            if (r.HasRows)
                            {
                                lic.Clear();
                                while (r.Read())
                                {
                                    lic.Add(r["lic"].ToString());
                                }
                            }
                        }
                        com.Parameters.Clear();
                        SqlTransaction tran;
                        tran = con.BeginTransaction("house");
                        com.Transaction = tran;
                        try
                        {
                            for (int ii = 0; ii < lic.Count; ii++)
                            {
                                int liver = 0;
                                double tarifv = 0;
                                double tarifk = 0;
                                string base_ = (lic[ii].ToString().Substring(0, 1) == "1" ? "Abon" : "Abonuk");
                                com.CommandText = @"select a.liver,st.vsum,st.ksum,st.psum from " + base_ + @".dbo.abonent" + frmMain.MaxCurPer + @" a inner join
" + base_ + @".dbo.street s  on a.str_code=s.cod_yl inner join
" + base_ + @".dbo.sptarif st on st.idtarif=s.tarifkod
where a.Str_code=@str and a.dom=@dom and a.lic=@lic and st.Per=@per";
                                com.Parameters.AddWithValue("@str", gv_gp.Rows[i].Cells["str"].Value);
                                com.Parameters.AddWithValue("@dom", gv_gp.Rows[i].Cells["house"].Value);
                                com.Parameters.AddWithValue("@Per", frmMain.MaxCurPer.ToString());
                                com.Parameters.AddWithValue("@lic", lic[ii]);
                                using (SqlDataReader r = com.ExecuteReader())
                                {
                                    if (r.HasRows)
                                    {
                                        r.Read();
                                        liver = Convert.ToInt32(r["liver"].ToString());
                                        tarifv = Convert.ToDouble(r["vsum"].ToString());
                                        tarifk = Convert.ToDouble(r["ksum"].ToString());
                                    }
                                }
                                com.Parameters.Clear();
                                DateTime dt = DateTime.Parse("01." + frmMain.MaxCurPer.Substring(4, 2) + "." + frmMain.MaxCurPer.Substring(0, 4));
                                double kv = 0, kk = 0, sv = 0, sk = 0;
                                kv = Math.Round((Convert.ToDouble(gv_gp.Rows[i].Cells["normav"].Value.ToString()) * liver * Convert.ToDouble(gv_gp.Rows[i].Cells["days"].Value.ToString())) / 30.4 * (-1), 3);
                                kk = Math.Round((Convert.ToDouble(gv_gp.Rows[i].Cells["normak"].Value.ToString()) * liver * Convert.ToDouble(gv_gp.Rows[i].Cells["days"].Value.ToString())) / 30.4 * (-1), 3);
                                sv = Math.Round(kv * tarifv, 2);
                                sk = Math.Round(kk * tarifk, 2);
                                com.CommandText = "insert into " + (lic[ii].ToString().Substring(0, 1) == "1" ? "Abon" : "Abonuk") + ".dbo.oplata" + frmMain.MaxCurPer + "([per],[perN],[lic],[liver],[lgot],[lgkan],[sotki],[kv],[kk],[kp],[oplata],[sodset],[cv],[ck],[cp],[BasisRecID],[Prim],[Sebestoim],[n_vl],[n_kl]) values('" + dt.ToString("MMMM yyyy") + "','" + frmMain.MaxCurPer + "','" + lic[ii] + "',0,0,0,0," + kv.ToString() + "," + kk.ToString() + ",0," + (sv + sk).ToString() + ",0," + sv + "," + sk.ToString() + ",0,1,'пересчет канализации при отключении ГОР водоснабжения',0,0,0) ";
                                com.ExecuteNonQuery();
                                com.Parameters.Clear();
                            }
                            tran.Commit();
                            gv_gp.Rows[i].Cells["done"].Value = "true";
                            Application.DoEvents();
                        }
                        catch
                        {
                            if (tran != null) tran.Rollback();
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    progressBar1.Value++;
                    Application.DoEvents();
                }
                progressBar1.Value = 0;
            }
        }
    }
}

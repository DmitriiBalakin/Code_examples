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
    public partial class frmBadUsluga : Form
    {
        SqlCommand db_com = new SqlCommand();
        public frmBadUsluga()
        {
            InitializeComponent();
            db_com.Connection = frmMain.db_con;
            maskedTextBox2.Text = "1";
            dateTimePicker1.MinDate = new DateTime(2012, 01, 01);
            dateTimePicker1.MaxDate = new DateTime(Convert.ToInt16(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt16(frmMain.MaxCurPer.Substring(4, 2)), 30);
        }

        private void frmBadUsluga_Shown(object sender, EventArgs e)
        {
            cmb_reason.Items.Add(new SelectData("0.1", "Давление в системе хол. водоснабжения"));
            cmb_reason.Items.Add(new SelectData("0.15", "Отсутствие водоснабжения в течении n часов"));
            cmb_reason.Items.Add(new SelectData("1", "Отклонение состава и свойств хол. воды"));
            cmb_reason.SelectedIndex = 0;
            cmb_client.Items.Add(new SelectData("1","Лицевой счет"));
            cmb_client.Items.Add(new SelectData("2", "Дом"));
            cmb_client.Items.Add(new SelectData("3", "Улица"));
            cmb_client.Items.Add(new SelectData("4", "Ведомство"));
            cmb_client.SelectedIndex = 0;
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
                                this.cmb_street_.Items.Add(new SelectData(Convert.ToString(db_read["cod_yl"]), result));
                            }
                            db_read.Close();
                            if (this.cmb_street.Items.Count > 0)
                            {
                                this.cmb_street.SelectedIndex = 0;
                                this.cmb_street_.SelectedIndex = 0;
                            }
                        }
                    }
                    db_com.CommandText = "select id, nameved from Abon.dbo.SpVedomstvo order by nameved";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            while (db_read.Read())
                            {
                                this.cmb_vedom.Items.Add(new SelectData(db_read["id"].ToString(), db_read["nameved"].ToString()));
                            }
                            db_read.Close();
                            if (this.cmb_vedom.Items.Count > 0)
                            {
                                this.cmb_vedom.SelectedIndex = 0;
                            }
                        }
                    }

                    gv_refresh();
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
        private void gv_refresh()
        {
            gv_nu.Rows.Clear();
            db_com.CommandText = "select * from abonuk.dbo.nachbu order by k";
            using (SqlDataReader db_read = db_com.ExecuteReader())
            {
                if (db_read.HasRows)
                {
                    while (db_read.Read())
                    {
                        string[] row = { "", "", "", "", "", "" };
                        row[0] = db_read["id"].ToString();
                        row[1] = db_read["k"].ToString();
                        row[2] = db_read["lic"].ToString();
                        row[3] = db_read["str_code"].ToString();
                        row[4] = db_read["dom"].ToString();
                        row[5] = db_read["kodvedom"].ToString();
                        this.gv_nu.Rows.Add(row);
                    }
                }
            }
            gv_nu.Refresh();
        }
        private void cmb_reason_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbl_k.Text = ((SelectData)cmb_reason.SelectedItem).Value;
        }

        private void cmb_client_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((SelectData)cmb_client.SelectedItem).Value == "1")
            {
                pnl_lic.Visible = true;
                pnl_house.Visible = false;
                pnl_street.Visible = false;
                pnl_vedom.Visible = false;
            }
            if (((SelectData)cmb_client.SelectedItem).Value == "2")
            {
                pnl_lic.Visible = false;
                pnl_house.Visible = true;
                pnl_street.Visible = false;
                pnl_vedom.Visible = false;
            }
            if (((SelectData)cmb_client.SelectedItem).Value == "3")
            {
                pnl_lic.Visible = false;
                pnl_house.Visible = false;
                pnl_street.Visible = true;
                pnl_vedom.Visible = false;
            }
            if (((SelectData)cmb_client.SelectedItem).Value == "4")
            {
                pnl_lic.Visible = false;
                pnl_house.Visible = false;
                pnl_street.Visible = false;
                pnl_vedom.Visible = true;
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
            if (cmb_reason.Text.Trim() != "" && maskedTextBox2.Text.Replace(" ", "").Trim() != "" && cmb_client.Text.Trim() != "" && dateTimePicker1.Value.Month == Convert.ToInt32(frmMain.MaxCurPer.Substring(4,2)))
            {
                double k = Convert.ToDouble(((SelectData)cmb_reason.SelectedItem).Value) * Convert.ToInt16(maskedTextBox2.Text.Replace(" ", ""));
                db_com.CommandText = "insert into abonuk.dbo.nachbu(per,k,$p) values(" + frmMain.MaxCurPer + "," + k.ToString() + ",$v)";
                if (((SelectData)cmb_client.SelectedItem).Value == "1")
                {
                    if (maskedTextBox1.Text.Length == 10)
                    {
                        db_com.CommandText = db_com.CommandText.Replace("$p", "lic");
                        db_com.CommandText = db_com.CommandText.Replace("$v", "'" + maskedTextBox1.Text + "'");
                    }
                    else MessageBox.Show("Проверьте лицевой счет", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (((SelectData)cmb_client.SelectedItem).Value == "2")
                {
                    db_com.CommandText = db_com.CommandText.Replace("$p", "str_code,dom");
                    db_com.CommandText = db_com.CommandText.Replace("$v", "'" + ((SelectData)cmb_street.SelectedItem).Value + "','" + ((SelectData)cmb_house.SelectedItem).Text + "'");
                }
                else if (((SelectData)cmb_client.SelectedItem).Value == "3")
                {
                    db_com.CommandText = db_com.CommandText.Replace("$p", "str_code");
                    db_com.CommandText = db_com.CommandText.Replace("$v", "'" + ((SelectData)cmb_street.SelectedItem).Value + "'");
                }
                else if (((SelectData)cmb_client.SelectedItem).Value == "4")
                {
                    db_com.CommandText = db_com.CommandText.Replace("$p", "kodvedom");
                    db_com.CommandText = db_com.CommandText.Replace("$v", "'" + ((SelectData)cmb_vedom.SelectedItem).Value + "'");
                }
                if (db_com.CommandText.IndexOf("$") < 0)
                {
                    db_com.ExecuteNonQuery();
                }
                gv_refresh();
            }
            if (cmb_reason.Text.Trim() != "" && maskedTextBox2.Text.Replace(" ", "").Trim() != "" && cmb_client.Text.Trim() != "" && (dateTimePicker1.Value.Month < Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)) && dateTimePicker1.Value.Year == Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)) || (dateTimePicker1.Value.Month <= Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)) && dateTimePicker1.Value.Year < Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)))))
            {
                if (((SelectData)cmb_client.SelectedItem).Value == "1")
                {
                    ///lic
                    oplata_lic(maskedTextBox1.Text,Convert.ToString(Convert.ToDouble(((SelectData)cmb_reason.SelectedItem).Value) * Convert.ToInt16(maskedTextBox2.Text.Replace(" ", ""))),null, dateTimePicker1.Value.Year.ToString()+((dateTimePicker1.Value.Month.ToString().Length > 1)?dateTimePicker1.Value.Month.ToString():"0"+dateTimePicker1.Value.Month.ToString()));
                }
                if (((SelectData)cmb_client.SelectedItem).Value == "2")
                {
                    ///house
                    List<string> lic = new List<string>();
                    string per = dateTimePicker1.Value.Year.ToString()+((dateTimePicker1.Value.Month.ToString().Length > 1)?dateTimePicker1.Value.Month.ToString():"0"+dateTimePicker1.Value.Month.ToString());
                    db_com.CommandText = "select a.lic from abon.dbo.abonent" + per + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 where a.str_code=@str and a.dom=@dom union all select a.lic from abonuk.dbo.abonent" + per + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 where a.str_code=@str and a.dom=@dom";
                    db_com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                    db_com.Parameters.AddWithValue("@dom", cmb_house.Text);
                    using (SqlDataReader read = db_com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            while (read.Read())
                            {
                                lic.Add(read["lic"].ToString());
                            }
                        }
                    }
                    db_com.Parameters.Clear();
                    progressBar1.Maximum = lic.Count;
                    progressBar1.Value = 0;
                    for (int i = 0; i < lic.Count; i++)
                    {
                        oplata_lic(lic.ElementAt(i), Convert.ToString(Convert.ToDouble(((SelectData)cmb_reason.SelectedItem).Value) * Convert.ToInt16(maskedTextBox2.Text.Replace(" ", ""))), null, dateTimePicker1.Value.Year.ToString() + ((dateTimePicker1.Value.Month.ToString().Length > 1) ? dateTimePicker1.Value.Month.ToString() : "0" + dateTimePicker1.Value.Month.ToString()));
                        progressBar1.Value++;
                    }
                    progressBar1.Value = 0;
                }
                if (((SelectData)cmb_client.SelectedItem).Value == "3")
                {
                    ///street
                    List<string> lic = new List<string>();
                    string per = dateTimePicker1.Value.Year.ToString() + ((dateTimePicker1.Value.Month.ToString().Length > 1) ? dateTimePicker1.Value.Month.ToString() : "0" + dateTimePicker1.Value.Month.ToString());
                    db_com.CommandText = "select a.lic from abon.dbo.abonent" + per + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 where a.str_code=@str union all select a.lic from abonuk.dbo.abonent" + per + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 where a.str_code=@str";
                    db_com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                    using (SqlDataReader read = db_com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            while (read.Read())
                            {
                                lic.Add(read["lic"].ToString());
                            }
                        }
                    }
                    db_com.Parameters.Clear();
                    progressBar1.Maximum = lic.Count;
                    progressBar1.Value = 0;
                    for (int i = 0; i < lic.Count; i++)
                    {
                        oplata_lic(lic.ElementAt(i), Convert.ToString(Convert.ToDouble(((SelectData)cmb_reason.SelectedItem).Value) * Convert.ToInt16(maskedTextBox2.Text.Replace(" ", ""))), null, dateTimePicker1.Value.Year.ToString() + ((dateTimePicker1.Value.Month.ToString().Length > 1) ? dateTimePicker1.Value.Month.ToString() : "0" + dateTimePicker1.Value.Month.ToString()));
                        progressBar1.Value++;
                    }
                    progressBar1.Value = 0;
                }
                if (((SelectData)cmb_client.SelectedItem).Value == "4")
                {
                    ///vedom
                    List<string> lic = new List<string>();
                    string per = dateTimePicker1.Value.Year.ToString() + ((dateTimePicker1.Value.Month.ToString().Length > 1) ? dateTimePicker1.Value.Month.ToString() : "0" + dateTimePicker1.Value.Month.ToString());
                    db_com.CommandText = "select a.lic from abon.dbo.abonent" + per + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 where a.kodvedom=@str and a.dom=@dom union all select a.lic from abonuk.dbo.abonent" + per + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 where a.kodvedom=@str";
                    db_com.Parameters.AddWithValue("@str", ((SelectData)cmb_vedom.SelectedItem).Value);
                    using (SqlDataReader read = db_com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            while (read.Read())
                            {
                                lic.Add(read["lic"].ToString());
                            }
                        }
                    }
                    db_com.Parameters.Clear();
                    progressBar1.Maximum = lic.Count;
                    progressBar1.Value = 0;
                    for (int i = 0; i < lic.Count; i++)
                    {
                        oplata_lic(lic.ElementAt(i), Convert.ToString(Convert.ToDouble(((SelectData)cmb_reason.SelectedItem).Value) * Convert.ToInt16(maskedTextBox2.Text.Replace(" ", ""))), null, dateTimePicker1.Value.Year.ToString() + ((dateTimePicker1.Value.Month.ToString().Length > 1) ? dateTimePicker1.Value.Month.ToString() : "0" + dateTimePicker1.Value.Month.ToString()));
                        progressBar1.Value++;
                    }
                    progressBar1.Value = 0;
                }
            }
        }

        public int oplata_lic(string lic,string k, SqlCommand com = null, string MaxCurPer_ = "")
            {
                int result = 0;
            frmKart find = new frmKart();
            MaxCurPer_ = (MaxCurPer_ == "") ? frmMain.MaxCurPer : MaxCurPer_;
            /// пересчет для lic
                if (find.find_lic_base(lic) == 3) { MessageBox.Show("Л/с "+lic+" активен в двух базах.","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error); }
                else if (find.find_lic_base(lic) == 1)
                {
                    try
                    {
                        double stornov = 0, stornok = 0, vstornov = 0, vstornok =0;
                        db_com.CommandText = "select nvfull,nkfull,cubev,cubek from abon.dbo.abonent" + MaxCurPer_ + " where lic='" + lic + "'";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                db_read.Read();
                                stornov = Math.Round((Convert.ToDouble(db_read["nvfull"].ToString()) / 30.4) * -1 * Convert.ToDouble(k),2);
                                stornok = Math.Round((Convert.ToDouble(db_read["nkfull"].ToString()) / 30.4) * -1 *Convert.ToDouble(k),2);
                                vstornov = Math.Round((Convert.ToDouble(db_read["cubev"].ToString()) / 30.4) * -1 * Convert.ToDouble(k),3);
                                vstornok = Math.Round((Convert.ToDouble(db_read["cubek"].ToString()) / 30.4) * -1 * Convert.ToDouble(k),3);
                            }
                        }
                        if ((stornov + stornok) != 0)
                        {
                            db_com.CommandText = "update abon.dbo.abonent" + frmMain.MaxCurPer + " set oplata=" + (stornov + stornok).ToString() + " where lic='" + lic + "'";
                            db_com.ExecuteNonQuery();
                            DateTime dt = new DateTime(Convert.ToInt32(MaxCurPer_.Substring(0, 4)), Convert.ToInt32(MaxCurPer_.Substring(4, 2)), DateTime.DaysInMonth(Convert.ToInt32(MaxCurPer_.Substring(0, 4)), Convert.ToInt32(MaxCurPer_.Substring(4, 2))));
                            db_com.CommandText = "insert into abon.dbo.oplata" + frmMain.MaxCurPer + "([per],[perN],[lic],[liver],[lgot],[lgkan],[sotki],[kv],[kk],[kp],[oplata],[sodset],[cv],[ck],[cp],[BasisRecID],[Prim],[Sebestoim],[n_vl],[n_kl]) values('" + dt.ToString("MMMM yyyy") + "','" + MaxCurPer_ + "','" + lic + "',0,0,0,0," + vstornov + "," + vstornok + ",0," + (stornov + stornok).ToString() + ",0," + stornov.ToString() + "," + stornok.ToString() + ",0,1,'пересчет за некачественную услугу',0,0,0) ";
                            db_com.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        result=1;
                    }
                }
                else if (find.find_lic_base(lic) == 2)
                {
                    try
                    {
                        double stornov = 0, stornok = 0, vstornov = 0, vstornok = 0;
                        db_com.CommandText = "select nvfull,nkfull,cubev,cubek from abonuk.dbo.abonent" + MaxCurPer_ + " where lic='" + lic + "'";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                db_read.Read();
                                stornov = Math.Round((Convert.ToDouble(db_read["nvfull"].ToString()) / 30.4) * -1 * Convert.ToDouble(k), 2);
                                stornok = Math.Round((Convert.ToDouble(db_read["nkfull"].ToString()) / 30.4) * -1 * Convert.ToDouble(k), 2);
                                vstornov = Math.Round((Convert.ToDouble(db_read["cubev"].ToString()) / 30.4) * -1 * Convert.ToDouble(k), 3);
                                vstornok = Math.Round((Convert.ToDouble(db_read["cubek"].ToString()) / 30.4) * -1 * Convert.ToDouble(k), 3);
                            }
                        }
                        db_com.CommandText = "update abonuk.dbo.abonent" + frmMain.MaxCurPer + " set oplata=" + (stornov + stornok).ToString() + " where lic='" + lic + "'";
                        db_com.ExecuteNonQuery();
                        DateTime dt = new DateTime(Convert.ToInt32(MaxCurPer_.Substring(0, 4)), Convert.ToInt32(MaxCurPer_.Substring(4, 2)), DateTime.DaysInMonth(Convert.ToInt32(MaxCurPer_.Substring(0, 4)), Convert.ToInt32(MaxCurPer_.Substring(4, 2))));
                        db_com.CommandText = "insert into abonuk.dbo.oplata" + frmMain.MaxCurPer + "([per],[perN],[lic],[liver],[lgot],[lgkan],[sotki],[kv],[kk],[kp],[oplata],[sodset],[cv],[ck],[cp],[BasisRecID],[Prim],[Sebestoim],[n_vl],[n_kl]) values('" + dt.ToString("MMMM yyyy") + "','" + MaxCurPer_ + "','" + lic + "',0,0,0,0," + vstornov + "," + vstornok + ",0," + (stornov + stornok).ToString() + ",0," + stornov.ToString() + "," + stornok.ToString() + ",0,1,'пересчет за некачественную услугу',0,0,0) ";
                        db_com.ExecuteNonQuery();
                    }
                    catch
                    {
                        result=1;
                    }
                }
                return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            con.Open();
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            com.CommandType = CommandType.Text;
            com.CommandTimeout = 0;
            SqlTransaction trans = null;
            frmKart find = new frmKart();
            ///string db_base = "";
            string MaxCurPer_ = "201507";
            /// пересчет для lic
            lbl_op.Text = "Перерасчет лицевых счетов";
            List<string> id = new List<string>();
            List<string> lic = new List<string>();
            List<string> k = new List<string>();
            List<string> str = new List<string>();
            List<string> dom = new List<string>();
            com.CommandText = "select * from abonuk.dbo.nachbu where lic is not null and per="+MaxCurPer_+" and done=0";
            using (SqlDataReader db_read = com.ExecuteReader())
            {
                if (db_read.HasRows)
                {
                    while (db_read.Read())
                    {
                        id.Add(db_read["id"].ToString());
                        lic.Add(db_read["lic"].ToString());
                        k.Add(db_read["k"].ToString());
                    }
                }
            }
            
            try
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = lic.Count;
                progressBar1.Value = 0;
                for (int i = 0; i < lic.Count; i++) {
                    trans = con.BeginTransaction();
                    com.Transaction = trans;
                    if (oplata_lic(lic.ElementAt(i), k.ElementAt(i), com) != 0) { trans.Rollback(); }
                    else
                    {
                        com.CommandText = "update abonuk.dbo.nachbu set done=1 where id = " + id.ElementAt(i);
                        com.ExecuteNonQuery();
                        trans.Commit();
                        progressBar1.Value++;
                        Application.DoEvents();
                    }
                }
            }
            catch
            {
                if (trans.Connection != null) trans.Rollback();
            }

            ////дом
            lbl_op.Text = "Перерасчет домов";
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 0;
            progressBar1.Value = 0;
            id.Clear();
            k.Clear();
            lic.Clear();
            List<string> k_ = new List<string>();
            com.CommandText = "select * from abonuk.dbo.nachbu where lic is null and kodvedom is null and str_code is not null and dom is not null and done = 0 and per='" + MaxCurPer_ + "'";
            using (SqlDataReader db_read = com.ExecuteReader())
            {
                if (db_read.HasRows)
                {
                    db_read.Read();
                    id.Add(db_read["id"].ToString());
                    k.Add(db_read["k"].ToString());
                    str.Add(db_read["str_code"].ToString());
                    dom.Add(db_read["dom"].ToString());
                }
            }

            progressBar1.Maximum = id.Count;
            for (int i = 0; i<id.Count;i++)
            {
                
            com.CommandText = "select lic from abon.dbo.abonent" + frmMain.CurPer + " a inner join abon.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 0 and v.bpaketc = 1 where a.str_code='" + str.ElementAt(i) + "' and dom='" + dom.ElementAt(i) + "'" +
                "union all select lic from abonuk.dbo.abonent" + frmMain.CurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 1 and v.bpaketc = 1 where a.str_code='" + str.ElementAt(i) + "' and dom='" + dom.ElementAt(i) + "'";
            using (SqlDataReader db_read = com.ExecuteReader())
            {
                if (db_read.HasRows)
                {
                    while (db_read.Read())
                    {
                        lic.Add(db_read["lic"].ToString());
                        k_.Add(k.ElementAt(i));
                    }
                }
            }

                trans = con.BeginTransaction();
                com.Transaction = trans;
                int error = 0;
                try
                {
                    string err_lic = "";
                    for (int ii = 0; ii < lic.Count; ii++) 
                        if (oplata_lic(lic.ElementAt(ii), k_.ElementAt(ii), com) != 0) 
                        { error = 1; err_lic = lic.ElementAt(ii);  break; } else Application.DoEvents();
                    if (error == 0)
                    {
                        com.CommandText = "update abonuk.dbo.nachbu set done='True' where id=" + id.ElementAt(i);
                        db_com.ExecuteNonQuery();
                        trans.Commit();
                    }
                    else { trans.Rollback(); MessageBox.Show("Cбой при пересчете л/с "+err_lic,"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error); }
                }
                catch
                {
                    if (trans.Connection != null) trans.Rollback();
                    break;
                }
                lic.Clear();
                k_.Clear();
                progressBar1.Value++; Application.DoEvents(); 

           }





            

            ////ведомство
            lbl_op.Text = "Перерасчет ведомств";
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            id.Clear();
            k.Clear();
            lic.Clear();
            List<string> base_ = new List<string>();
            List<string> id_ = new List<string>();
            k_.Clear();
            List<string> vedom_ = new List<string>();
            com.CommandText = "select * from abonuk.dbo.nachbu where lic is null and kodvedom is not null and str_code is null and dom is null and lic is null and done = 0 and per='" + MaxCurPer_ + "'";
            using (SqlDataReader db_read = com.ExecuteReader())
            {
                if (db_read.HasRows)
                {
                    db_read.Read();
                    id_.Add(db_read["id"].ToString());
                    if (db_read["base"].ToString() == "True") base_.Add("AbonUk"); else base_.Add("Abon");
                    k_.Add(db_read["k"].ToString());
                    vedom_.Add(db_read["kodvedom"].ToString());
                }
            }

            for (int i = 0; i < id_.Count; i++)
            {
                com.CommandText = "select lic from "+base_.ElementAt(i)+".dbo.abonent"+MaxCurPer_+" where kodvedom="+vedom_.ElementAt(i)+" and gruppa3=0";
                using (SqlDataReader db_read = com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            lic.Add(db_read["lic"].ToString());
                            k.Add(k_.ElementAt(i));
                        }
                    }
                }
                progressBar1.Maximum = lic.Count;
                trans = con.BeginTransaction();
                com.Transaction = trans;
                int error = 0;
                try
                {
                    string err_lic = "";
                    for (int ii = 0; ii < lic.Count; ii++) if (oplata_lic(lic.ElementAt(ii), k.ElementAt(ii), com) != 0) { error = 1; err_lic = lic.ElementAt(ii);  break; } else { progressBar1.Value++; Application.DoEvents(); }
                    if (error == 0)
                    {
                        com.CommandText = "update abonuk.dbo.nachbu set done=1 where id=" + id_.ElementAt(i);
                        db_com.ExecuteNonQuery();
                        trans.Commit();
                    }
                    else { trans.Rollback(); MessageBox.Show("Cбой при пересчете л/с "+err_lic,"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error); }
                }
                catch
                {
                    if (trans.Connection != null) trans.Rollback();
                }

            }
            con.Close();
            lbl_op.Text = "";
            progressBar1.Value = 0;
        }


////        select a.lic from (
////SELECT     COUNT(lic) AS cnt, lic
////FROM         oplata201507
////WHERE     (BasisRecID = 1) AND ([user_name] = 'ASU-LEBEDEV')
////GROUP BY lic)a where a.cnt>=2

////delete top(1) from oplata201507 where lic in (select a.lic from (
////SELECT     COUNT(lic) AS cnt, lic
////FROM         oplata201507
////WHERE     (BasisRecID = 1) AND ([user_name] = 'ASU-LEBEDEV')
////GROUP BY lic)a where a.cnt>=2) and BasisRecID=1 and [user_name] = 'ASU-LEBEDEV'

    }
}

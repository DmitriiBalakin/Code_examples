using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace water
{
    public partial class frmKart : Form
    {
        SqlCommand db_com = new SqlCommand();
        SqlConnection con = new SqlConnection();
        public string db_base = "Abon";
        Boolean flag_change_base = false;
        CalculateWater.MainForm Calc = new CalculateWater.MainForm();


        public frmKart()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch { }
            db_com.Connection = con;
            db_com.CommandTimeout = 0;
        }

        private Boolean search_lic = false;

        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {

        }

        private void frmKart_Shown(object sender, EventArgs e)
        {
            
            
            /// заполнение улиц
            if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                    db_com.CommandText = "select cod_yl, yl_name from "+db_base+".dbo.Street where cod_yl <> 'тмп' order by yl_name";
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
                MessageBox.Show("Соединение с базой данных не установлено.\nПытаюсь восстановление подключение", "Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
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



        private void cmb_street_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_street.DroppedDown = true;
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!search_lic)
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
                                cmb_house.Items.Add(new SelectData("", result,Convert.ToInt16(db_read_house["ved"].ToString())));
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
        }

        private void cmb_house_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_house.DroppedDown = true;
        }

        private void cmb_house_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!search_lic)
            {
                cmb_flat.Items.Clear();
                cmb_flat.Text = "";
                cmb_flat.SelectedIndex = -1;
                string street = ((SelectData)this.cmb_street.SelectedItem).Value;
                int Ved = ((SelectData)this.cmb_house.SelectedItem).Ved;
                if (Ved == 1) db_base = "Abon";
                if (Ved == 2) db_base = "AbonUk";
                if (Ved == 3)
                {
                    try
                    {
                        db_com.CommandText = "select UK = case (v.bUk) when '1' then '1' when '0' then '0' end from " + db_base + ".dbo.abonent" + frmMain.CurPer +
                " a inner join " + db_base + ".dbo.spvedomstvo v on a.kodvedom=v.id where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Items[cmb_house.SelectedIndex] + "' and a.flat='" + cmb_flat.Text + "'";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                db_read.Read();
                                if (db_read["UK"].ToString() == "1") db_base = "AbonUk";
                                if (db_read["UK"].ToString() == "0") db_base = "Abon";
                            }
                        }
                    }
                    catch { }
                }
                try
                {
                    db_com.CommandText = "select lic,flat,flat, fam, kontr from "+db_base+".dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' and dom='" + cmb_house.Items[cmb_house.SelectedIndex] + "' order by flat_int";

                    using (SqlDataReader db_read_house = db_com.ExecuteReader())
                    {
                        if (db_read_house.HasRows)
                        {
                            while (db_read_house.Read())
                            {
                                string result = db_read_house["flat"].ToString().Trim() +" | "+db_read_house["fam"].ToString().Trim();
                                cmb_flat.Items.Add(new SelectData(db_read_house["lic"].ToString(), result));
                            }
                            db_read_house.Close();
                            ///if (this.cmb_flat.Items.Count > 0)
                            ///{
                            ///    cmb_flat.SelectedIndex = 0;
                            ///}
                        }
                        else
                        {
                            tab.Visible = false;
                            txtlic.Text = "";
                            lbl_nachisl.Text = "0.00";
                            panel3.Enabled = false;
                        }

                    }

                }
                catch (Exception error)
                {
                    MessageBox.Show("Ошибка получения списка квартир.\n" + error.Message, "Ошибка");
                }
            }
        }

        private void cmb_flat_SelectedIndexChanged(object sender, EventArgs e)
        {   DateTime time;
        if (cmb_flat.Text.Length > 0)
        {
            tab.Enabled = false;
            Application.DoEvents();
            string lic = "";
            if (db_base == "AbonUk") lic = "2" + ((SelectData)cmb_flat.SelectedItem).Value.Substring(1, 9);
            if (db_base == "Abon") lic = "1" + ((SelectData)cmb_flat.SelectedItem).Value.Substring(1, 9);
            db_com.CommandText = "select a.*, aa.*, UK = case (v.bUk) when '1' then '1' when '0' then '0' end from " + db_base + ".dbo.abonent" + frmMain.CurPer +
                " a inner join " + db_base + ".dbo.spvedomstvo v on a.kodvedom=v.id " +
                "inner join " + db_base + ".dbo.abon aa on a.lic = aa.lic" +
                " where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Items[cmb_house.SelectedIndex] + "' and a.lic = '"+lic+"' and aa.perend=0";
            if (!flag_change_base)
            {
                try
                {
                    using (SqlDataReader db_reader = db_com.ExecuteReader())
                    {
                        if (db_reader.HasRows)
                        {
                            db_reader.Read();
                            if (db_reader["uk"].ToString().Trim() == "1") { lbl_base.Text = "УК"; lbl_obase.Text = "Жилье УК"; db_base = "AbonUk"; } else { lbl_base.Text = "Жилье"; lbl_obase.Text = "Жилье"; db_base = "Abon"; }
                        }
                    }
                }
                catch { }
            }
            if (db_base == "AbonUk") lbl_obase.Text = "Жилье УК"; else if (db_base == "Abon") lbl_obase.Text = "Жилье";
            db_com.CommandText = "select a.*, aa.*, UK = case (v.bUk) when '1' then '1' when '0' then '0' end from " + db_base + ".dbo.abonent" + frmMain.CurPer +
                " a inner join " + db_base + ".dbo.spvedomstvo v on a.kodvedom=v.id " +
                "inner join " + db_base + ".dbo.abon aa on a.lic = aa.lic" +
                " where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Items[cmb_house.SelectedIndex] + "' and a.lic='"+lic+"' and aa.perend=0";
            try
            {
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        ///Calc.Query((byte)(db_base == "Abon" ? 0 : 1), frmMain.MaxCurPer, frmMain.db_con, txtlic.Text);
                        db_reader.Read();
                        this.tab.Visible = true;
                        panel3.Enabled = true;

                        if (db_reader["uk"].ToString().Trim() == "1") lbl_base.Text = "УК"; else lbl_base.Text = "Жилье";
                        lbl_fio.Text = Convert.ToString(db_reader["Fam"]);
                        txtlic.Text = Convert.ToString(db_reader["lic"]);
                        lbl_live.Value = Convert.ToInt16(db_reader["liver"]);
                        lbl_liver.Text = lbl_live.Value.ToString();
                        lbl_nachisl.Text = Convert.ToString(db_reader["nachisl"]);
                        lbl_phone.Text = Convert.ToString(db_reader["numtel"]);
                        lbl_email.Text = db_reader["email"].ToString();
                        lbl_vedomstvo.Text = db_reader["vedom"].ToString();
                        if (db_reader["isemail"].ToString() == "True") send_email.CheckState = CheckState.Checked; else send_email.CheckState = CheckState.Unchecked;
                        if (db_reader["gruppa3"].ToString() == "True") { group3.CheckState = CheckState.Checked; lbl_nachisl.Text = "не начисляем"; } else group3.CheckState = CheckState.Unchecked;
                        if (db_reader["prochee"].ToString() == "True") lbl_print.CheckState = CheckState.Checked; else lbl_print.CheckState = CheckState.Unchecked;

                        if (db_reader["Vvodomer"].ToString() == "True") { btn_vvodomer.BackColor = Color.PaleGreen; btn_vvodomer.Text = "ВОДОМЕР"; } else { btn_vvodomer.BackColor = Color.DimGray; btn_vvodomer.Text = "ВОДОМЕР"; }
                        if (db_reader["vodkod"].ToString() != "0") { btn_vvodomer.BackColor = Color.PaleGreen; btn_vvodomer.Text = "ОБЩИЙ\nСЧЕТЧИК"; }
                        
                        if (db_reader["Pvodomer"].ToString() == "True") btn_pvodomer.BackColor = Color.PaleGreen; else btn_pvodomer.BackColor = Color.DimGray;
                        if (db_reader["SecHome"].ToString() == "True") btn_sechome.BackColor = Color.PaleGreen; else btn_sechome.BackColor = Color.DimGray;
                        if (db_reader["skvagina"].ToString() == "True") btn_skvagina.BackColor = Color.PaleGreen; else btn_skvagina.BackColor = Color.DimGray;
                        if (db_reader["skvpol"].ToString() == "True") btn_skvpol.BackColor = Color.PaleGreen; else btn_skvpol.BackColor = Color.DimGray;
                        if (db_reader["sud"].ToString() == "True") btn_sud.BackColor = Color.PaleGreen; else btn_sud.BackColor = Color.DimGray;
                        if ((db_reader["lgkan"].ToString() == "1") || (db_reader["lgkan"].ToString() == "2")) btn_lgkan.BackColor = Color.PaleGreen; else btn_lgkan.BackColor = Color.DimGray;
                        if (db_reader["DateOfBirth"].ToString() != "")
                        {
                            time = Convert.ToDateTime(db_reader["DateOfBirth"].ToString());
                            lbl_birthday.Text = time.ToString("dd.MM.yyyy");
                        }
                        else lbl_birthday.Text = "";
                        if (db_reader["dtnotlive"].ToString() != "")
                        {
                            time = Convert.ToDateTime(db_reader["dtnotlive"].ToString());
                            lbl_not_live.Text = time.ToString("dd.MM.yyyy");
                        }
                        else lbl_not_live.Text = "";
                        //// ODN


                        ///очистка таблицы
                        dg_liver.Rows.Clear();
                        if (dg_liver.Rows.Count > 0)
                        {
                            int i = 0;
                            int ii = dg_liver.Rows.Count;
                            while (i < ii)
                            {
                                dg_liver.Rows.RemoveAt(i);
                                i++;
                            }
                            dg_liver.Refresh();
                        }
                        if (db_reader["vibilo"].ToString() != "0")
                        {
                            string[] row = new string[] { db_reader["lic"].ToString(), db_reader["vibilo"].ToString(), "", db_reader["vibilodata"].ToString().Substring(0,db_reader["vibilodata"].ToString().IndexOf(" ")), "..." };
                            //row = new string[] { " " };
                            dg_liver.Rows.Add(row);
                            if (Convert.ToInt16(db_reader["vibilo"].ToString()) > 0) dg_liver.Rows[dg_liver.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                            if (Convert.ToInt16(db_reader["vibilo"].ToString()) < 0) dg_liver.Rows[dg_liver.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                        if (db_reader["vibilo2"].ToString() != "0")
                        {
                            string[] row = new string[] { db_reader["lic"].ToString(), db_reader["vibilo2"].ToString(), "", db_reader["vibilodata2"].ToString().Substring(0, db_reader["vibilodata2"].ToString().IndexOf(" ")), "..." };
                            dg_liver.Rows.Add(row);
                            if (Convert.ToInt16(db_reader["vibilo2"].ToString()) > 0) dg_liver.Rows[dg_liver.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                            if (Convert.ToInt16(db_reader["vibilo2"].ToString()) < 0) dg_liver.Rows[dg_liver.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }
                    db_reader.Close();

                }
                db_com.CommandText = "select * from Common.dbo.HousesData h " +
                    "inner join " +
                    "(select id_house, (cast(numhouse as nvarchar)+LitHouse) as dom from Common.dbo.SpHouses where Street_Code in " +
                    "(select id_street as Street_Code from common.dbo.SpStreets where Code_Yl = '" + ((SelectData)this.cmb_street.SelectedItem).Value + "')) a on a.Id_House = h.House_Code and a.dom = '" + cmb_house.Text.ToString() + "' " +
                    "where PerEnd = 0 ";
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        db_reader.Read();
                        if (db_reader["iscalcodn"].ToString() == "True")
                        {
                            btn_odn.BackColor = Color.PaleGreen;
                            ///btn_odn.Text = btn_odn.Text+"\n"+db_reader["house_code"].ToString();
                        }
                        else btn_odn.BackColor = Color.DimGray;
                        db_reader.Close();
                    }
                    else db_reader.Close();
                }
                //// чтение водомеров
                db_com.CommandText = "exec " + db_base + ".dbo.getvodomerviewnew '" + txtlic.Text + "'";
                ///db_com.CommandText = "select p.modirec,p.kubh1n,p.kubh2n,p.kubh3n,p.kubg1n,p.kubg2n,p.kubg3n,p.vnv,p.vnk,p.prim_code, d.h1,d.h2,d.h3,d.g1,d.g2,d.g3 from " + db_base + ".dbo.posvod p inner join " + db_base + ".dbo.vodomerdate" + frmMain.MaxCurPer + " d on d.lic=p.lic where p.lic='" + txtlic.Text + "' order by p.modirec";
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    dv_vodomer.Rows.Clear();
                    string[] row1 = { " ", "", "", "", "", "", "", " ", " ", " ", " " };
                    while (db_reader.Read())
                    {
                        string[] row = { "", "", "", "", "", "", "", "", "", "", "" };
                        row[0] = db_reader["Modirec"].ToString().Substring(0, db_reader["modirec"].ToString().IndexOf(" "));
                        row[1] = db_reader["kubh1n"].ToString();
                        row[2] = db_reader["kubh2n"].ToString();
                        row[3] = db_reader["kubh3n"].ToString();
                        row[4] = db_reader["kubg1n"].ToString();
                        row[5] = db_reader["kubg2n"].ToString();
                        row[6] = db_reader["kubg3n"].ToString();
                        row[7] = "-";
                        row[8] = db_reader["kubv"].ToString();
                        row[9] = db_reader["kubk"].ToString();
                        row[10] = db_reader["prim"].ToString();
                        dv_vodomer.Rows.Add(row);
                        dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightYellow;
                        if (db_reader["h1"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X1"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h1"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X1"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h1"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X1"].Style.BackColor = Color.LightGreen;
                        if (db_reader["h2"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X2"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h2"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X2"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h2"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X2"].Style.BackColor = Color.LightGreen;
                        if (db_reader["h3"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X3"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h3"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X3"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["h3"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["X3"].Style.BackColor = Color.LightGreen;
                        if (db_reader["g1"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G1"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g1"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G1"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g1"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G1"].Style.BackColor = Color.LightGreen;
                        if (db_reader["g2"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G2"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g2"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G2"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g2"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G2"].Style.BackColor = Color.LightGreen;
                        if (db_reader["g3"].ToString() == "") dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G3"].Style.BackColor = Color.Gray;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g3"].ToString()), DateTime.Now) < 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G3"].Style.BackColor = Color.Red;
                        else if (DateTime.Compare(Convert.ToDateTime(db_reader["g3"].ToString()), DateTime.Now) > 0) dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells["G3"].Style.BackColor = Color.LightGreen;
                        if (db_reader["h1"].ToString().Length > 1) row1[1] = db_reader["h1"].ToString().Substring(0, db_reader["h1"].ToString().IndexOf(" "));
                        if (db_reader["h2"].ToString().Length > 1) row1[2] = db_reader["h2"].ToString().Substring(0, db_reader["h2"].ToString().IndexOf(" "));
                        if (db_reader["h3"].ToString().Length > 1) row1[3] = db_reader["h3"].ToString().Substring(0, db_reader["h3"].ToString().IndexOf(" "));
                        if (db_reader["g1"].ToString().Length > 1) row1[4] = db_reader["g1"].ToString().Substring(0, db_reader["g1"].ToString().IndexOf(" "));
                        if (db_reader["g2"].ToString().Length > 1) row1[5] = db_reader["g2"].ToString().Substring(0, db_reader["g2"].ToString().IndexOf(" "));
                        if (db_reader["g3"].ToString().Length > 1) row1[6] = db_reader["g3"].ToString().Substring(0, db_reader["g3"].ToString().IndexOf(" "));
                    }
                    dv_vodomer.Rows.Add(row1);
                    db_reader.Close();
                    dv_vodomer.Refresh();
                    dv_vodomer.Rows[dv_vodomer.Rows.Count - 1].Cells[0].Selected = true;
                }

                db_com.CommandText = "exec " + db_base + ".dbo.getviewpost '" + txtlic.Text + "', 0";
                ///db_com.CommandText = "select p.modirec,p.kubh1n,p.kubh2n,p.kubh3n,p.kubg1n,p.kubg2n,p.kubg3n,p.vnv,p.vnk,p.prim_code, d.h1,d.h2,d.h3,d.g1,d.g2,d.g3 from " + db_base + ".dbo.posvod p inner join " + db_base + ".dbo.vodomerdate" + frmMain.MaxCurPer + " d on d.lic=p.lic where p.lic='" + txtlic.Text + "' order by p.modirec";
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        gv_pay.Rows.Clear();
                        while (db_reader.Read())
                        {
                            string[] row = { "", "", "", "" };
                            row[0] = db_reader["Период"].ToString().Substring(3,7).Replace("-","");
                            row[1] = db_reader["Сумма"].ToString();
                            row[2] = db_reader["brik"].ToString();
                            row[3] = "";
                            gv_pay.Rows.Add(row);
                            if (db_reader["brik"].ToString() == "57") gv_pay.Rows[gv_pay.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Yellow;
                            else gv_pay.Rows[gv_pay.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Ivory;
                        }
                    }
                    gv_pay.Refresh();
                }
                //////////////////  ОТСРОЧКА
                db_com.Parameters.Clear();
                db_com.CommandText = "Use " + db_base + " select a.sdolgbeg, a.nachisl, sum(isnull(p.opl,0)) as pos, sum(isnull(s.cv,0)+isnull(s.ck,0)+isnull(s.cp,0)) as storno from abonent" + frmMain.MaxCurPer + " a  left join pos"+frmMain.MaxCurPer+" p on p.lic=a.lic and p.brik<>27 and p.brik<>1000 left join oplata"+frmMain.MaxCurPer+" s on s.lic=a.lic where a.lic=" + txtlic.Text+" group by a.sdolgbeg,a.nachisl";
                using (SqlDataReader r = db_com.ExecuteReader())
                {
                    if (r.HasRows)
                    {
                        r.Read();
                        label17.Text = Convert.ToString(Convert.ToDouble(r["sdolgbeg"].ToString()));
                    }
                }

                db_com.Parameters.Clear();
                db_com.CommandText = "select * from abon.dbo.otsrochka where right(lic,9)=right(" + txtlic.Text+",9) and per=0";
                using (SqlDataReader r = db_com.ExecuteReader())
                {
                    if (r.HasRows)
                    {
                        r.Read();
                        textBox1.Text = r["plat"].ToString();
                        label23.Text = r.IsDBNull(r.GetOrdinal("datep1")) ? "нет" : r["datep1"].ToString().Substring(0, 10);
                        label24.Text = r.IsDBNull(r.GetOrdinal("datep2")) ? "нет" : r["datep2"].ToString().Substring(0, 10);
                        ///MessageBox.Show("-"+r["date_off"].ToString()+"-");
                        label28.Text = !r.IsDBNull(r.GetOrdinal("date_off")) ? r["date_off"].ToString().Substring(0, 10) : "нет";
                        label27.Text = !r.IsDBNull(r.GetOrdinal("date_on")) ? r["date_on"].ToString().Substring(0, 10) : "нет";
                        label25.Text = !r.IsDBNull(r.GetOrdinal("date1")) ? r["date1"].ToString().Substring(0, 10) : "нет";
                        label26.Text = !r.IsDBNull(r.GetOrdinal("date2")) ? r["date2"].ToString().Substring(0, 10) : "нет";
                        label35.Text = !r.IsDBNull(r.GetOrdinal("date_poff")) ? r["date_poff"].ToString().Substring(0, 10) : "нет";
                        label33.Text = r["tech"].ToString() == "True" ? "ЕСТЬ" : "НЕТ";
                        richTextBox1.Text = !r.IsDBNull(r.GetOrdinal("about")) ? r["about"].ToString() : "";
                        textBox2.Text = r["pay_on"].ToString().Replace(",", ".");
                    }
                    else
                    {
                        textBox1.Text = "";
                        label23.Text = "нет";
                        label24.Text = "нет";
                        ///MessageBox.Show("-"+r["date_off"].ToString()+"-");
                        label28.Text = "нет";
                        label27.Text = "нет";
                        label25.Text = "нет";
                        label26.Text = "нет";

                        label33.Text = "ЕСТЬ";

                        textBox2.Text = "";
                    }
                }
                tab.Enabled = true;
                panel3.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //tab.Enabled = true;
        }
        }

        public int find_lic(string flic)
        {
            int result = 0;
            string db_fbase = "Abon";
            if (flic.Substring(0, 1) == "1") db_fbase = "Abon";
            else if (flic.Substring(0, 1) == "2") db_fbase = "AbonUk";
            else goto END;
            try
            {
                db_com.CommandText = "select UK = case (v.bUk) when '1' then '1' when '0' then '0' end from AbonUk.dbo.abonent" + frmMain.CurPer + " a inner join " + db_fbase + ".dbo.spvedomstvo v on a.kodvedom=v.id where a.lic='2" + flic.Substring(1,9) + "'";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        result = 2;
                    }
                }
                    db_com.CommandText = "select UK = case (v.bUk) when '1' then '1' when '0' then '0' end from Abon.dbo.abonent" + frmMain.CurPer + " a inner join " + db_fbase + ".dbo.spvedomstvo v on a.kodvedom=v.id where a.lic='1" + flic.Substring(1,9) + "'";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            result += 1;
                        }
                    }
            }
            catch { }
            END:
            return result;
        }

        public int find_lic_base(string flic)
        {
            int result = 0;
            if (flic.Length != 10) goto END;
            try
            {
                db_com.CommandText = "select UK = case (v.bUk) when '1' then '1' when '0' then '0' end from AbonUk.dbo.abonent" + frmMain.CurPer + " a inner join AbonUk.dbo.spvedomstvo v on a.kodvedom=v.id where a.lic='2" + flic.Substring(1, 9) + "'";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        db_read.Read();
                        if (db_read["UK"].ToString().Trim() == "1") result = 2;
                        db_read.Close();
                    }
                }
                db_com.CommandText = "select UK = case (v.bUk) when '1' then '1' when '0' then '0' end from Abon.dbo.abonent" + frmMain.CurPer + " a inner join Abon.dbo.spvedomstvo v on a.kodvedom=v.id where a.lic='1" + flic.Substring(1, 9) + "'";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        db_read.Read();
                        if (db_read["UK"].ToString().Trim() == "0") result += 1;
                        db_read.Close();
                    }
                }
            }
            catch
            {
            }
        END:
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel3.Enabled = false;
            tab.Enabled = false;
            string lic = txtlic.Text.Replace(" ", "");
            if (lic.Length == 10)
            {

                if (find_lic_base(lic) == 1) { lic = "1" + lic.Substring(1, 9); db_base = "Abon"; txtlic.Text = lic; }
                else if (find_lic_base(lic) == 2) { lic = "2" + lic.Substring(1, 9); db_base = "AbonUk"; txtlic.Text = lic; }
                else if (find_lic_base(lic) == 3) { MessageBox.Show("Л/с активен в двух базах!","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error); };

                db_com.CommandText = "select a.str_code,a.dom,a.flat,UK = case (v.bUk) when '1' then '1' when '0' then '0' end,s.yl_name from " + db_base + ".dbo.abonent" + frmMain.CurPer + " a inner join " + db_base + ".dbo.spvedomstvo v on a.kodvedom=v.id inner join " + db_base + ".dbo.street s on a.str_code = s.cod_yl where a.lic='" + lic + "'";
                try
                {
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        db_read.Read();
                        if (db_read.HasRows)
                        {
                            string street = db_read["yl_name"].ToString();
                            if (db_read["UK"].ToString() == "1") lbl_base.Text = "УК"; else lbl_base.Text = "Жилье";
                            string house = db_read["dom"].ToString();
                            string flat = db_read["flat"].ToString();
                            db_read.Close();
                            cmb_street.SelectedIndex = cmb_street.FindString(street.Trim(), -1);
                            ///cmb_street.Text = street;
                            cmb_house.SelectedIndex = cmb_house.FindString(house.Trim(), -1);
                            ///cmb_house.SelectedIndexChanged(this.Handle, e);
                            ///cmb_house.Text = house;
                            ///cmb_flat.FindString(flat, 0);
                            ////((SelectData)cmb_flat[i]).Value.ToString());
                            for(int i=0;i<cmb_flat.Items.Count;i++) {
                                ////MessageBox.Show(((SelectData)cmb_flat.Items[i]).Value.ToString()+"\n"+txtlic.Text,"");
                                if (((SelectData)cmb_flat.Items[i]).Value.ToString().Substring(1, ((SelectData)cmb_flat.Items[i]).Value.ToString().Length-1) == txtlic.Text.Substring(1, 9))
                                {
                                    cmb_flat.SelectedIndex = i;
                                    cmb_flat_SelectedIndexChanged(this.Handle, e); 
                                    break;
                                }
                            }
                            ///cmb_flat_SelectedIndexChanged(this.Handle, e);
                        }
                        else
                        {
                            MessageBox.Show("Л/с " + txtlic.Text + " не найден", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void group3_CheckStateChanged(object sender, EventArgs e)
        {
            //if (group3.CheckState == CheckState.Checked)
            //{
            //    DialogResult dialog = MessageBox.Show("Вы действительно хотите перевести л/с в 3 группу?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (dialog == System.Windows.Forms.DialogResult.Yes)
            //    {
            //        /// процедура перевода в 3 группу
            //        group3.CheckState = CheckState.Checked;
            //    }
            //    else if (dialog == System.Windows.Forms.DialogResult.No) group3.CheckState = CheckState.Unchecked;
            //}
            //else if (group3.CheckState == CheckState.Unchecked)
            //{
            //    DialogResult dialog = MessageBox.Show("Вы действительно хотите отменить 3 группу для л/с?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (dialog == System.Windows.Forms.DialogResult.Yes)
            //    {
            //        /// процедура восстановления из 3 группы
            //        group3.CheckState = CheckState.Unchecked;
            //    }
            //    else if (dialog == System.Windows.Forms.DialogResult.No) group3.CheckState = CheckState.Checked;
            //}
        }


        private void button8_Click(object sender, EventArgs e)
        {
            pnl_live.Enabled = true;
        }

        private void dg_liver_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 4) && (dg_liver[4,e.RowIndex].Value.ToString() != "")) MessageBox.Show(dg_liver[0,e.RowIndex].Value.ToString(),"");
        }

        private void cmb_flat_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_flat.DroppedDown = true;
        }

        private void lbl_live_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.ShowDialog();
        }

        private static string GetDefaultPrinterName()
        {
            String[] printers = new string[PrinterSettings.InstalledPrinters.Count];
            PrinterSettings.InstalledPrinters.CopyTo(printers, 0);
            for (int i = 0; i < printers.Length; i++)
                if (new PrinterSettings() { PrinterName = printers[i] }.IsDefaultPrinter)
                    return printers[i];
            return null;
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            printDocument1.PrinterSettings.PrinterName = GetDefaultPrinterName();
            printDocument1.DefaultPageSettings.Margins.Left = 213;
            printDocument1.DefaultPageSettings.Margins.Top = 2;
            printDocument1.DefaultPageSettings.Margins.Bottom = 2;
            printDocument1.DefaultPageSettings.Landscape = false;
            
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (txtlic.Text.Length == 10)
            {
                if (txtlic.Text.Substring(0, 1) == "1")
                {
                    if ((find_lic("2" + txtlic.Text.Substring(1, 9)) == 2) | (find_lic("2" + txtlic.Text.Substring(1, 9)) == 3))
                    {
                        txtlic.Text = "2" + txtlic.Text.Substring(1, 9); db_base = "AbonUk"; lbl_obase.Text = "Жилье УК"; flag_change_base = true;
                        button1_Click(this, e);
                    }
                    else MessageBox.Show("Л/с 2" + txtlic.Text.Substring(1, 9) + " не существует", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (txtlic.Text.Substring(0, 1) == "2")
                {
                    if ((find_lic("1" + txtlic.Text.Substring(1, 9)) == 1) | (find_lic("1" + txtlic.Text.Substring(1, 9)) == 3))
                    {
                        txtlic.Text = "1" + txtlic.Text.Substring(1, 9); db_base = "Abon"; lbl_obase.Text = "Жилье"; flag_change_base = true;
                        button1_Click(this, e);
                    }
                    else MessageBox.Show("Л/с 1" + txtlic.Text.Substring(1, 9) + " не существует", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            pnl_live.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmPrn frmCPrn = new frmPrn();
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath()+"tmp"+txtlic.Text+".html",FileMode.Create,FileAccess.ReadWrite),Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmp" + txtlic.Text + ".html";
            file.WriteLine("<html><head><title>Сводный отчет л/c " + txtlic.Text + "</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/>"+
                "<style rel='stylesheet' type='text/css'>"+
                "body {font-family: verdana; font-size:11pt; background-color: #FFFFFF;}" +
                "tbody td {font-size:9pt; }\n"+
                "thead td {font-size:9pt; }\n" +
                "tfoot td {font-size:11pt; }\n" +
                "</style></head><body><font size='2' color='black' face='Verdana'>");
            file.WriteLine("<table border='0' width='990pt'><tfoot><tr><td>");
            
            try
            {
                db_com.CommandText = "SELECT PUNKT,NAME_UL,PREF_UL, LEFT(UCH, 6) ind From "+db_base+".dbo.StreetUch WHERE CAST(S AS nvarchar(10))+ISNULL(LIT,N'')=N'" + cmb_house.Text + "' AND (cod_yl = N'" + ((SelectData)cmb_street.SelectedItem).Value + "') ";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        db_read.Read();
                        file.WriteLine("Сводная информация по счету " + txtlic.Text + ". Адрес: "+db_read["ind"].ToString()+", "+db_read["punkt"].ToString()+", "+db_read["pref_ul"].ToString()+" "+db_read["name_ul"].ToString()+", Д. "+cmb_house.Text);
                        if (cmb_flat.Text.Trim() != "0") file.WriteLine(", КВ. " + cmb_flat.Text);
                    }
                }
            }
            catch
            {
            }
            file.WriteLine("</td></tr></tfoot></table>");
            file.WriteLine("<table border='0' width='990pt' cellspacing=\"0\" cellpadding=\"2\">");
            /* select   min(CAST(substring([name],8,6) as int)) as ot,  max( CAST(substring([name],8,6) as int)) as do From sysobjects Where (type ='U' or type ='V')  and name Like'Abonent______'  and IsNumeric(substring([name],8,6))=1
             * 
             * per < 200502
             SELECT isnull((select sum(an.Amount) From AbonentNach201207 an INNER JOIN SpGrantServices gs ON an.GranServiceID = gs.[Id] where an.lic=a.lic and gs.TypeServicesID = 3),0) as Сотки, a.OplPoliv as ОплПолив,(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_vl else a.N_vl end) as НормВ,'' as [В ОДН],(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_kl else a.N_kl end) as НормК,'' as [К ОДН],a.Poliv as Полив,(a.Liver-a.Vibilo-a.Vibilo2) as Прож,a.Lgot as Льготы,a.SnDeb-a.SnKred as ДолгНач,a.Nachisl as Начисл,'' as [Нач ОДН],a.Pos as Поступл,a.Oplata as Сторно,a.SnDeb-a.SnKred+a.Nachisl-a.Pos+a.Oplata+a.Poliv-a.spisan as ДолгКон,rtrim(a.Fam) as ФИО,isnull(a.PosPoliv,0) as ОплачПол,a.spisan as spis,a.SDolgBeg,a.SDolgBeg+a.Nachisl-ISNULL(p.Pos,0)+a.Oplata+a.Poliv-a.spisan SDolgEnd,ISNULL(p.Pos,0) as PosD From Abonent201207 a CROSS JOIN      (SELECT SUM(opl+poliv) AS pos From Pos201207 WHERE lic =1010122800 AND brik<>27) as p Where a.lic =1010122800
            */
            file.WriteLine("<thead><tr>");
            file.WriteLine("<td>Дата</td>");
            file.WriteLine("<td>ФИО</td>");
            file.WriteLine("<td>Прожив</td>");
            file.WriteLine("<td>Сотки</td>");
            file.WriteLine("<td>Полив</td>");
            file.WriteLine("<td>Опл. полив</td>");
            file.WriteLine("<td>Норм В</td>");
            file.WriteLine("<td>Норм К</td>");
            file.WriteLine("<td>Долг Нач</td>");
            file.WriteLine("<td>Начислено</td>");
            file.WriteLine("<td>Нач ОДН</td>");
            file.WriteLine("<td>Сторно</td>");
            file.WriteLine("<td>Поступление</td>");
            file.WriteLine("<td>Долг Кон</td>");
            file.WriteLine("<td>Ведомство</td>");
            file.WriteLine("</tr></thead>");
            file.WriteLine("<tbody>");
            try
            {
                Int32 perbeg = Convert.ToInt32(Convert.ToString((Convert.ToInt32(frmMain.MaxCurPer.ToString().Substring(0,4))-3))+frmMain.MaxCurPer.Substring(4,2));
                Int32 perend = Convert.ToInt32(frmMain.MaxCurPer.ToString());
                bool color = false;
                while (perbeg <= perend)
                {
                    //db_com.CommandText = "SELECT isnull((select sum(an.Amount) From " + db_base + ".dbo.AbonentNach" + perbeg.ToString() + " an INNER JOIN " + db_base + ".dbo.SpGrantServices gs ON an.GranServiceID = gs.[Id] where an.lic=a.lic and gs.TypeServicesID = 3),0) as sotki, a.OplPoliv as ОплПолив,(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_vl else a.N_vl end) as НормВ,'' as [В ОДН],(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_kl else a.N_kl end) as НормК,a.Poliv as Полив,(a.Liver-a.Vibilo-a.Vibilo2) as Прож,a.Lgot as Льготы,a.SnDeb-a.SnKred as ДолгНач,a.Nachisl as Начисл,isnull(a.OverNV_full,0) as [Нач ОДН],a.Pos as Поступл,a.Oplata as Сторно,a.SnDeb-a.SnKred+a.Nachisl-a.Pos+a.Oplata+a.Poliv-a.spisan as ДолгКон,rtrim(a.Fam) as ФИО,isnull(a.PosPoliv,0) as ОплачПол,a.spisan as spis,a.SDolgBeg,a.SDolgBeg+a.Nachisl-ISNULL(p.Pos,0)+a.Oplata+a.Poliv-a.spisan SDolgEnd,ISNULL(p.Pos,0) as PosD From " + db_base + ".dbo.Abonent" + perbeg.ToString() + " a CROSS JOIN      (SELECT SUM(opl+poliv) AS pos From " + db_base + ".dbo.Pos" + perbeg.ToString() + " WHERE lic ='" + txtlic.Text + "' AND brik<>27) as p Where a.lic ='" + txtlic.Text + "'";
                    db_com.CommandText = "SELECT isnull((select sum(an.Amount) From " + db_base + ".dbo.AbonentNach" + perbeg.ToString() + " an INNER JOIN " + db_base + ".dbo.SpGrantServices gs ON an.GranServiceID = gs.[Id] where an.lic=a.lic and gs.TypeServicesID = 3),0) as sotki,a.OplPoliv as ОплПолив,(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_vl else a.N_vl end) as НормВ,(case when a.vvodomer=0 and (a.vodkod is null) then a.allN_kl else a.N_kl end) as НормК,a.Poliv as Полив,(a.Liver-a.Vibilo-a.Vibilo2) as Прож,a.SDolgBeg as ДолгНач,a.Nachisl as Начисл,isnull(a.OverNV_full,0) as [Нач ОДН],a.Pos as Поступл,a.Oplata as Сторно,a.sdolgend as ДолгКон,rtrim(a.Fam) as ФИО,isnull(a.PosPoliv,0) as ОплачПол,a.spisan as spis,a.SDolgBeg,a.SDolgEnd as ДоглКон,ISNULL(p.Pos,0) as PosD, a.vedom From " + db_base + ".dbo.Abonent" + perbeg.ToString() + " a CROSS JOIN      (SELECT SUM(opl+poliv) AS pos From " + db_base + ".dbo.Pos" + perbeg.ToString() + " WHERE lic ='" + txtlic.Text + "' AND brik<>27) as p Where a.lic ='" + txtlic.Text + "'";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            
                            while (db_read.Read())
                            {
                                if (color = !color) file.WriteLine("<tr style=\"background-color:#C0FFC0\">"); else file.WriteLine("<tr>");
                                file.WriteLine("<td>"+perbeg.ToString().Substring(0,4)+" "+perbeg.ToString().Substring(4,2)+"</td>");
                                file.WriteLine("<td>" + db_read["ФИО"].ToString().Substring(0,db_read["ФИО"].ToString().IndexOf(" ")) + "</td>");
                                file.WriteLine("<td>" + db_read["Прож"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["sotki"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["Полив"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["ОплПолив"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["НормВ"].ToString().Substring(0, db_read["НормВ"].ToString().IndexOf(".") + 3) + "</td>");
                                file.WriteLine("<td>" + db_read["НормК"].ToString().Substring(0,db_read["НормК"].ToString().IndexOf(".")+3) + "</td>");
                                file.WriteLine("<td><b>" + db_read["ДолгНач"].ToString() + "</b></td>");
                                file.WriteLine("<td>" + db_read["Начисл"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["Нач ОДН"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["Сторно"].ToString() + "</td>");
                                file.WriteLine("<td>" + db_read["Поступл"].ToString() + "</td>");
                                file.WriteLine("<td><b>" + db_read["ДолгКон"].ToString() + "</b></td>");
                                file.WriteLine("<td><b>" + db_read["vedom"].ToString() + "</b></td>");
                                file.WriteLine("</tr>");
                            }
                        }
                    }
                    perbeg = Convert.ToInt32(frmToNewVedom.PERPLUS1(perbeg.ToString()));
                }
            }
            catch
            {
            }
            file.WriteLine("</tbody>");
            file.WriteLine("</font>");
            file.WriteLine("</table>");
            file.WriteLine("<table border='0' width='990pt'><tfoot><tr><td>");
            file.WriteLine("Зам. генерального директора по сбыту МПП ВКХ \"Орелводоканал\" Рассказов А.И.");
            file.WriteLine("</td></tr></tfoot></table>");
            file.WriteLine("</font></body>\n</html>");
            file.Close();
            frmCPrn.Text = "Печать "+txtlic.Text;
            frmCPrn.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte Base = (byte)((txtlic.Text.Substring(0,1) == "1") ? 0 : 1);
            Calc.Query(Base, frmMain.MaxCurPer, txtlic.Text);
            button1_Click(this, e);
        }

        private void txtlic_TextChanged(object sender, EventArgs e)
        {
            ///if (!auto_search) if (txtlic.Text.Length == 10) { auto_search = true; button1_Click(this, e); auto_search = false;}

        }

        private void txtlic_KeyPress(object sender, KeyEventArgs e)
        {
            if (txtlic.Text.Length == 10) button1_Click(this, e);
        }

        private void txtlic_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private bool key_right = false;

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)) || (e.KeyCode == Keys.Clear) || (e.KeyCode == Keys.Back))
            {
                key_right = true;
            }
            else key_right = false;
            //if (((e.KeyChar >= '0') && (e.KeyChar <= '9')) || (e.KeyChar == ',') || (e.KeyChar == '.'))
            //{

            //}
            //else
            //{

            //}
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((e.KeyChar == '.') || (e.KeyChar == ',')) && (textBox1.Text.IndexOf(".") < 0) && (textBox1.Text.IndexOf(",") < 0))
            {
                key_right = true;
                if (e.KeyChar == ',') e.KeyChar = '.';
            }
            if (!key_right)
            {
                e.Handled = true;
            }
            else
            {  
                if (textBox1.Text.Length > 0 && textBox1.Text.Substring(0, 1) == "0") textBox1.Text = textBox1.Text.Substring(1, textBox1.Text.Length - 1);
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) textBox1.Text = "0.00";
            if (textBox1.Text.IndexOf(".") >= 0)
            {
                if (textBox1.Text.IndexOf(".") == 0) textBox1.Text = "0" + textBox1.Text;
                if (textBox1.Text.Substring(textBox1.Text.IndexOf("."), textBox1.Text.Length - textBox1.Text.IndexOf(".") - 1).Length < 2) textBox1.Text += "00";
                if (textBox1.Text.Substring(textBox1.Text.IndexOf("."), textBox1.Text.Length - textBox1.Text.IndexOf(".") - 1).Length > 2) textBox1.Text = textBox1.Text.Substring(0,textBox1.Text.IndexOf(".")+3);
            }
        }

        private bool key_right_ = false;

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)) || (e.KeyCode == Keys.Clear) || (e.KeyCode == Keys.Back))
            {
                key_right_ = true;
            }
            else key_right_ = false;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((e.KeyChar == '.') || (e.KeyChar == ',')) && (textBox2.Text.IndexOf(".") < 0) && (textBox2.Text.IndexOf(",") < 0))
            {
                key_right_ = true;
                if (e.KeyChar == ',') e.KeyChar = '.';
            }
            if (!key_right_)
            {
                e.Handled = true;
            }
            else
            {
                if (textBox2.Text.Length > 0 && textBox2.Text.Substring(0, 1) == "0") textBox2.Text = textBox2.Text.Substring(1, textBox2.Text.Length - 1);
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0) textBox2.Text = "0.00";
            if (textBox2.Text.IndexOf(".") >= 0)
            {
                if (textBox2.Text.IndexOf(".") == 0) textBox2.Text = "0" + textBox2.Text;
                if (textBox2.Text.Substring(textBox2.Text.IndexOf("."), textBox2.Text.Length - textBox2.Text.IndexOf(".") - 1).Length < 2) textBox2.Text += "00";
                if (textBox2.Text.Substring(textBox2.Text.IndexOf("."), textBox2.Text.Length - textBox2.Text.IndexOf(".") - 1).Length > 2) textBox2.Text = textBox2.Text.Substring(0, textBox2.Text.IndexOf(".") + 3);
            }
        }

        private void cal_show()
        {
            if (monthCalendar1.Visible == false)
                monthCalendar1.Visible = true;
            else
            {
                cal_tag = 0;
                monthCalendar1.Visible = false;
            }
        }
        private int cal_tag=0;
        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (cal_tag == 1)
            {
                label23.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label23.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label23.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 2)
            {
                label24.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label24.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label24.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 3)
            {
                label28.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label28.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label28.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 4)
            {
                label27.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label27.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label27.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 5)
            {
                label25.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label25.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label25.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 6)
            {
                label26.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label26.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString())+".";
                label26.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            if (cal_tag == 7)
            {
                label35.Text = monthCalendar1.SelectionRange.Start.Day.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Day.ToString() : monthCalendar1.SelectionRange.Start.Day.ToString();
                label35.Text += "." + (monthCalendar1.SelectionRange.Start.Month.ToString().Length == 1 ? "0" + monthCalendar1.SelectionRange.Start.Month.ToString() : monthCalendar1.SelectionRange.Start.Month.ToString()) + ".";
                label35.Text += monthCalendar1.SelectionRange.Start.Year.ToString();
            }
            cal_tag = 0;
            monthCalendar1.Visible = false;
        }


        private void button13_Click_1(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 1;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 2;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 3;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 4;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 5;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 6;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (label33.Text == "ЕСТЬ") label33.Text = "НЕТ";
            else
                if (label33.Text == "НЕТ") label33.Text = "ЕСТЬ";
        }

        private void monthCalendar1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                cal_tag = 0;
                monthCalendar1.Visible = false;
            }
        }

        private void monthCalendar1_Leave(object sender, EventArgs e)
        {
            ; ; ;
        }

        bool in_cal = false;
        private void monthCalendar1_MouseEnter(object sender, EventArgs e)
        {
            in_cal = true;
        }

        private void monthCalendar1_MouseLeave(object sender, EventArgs e)
        {
            if (in_cal)
            {
                cal_tag = 0;
                monthCalendar1.Visible = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (txtlic.Text.Length == 10 && (label25.Text != "нет" || label26.Text != "нет") || (label23.Text != "нет" || label24.Text != "нет"))
            {
                /// сохранение рассрочки
                SqlCommand com = new SqlCommand();
                com.Connection = con;
                com.CommandText = "select lic from abon.dbo.otsrochka where right(lic,9)=@lic and per=0";
                com.Parameters.AddWithValue("@lic",txtlic.Text.Substring(1,9));
                using (SqlDataReader r = com.ExecuteReader())
                {
                    com.Parameters.Clear();
                    if (r.HasRows)
                    {
                        ///update имеющуюся запись
                        com.CommandText = @"update abon.dbo.otsrochka
set plat = @plat,
    date1 = " + (label25.Text != "нет" ? "Convert(date,'" + label25.Text + "',104)" : "NULL") + @",
    date2 = " + (label26.Text != "нет" ? "Convert(date,'" + label26.Text + "',104)" : "NULL") + @",
    date_off =" + (label28.Text != "нет" ? "Convert(date,'" + label28.Text + "',104)" : "NULL") + @",
    date_on = " + (label27.Text != "нет" ? "Convert(date,'" + label27.Text + "',104)" : "NULL") + @",
    datep1 = " + (label23.Text != "нет" ? "Convert(date,'" + label23.Text + "',104)" : "NULL") + @",
    datep2 = " + (label24.Text != "нет" ? "Convert(date,'" + label24.Text + "',104)" : "NULL") + @",
    date_poff = " + (label35.Text != "нет" ? "Convert(date,'" + label35.Text + "',104)" : "NULL") + @",
    about = @about,
    tech = @tech,
    pay_on = @pay_on
where right(lic,9)=" +txtlic.Text.Substring(1,9)+@" and per = 0";
                        com.Parameters.AddWithValue("@plat", textBox1.Text == "" ? 0 : Convert.ToDouble(textBox1.Text));
                        com.Parameters.AddWithValue("@tech", label33.Text == "ЕСТЬ" ? true : false);
                        com.Parameters.AddWithValue("@about", " " + richTextBox1.Text);
                        com.Parameters.AddWithValue("@pay_on", textBox2.Text == "" ? 0 : Convert.ToDouble(textBox2.Text));
                    }
                    else
                    {
                        ///добавляем новую запись
                        com.CommandText = "insert into abon.dbo.otsrochka(lic,sdolgbeg,nachisl,pos,plat,per,date1,date2,[date_off],date_on,datep1,datep2,date_poff,about,tech,pay_on) "+
"values(@lic,@sdolgbeg,@nachisl,@pos,@plat,0," + (label25.Text != "нет" ? "Convert(date,'" + label25.Text + "',104)" : "NULL") + "," + (label26.Text != "нет" ? "Convert(date,'" + label26.Text + "',104)" : "NULL") + ","+
 (label28.Text != "нет" ? "Convert(date,'" + label28.Text + "',104)" : "NULL") + "," + (label27.Text != "нет" ? "Convert(date,'" + label27.Text + "',104)" : "NULL") + ","+
 (label23.Text != "нет" ? "Convert(date,'" + label23.Text + "',104)" : "NULL") + "," + (label24.Text != "нет" ? "Convert(date,'" + label24.Text + "',104)" : "NULL") + "," + (label35.Text != "нет" ? "Convert(date,'" + label35.Text + "',104)" : "NULL") + ",@about,@tech,@pay_on)";
                        com.Parameters.AddWithValue("@lic", txtlic.Text);
                        com.Parameters.AddWithValue("@sdolgbeg", Convert.ToDouble(label17.Text));
                        com.Parameters.AddWithValue("@nachisl", 0);
                        com.Parameters.AddWithValue("@pos", 0);
                        com.Parameters.AddWithValue("@plat", textBox1.Text== "" ? 0 : Convert.ToDouble(textBox1.Text));
                        //com.Parameters.AddWithValue("@date1", label25.Text != "нет" ? "Convert(date,'"+label25.Text.Replace(".", "-")+"',104)": "NULL");
                        //com.Parameters.AddWithValue("@date2", label26.Text != "нет" ? "Convert(date,'" + label26.Text.Replace(".", "-") + "',104)" : "NULL");
                        //com.Parameters.AddWithValue("@date_off", label28.Text != "нет" ? "Convert(date,'" + label28.Text.Replace(".", "-") + "',104)" : "NULL");
                        //com.Parameters.AddWithValue("@date_on", label27.Text != "нет" ? "Convert(date,'" + label27.Text.Replace(".", "-") + "',104)" : "NULL");
                        //com.Parameters.AddWithValue("@datep1", label23.Text != "нет" ? "Convert(date,'" + label23.Text.Replace(".", "-") + "',104)" : "NULL");
                        //com.Parameters.AddWithValue("@datep2", label24.Text != "нет" ? "Convert(date,'" + label24.Text.Replace(".", "-") + "',104)" : "NULL");
                        com.Parameters.AddWithValue("@tech", label33.Text == "ЕСТЬ" ? true : false);
                        com.Parameters.AddWithValue("@about", " " + richTextBox1.Text);
                        com.Parameters.AddWithValue("@pay_on", textBox2.Text== "" ? 0 : Convert.ToDouble(textBox2.Text));
                    }
                }
                try
                {
                    com.ExecuteNonQuery();
                    com.Parameters.Clear();
                    MessageBox.Show("Сохранение успешно выполнено.","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch(Exception error)
                {
                    com.Parameters.Clear();
                    MessageBox.Show(error.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            cal_show();
            cal_tag = 7;
        }

        // пеня по лицевому счету
        private void PeniButton_Click(object sender, EventArgs e)
        {
            PeniShow PS = new PeniShow(txtlic.Text, con);
            PS.Show();
        }



    }
}

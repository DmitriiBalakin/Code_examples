using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace water
{
    public partial class frmToNewVedom : Form
    {
        SqlCommand db_com = new SqlCommand();
        SqlDataReader db_reader;

        public frmToNewVedom()
        {
            InitializeComponent();
            db_com.Connection = frmMain.db_con;
            db_com.CommandType = CommandType.Text;
            db_com.CommandTimeout = 0;
        }

        private string inttomonth(int mon)
        {
            string str_mon = "";
            str_mon = mon.ToString();
            if (str_mon.Length == 1) str_mon = "0" + mon.ToString();
            switch (str_mon)
            {
                case "01": str_mon = "Январь"; break;
                case "02": str_mon = "Февраль"; break;
                case "03": str_mon = "Март"; break;
                case "04": str_mon = "Апрель"; break;
                case "05": str_mon = "Май"; break;
                case "06": str_mon = "Июнь"; break;
                case "07": str_mon = "Июль"; break;
                case "08": str_mon = "Август"; break;
                case "09": str_mon = "Сентябрь"; break;
                case "10": str_mon = "Октябрь"; break;
                case "11": str_mon = "Ноябрь"; break;
                case "12": str_mon = "Декабрь"; break;
            }
            return str_mon;
        }

        private string inttomonth(string mon)
        {
            string str_mon = "";
            if (mon.Length == 1) str_mon = "0" + mon; else str_mon = mon;
            switch (mon)
            {
                case "01": str_mon = "Январь"; break;
                case "02": str_mon = "Февраль"; break;
                case "03": str_mon = "Март"; break;
                case "04": str_mon = "Апрель"; break;
                case "05": str_mon = "Май"; break;
                case "06": str_mon = "Июнь"; break;
                case "07": str_mon = "Июль"; break;
                case "08": str_mon = "Август"; break;
                case "09": str_mon = "Сентябрь"; break;
                case "10": str_mon = "Октябрь"; break;
                case "11": str_mon = "Ноябрь"; break;
                case "12": str_mon = "Декабрь"; break;
            }
            return str_mon;
        }

        private void GetListPerehod()
        {
            try
            {
                gv_exist_list.Rows.Clear();
                gv_exist_list.Refresh();
                db_com.CommandText = 
                    "select p.id as id, s.yl_name as yl_name,p.dom as dom, v.NameVed as old_ved, buks,vv.NameVed as new_ved, bukn, p.perbeg as perbeg,done from abonuk.dbo.perehod p " +
                    "inner join abonuk.dbo.street s on s.cod_yl = p.str_code " +
                    "inner join abonuk.dbo.spvedomstvo v on p.old_ved=v.id " +
                    "inner join abonuk.dbo.spvedomstvo vv on p.new_ved=vv.id " +
                    "where p.perbeg >=" +PERPLUS1(frmMain.MaxCurPer) + " " +
                    "order by p.perbeg,s.yl_name, p.dom, v.nameved,vv.nameved,p.id,buks,bukn";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            string[] row = { "", "", "", "", "", "" , "", "", "" ,""};
                            row[0] = db_read["yl_name"].ToString();
                            row[1] = db_read["dom"].ToString();
                            row[2] = db_read["old_ved"].ToString();
                            if ((db_read["buks"].ToString() == "True") | (db_read["buks"].ToString() == "1")) row[3] = "УК"; else row[3] = "Жилье";
                            row[4] = db_read["new_ved"].ToString();
                            if ((db_read["bukn"].ToString() == "True") | (db_read["bukn"].ToString() == "1")) row[5] = "УК"; else row[5] = "Жилье";
                            row[6] = db_read["perbeg"].ToString();
                            if ((db_read["done"].ToString() == "True") | (db_read["done"].ToString() == "1")) row[7] = "Выполнено"; else row[7] = "Удалить";
                            row[8] = db_read["id"].ToString();
                            if ((db_read["done"].ToString() == "True") | (db_read["done"].ToString() == "1")) row[9] = "1"; else row[9] = "0";
                            gv_exist_list.Rows.Add(row);
                        }
                        db_read.Close();
                        for (int i = 0; i < gv_exist_list.Rows.Count; i++)
                            if (gv_exist_list.Rows[i].Cells["done"].Value.ToString() == "1") gv_exist_list.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen; else gv_exist_list.Rows[i].DefaultCellStyle.BackColor = Color.LightYellow;
                        gv_exist_list.Refresh();
                    }
                    else db_read.Close();
                }
            }
            catch(Exception ex)
            {
                
                MessageBox.Show(ex.Message+"\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            gv_exist_list.Refresh();

        }
        private void ToNewVedom_Shown(object sender, EventArgs e)
        {
            lbl_no_one_ved.Visible = false;
            lbl_zero_ved.Visible = false;
            lbl_MaxCurPer.Text = inttomonth(frmMain.MaxCurPer.Substring(4, 2)) + " " + frmMain.MaxCurPer.Substring(0, 4);
            lbl_PerNewVedom.Text = inttomonth((Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)) + 1) > 12 ? 1 : (Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)) + 1)) + " " + (Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)) + 1 > 12 ? (Convert.ToUInt32(frmMain.MaxCurPer.Substring(0, 4))+1).ToString():frmMain.MaxCurPer.Substring(0, 4));
            if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                    GetListPerehod();
                    db_com.CommandText = "select cod_yl, yl_name from AbonUK.dbo.Street where cod_yl <> 'тмп' order by yl_name";
                    db_reader = db_com.ExecuteReader();
                        if (db_reader.HasRows)
                        {
                            while (db_reader.Read())
                            {
                                string result = Convert.ToString(db_reader["yl_name"]);
                                result = result.Trim();
                                //result = result + " " + Convert.ToString(db_reader["SOCR"]);
                                //result = result.Trim();
                                this.cmb_street.Items.Add(new SelectData(Convert.ToString(db_reader["cod_yl"]), result));
                            }
                            db_reader.Close();
                            if (this.cmb_street.Items.Count > 0)
                            {
                                this.cmb_street.SelectedIndex = 0;
                            }
                        }


                    ///update_house(((SelectData)this.cmb_street.SelectedItem).Value);
                }
                catch
                {
                    if (!db_reader.IsClosed) db_reader.Close();
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

        private void cmb_street_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_street.DroppedDown = true;
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_house.Items.Clear();
            cmb_house.Text = "";
            string street = ((SelectData)this.cmb_street.SelectedItem).Value;
            try
            {
                db_com.CommandText = "select dom, SUM(v) as ved from (" +
                        "select dom, 2 as v from Abonuk.dbo.abonent" + frmMain.MaxCurPer + " where str_code = '" + street + "' group by dom " +
                        "union all " +
                        "select dom, 1 as v from Abon.dbo.abonent" + frmMain.MaxCurPer + " where str_code = '" + street + "' group by dom" +
                        ") d group by dom order by dom";

                using (SqlDataReader db_reader_house = db_com.ExecuteReader())
                {
                    if (db_reader_house.HasRows)
                    {
                        while (db_reader_house.Read())
                        {
                            cmb_house.Items.Add(new SelectData("", db_reader_house["dom"].ToString()));
                        }
                        db_reader_house.Close();
                        if (this.cmb_house.Items.Count > 0)
                        {
                            cmb_house.SelectedIndex = 0;
                        }
                    }

                }

            }
            catch (Exception error)
            {
                if (!db_reader.IsClosed) db_reader.Close();
                MessageBox.Show("Ошибка получения списка домов.\n" + error.Message, "Ошибка");
            }
        }

        private void cmb_house_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbl_no_one_ved.Visible = false;
            lbl_zero_ved.Visible = false;
            cmb_cur_ved.Items.Clear();
            cmb_cur_ved.Text = "";
            cmb_new_ved.Items.Clear();
            cmb_new_ved.Text = "";
            lbl_count_lic.Text = "-";
            lbl_count_lic_3gr.Text = "-";

            if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                 
                            db_com.CommandText = "select kodvedom, vedom, v.buk as buk from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on a.kodvedom = v.id and v.buk=1 where str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "' group by vedom, kodvedom, v.buk" +
                                " union all " +
                                "select kodvedom, vedom, v.buk as buk from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on a.kodvedom = v.id and v.buk=0 where str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "' group by vedom, kodvedom, v.buk";
                            using (db_reader = db_com.ExecuteReader())
                            {
                                btn_change_ved.Enabled = true;
                                if (db_reader.HasRows)
                                {
                                    int count = 0;
                                    while (db_reader.Read())
                                    {
                                        cmb_cur_ved.Items.Add(new SelectData(db_reader["buk"].ToString(), db_reader["kodvedom"].ToString(), db_reader["vedom"].ToString()));
                                        count++;
                                    }
                                    db_reader.Close();
                                    cmb_cur_ved.SelectedIndex = 0;
                                    if (count > 1) { lbl_no_one_ved.Visible = true; btn_change_ved.Enabled = false; }
                                }
                                else { lbl_zero_ved.Visible = true; btn_change_ved.Enabled = false; }
                            }
                            
                }
                catch (Exception ex)
                {
                    if (!db_reader.IsClosed) db_reader.Close();
                    MessageBox.Show(ex.Message+"\nОбратитесь в отдел АСУ","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("Соединение с Базой Данных прерванно.", "Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        private void cmb_cur_ved_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_new_ved.Items.Clear();
            cmb_new_ved.Text = "";
            lbl_count_lic.Text = "-";
            lbl_count_lic_3gr.Text = "-";

            if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                    db_com.CommandText = "select sum(bb.cnt_lic) as cnt_lic from (select count(a.lic) as cnt_lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Text + "' and a.kodvedom = '" + ((SelectData)cmb_cur_ved.SelectedItem).Value + "'" +
                        " union all " +
                        "select count(a.lic) as cnt_lic from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Text + "' and a.kodvedom = '" + ((SelectData)cmb_cur_ved.SelectedItem).Value + "') bb";
                    db_reader = db_com.ExecuteReader();
                    if (db_reader.HasRows)
                    {
                        db_reader.Read();
                        lbl_count_lic.Text = db_reader["cnt_lic"].ToString();
                    }
                    db_reader.Close();

                    db_com.CommandText = "select sum(cnt_lic) as cnt_lic from (select count(a.lic) as cnt_lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Text + "' and a.kodvedom = '" + ((SelectData)cmb_cur_ved.SelectedItem).Value + "' and a.gruppa3 = '1'" +
                        " union all " +
                        "select count(a.lic) as cnt_lic from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 where a.str_code='" + ((SelectData)this.cmb_street.SelectedItem).Value + "' and a.dom='" + cmb_house.Text + "' and a.kodvedom = '" + ((SelectData)cmb_cur_ved.SelectedItem).Value + "' and a.gruppa3 = '1') bb";
                    db_reader = db_com.ExecuteReader();
                    if (db_reader.HasRows)
                    {
                        db_reader.Read();
                        lbl_count_lic_3gr.Text = db_reader["cnt_lic"].ToString();
                    }
                    db_reader.Close();
                    
                    db_com.CommandText = "select id, nameved, buk from abonuk.dbo.spvedomstvo where id <> '"+((SelectData)this.cmb_cur_ved.SelectedItem).Value+"' order by nameved";
                    db_reader = db_com.ExecuteReader();
                    if (db_reader.HasRows)
                    {
                        while (db_reader.Read())
                        {
                            cmb_new_ved.Items.Add(new SelectData(db_reader["buk"].ToString(), db_reader["ID"].ToString(), db_reader["nameved"].ToString()));
                        }
                        db_reader.Close();
                        cmb_new_ved.SelectedIndex = 0;
                    }
                    db_reader.Close();
                }
                catch (Exception ex)
                {
                    if (!db_reader.IsClosed) db_reader.Close();
                    MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else MessageBox.Show("Соединение с Базой Данных прерванно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static string PERPLUS1(string per)
        {
            string newper = "", year = "", mon = "";
            year = per.Substring(0, 4);
            mon = per.Substring(4, 2);
            if (mon == "12") 
            {
                year = (Convert.ToInt32(per.Substring(0, 4))+1).ToString();
                newper = year+"01";
            }
            else
            {
                mon = (Convert.ToInt32(per.Substring(4, 2)) + 1).ToString();
                if (mon.Length == 1) mon = "0" + mon;
                newper = year + mon;
            }
            return newper;
        }

        private void btn_change_ved_Click(object sender, EventArgs e)
        {
            if (lbl_zero_ved.Visible == false)
            {
                try
                {
                    db_com.CommandText = "select * from abonuk.dbo.perehod where str_code='" + ((SelectData)cmb_street.SelectedItem).Value + "' and dom='" + ((SelectData)cmb_house.SelectedItem).Text + "' and perbeg="+PERPLUS1(frmMain.MaxCurPer)+"";
                    db_reader = db_com.ExecuteReader();
                    if (db_reader.HasRows)
                    {
                        db_reader.Close();
                        MessageBox.Show("Данный дом уже добавлен на смену ведомства", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (!db_reader.IsClosed) db_reader.Close();
                        string buks = ((SelectData)cmb_cur_ved.SelectedItem).buk == "True" ? "1" : "0";
                        string bukn = ((SelectData)cmb_new_ved.SelectedItem).buk == "True" ? "1" : "0";
                        db_com.CommandText = "insert into abonuk.dbo.Perehod(str_code,dom,old_ved,buks,new_ved,bukn,perbeg,deb_old,kred_old) values('" + ((SelectData)cmb_street.SelectedItem).Value + "','" + ((SelectData)cmb_house.SelectedItem).Text + "'," + ((SelectData)cmb_cur_ved.SelectedItem).Value + "," + buks + "," + ((SelectData)cmb_new_ved.SelectedItem).Value + "," + bukn + ",'" + PERPLUS1(frmMain.MaxCurPer) + "'," + ((deb_old.CheckState == CheckState.Checked) ? 1 : 0) + "," + ((kred_old.CheckState == CheckState.Checked) ? 1 : 0) + ")";
                        db_com.ExecuteNonQuery();
                        GetListPerehod();
                    }
                }
                catch (Exception ex)
                {
                    if (!db_reader.IsClosed) db_reader.Close();
                    MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Не выбрано ведомство", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gv_exist_list_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex <= gv_exist_list.Rows.Count) & (e.RowIndex >= 0))
            {
                if (gv_exist_list.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "Удалить")
                {
                    try
                    {
                        db_com.CommandText = "delete from abonuk.dbo.perehod where id = '"+gv_exist_list.Rows[e.RowIndex].Cells["id"].Value.ToString()+"' and done<>1";
                        db_com.ExecuteNonQuery();
                        GetListPerehod();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void cmb_house_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_house.DroppedDown = true;
        }

        private void cmb_cur_ved_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_cur_ved.DroppedDown = true;
        }

        private void cmb_new_ved_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_new_ved.DroppedDown = true;
        }

        private void Perehod_two()
        {
            List<string> id  = new List<string>();
            List<string> str = new List<string>();
            List<string> dom = new List<string>();
            List<string> ved = new List<string>();
            List<string> old_base = new List<string>();
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;

            try
            {
                db_com.CommandText = "select * from abonuk.dbo.perehod where done=0 and perbeg='" + frmMain.MaxCurPer + "'";
                db_reader = db_com.ExecuteReader();
                if (db_reader.HasRows)
                {
                    while (db_reader.Read())
                    {
                        id.Add(db_reader["id"].ToString());
                        str.Add(db_reader["str_code"].ToString());
                        dom.Add(db_reader["dom"].ToString());
                        ved.Add(db_reader["new_ved"].ToString());
                        if (db_reader["buks"].ToString() == "1" || db_reader["buks"].ToString() == "True") old_base.Add("AbonUk"); else old_base.Add("Abon");
                    }
                    db_reader.Close();
                    progressBar1.Maximum = id.Count - 1;
                    for(int i = 0;i<id.Count;i++)
                    {
                        db_com.CommandText = "exec "+old_base.ElementAt(i).ToString().Trim()+".dbo.transinnewcompany "+ved.ElementAt(i).ToString()+", '1=0 or (a.str_code=N''"+str.ElementAt(i).ToString().Trim()+"'' and a.dom=N''"+dom.ElementAt(i).ToString().Trim()+"'')'";
                        db_com.ExecuteNonQuery();
                        db_com.CommandText = "update abonuk.dbo.perehod set done=1 where id="+id.ElementAt(i).ToString()+"";
                        db_com.ExecuteNonQuery();
                        db_com.CommandText = "update abonuk.dbo.streetuch set idvendor = 0 WHERE cod_yl = '" + str.ElementAt(i).ToString().Trim() + "' and CAST(S as nvarchar) + ISNULL(Lit, '') = '" + dom.ElementAt(i).ToString().Trim() + "'";
                        db_com.ExecuteNonQuery();
                        db_com.CommandText = "update abon.dbo.streetuch set idvendor = 0 WHERE cod_yl = '" + str.ElementAt(i).ToString().Trim() + "' and CAST(S as nvarchar) + ISNULL(Lit, '') = '" + dom.ElementAt(i).ToString().Trim() + "'";
                        db_com.ExecuteNonQuery();
                        Application.DoEvents();
                    }
                    //db_com.CommandText = "exec abon.dbo.setvendortranshouses";
                    //db_com.ExecuteNonQuery();
                    //db_com.CommandText = "exec abonuk.dbo.setvendortranshouses";
                    //db_com.ExecuteNonQuery();
                }
                else db_reader.Close();
                progressBar1.Visible = false;
                GetListPerehod();
            }
            catch (Exception ex)
            {
                progressBar1.Visible = false;
                if (!db_reader.IsClosed) db_reader.Close();
                MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetListPerehod();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Perehod_two();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool result = true;
            ///переброска сальдо в водоканал
            db_com.Parameters.Clear();
            db_com.CommandText = @"select * from abonuk.dbo.perehod where perbeg=@per and (deb_old=1 or kred_old=1) and buks=1 and perebros=0";
            //db_com.Parameters.AddWithValue("@per", frmToNewVedom.PERPLUS1(frmMain.MaxCurPer));
            db_com.Parameters.AddWithValue("@per", frmToNewVedom.PERPLUS1(frmMain.CurPer));
            using (SqlDataReader read = db_com.ExecuteReader())
            {
                if (read.HasRows)
                {
                    NewMonth nm = new NewMonth(frmMain.db_con.ConnectionString);
                    while (read.Read())
                    {
                        if (!nm.Saldo27(read["str_code"].ToString(), read["dom"].ToString(), Convert.ToBoolean(read["deb_old"].ToString()), Convert.ToBoolean(read["kred_old"].ToString()))) result = false;
                        Application.DoEvents();
                    }
                }
            }
            if (result) MessageBox.Show("Сальдо по переходящим домам сформированно","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Information);
            else MessageBox.Show("Сальдо по переходящим домам не сформированно", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            NewMonth nm = new NewMonth();
            if (nm.vodomer2norm()) MessageBox.Show("Усе хорошо"); else MessageBox.Show("Где то жопа");
        }

        private void deb_old_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            datagrid2html prn = new datagrid2html();
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath() + "tmpNewVed.html", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmpNewVed.html";
            file.WriteLine("<html><head><title>Переход домов по ведомствам</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/>" +
                "<style rel='stylesheet' type='text/css'>" +
                "body {font-family: verdana; font-size:11pt; background-color: #FFFFFF;}" +
                "tbody td {font-size:9pt; }\n" +
                "thead td {font-size:9pt; }\n" +
                "tfoot td {font-size:11pt; }\n" +
                "</style></head><body>");
            file.WriteLine(prn.gv2html(gv_exist_list));
            file.WriteLine("</body></html>");
            file.Close();
            frmPrn frmCPrn = new frmPrn();
            frmCPrn.Text = "Печать список переходящих домов";
            frmCPrn.Show();
        }

    }
}

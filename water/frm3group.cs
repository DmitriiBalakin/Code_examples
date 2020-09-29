using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace water
{
    public partial class frm3group : Form
    {
        public frm3group()
        {
            InitializeComponent();
        }

        SqlCommand db_com = new SqlCommand();
        private Boolean only_new = false; //// если оставляем л/с с 3 группой по старым договорам в доме, то true

        private void frm3group_Shown(object sender, EventArgs e)
        {
            db_com.Connection = frmMain.db_con;
            db_com.CommandTimeout = 0;

            btn_3group.Enabled = false;

            string date = frmMain.MaxCurPer.Substring(4, 2) + "-" + frmMain.MaxCurPer.Substring(0, 4);
            label6.Text = "от 01-" + date+" г.";
            
            /// заполнение улиц
            if (frmMain.db_con.State == ConnectionState.Open)
            {
                try
                {
                    db_com.CommandText = "select id, nameved from abonuk.dbo.spvedomstvo";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        while (db_read.Read())
                        {
                            string result = db_read["nameved"].ToString();
                            cmb_newvedomstvo.Items.Add(new SelectData(Convert.ToString(db_read["id"]), result));
                        }
                        db_read.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

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
            else MessageBox.Show("Соединение с базой данных не установлено.", "Ошибка");
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_house.Items.Clear();
            cmb_house.Text = "";
            string street = ((SelectData)this.cmb_street.SelectedItem).Value;
            try
            {
                db_com.CommandText = "select dom from abon.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' group by dom order by dom";

                using (SqlDataReader db_read_house = db_com.ExecuteReader())
                {
                    if (db_read_house.HasRows)
                    {
                        while (db_read_house.Read())
                        {
                            string result = Convert.ToString(db_read_house["dom"]);
                            ///result += Convert.ToString(db_read_house["lit"]);
                            cmb_house.Items.Add(new SelectData("", result));
                        }
                        db_read_house.Close();
                        if (this.cmb_house.Items.Count > 0)
                        {
                            cmb_house.SelectedIndex = 0;
                        }
                    }
                    
                }
                
            }
            catch(Exception error)
            {
                MessageBox.Show("Ошибка получения списка домов.\n"+error.Message, "Ошибка");
            }
        }

        private void cmb_street_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_street.DroppedDown = true;
        }

        private void cmb_house_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_house.DroppedDown = true;
        }

        private void cmb_house_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_3group.Enabled = false;
            label7.Text = "";
            try
            {
                db_com.CommandText = "select a.lic, a.flat from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=0 and a.str_code=@str and a.dom=@dom union all select a.lic, a.flat from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=1 and a.str_code=@str and a.dom=@dom";
                db_com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.ToString().Trim());
                db_com.Parameters.AddWithValue("@dom", cmb_house.Text.Trim());
                using (SqlDataReader r = db_com.ExecuteReader())
                {
                    if (r.HasRows)
                    {
                        cmb_flat.Items.Clear();
                        cmb_flat.Text = "";
                        while (r.Read())
                        {
                            cmb_flat.Items.Add(r["lic"].ToString()+" | "+r["flat"].ToString());
                        }
                    }
                }
                db_com.Parameters.Clear();
                string street = ((SelectData)this.cmb_street.SelectedItem).Value;
                db_com.CommandText = "select top (1) a.kodvedom, a.vedom, UK = case(v.buk) when '1' then '1' when '0' then '0' end from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on a.kodvedom = v.id where a.str_code = '" + street + "' and a.dom='"+cmb_house.Text+"'";
                
                using (SqlDataReader db_read_house = db_com.ExecuteReader())
                {
                    db_read_house.Read();
                    if (db_read_house.HasRows)
                    {
                        lbl_vedomstvo.Text = db_read_house["vedom"].ToString();
                        if (db_read_house["uk"].ToString() == "1") lbl_vedomstvo.Text = lbl_vedomstvo.Text + " (УК)";
                    }
                    db_read_house.Close();
                }

                db_com.CommandText = "select count(lic) as count from abon.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' and dom='" + cmb_house.Text + "'";
                int count_lic = 0;
                using (SqlDataReader db_read_house = db_com.ExecuteReader())
                {
                    db_read_house.Read();
                    if (db_read_house.HasRows)
                        count_lic = Convert.ToInt32(db_read_house["count"].ToString());
                }
                db_com.CommandText = "select count(lic) as count from abon.dbo.abonent" + frmMain.CurPer + " where str_code = '" + street + "' and dom='" + cmb_house.Text + "' and gruppa3=1";
                int count_gr3 = 0;
                using (SqlDataReader db_read_house = db_com.ExecuteReader())
                {
                    db_read_house.Read();
                    if (db_read_house.HasRows)
                        count_gr3 = Convert.ToInt32(db_read_house["count"].ToString());
                }
                if (count_lic == count_gr3)
                {
                    btn_3group.Enabled = false;
                    MessageBox.Show("Данный дом находится в 3 группе", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (count_gr3 > 0) 
                {
                    DialogResult dlg_res = MessageBox.Show("В данном доме есть л/с в 3 группе\nОставить по ним прежний договор?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dlg_res == System.Windows.Forms.DialogResult.Yes) 
                    {
                        only_new = true;
                        btn_3group.Enabled = true;
                        label7.Text = "Л/с в 3 группе сохранят старые договора";
                    }
                    else 
                    {
                        btn_3group.Enabled = true;
                        label7.Text = "Л/с в 3 группе будет присвоен новый договор";
                    }


                }
                else btn_3group.Enabled = true;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void btn_3group_Click(object sender, EventArgs e)
        {
            string street = ((SelectData)this.cmb_street.SelectedItem).Value;
            SqlTransaction trans = frmMain.db_con.BeginTransaction();
            try
            {
                db_com.CommandText = "update abon.dbo.abonent" + frmMain.CurPer + " set gruppa3='1', Kontr='20' where str_code='" + street + "' and dom='" + cmb_house.Text + "' and gruppa3 <> '1'";
                db_com.Transaction = trans;
                db_com.ExecuteNonQuery();
                db_com.CommandText = "delete from abon.dbo.group3dog where lic in (select lic from abon.dbo.abonent" + frmMain.CurPer + " where str_code='" + street + "' and dom='" + cmb_house.Text + "')";
                db_com.ExecuteNonQuery();
                string num_dog = Convert.ToString(maskedTextBox1.Text);
                string date = DateTime.Now.ToString("01-MM-yyyy");
                db_com.CommandText = "insert into ";
                db_com.ExecuteNonQuery();
                trans.Commit();
                MessageBox.Show("Дом переведен в 3 группу", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                trans.Rollback();
                MessageBox.Show(ex.Message+"\n Изменения по лицевым счетам отменены.\nОбратитесь в отдел АСУ.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_newvedomstvo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_newvedomstvo.SelectedIndex != -1)
            {
            db_com.CommandText = "select uk = case(buk) when '1' then '1' when '0' then '0' end from abonuk.dbo.spvedomstvo where id='" + ((SelectData)this.cmb_newvedomstvo.SelectedItem).Value.ToString() +"'";
            lbl_buk.Text = "";
            try
            {
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    db_read.Read();
                    if (db_read.HasRows)
                    {
                        if (db_read["uk"].ToString() == "1") lbl_buk.Text = "УК";
                    }
                    db_read.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }
        }

        private void cmb_flat_KeyPress(object sender, KeyPressEventArgs e)
        {
            cmb_flat.DroppedDown = true;
        }

    }
}

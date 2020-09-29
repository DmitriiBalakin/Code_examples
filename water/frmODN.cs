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
using System.Globalization;

namespace water
{
    public partial class frmODN : Form
    {
        SqlCommand db_com = new SqlCommand();
        List<string[]> houses = new List<string[]>();
        public frmODN()
        {
            InitializeComponent();
            db_com.Connection = frmMain.db_con;
        }

        private void frmODN_Shown(object sender, EventArgs e)
        {
            gv_odn.Rows.Clear();
            maskedTextBox1.Text = "-003,00";
            maskedTextBox2.Text = "020,00";
            db_com.Parameters.Clear();
            if (frmMain.MaxCurPer != frmMain.CurPer)
            {
                button1.Enabled = false;
                button3.Enabled = false;
            }
            if (frmMain.db_con.State == ConnectionState.Open)
            {
                /// список домов с ОДН
                try
                {
                    /// заполняем грид
                    db_com.CommandText = @"SELECT b.House_Code, c.Code_Yl,
                                            c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse as dom, v.HomeVolume, v.OrgVolume, v.HouseVolume, (-v.HomeVolume-v.OrgVolume+v.HouseVolume) as ODN,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation, sum(aa.liver-aa.vibilo-aa.vibilo2) as liver, isnull(case when (-v.HomeVolume-v.OrgVolume+v.HouseVolume)>0 then ((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/(b.AreaHabitation+b.AreaNotHabitation)) else ((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/sum(aa.liver-aa.vibilo-aa.vibilo2)) end,0) as k,b.IsCalcODN
                                            FROM [Common].[dbo].[spHouses] a, [Common].[dbo].[HousesData] b left join Common.dbo.volume v on v.House_Code=b.House_Code and v.TypeService_Code = 1 and v.PerCalc=@per, [Common].[dbo].[SpStreets] c,
                                            [Common].[dbo].[HouseCounterAdres] d,
                                            abon.dbo.SpVedomstvo vv, abon.dbo.abonent" + frmMain.MaxCurPer+ @" aa
                                            WHERE b.perend=0 and b.House_code = a.id_house  and
                                            a.Street_Code = c.Id_Street  
                                            and (b.IsCalcODN = 1 or b.IsCalcODN = 2) and aa.Str_code = c.Code_Yl and aa.dom=(cast(a.numhouse as nvarchar(10)) + a.LitHouse) and aa.gruppa3=0 and aa.KodVedom=vv.ID and vv.bUK=0 and vv.bPaketC=1 
                                            group by b.House_Code, c.Code_Yl,
                                            c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse, v.HomeVolume, v.OrgVolume, v.HouseVolume, -v.HomeVolume-v.OrgVolume+v.HouseVolume,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation, b.IsCalcODN
                                            order by c.Nm_Street";
                    db_com.Parameters.AddWithValue("@per", frmMain.MaxCurPer);
                    using (SqlDataReader read = db_com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            gv_odn.Rows.Clear();
                            SqlConnection tar_con = new SqlConnection();
                            tar_con.ConnectionString = frmMain.db_con.ConnectionString;
                            tar_con.Open();
                            while (read.Read())
                            {
                                CalculateWater.Tarifs tarif = new CalculateWater.Tarifs(frmMain.MaxCurPer, frmMain.MaxCurPer, read["code_yl"].ToString(), tar_con, (byte)0);
                                string[] row = {"False",read["house_code"].ToString(), read["code_yl"].ToString(), read["nm_street"].ToString(), read["dom"].ToString(), read["homevolume"].ToString(), read["orgvolume"].ToString(), read["housevolume"].ToString(), read["odn"].ToString(), read["allarea"].ToString(), read["areahabitation"].ToString(), read["areanothabitation"].ToString(), read["liver"].ToString(), read["k"].ToString(),"0","0",tarif.TarV.ToString(), read["iscalcODN"].ToString() };
                                gv_odn.Rows.Add(row);
                            }
                            tar_con.Close();
                        }
                    }
                    db_com.Parameters.Clear();
                    db_com.CommandText = @"SELECT b.House_Code, c.Code_Yl,
                                            c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse as dom, v.HomeVolume, v.OrgVolume, v.HouseVolume, (-v.HomeVolume-v.OrgVolume+v.HouseVolume) as ODN,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation, sum(aa.liver-aa.vibilo-aa.vibilo2) as liver, isnull(case when (-v.HomeVolume-v.OrgVolume+v.HouseVolume)>0 then ((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/(b.AreaHabitation+b.AreaNotHabitation)) else ((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/sum(aa.liver-aa.vibilo-aa.vibilo2)) end,0) as k,b.IsCalcODN
                                            FROM [Common].[dbo].[spHouses] a, [Common].[dbo].[HousesData] b left join Common.dbo.volume v on v.House_Code=b.House_Code and v.TypeService_Code = 1 and v.PerCalc=@per, [Common].[dbo].[SpStreets] c,
                                            [Common].[dbo].[HouseCounterAdres] d,
                                            abonuk.dbo.SpVedomstvo vv, abonuk.dbo.abonent" + frmMain.MaxCurPer + @" aa
                                            WHERE b.perend=0 and b.House_code = a.id_house  and
                                            a.Street_Code = c.Id_Street  
                                            and (b.IsCalcODN = 1 or b.IsCalcODN = 2) and aa.Str_code = c.Code_Yl and aa.dom=(cast(a.numhouse as nvarchar(10)) + a.LitHouse) and aa.gruppa3=0 and aa.KodVedom=vv.ID and vv.bUK=0 and vv.bPaketC=1 
                                            group by b.House_Code, c.Code_Yl,
                                            c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse, v.HomeVolume, v.OrgVolume, v.HouseVolume, -v.HomeVolume-v.OrgVolume+v.HouseVolume,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation, b.IsCalcODN 
                                            order by c.Nm_Street";
                    db_com.Parameters.AddWithValue("@per", frmMain.MaxCurPer);
                    using (SqlDataReader read = db_com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            SqlConnection tar_con = new SqlConnection();
                            tar_con.ConnectionString = frmMain.db_con.ConnectionString;
                            tar_con.Open();
                            
                            while (read.Read())
                            {
                                CalculateWater.Tarifs tarif = new CalculateWater.Tarifs(frmMain.MaxCurPer, frmMain.MaxCurPer, read["code_yl"].ToString(), tar_con, (byte)1);
                                string[] row = { "False", read["house_code"].ToString(), read["code_yl"].ToString(), read["nm_street"].ToString(), read["dom"].ToString(), read["homevolume"].ToString(), read["orgvolume"].ToString(), read["housevolume"].ToString(), read["odn"].ToString(), read["allarea"].ToString(), read["areahabitation"].ToString(), read["areanothabitation"].ToString(), read["liver"].ToString(), read["k"].ToString(), "0", "0" ,tarif.TarV.ToString(), read["iscalcODN"].ToString() };
                                gv_odn.Rows.Add(row);
                            }
                            tar_con.Close();
                        }
                    }

                    for (int i = 0; i < gv_odn.Rows.Count; i++)
                    {
                        if (("" + gv_odn.Rows[i].Cells["ss"].ToString()) == "") { gv_odn.Rows[i].Cells["ss"].Value = "0"; continue; }
                        if (Convert.ToDouble(gv_odn.Rows[i].Cells["k"].Value.ToString()) >= 0)
                        {
                            gv_odn.Rows[i].Cells["ss"].Value = "50";
                            
                            gv_odn.Rows[i].Cells["rub"].Value = Convert.ToString(Math.Round( Convert.ToDouble(gv_odn.Rows[i].Cells["k"].Value.ToString().Trim()) * Convert.ToDouble(gv_odn.Rows[i].Cells["ss"].Value.ToString().Trim()) * Convert.ToDouble(gv_odn.Rows[i].Cells["tar"].Value.ToString()), 2));
                        }
                        else
                        {
                            gv_odn.Rows[i].Cells["ss"].Value = "1";
                            
                            gv_odn.Rows[i].Cells["rub"].Value = Convert.ToString(Math.Round(Convert.ToDouble(gv_odn.Rows[i].Cells["k"].Value.ToString().Trim()) * Convert.ToDouble(gv_odn.Rows[i].Cells["ss"].Value.ToString().Trim()) *  Convert.ToDouble(gv_odn.Rows[i].Cells["tar"].Value.ToString()), 2));
                        }
                        if (gv_odn.Rows[i].Cells["iscalcODN"].Value.ToString() ==  "2")
                        {
                            gv_odn.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                            
                        }
                        else
                        {
                            gv_odn.Rows[i].DefaultCellStyle.BackColor = Color.LightSkyBlue;
                        }
                    }
                }
                catch { }
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


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result;
            result = MessageBox.Show("Выполнить расчет ОДН?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                CalcODN Calc = new CalcODN(frmMain.MaxCurPer);
                Calc.CalculateODN(progress);
            }
            frmODN_Shown(this,e);
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;
            MessageBox.Show("Расчет ОДН выполнен", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
           /// CalculateWater.ODNCalculate Calc = new CalculateWater.ODNCalculate(frmMain.db_con, frmMain.CurPer, frmMain.CurPer, Convert.ToInt32(label1.Text));
        }

        private void progress(int max, int cur)
        {
            progressBar1.Maximum = max;
            progressBar1.Value = cur;
            Application.DoEvents();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> lic = new List<string>();
            for (int i = 0; i < gv_odn.Rows.Count; i++)
            {
                if (gv_odn.Rows[i].Cells["chk_notprn"].Value.ToString() == "True")
                {
                    /// ОПАСНО ПЕРИОД
                    db_com.CommandText = @"select a.lic from abon.dbo.abonent" + frmMain.MaxCurPer + @" a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=0 and v.bpaketc = 1 where a.str_code=@str and a.dom=@dom
                                      union all
                                      select a.lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + @" a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom and v.buk=1 and v.bpaketc = 1 where a.str_code=@str and a.dom=@dom ";
                    db_com.Parameters.AddWithValue("@str", gv_odn.Rows[i].Cells["str_code"].Value.ToString().Trim());
                    db_com.Parameters.AddWithValue("@dom",  gv_odn.Rows[i].Cells["dom"].Value.ToString().Trim());
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

                    db_com.CommandText = "delete from common.dbo.volume where house_code=@house and percalc=@per";
                    db_com.Parameters.AddWithValue("@house",  gv_odn.Rows[i].Cells["house_code"].Value.ToString().Trim());
                    db_com.Parameters.AddWithValue("@per", frmMain.CurPer);
                    db_com.ExecuteNonQuery();
                    db_com.Parameters.Clear();
                }
            }

            if (lic.Count > 0)
            {
                progressBar1.Maximum = lic.Count;
                progressBar1.Value = 0;
                for(int i=0;i<lic.Count;i++)
                {
                    /// ОПАСНО ПЕРИОД
                    db_com.CommandText = "update a set a.overcubev = 0, a.overnv_full = 0 from " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonent" + frmMain.CurPer + " a where a.lic=@lic";
                    db_com.Parameters.AddWithValue("@lic", lic.ElementAt(i).ToString().Trim());
                    db_com.ExecuteNonQuery();
                    db_com.CommandText = "delete from " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.oplata" + frmMain.CurPer + " where lic=@lic and basisrecid=42";
                    db_com.ExecuteNonQuery();
                    db_com.Parameters.Clear();
                    if (frmMain.CurPer != frmMain.MaxCurPer)
                    {
                        db_com.Parameters.AddWithValue("@lic", lic.ElementAt(i).ToString().Trim());
                        db_com.CommandText = "update n set n.saldo = p.saldo + p.charge + p.corrcharge -p.pay from " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonservsaldo" + frmMain.MaxCurPer + " n, " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonservsaldo" + frmMain.CurPer + " p where n.lic=p.lic and n.abonserv_code=p.abonserv_code and n.lic=@lic";
                        db_com.ExecuteNonQuery();
                        db_com.Parameters.Clear();
                        db_com.Parameters.AddWithValue("@lic", lic.ElementAt(i).ToString().Trim());
                        db_com.CommandText = "update n set n.saldoodn = p.saldoodn + p.chargeodn + p.corrodncharge -p.payodn from " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonservsaldo" + frmMain.MaxCurPer + " n, " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonservsaldo" + frmMain.CurPer + " p where n.lic=p.lic and n.abonserv_code=p.abonserv_code and n.lic=@lic";
                        db_com.ExecuteNonQuery();
                        db_com.Parameters.Clear();
                        db_com.CommandText = "update n set n.sndeb=p.skdeb, n.snkred=p.skkred, n.sdolgbeg = p.sdolgend from " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonent" + frmMain.MaxCurPer + " n, " + ((lic.ElementAt(i).ToString().Substring(0, 1) == "2") ? "abonuk" : "abon") + ".dbo.abonent" + frmMain.CurPer + " p where n.lic=p.lic and n.lic = @lic";
                        db_com.Parameters.AddWithValue("@lic", lic.ElementAt(i).ToString().Trim());
                        db_com.ExecuteNonQuery();
                        db_com.Parameters.Clear();
                    }
                    if (i % 10 == 0) { progressBar1.Value = i; Application.DoEvents(); }
                }
                progressBar1.Value = 0;
                frmODN_Shown(this,e);
            }

        }

        private void gv_odn_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
}

        private void gv_odn_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gv_odn.Rows[e.RowIndex].Cells["chk_notprn"].Value.ToString() == "False")
            {
                string[] row = { "True" };
                gv_odn.Rows[e.RowIndex].Cells["chk_notprn"].Value = row[0];
            }
            else
            {
                string[] row = { "False" };
                gv_odn.Rows[e.RowIndex].Cells["chk_notprn"].Value = row[0];
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            datagrid2html prn = new datagrid2html();
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath() + "tmpODN.html", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmpODN.html";
            file.WriteLine("<html><head><title>Начисление ОДН</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/>"+
                "<style rel='stylesheet' type='text/css'>"+
                "body {font-family: verdana; font-size:11pt; background-color: #FFFFFF;}" +
                "tbody td {font-size:9pt; }\n"+
                "thead td {font-size:9pt; }\n" +
                "tfoot td {font-size:11pt; }\n" +
                "</style></head><body>");
            file.WriteLine(prn.gv2html(gv_odn));
            file.WriteLine("</body></html>");
            file.Close();
            frmPrn frmCPrn = new frmPrn();
            frmCPrn.Text = "Печать Начисление ОДН";
            frmCPrn.Show();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox1.CheckState == CheckState.Checked)
                {
                    for (int i = 0; i < gv_odn.Rows.Count; i++)
                    {
                        if (Convert.ToDouble(gv_odn.Rows[i].Cells["rub"].Value.ToString()) < Convert.ToDouble("-0" + maskedTextBox1.Text.Replace("-","").Replace(" ", "")))
                        {
                            gv_odn.Rows[i].Cells["chk_notprn"].Value = "True";
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < gv_odn.Rows.Count; i++)
                    {
                        if (Convert.ToDouble(gv_odn.Rows[i].Cells["rub"].Value.ToString()) < Convert.ToDouble("-0" + maskedTextBox1.Text.Replace("-","").Replace(" ", "")))
                        {
                            gv_odn.Rows[i].Cells["chk_notprn"].Value = "False";
                        }
                    }
                }
            }
            catch { }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.CheckState == CheckState.Checked)
            {
                for (int i = 0; i < gv_odn.Rows.Count; i++)
                {
                    if (Convert.ToDouble(gv_odn.Rows[i].Cells["rub"].Value.ToString()) > Convert.ToDouble("0" + maskedTextBox2.Text.Replace(" ", "")))
                    {
                        gv_odn.Rows[i].Cells["chk_notprn"].Value = "True";
                    }
                }
            }
            else
            {
                for (int i = 0; i < gv_odn.Rows.Count; i++)
                {
                    if (Convert.ToDouble(gv_odn.Rows[i].Cells["rub"].Value.ToString()) > Convert.ToDouble("0" + maskedTextBox2.Text.Replace(" ", "")))
                    {
                        gv_odn.Rows[i].Cells["chk_notprn"].Value = "False";
                    }
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            frmODN_Shown(this,e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //CalcODN Calc = new CalcODN("201907", 83910);
            //Calc.CalculateODN(progress);
            
            DialogResult result;
            result = MessageBox.Show("Выполнить расчет ОДН?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i < gv_odn.Rows.Count; i++)
                {
                    if (gv_odn.Rows[i].Cells["chk_notprn"].Value.ToString() == "True")
                    {
                        CalcODN Calc = new CalcODN(frmMain.MaxCurPer, Convert.ToInt32(gv_odn.Rows[i].Cells["house_code"].Value.ToString()));
//                        CalcODN Calc = new CalcODN("201901", Convert.ToInt32(gv_odn.Rows[i].Cells["house_code"].Value.ToString()));
                        Calc.CalculateODN(progress);
                        gv_odn.Rows[i].Cells["chk_notprn"].Value = "False";
                    }
                }
            }
            frmODN_Shown(this, e);
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;
            MessageBox.Show("Расчет ОДН выполнен", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
  
        }
    }
}
/*SELECT b.House_Code, c.Code_Yl, b.IsCalcODN,
                                            c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse as dom, v.HomeVolume, v.OrgVolume, v.HouseVolume, (-v.HomeVolume-v.OrgVolume+v.HouseVolume) as ODN,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation, sum(aa.liver-aa.vibilo-aa.vibilo2) as liver, isnull(case when (-v.HomeVolume-v.OrgVolume+v.HouseVolume)>0 then((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/(b.AreaHabitation+b.AreaNotHabitation)) else ((-v.HomeVolume-v.OrgVolume+v.HouseVolume)/sum(aa.liver-aa.vibilo-aa.vibilo2)) end,0) as k
                                           FROM[Common].[dbo].[spHouses]
a, [Common].[dbo].[HousesData]
b left join Common.dbo.volume v on v.House_Code= b.House_Code and v.TypeService_Code = 1 and v.PerCalc= 201808, [Common].[dbo].[SpStreets] c,
[Common].[dbo].[HouseCounterAdres] d,
abon.dbo.SpVedomstvo vv, abon.dbo.abonent201808 aa

WHERE b.perend= 0 and b.House_code = a.id_house  and
a.Street_Code = c.Id_Street--and a.Id_House = d.House_Code

and (b.IsCalcODN = 1 or b.IsCalcODN = 2) and aa.Str_code = c.Code_Yl and aa.dom= (cast(a.numhouse as nvarchar(10)) + a.LitHouse) and aa.gruppa3= 0 and aa.KodVedom= vv.ID and vv.bUK= 0 and vv.bPaketC= 1

group by b.House_Code, c.Code_Yl, b.IsCalcODN,
c.Nm_Street, cast(a.numhouse as nvarchar(10)) + a.LitHouse, v.HomeVolume, v.OrgVolume, v.HouseVolume, -v.HomeVolume-v.OrgVolume+v.HouseVolume,
                                            b.AllArea, b.AreaHabitation, b.AreaNotHabitation
                                            order by c.Nm_Street*/

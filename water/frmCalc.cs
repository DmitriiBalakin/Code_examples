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

    public partial class frmCalc : Form
    {

        SqlCommand db_com = new SqlCommand();
        bool can_close = true;
        double TarifWaterODN = 19.47;

        private SqlConnection db_con = new SqlConnection();
        public frmCalc()
        {
            InitializeComponent();
            db_con.ConnectionString = "Data Source=SERVERAB;Initial Catalog=Abon;Integrated Security=True;Persist Security Info=False;User ID=SqlAbon;Connect Timeout=0;TrustServerCertificate=False";
            db_com.Connection = frmMain.db_con;
            db_com.CommandType = CommandType.Text;
            db_com.CommandTimeout = 0;
        }


        public void CalcDeleg1(int X, int Y, string lic)
        {
            this.progressBar1.Maximum = X;
            this.progressBar1.Value = Y;
            label3.Text = Y.ToString() + " из " + X.ToString() + "         " + lic;
            Application.DoEvents();
        }
        
        public void CalcDeleg2(string Z)
        {
            this.label1.Text = Z.Trim();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection calc_con = new SqlConnection();
            calc_con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                calc_con.Open();
            if (System.Windows.Forms.DialogResult.Yes == MessageBox.Show("Произвести начисление?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                CalculateWater.MainForm Calc = new CalculateWater.MainForm();
                panel1.Enabled = false;
                can_close = false;
                if (chkG.CheckState == CheckState.Checked)
                {
                    chkG.CheckState = CheckState.Unchecked;
                    label1.Text = "Начисление Жилье";
                    timer1.Enabled = true;
                    Calc.Query(CalculateWater.MainForm.Abon, frmMain.MaxCurPer, "", CalcDeleg1, CalcDeleg2);
                    while (!label1.Text.Contains("выполнены")) { }
                    timer1.Enabled = false;
                }
                if (chkU.CheckState == CheckState.Checked)
                {
                    chkU.CheckState = CheckState.Unchecked;
                    label1.Text = "Начисление Жилье УК";
                    timer1.Enabled = true;
                    Calc.Query(CalculateWater.MainForm.AbonUK, frmMain.MaxCurPer, "", CalcDeleg1, CalcDeleg2,calc_con);
                    while (!label1.Text.Contains("выполнены")) { }
                    timer1.Enabled = false;
                }

                if (cmb_vedom.Text != "")
                {
                    label1.Text = "Начисление для ведомства";
                    List<string> lic = new List<string>();

                    progressBar1.Maximum = 0;
                    progressBar1.Value = 0;
                    try
                    {
                        can_close = false;
                        byte base_ = 0;
                        db_com.CommandText = "select lic from abon.dbo.abonent" + frmMain.MaxCurPer + " a inner join abon.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 0 and v.bpaketc = 1 where a.str_code='" + ((SelectData)cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "'" +
                            "union all select lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id = a.kodvedom and v.buk = 1 and v.bpaketc = 1 where a.str_code='" + ((SelectData)cmb_street.SelectedItem).Value + "' and dom='" + cmb_house.Text + "'";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                while (db_read.Read())
                                {
                                    lic.Add(db_read["lic"].ToString());
                                }
                            }
                        }


                        progressBar1.Maximum = lic.Count;
                        for (int i = 0; i < lic.Count; i++)
                        {
                            if (lic.ElementAt(i).Substring(0, 1) == "1") base_ = 0;
                            if (lic.ElementAt(i).Substring(0, 1) == "2") base_ = 1;
                            Calc.Query(base_, frmMain.MaxCurPer, lic.ElementAt(i).ToString(), null, null, calc_con);
                            progressBar1.Value++;
                            Application.DoEvents();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (cmb_street.Text != "" && cmb_house.Text != "")
                {
                    label1.Text = "Начисление для дома";
                    List<string> lic = new List<string>();

                    progressBar1.Maximum = 0;
                    progressBar1.Value = 0;
                    try
                    {
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
                                }
                            }
                        }


                        progressBar1.Maximum = lic.Count;
                        for (int i = 0; i < lic.Count; i++)
                        {
                            if (lic.ElementAt(i).Substring(0, 1) == "1") base_ = 0;
                            if (lic.ElementAt(i).Substring(0, 1) == "2") base_ = 1;
                            Calc.Query(base_, frmMain.CurPer, lic.ElementAt(i).ToString(), null, null, calc_con);
                            progressBar1.Value++;
                            Application.DoEvents();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                progressBar1.Value = 0;
                progressBar1.Maximum = 0;
                label1.Text = "Начисления выполнены";
                can_close = true;
                panel1.Enabled = true;
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmCalc_Shown(object sender, EventArgs e)
        {
            DateTime dt = new DateTime(Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)), DateTime.DaysInMonth(Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2))));
            label6.Text = "Текущий период: " + dt.ToString("MMMM yyyy");
            label1.Text = "";
            label3.Text = "";
            try
            {
                db_con.Open();
            }
            catch { }
            if (db_con.State == ConnectionState.Open)
            {
                try
                {
                    db_com.CommandText = "select cod_yl, yl_name from Abon.dbo.Street where cod_yl <> 'тмп' order by yl_name";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            cmb_street.Items.Add("");
                            while (db_read.Read())
                            {
                                string result = Convert.ToString(db_read["yl_name"]);
                                result = result.Trim();
                                this.cmb_street.Items.Add(new SelectData(db_read["cod_yl"].ToString(), result));
                            }
                            db_read.Close();
                            if (this.cmb_street.Items.Count > 0)
                            {
                                this.cmb_street.SelectedIndex = -1;
                            }
                        }
                    }
                    db_com.CommandText = "select id,nameved from abonuk.dbo.spvedomstvo order by nameved,id";
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            cmb_vedom.Items.Add("");
                            while (db_read.Read())
                            {
                                this.cmb_vedom.Items.Add(new SelectData(db_read["id"].ToString(),db_read["nameved"].ToString()));
                            }
                        }
                    }
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
                    db_con.Open();
                }
                catch
                {
                    MessageBox.Show("Не удалось восстановить подключение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_street.Text != "")
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
            else
            {
                cmb_house.Items.Clear();
                cmb_house.Text = "";
            }
        }

        private void frmCalc_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!can_close) { e.Cancel = true; MessageBox.Show("Дождитесь завершения расчета", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.DialogResult.Yes == MessageBox.Show("Произвести расчет среднего?","Внимание",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
            {
            panel1.Enabled = false;
            //////
            SqlConnection db_con = new SqlConnection();
            db_con.ConnectionString = frmMain.db_con.ConnectionString;
            db_con.Open();
            SqlCommand db_cmd = new SqlCommand();
            db_cmd.CommandType = CommandType.Text;
            //////
            try
            {
                SetMeanCube Calc = new SetMeanCube(db_con);

                if (chkGS.CheckState == CheckState.Checked) { label1.Text = "Начисление среднего Жилье"; if (Calc.CalculateAllMean(CalcDeleg1, 0, frmMain.MaxCurPer, (ODN.CheckState == CheckState.Checked) ? true : false)) label1.Text = "Начисление среднего Жилье завершено"; }
                if (chkUS.CheckState == CheckState.Checked) { label1.Text = "Начисление среднего Жилье УК"; if (Calc.CalculateAllMean(CalcDeleg1, 1, frmMain.MaxCurPer, (ODN.CheckState == CheckState.Checked) ? true : false)) label1.Text = "Начисление среднего Жилье УК завершено"; }
                if (maskedTextBox1.Text.Trim().Length == 10)
                {
                    maskedTextBox1.Text.Replace(" ", "");
                    if (maskedTextBox1.Text.Length == 10) Calc.CalculateMean(maskedTextBox1.Text, frmMain.MaxCurPer);
                }

                db_con.Close();
            }
            catch { } 
            panel1.Enabled = true;
            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = 0;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // --------------------- Начисление пени ------------------
            SqlConnection conn = new SqlConnection(db_con.ConnectionString);
            conn.Open();
            CalcPeni calcPeni;
            calcPeni = new CalcPeni(1, "201705", conn, "2333920001");
            calcPeni.CalcPeniAll();
            MessageBox.Show("начислено!", "Все");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------------------------------------------------------------------------------------------------
            //int j = 0;
            //CalculatePeni сalculatePeni1 = new CalculatePeni("1000065800", frmMain.MaxCurPer);
            //j += сalculatePeni1.CalcPeniLic();

            //-----------------------------------------------------------------------------------------------------------------------------------------
            // --------------------- Начисление пени ------------------
            bool b = false;
            SqlConnection conn = new SqlConnection(db_con.ConnectionString);
            conn.Open();
            List<string> Lics = new List<string>();
            string sqlstr = @"UPDATE Abon.dbo.AbonServSaldo" + frmMain.MaxCurPer + @" SET Charge = 0 WHERE AbonServ_Code = 6;
                      UPDATE AbonUK.dbo.AbonServSaldo" + frmMain.MaxCurPer + @" SET Charge = 0 WHERE AbonServ_Code = 6;";
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            if (b) cmd.ExecuteNonQuery();
            sqlstr = "UPDATE Abon.dbo.Abonent" + frmMain.MaxCurPer + @" SET Peni = 0 
                      UPDATE AbonUK.dbo.Abonent" + frmMain.MaxCurPer + @" SET Peni = 0;";
            cmd = new SqlCommand(sqlstr, conn);
            if (b) cmd.ExecuteNonQuery();
            sqlstr = "SELECT lic FROM abonuk.dbo.abonent" + frmMain.MaxCurPer + @" WHERE kodvedom IN (SELECT ID FROM abonuk.dbo.spvedomstvo WHERE bUK = 1 and bPaketC = 1)
                             and Gruppa3 = 0 and prochee = 0 and lic >= '200000000' and sdolgbeg - nachisl - poliv - overnv_full > 100 and sdolgbeg > 200 and sdolgend > 200 " +
                             (!b ? " and Peni = 0 ": "") +
                             @"UNION ALL
                             SELECT lic FROM abon.dbo.abonent" + frmMain.MaxCurPer + @" WHERE kodvedom IN (SELECT ID FROM abon.dbo.spvedomstvo WHERE bUK = 0 and bPaketC = 1)
                             and Gruppa3 = 0 and prochee = 0 and lic >= '100000000' and sdolgbeg - nachisl - poliv - overnv_full > 100 and sdolgbeg > 200 and sdolgend > 200 " +
                             (!b ? " and Peni = 0 " : "") +
                             "";
            cmd = new SqlCommand(sqlstr, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                    {
                        Lics.Add(DRaeder["lic"].ToString());
                    }
                }
            }
            label1.Text = DateTime.Now.ToString() + "    Начисление пени";
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = Lics.Count;
            int j = 0;
            foreach (string Lic in Lics)
            {
                CalculatePeni сalculatePeni = new CalculatePeni(Lic, frmMain.MaxCurPer);
                j += сalculatePeni.CalcPeniLic();
                progressBar1.Value++;
                label3.Text = progressBar1.Value.ToString() + " / " + j.ToString() + " из " + progressBar1.Maximum.ToString() + "      " + Lic;
                Application.DoEvents(); 
            }


            //CalcPeni calcPeni;
            //calcPeni = new CalcPeni(1, frmMain.MaxCurPer, conn);
            //calcPeni.CalcPeniAll();
            //calcPeni = new CalcPeni(4, frmMain.MaxCurPer, conn);
            label1.Text = label1.Text + "    " + DateTime.Now.ToString() +  "    Пеня начислена!";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // ------------------- Начисление ОДН ---------------------
            string str_sql;
            SqlCommand cmd;
            int Counts;

            str_sql = @"DELETE FROM Common.dbo.Volume WHERE PerSave = " + frmMain.MaxCurPer;
            cmd = new SqlCommand(str_sql, db_con);
            cmd.CommandTimeout = 0;
            Counts = cmd.ExecuteNonQuery();
            label1.Text = "Начисление ODN (" + Counts.ToString() + ")/";


            str_sql = @"INSERT INTO Common.dbo.Volume ([TypeService_Code],[House_Code],[HomeVolume],[HomeVolumeODN],[OrgVolume],[OrgVolumeODN],
                    [PerSave],[PerCalc],[DtGetVolume],[HouseVolume],[ODNatNormaCube])
                    SELECT 1 as TypeService_Code, a.House_Code, ROUND(d.HomeVolume, 8), ROUND(a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaHabitation, 8) as HomeVolumeODN,
                    0 as OrgVolume, ROUND(a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaNotHabitation, 8) as OrgVolumeODN, " + frmMain.MaxCurPer + ", " + frmMain.MaxCurPer +@",
                    @LastDate, ROUND(d.HomeVolume + a.SumaryODNHouse, 8), ROUND(a.SumaryODNHouse, 8)
                    FROM Common.dbo.HousesData a INNER JOIN Common.dbo.spHouses b ON a.House_Code = b.Id_House INNER JOIN Common.dbo.SpStreets c ON b.Street_Code = c.Id_Street
                    INNER JOIN (SELECT str_code, dom, SUM(CubeV) as HomeVolume, SUM(Area) as Area FROM abon.dbo.abonent" + frmMain.MaxCurPer + @" GROUP BY str_code, dom) d ON d.str_code = c.Code_Yl and d.dom = cast(b.numhouse as nvarchar(10)) + b.LitHouse
                    WHERE a.IsCalcODN = 2 and a.perend = 0";
            cmd = new SqlCommand(str_sql, db_con);
            cmd.Parameters.AddWithValue("@LastDate", Convert.ToDateTime("01." + frmMain.MaxCurPer.Substring(4, 2) + "." + frmMain.MaxCurPer.Substring(0, 4)));
            cmd.CommandTimeout = 0;
            Counts = cmd.ExecuteNonQuery();
            label1.Text = label1.Text + "(" + Counts.ToString() + "). ";

            str_sql = @"UPDATE abon.dbo.abonent" + frmMain.MaxCurPer + " SET OverNV_Full = 0, OverCubeV = 0";
            cmd = new SqlCommand(str_sql, db_con);
            cmd.CommandTimeout = 0;
            Counts = cmd.ExecuteNonQuery();
            label1.Text = label1.Text + "(" + Counts.ToString() + ")/";

            str_sql = @"UPDATE e SET e.OverNV_Full = ROUND(e.area * (a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaHabitation) / a.AreaHabitation * " + TarifWaterODN.ToString() + @", 2),
                        e.OverCubeV = ROUND(e.area * (a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaHabitation) / a.AreaHabitation, 8)
                        FROM Common.dbo.HousesData a
                        INNER JOIN Common.dbo.spHouses b ON a.House_Code = b.Id_House
                        INNER JOIN Common.dbo.SpStreets c ON b.Street_Code = c.Id_Street
                        INNER JOIN (SELECT str_code, dom, SUM(CubeV) as HomeVolume, SUM(Area) as Area FROM abon.dbo.abonent" + frmMain.MaxCurPer + @" GROUP BY str_code, dom) d
                        ON d.str_code = c.Code_Yl and d.dom = cast(b.numhouse as nvarchar(10)) + b.LitHouse
                        INNER JOIN abon.dbo.abonent" + frmMain.MaxCurPer + @" e ON d.Str_Code = e.Str_Code and d.dom = e.dom
                        WHERE a.IsCalcODN = 2 and a.perend = 0 and e.gruppa3 = 0";
            cmd = new SqlCommand(str_sql, db_con);
            cmd.CommandTimeout = 0;
            Counts = cmd.ExecuteNonQuery();
            label1.Text = label1.Text + "(" + Counts.ToString() + "). ";

//            str_sql = @"UPDATE e SET e.OverNV_Full = ROUND(e.area * (a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaHabitation) / a.AreaHabitation * " + TarifWaterODN.ToString() + @", 2),
//                        e.OverCubeV = ROUND(e.area * (a.SumaryODNHouse / (AreaHabitation + AreaNotHabitation) * AreaHabitation) / a.AreaHabitation, 8)
//                        FROM Common.dbo.HousesData a
//                        INNER JOIN Common.dbo.spHouses b ON a.House_Code = b.Id_House
//                        INNER JOIN Common.dbo.SpStreets c ON b.Street_Code = c.Id_Street
//                        INNER JOIN (SELECT str_code, dom, SUM(CubeV) as HomeVolume, SUM(Area) as Area FROM abon.dbo.abonent" + frmMain.MaxCurPer + @" GROUP BY str_code, dom) d
//                        ON d.str_code = c.Code_Yl and d.dom = cast(b.numhouse as nvarchar(10)) + b.LitHouse
//                        INNER JOIN abon.dbo.abonent" + frmMain.MaxCurPer + @" e ON d.Str_Code = e.Str_Code and d.dom = e.dom
//                        WHERE a.IsCalcODN = 2 and a.perend = 0 and e.gruppa3 = 0 and e.prochee = 0";
//            cmd = new SqlCommand(str_sql, db_con);
//            cmd.CommandTimeout = 0;
//            Counts = cmd.ExecuteNonQuery();
//            label1.Text = label1.Text + "Таблица ABONENT начислена (" + Counts.ToString() + ").";

            //str_sql = "EXEC Abon.dbo.SetSaldo " + frmMain.MaxCurPer;
            //cmd.CommandTimeout = 0;
            //cmd.ExecuteNonQuery();
            label1.Text = label1.Text + "ОДН начислено.";
        }

    }
}

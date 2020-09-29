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
    public partial class frmOnlineCash : Form
    {
        public frmOnlineCash()
        {
            InitializeComponent();
            brik_list.Rows.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VirtualCash VC;
            if (brik_list.Rows.Count > 0)
            {
                for (int i = 0; i < brik_list.RowCount; i++)
                {
                    VC = new VirtualCash(frmMain.db_con, brik_list[0, i].Value.ToString(), Convert.ToDateTime(brik_list[3, i].Value), brik_list[2, i].Value.ToString(),
                                         brik_list[1, i].Value.ToString(), brik_list[4, i].Value.ToString(), brik_list[5, i].Value.ToString(), date_edit.Value, true);
                    VC.MakeFile();
                }
            }
            brik_list.Rows.Clear();
        }

        // -- вставка данных для выгрузки -----------------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {
            VirtualCash VC;
            VC = new VirtualCash(frmMain.db_con, brik_edit.Text.Substring(0, 3), Convert.ToDateTime(date_edit.Text), pach_edit.Text, pref_edit.Text,
                                 brik_edit.Text.Substring(0, 3) + "_" + date_edit.Text.Replace(".", "") + "_" + pach_edit.Text, peredit.Text, date_edit.Value, false);
            VC.MakeFile();
            string[] row = { "", "", "", "", "", "", "", "" };
            row[0] = brik_edit.Text.Substring(0, 3);
            row[1] = pref_edit.Text;
            row[2] = pach_edit.Text;
            row[3] = date_edit.Text;
            Random rnd = new Random();
            row[4] = brik_edit.Text.Substring(0, 3) + "_" + date_edit.Text.Replace(".", "") + "_" + rnd.Next(100, 999).ToString() + "_" + pach_edit.Text;
            row[5] = peredit.Text;
            row[6] = VC.FLCount.ToString();
            row[7] = Math.Round(VC.FLSumma, 2, MidpointRounding.AwayFromZero).ToString();
            brik_list.Rows.Add(row);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            brik_list.Rows.RemoveAt(brik_list.CurrentRow.Index);
            brik_list.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (peredit.Text != "")
            {
                RecalcDownSumma(peredit.Text, "abon", "pay");
                RecalcDownSumma(peredit.Text, "abonuk", "pay");
                MessageBox.Show("Рассчет окончен!", "Все");
            }
        }

        private class SaldoMake
        {
            public string Lic;
            public double Saldo;
            public Int32 Abonserv_Code;
            public int Post_key;
            public double WaterSum;
            public double KanalSum;
            public double PeniSum;
            public double AvansSum;

            public SaldoMake(string lic, double saldo = 0, Int32 abonserv_code = 0, int post_key = 0, double watersum = 0, double kanalsum = 0, double penisum = 0, double avanssum = 0)
            {
                Lic = lic;
                Saldo = saldo;
                Abonserv_Code = abonserv_code;
                Post_key = post_key;
                WaterSum = watersum;
                KanalSum = kanalsum;
                PeniSum = penisum;
                AvansSum = avanssum;
            }
        }

        private void RecalcDownSumma(string Per, string BaseName, string Pole)
        {
            List<SaldoMake> Lics = new List<SaldoMake>();
            string sqlstr = "UPDATE " + BaseName + ".dbo.abonservsaldo" + Per + " SET " + Pole + " = 0 WHERE lic IN (SELECT lic FROM " + BaseName + ".dbo.abonservsaldo" + Per + " GROUP BY lic HAVING SUM(" + Pole + ") = 0)";
            SqlCommand cmd = new SqlCommand(sqlstr, frmMain.db_con);
            cmd.ExecuteNonQuery();
            sqlstr = "SELECT lic, " + Pole + " as saldo, abonserv_code FROM " + BaseName + ".dbo.abonservsaldo" + Per + " " +
                            "WHERE lic IN (SELECT lic FROM " + BaseName + ".dbo.abonservsaldo" + Per + " WHERE " + Pole + " > 0) " +
                            "and lic IN (SELECT lic FROM " + BaseName + ".dbo.abonservsaldo" + Per + " WHERE " + Pole + " < 0)  and " + Pole + " <> 0 ORDER BY lic, abonserv_code";
            cmd = new SqlCommand(sqlstr, frmMain.db_con);
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
                if (DRaeder.HasRows)
                    while (DRaeder.Read())
                        Lics.Add(new SaldoMake(DRaeder["lic"].ToString(), Convert.ToDouble(DRaeder["saldo"].ToString()), Convert.ToInt32(DRaeder["abonserv_code"].ToString())));
            string Lic = "";
            double SummaMoneysP = 0;
            double SummaMoneysM = 0;
            int i = 0;
            int j = 0;
            if (Lics.Count > 0)
            {
                while (i <= Lics.Count)
                {
                    if (i == Lics.Count || Lic != Lics[i].Lic)
                    {
                        if (i != 0)
                        {
                            double Stp = Math.Min(Math.Abs(SummaMoneysP), Math.Abs(SummaMoneysM));
                            double Stm = -Stp;
                            for (int k = i - 1; k >= j; k--)
                            {
                                if (Lics[k].Saldo > 0)
                                {
                                    if (Lics[k].Saldo > Stp)
                                    {
                                        Lics[k].Saldo -= Stp;
                                        Stp = 0;
                                    }
                                    else
                                    {
                                        Stp -= Lics[k].Saldo;
                                        Lics[k].Saldo = 0;
                                    }
                                }
                                else
                                {
                                    if (Lics[k].Saldo < Stm)
                                    {
                                        Lics[k].Saldo -= Stm;
                                        Stm = 0;
                                    }
                                    else
                                    {
                                        Stm -= Lics[k].Saldo;
                                        Lics[k].Saldo = 0;
                                    }
                                }
                            }
                        }
                        if (i < Lics.Count)
                        {
                            SummaMoneysP = Lics[i].Saldo > 0 ? Lics[i].Saldo : 0;
                            SummaMoneysM = Lics[i].Saldo < 0 ? Lics[i].Saldo : 0;
                            Lic = Lics[i].Lic;
                        }
                        j = i;
                    }
                    else
                    {
                        SummaMoneysP = Lics[i].Saldo > 0 ? SummaMoneysP + Lics[i].Saldo : SummaMoneysP;
                        SummaMoneysM = Lics[i].Saldo < 0 ? SummaMoneysM + Lics[i].Saldo : SummaMoneysM;
                    }
                    i++;
                }
                sqlstr = "UPDATE " + BaseName + ".dbo.abonservsaldo" + Per + " SET " + Pole + " = @saldo WHERE lic = @lic and abonserv_code = @asc";
                int z = 0;
                foreach (SaldoMake ls in Lics)
                {
                    cmd = new SqlCommand(sqlstr, frmMain.db_con);
                    cmd.Parameters.AddWithValue("@saldo", Math.Round(ls.Saldo, 2));
                    cmd.Parameters.AddWithValue("@lic", ls.Lic);
                    cmd.Parameters.AddWithValue("@asc", ls.Abonserv_Code);
                    cmd.ExecuteNonQuery();
                    z++;
                }
            }
        }

        // -- Распределение поступлений по услугам --
        private void button6_Click(object sender, EventArgs e)
        {
            ReSetPos(peredit.Text);
            MessageBox.Show("Рассчет окончен!", "Все");
        }

// ------ Распределяем поступления по одному лицевому за весь период -----------------
        private void ReSetPosLic(string Per, string lic)
        {
            string SQLText;
            double WaterBegin = 0;
            double KanalBegin = 0;
            double PolivBegin = 0;
            double KoefBegin = 0;
            double WaterKoef = 0;
            double KanalKoef = 0;
            double PolivKoef = 0;
            double KoefKoef = 0;
            double Avans = 0;
            // -- Выбираем начальные остатки периода --
            SQLText = String.Format(@"SELECT Abonserv_code, saldo + saldoodn as saldo FROM {1}.dbo.abonservsaldo{0} WHERE lic = @lic", Per, (lic.Substring(0,1) == "1" ? "Abon": "AbonUK"));
            SqlCommand cmd = new SqlCommand(SQLText, frmMain.db_con);
            cmd.Parameters.AddWithValue("@lic", lic);
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                    {
                        switch (Convert.ToInt32(DRaeder["Abonserv_code"]))
                        {
                            case 1:
                                if (Convert.ToDouble(DRaeder["saldo"]) > 0)
                                {
                                    WaterBegin += Convert.ToDouble(DRaeder["saldo"]);
                                }
                                else
                                {
                                    Avans += -Convert.ToDouble(DRaeder["saldo"]);
                                }
                                break;
                            case 2:
                                if (Convert.ToDouble(DRaeder["saldo"]) > 0)
                                {
                                    KanalBegin += Convert.ToDouble(DRaeder["saldo"]);
                                }
                                else
                                {
                                    Avans += -Convert.ToDouble(DRaeder["saldo"]);
                                }
                                break;
                            case 3:
                                if (Convert.ToDouble(DRaeder["saldo"]) > 0)
                                {
                                    PolivBegin += Convert.ToDouble(DRaeder["saldo"]);
                                }
                                else
                                {
                                    Avans += -Convert.ToDouble(DRaeder["saldo"]);
                                }
                                break;
                            case 6:
                                if (Convert.ToDouble(DRaeder["saldo"]) < 0)
                                {
                                    Avans += -Convert.ToDouble(DRaeder["saldo"]);
                                }
                                break;
                            case 7:
                                if (Convert.ToDouble(DRaeder["saldo"]) > 0)
                                {
                                    KoefBegin += Convert.ToDouble(DRaeder["saldo"]);
                                }
                                else
                                {
                                    Avans += -Convert.ToDouble(DRaeder["saldo"]);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
// ------- Если переплаты больше, чем долгов, то обнуляем долги, уменьшаем на их число аванс ------------------------
            if(WaterBegin + KanalBegin + PolivBegin + KoefBegin <= Avans)
            {
                Avans = Avans - WaterBegin - KanalBegin - PolivBegin - KoefBegin;
                WaterBegin = 0;
                KanalBegin = 0;
                PolivBegin = 0;
                KoefBegin = 0;
            }
// ------- Высчитываем пропорции начального долга между водой, канализацией и пени ----------------------------------
            else if(WaterBegin + KanalBegin + PolivBegin + KoefBegin > 0)
            {
                if (WaterBegin > 0)
                    WaterKoef = WaterBegin / (WaterBegin + KanalBegin + PolivBegin + KoefBegin);
                if (KanalBegin > 0)
                    KanalKoef = KanalBegin / (WaterBegin + KanalBegin + PolivBegin + KoefBegin);
                if (PolivBegin > 0)
                    PolivKoef = PolivBegin / (WaterBegin + KanalBegin + PolivBegin + KoefBegin);
                if (KoefBegin > 0)
                    KoefKoef  = KoefBegin  / (WaterBegin + KanalBegin + PolivBegin + KoefBegin);
            }
            // -- Выбираем поступления по лицевому --
            SQLText = String.Format(@"SELECT 'abon' as db, post_key, opl, data_p FROM abon.dbo.pos{0} WHERE lic = '1' + RIGHT(@lic, 9) and  not brik IN(27, 50, 57) and opl > 0 UNION ALL
                                      SELECT 'abonuk' as db, post_key, opl, data_p FROM abonuk.dbo.pos{0} WHERE lic = '2' + RIGHT(@lic, 9) and  not brik IN(27, 50, 57) and opl > 0 ORDER BY data_p, post_key", Per);
            cmd = new SqlCommand(SQLText, frmMain.db_con);
            cmd.Parameters.AddWithValue("@lic", lic);
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                    {
                        double Water = 0;
                        double Kanal = 0;
                        double Poliv = 0;
                        double Koeffic = 0;
                        double Av = 0;
                        if (WaterBegin + KanalBegin + PolivBegin + KoefBegin < Convert.ToDouble(DRaeder["opl"]))
                        {
                            Water = Math.Round(WaterBegin, 2);
                            Kanal = Math.Round(KanalBegin, 2);
                            Poliv = Math.Round(PolivBegin, 2);
                            Koeffic = Math.Round(KoefBegin, 2);
                            Av = Convert.ToDouble(DRaeder["opl"]) - WaterBegin - KanalBegin - KoefBegin - PolivBegin;
                            WaterBegin = 0;
                            KanalBegin = 0;
                            KoefBegin = 0;
                            PolivBegin = 0;
                            Avans += Av;
                        }
                        else
                        {
                            Water = Math.Round(Convert.ToDouble(DRaeder["opl"]) * WaterKoef, 2);
                            Kanal = Math.Round(Convert.ToDouble(DRaeder["opl"]) * KanalKoef, 2);
                            Koeffic = Math.Round(Convert.ToDouble(DRaeder["opl"]) * KoefKoef, 2);
                            Poliv  = Math.Round(Convert.ToDouble(DRaeder["opl"]) - Water - Kanal - Koeffic, 2);
                            WaterBegin -= Water;
                            KanalBegin -= Kanal;
                            KoefBegin -= Koeffic;
                            PolivBegin -= Poliv;
                        }
                        SqlConnection comm;
                        comm = new SqlConnection(frmMain.db_con.ConnectionString);
                        comm.Open();
                        SQLText = String.Format(@"UPDATE {1}.dbo.pos{0} SET WPay = @WPay, KPay = @KPay, PPay = @PPay, KoPay = @KoPay, APay = @APay
                                                  WHERE post_key = @post_key", Per, DRaeder["db"].ToString());
                        SqlCommand cmd1 = new SqlCommand(SQLText, comm);
                        cmd1.Parameters.AddWithValue("@WPay", Water);
                        cmd1.Parameters.AddWithValue("@KPay", Kanal);
                        cmd1.Parameters.AddWithValue("@PPay", Poliv);
                        cmd1.Parameters.AddWithValue("@KoPay", Koeffic);
                        cmd1.Parameters.AddWithValue("@APay", Av);
                        cmd1.Parameters.AddWithValue("@post_key", Convert.ToInt32(DRaeder["post_key"]));
                        cmd1.ExecuteNonQuery();
                        comm.Close();
                    }
                }
            }
        }

        // ------ Выбираем лицевые с поступлениями за текущий день --------------------------------------------------------------------------
        private void ReSetPos(string Per)
        {
            List<string> Lics = new List<string>();
            string SQLText;
            SqlCommand cmd;
            //// -- Обнуляем предыдущее распределение перед -----------------------------------------------------------------------------------
            //SQLText = String.Format(@"UPDATE abon.dbo.pos{0} SET WPay = 0, KPay = 0, PPay = 0, KoPay = 0, APay = 0;
            //                          UPDATE abonuk.dbo.pos{0} SET WPay = 0, KPay = 0, PPay = 0, KoPay = 0, APay = 0;", Per);
            //cmd = new SqlCommand(SQLText, frmMain.db_con);
            //cmd.CommandTimeout = 0;
            //cmd.ExecuteNonQuery();
            // -- Выбираем все платежи из двух баз ------------------------------------------------------------------------------------------
            SQLText = String.Format(@"SELECT DISTINCT lic FROM abon.dbo.pos{0} WHERE not brik IN(27, 50, 57) and lic > 10000000 and data_p = @data_p
                                      UNION ALL
                                      SELECT DISTINCT lic FROM abonuk.dbo.pos{0} WHERE not brik IN(27, 50, 57) and lic > 10000000 and data_p = @data_p", Per);
            cmd = new SqlCommand(SQLText, frmMain.db_con);
            cmd.Parameters.AddWithValue("@data_p", date_edit.Value.Date);
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
            for(int i = 0; i < Lics.Count; i++)
            {
                ReSetPosLic(Per, Lics[i]);
                button6.Text = i.ToString() + " / " + Lics.Count.ToString();
                Application.DoEvents();

            }
        }
    }
}

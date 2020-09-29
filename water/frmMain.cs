using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Odbc;

namespace water
{
    public partial class frmMain : Form
    {
        public static Boolean exit_ = false;
        public static string CurPer = "";
        public static string MaxCurPer = "";
        public static string UrlPrn = "";
        public static SqlConnection db_con = new SqlConnection();
        public static string Host = System.Net.Dns.GetHostName();


        public frmMain()
        {
            InitializeComponent();
            try
            {
                /// соединение с БД
                db_con.ConnectionString = "Data Source=SERVERAB;Initial Catalog=Abon;Integrated Security=True;Persist Security Info=False;User ID=SqlAbon;Connect Timeout=0;TrustServerCertificate=False";
                db_con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = db_con;
                cmd.CommandText = "select * from (select right(Abon.dbo.GetLastTable('Abonent'),6) as CurPer) a";
                CurPer = (string)cmd.ExecuteScalar();
                MaxCurPer = CurPer;
                curPerToolStripMenuItem.Text = "Текущий период " + CurPer;
                /// изменение региональных настроек
                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(Application.CurrentCulture.Name);
                culture.NumberFormat.CurrencyDecimalSeparator = ".";
                culture.NumberFormat.NumberDecimalSeparator = ".";
                Application.CurrentCulture = culture;
            }
            catch (Exception ex)
            {
                if (db_con.State == ConnectionState.Connecting) db_con.Close();
                MessageBox.Show("Не удалось соединиться с базой данных.\nПриложение будет закрыто.\nОбратитесь в отдел АСУ.\n(" + ex.Message + ")", "Ошибка");
                exit_ = true;
            }
        }


        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void принятьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmPayGet frmPayGet_ = new frmPayGet();
            frmPayGet_.MdiParent = this;
            frmPayGet_.Show();
        }

        private void нормативыToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void карточкаЛсToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmKart frmKart_ = new frmKart();
            frmKart_.MdiParent = this;
            frmKart_.Show();
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (exit_) ExitToolsStripMenuItem_Click(this, e);
            //            frmWellcome frmW = new frmWellcome();
            //            frmW.MdiParent = this;
            //            frmW.Show();
        }

        private void созданиеЛсToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCreate frmCreate_ = new frmCreate();
            frmCreate_.MdiParent = this;
            frmCreate_.Show();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (db_con.State == ConnectionState.Connecting) db_con.Close();
        }

        private void перевестиДомВ3ГруппуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm3group frm3group_ = new frm3group();
            frm3group_.MdiParent = this;
            frm3group_.Show();
        }

        private void curPerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int year = Convert.ToInt16(MaxCurPer.Substring(0, 4));
            int month = Convert.ToInt16(MaxCurPer.Substring(4, 2));
            PerCalendar.MinDate = new DateTime(2012, 01, 01);
            PerCalendar.MaxDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            PerCalendar.Visible = true;
        }

        private void PerCalendar_Leave(object sender, EventArgs e)
        {
            PerCalendar.Visible = false;
        }

        private void PerCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            CurPer = PerCalendar.SelectionRange.Start.ToString("yyyyMM");
            curPerToolStripMenuItem.Text = "Текущий период " + CurPer;
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerCalendar.Visible = false;
        }

        private void сальдоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSaldo frmSaldo_ = new frmSaldo();
            frmSaldo_.MdiParent = this;
            frmSaldo_.Show();
        }

        private void отчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmPayView frmPayView_ = new frmPayView();
            frmPayView_.MdiParent = this;
            frmPayView_.Show();
        }

        private void переводДомаВДругоеВедомствоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmToNewVedom frmToNewVedom_ = new frmToNewVedom();
            frmToNewVedom_.MdiParent = this;
            frmToNewVedom_.Show();
        }

        private void закрытиеПериодаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmClosePer frmClosePer_ = new frmClosePer();
            frmClosePer_.MdiParent = this;
            frmClosePer_.Show();
        }

        private void некачественнаяУслугаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBadUsluga frmBadUsluga_ = new frmBadUsluga();
            frmBadUsluga_.MdiParent = this;
            frmBadUsluga_.Show();
        }

        private void приемДвиженияОтУКToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMoveCountLic frmMCL = new frmMoveCountLic();
            frmMCL.MdiParent = this;
            frmMCL.Show();
        }


        private void жильеУКToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ;
        }


        private void некачественнаяУслугаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmBadUsluga frmBadUsluga_ = new frmBadUsluga();
            frmBadUsluga_.MdiParent = this;
            frmBadUsluga_.Show();
        }

        private void оДНToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ///CalculateWater.ODNCalculate Calc = new CalculateWater.ODNCalculate(db_con, MaxCurPer, MaxCurPer);
        }

        private void расчетОДНToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmODN frmODN_ = new frmODN();
            frmODN_.MdiParent = this;
            frmODN_.Show();
        }



        private void дляДомаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmCHouse frmCHouse_ = new frmCHouse();
            frmCHouse_.MdiParent = this;
            frmCHouse_.Show();
        }

        private void приемПлатежейToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmErrPay frmErrPay_ = new frmErrPay();
            frmErrPay_.MdiParent = this;
            frmErrPay_.Show();
        }

        private void жильеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmCalc frmC = new frmCalc();
            frmC.MdiParent = this;
            frmC.Show();
        }

        private void жильеУКToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmCalc frmC = new frmCalc();
            frmC.Owner = this;
            frmC.Show();
        }

        private void дляВедомстваToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCalc frmC = new frmCalc();
            frmC.MdiParent = this;
            frmC.Show();
        }

        private void среднееToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(5))
            {
                frmCalc frmC = new frmCalc();
                frmC.MdiParent = this;
                frmC.Tag = 5;
                frmC.Show();
            }
        }

        private void оплатаКвитанцийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(6))
            {
                frmKassa frmK = new frmKassa();
                frmK.MdiParent = this;
                frmK.Tag = 6;
                frmK.Show();
            }
        }

        private void движениеЛСToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!searchform(7))
            {
                frmMoveLicError frmMLE = new frmMoveLicError();
                frmMLE.MdiParent = this;
                frmMLE.Tag = 7;
                frmMLE.Show();
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void улицыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(8))
            {
                frmStreet frmStr = new frmStreet();
                frmStr.MdiParent = this;
                frmStr.Tag = 8;
                frmStr.Show();
            }
        }

        private void приемПочтыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(9))
            {
                frmMail frmM = new frmMail();
                frmM.MdiParent = this;
                frmM.Tag = 9;
                frmM.Show();
            }
        }

        private void количествоПроживающихToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(10))
            {
                frmMoveLic frmML = new frmMoveLic();
                frmML.MdiParent = this;
                frmML.Tag = 10;
                frmML.Show();
            }
        }

        private void управляющиеКомпанииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(11))
            {
                frmUpr frmUk = new frmUpr();
                frmUk.MdiParent = this;
                frmUk.Tag = 11;
                frmUk.Show();
            }
        }


        private bool searchform(int tag_)
        {
            bool result = false;
            foreach (Form f in MdiChildren)
            {
                if (f.Tag != null && f.Tag.ToString() == tag_.ToString()) result = true;
            }
            return result;
        }

        private void списаниеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(12))
            {
                frmSpis frmSp = new frmSpis();
                frmSp.MdiParent = this;
                frmSp.Tag = 12;
                frmSp.Show();
            }
        }

        private void горячееВодоснабжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(13))
            {
                frmGorPer frmGP = new frmGorPer();
                frmGP.MdiParent = this;
                frmGP.Tag = 13;
                frmGP.Show();
            }
        }

        private void перевестиЛсВ3ГруппуToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void рассрочкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(14))
            {
                frmOtsrochka frmOts = new frmOtsrochka();
                frmOts.MdiParent = this;
                frmOts.Tag = 14;
                frmOts.Show();
            }
        }

        private void tempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!searchform(15))
            {
                frmTemp frmTmp = new frmTemp();
                frmTmp.MdiParent = this;
                frmTmp.Tag = 15;
                frmTmp.Show();
            }
        }

        private void пеняToolStripMenuItem_Click(object sender, EventArgs e)
        {
//            CalculatePeni calculatePeni = new CalculatePeni("2000025300", "201806");
//            Memo1.Text = calculatePeni.GetResultTimes();
            //// --------------------- Начисление пени ------------------
            //SqlConnection conn = new SqlConnection(db_con.ConnectionString);
            //conn.Open();
            //CalcPeni calcPeni;
            //calcPeni = new CalcPeni(1, MaxCurPer, conn);
            //calcPeni.CalcPeniAll();
            //calcPeni = new CalcPeni(4, MaxCurPer, conn);
            //MessageBox.Show("начислено!", "Все");
        }

        private void выгрузкаВСБЕРБАНКToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SberbankReestr sberbankReestr = new SberbankReestr();
            sberbankReestr.MakeReestr();
            
            //SqlCommand cmd = new SqlCommand("Abon.dbo.GetSberInf", db_con);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("@New", SqlDbType.Bit).Value = 1;
            //List<String> CD = new List<String>();
            //cmd.CommandTimeout = 0;
            //Int32 SizeOfFile = 0;
            //Int32 NumberOfFile = 1;
            //using (SqlDataReader rsNach = cmd.ExecuteReader())
            //{
            //    if (rsNach.HasRows)
            //    {
            //        while (rsNach.Read())
            //        {
            //            CD.Add(rsNach["lic"].ToString() + ";;" + rsNach["adr"].ToString() + ";" + rsNach["bper"].ToString() + ";" +
            //              rsNach["SUMMA"].ToString() + ";;;;;");
            //            SizeOfFile = SizeOfFile + CD[CD.Count - 1].Length;
            //            if (SizeOfFile > 1500000)
            //            {
            //                SaveToSber(NumberOfFile, CD);
            //                NumberOfFile++;
            //                CD.Clear();
            //                SizeOfFile = 0;
            //            }
            //        }
            //        if(CD.Count > 0)
            //          SaveToSber(NumberOfFile, CD);
            //    }
            //    Console.WriteLine(SizeOfFile);
            //    rsNach.Close();
            //}
            MessageBox.Show("Выгрузка завершена!", "Все");
        }

        private void SaveToSber(Int32 NumberOfFile, List<string> cd)
        {
//            FileInfo fi = new FileInfo("D:\\5701000368-RN01170" + NumberOfFile.ToString() + ".txt");
//            StreamWriter sw = fi.AppendText();
            using (var sw = new StreamWriter("D:\\5701000368-RN0" + NumberOfFile.ToString() + "1704.txt", false, Encoding.GetEncoding(1251)))
            foreach (string lists in cd)
                sw.WriteLine(lists);
//            sw.Close();
        }

        private void проверкаПередЗакрытиемToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Verification ver = new Verification(db_con, "201708", 0);
            ver.MakeVerification();
            ver = new Verification(db_con, "201708", 1);
            ver.MakeVerification();
        }

        private class SaldoMake
        {
            public string Lic;
            public double Saldo;
            public Int32 Abonserv_Code;
            public SaldoMake(string lic, double saldo, Int32 abonserv_code)
            {
                Lic = lic;
                Saldo = saldo;
                Abonserv_Code = abonserv_code;
            }
        }

        private void начислениеПениToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecalcDownSumma("202009", "abon", "saldo");
            RecalcDownSumma("202009", "abonuk", "saldo");
            RecalcDownSumma("202008", "abon", "pay");
            RecalcDownSumma("202008", "abonuk", "pay");
            MessageBox.Show("Рассчет окончен!", "Все");
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

// -- тест пени -- 
        private void тестПениToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Lic1 = "1314453900";
            string Sst;
//            Sst = Sst + DateTime.Now.ToString() + " - ";
            CalculatePeni сalculatePeni = new CalculatePeni(Lic1, frmMain.MaxCurPer);
            сalculatePeni.CalcPeniLic();
            Sst = сalculatePeni.GetResultTimes();
//            textBox1.Text = Sst;
//            Sst = Sst + DateTime.Now.ToString();
//            MessageBox.Show(Sst, "Все");
        }

        private void выгрузкаКвитанцийВЭнергосбытToolStripMenuItem_Click(object sender, EventArgs e)
        {
//                String DatabaseName = "d:\\workspace\\temptable.dbf";
//                OleDbConnection OleDbCon = new OleDbConnection(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=dBASE IV;", DatabaseName));
                OdbcConnection OdbcCon = new OdbcConnection("Driver={Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)};DBQ=d:\\workspace");
            
                OdbcCommand oCmd = new OdbcCommand();
                oCmd.Connection = OdbcCon;
                oCmd.Connection.Open();
                //oCmd.CommandText = "DROP TABLE temptable";
                //oCmd.CommandType = CommandType.Text;
                //oCmd.ExecuteNonQuery();
                oCmd.CommandText = @"CREATE TABLE temptable (POSTCODE char(6), ADDRESS char(150), EPDNUM numeric, LIC char(10), FIO char(50), MNT numeric, YER numeric, 
                                    NREG numeric, SFLAT NUMERIC, MPDATEH1 char(10), MPDATEH2 char(10), MPDATEH3 char(10), MPDATEG1 char(10), MPDATEG2 char(10), MPDATEG3 char(10), 
                                    NPU char(2), VX1 char(10), VX2 char(10), VX3 char(10), VG1 char(10), VG2 char(10), VG3 char(10), SLDIN numeric, NACH numeric, PERER numeric, 
                                    SPAY numeric, SALDOTTL numeric, LINECODE1 char(28), LINECODE2 char(28), LINECODE3 char(28), LINECODE4 char(28), 
                                    USLG01 char(20), SLDIN01 numeric, VAL01 numeric, NACH01 numeric, PERER01 numeric, SPAY01 numeric, CLCSUM01 numeric, TAR01 numeric, 
                                    USLG02 char(20), SLDIN02 numeric, VAL02 numeric, NACH02 numeric, PERER02 numeric, SPAY02 numeric, CLCSUM02 numeric, TAR02 numeric, 
                                    USLG03 char(20), SLDIN03 numeric, VAL03 numeric, NACH03 numeric, PERER03 numeric, SPAY03 numeric, CLCSUM03 numeric, TAR03 numeric, 
                                    USLG04 char(20), SLDIN04 numeric, VAL04 numeric, NACH04 numeric, PERER04 numeric, SPAY04 numeric, CLCSUM04 numeric, TAR04 numeric, 
                                    USLG05 char(20), SLDIN05 numeric, VAL05 numeric, NACH05 numeric, PERER05 numeric, SPAY05 numeric, CLCSUM05 numeric, TAR05 numeric, 
                                    USLG06 char(20), SLDIN06 numeric, VAL06 numeric, NACH06 numeric, PERER06 numeric, SPAY06 numeric, CLCSUM06 numeric, TAR06 numeric, 
                                    AOGUID char(36), HOUSEGUID char(36), HOUSN char(20), FLATN char(20))";
                oCmd.ExecuteNonQuery();

                string sqlstr = @"SELECT b.PostIndex as POSTCODE,
                                b.ray_fias + ', ' + b.prefyl_fias  + ' ' + b.yl_name_fias + ', дом ' + a.dom + CASE WHEN a.flat = '0' THEN '' ELSE ', квартира ' + a.flat END as [ADDRESS],
                                lic_epd.epdnum as EPDNUM,
                                a.lic as LIC,
                                a.fam as FIO,
                                7 as MNT,
                                2018 as YER,
                                CASE WHEN a.liver + a.vibilo + a.vibilo2 < 1 THEN 1 ELSE a.liver + a.vibilo + a.vibilo2 END as NREG,
                                a.area as SFLAT,
                                c.H1 as MPDATEH1, c.H2 as MPDATEH2, c.H3 as MPDATEH3, c.G1 as MPDATEG1, c.G2 as MPDATEG2, c.G3 as MPDATEG3,
                                '0' + CASE WHEN H1 is NULL THEN '0' ELSE '1' END + CASE WHEN H2 is NULL THEN '0' ELSE '1' END + CASE WHEN H3 is NULL THEN '0' ELSE '1' END +
                                CASE WHEN G1 is NULL THEN '0' ELSE '1' END + CASE WHEN G2 is NULL THEN '0' ELSE '1' END + CASE WHEN G3 is NULL THEN '0' ELSE '1' END as NPU,
                                d.KubH1n as VX1, d.KubH2n as VX2, d.KubH3n as VX3, d.KubG1n as VG1, d.KubG2n as VG2, d.KubG3n as VG3,
                                a.sdolgbeg as SLDIN,
                                a.nachisl as NACH,
                                a.Oplata as PERER,
                                ISNULL(e.Opl, 0) as SPAY,
                                a.sdolgend as SALDOTTL,
                                CAST(a.Lic as varchar) + '201806' + '00000000' + '101' as LINECODE1,
                                CAST(a.Lic as varchar) + '201806' + '00000000' + '201' as LINECODE2,
                                CAST(a.Lic as varchar) + '201806' + '00000000' + '301' as LINECODE3,
                                CAST(a.Lic as varchar) + '201806' + '00000000' + '401' as LINECODE4,
                                USLG01, SLDIN01, VAL01, NACH01, PERER01, SPAY01, CLCSUM01, st.vsum as TAR01, 
                                USLG02, SLDIN02, VAL02, NACH02, PERER02, SPAY02, CLCSUM02, st.ksum as TAR02, 
                                USLG03, SLDIN03, VAL03, NACH03, PERER03, SPAY03, CLCSUM03, st.vsum as TAR03, 
                                USLG04, SLDIN04, VAL04, NACH04, PERER04, SPAY04, CLCSUM04, st.vsum as TAR04, 
                                USLG05, SLDIN05, VAL05, NACH05, PERER05, SPAY05, CLCSUM05, st.vsum as TAR05, 
                                USLG06, SLDIN06, VAL06, NACH06, PERER06, SPAY06, CLCSUM06, st.vsum as TAR06,
                                b.aoguid_yl_fias as AOGUID,
                                stu.HOUSEGUID as HOUSEGUID,
                                a.dom as HOUSN,
                                a.flat as FLATN
                                FROM abonent201806 a
                                INNER JOIN street b ON a.str_code = b.cod_yl
                                INNER JOIN vodomerdate201806 c ON a.lic = c.lic
                                LEFT JOIN maxposvod d ON a.lic = d.lic
                                LEFT JOIN (SELECT lic, SUM(opl) as Opl FROM pos201806 WHERE not (brik IN (27, 57, 278)) GROUP BY lic) e ON a.lic = e.lic
                                INNER JOIN (SELECT lic, 'Водоснабжение' as USLG01, saldo as SLDIN01, ChargeCube as VAL01, Charge as NACH01, CorrCharge as PERER01, pay as SPAY01, saldo + Charge + CorrCharge - pay as CLCSUM01 FROM abonservsaldo201806 WHERE abonserv_code = 1) asc1 ON a.lic = asc1.lic
                                INNER JOIN (SELECT lic, 'Водоотведение' as USLG02, saldo as SLDIN02, ChargeCube as VAL02, Charge as NACH02, CorrCharge as PERER02, pay as SPAY02, saldo + Charge + CorrCharge - pay as CLCSUM02 FROM abonservsaldo201806 WHERE abonserv_code = 2) asc2 ON a.lic = asc2.lic
                                INNER JOIN (SELECT lic, 'Полив' as USLG03, saldo as SLDIN03, ChargeCube as VAL03, Charge as NACH03, CorrCharge as PERER03, pay as SPAY03, saldo + Charge + CorrCharge - pay as CLCSUM03 FROM abonservsaldo201806 WHERE abonserv_code = 3) asc3 ON a.lic = asc3.lic
                                INNER JOIN (SELECT lic, 'Повышающий коэффициент' as USLG04, saldo as SLDIN04, 0 as VAL04, Charge as NACH04, CorrCharge as PERER04, pay as SPAY04, saldo + Charge + CorrCharge - pay as CLCSUM04 FROM abonservsaldo201806 WHERE abonserv_code = 7) asc4 ON a.lic = asc4.lic
                                INNER JOIN (SELECT lic, 'Пеня' as USLG05, saldo as SLDIN05, 0 as VAL05, Charge as NACH05, CorrCharge as PERER05, pay as SPAY05, saldo + Charge + CorrCharge - pay as CLCSUM05 FROM abonservsaldo201806 WHERE abonserv_code = 6) asc5 ON a.lic = asc5.lic
                                INNER JOIN (SELECT lic, 'ОДН' as USLG06, saldoodn as SLDIN06, ChargeodnCube as VAL06, Chargeodn as NACH06, CorrodnCharge as PERER06, payodn as SPAY06, saldoodn + Chargeodn + CorrodnCharge - payodn as CLCSUM06 FROM abonservsaldo201806 WHERE abonserv_code = 1) asc6 ON a.lic = asc6.lic
                                INNER JOIN SpTarif st ON b.TarifKod = st.idTarif 
                                INNER JOIN StreetUch stu ON a.str_code = stu.Cod_yl and a.dom = Cast(stu.S as varchar) + stu.lit
                                INNER JOIN lic_epd ON a.lic = lic_epd.licfull
                                WHERE a.gruppa3 = 0 and a.prochee = 0 and a.kodvedom IN (SELECT ID FROM spvedomstvo where buk = 0 and bpaketc = 1)
                                and vvodomer = 1 and st.Per = 201806 and a.lic IN (1005290200, 1012699700, 1012906200, 1012906300, 1013364200)
                                order by a.lic";
                SqlCommand cmd = new SqlCommand(sqlstr, frmMain.db_con);
                using (SqlDataReader DRaeder = cmd.ExecuteReader())
                    if (DRaeder.HasRows)
                        while (DRaeder.Read())
                        {
                            oCmd.CommandText = @"INSERT INTO temptable (POSTCODE, ADDRESS, EPDNUM, LIC, FIO, MNT, YER, NREG, SFLAT, MPDATEH1, MPDATEH2, MPDATEH3, MPDATEG1, MPDATEG2, MPDATEG3,
                                                NPU, VX1, VX2, VX3, VG1, VG2, VG3, SLDIN, NACH, PERER, SPAY, SALDOTTL, LINECODE1, LINECODE2, LINECODE3, LINECODE4,
                                                USLG01, SLDIN01, VAL01, NACH01, PERER01, SPAY01, CLCSUM01, TAR01, USLG02, SLDIN02, VAL02, NACH02, PERER02, SPAY02, CLCSUM02, TAR02,
                                                USLG03, SLDIN03, VAL03, NACH03, PERER03, SPAY03, CLCSUM03, TAR03, USLG04, SLDIN04, VAL04, NACH04, PERER04, SPAY04, CLCSUM04, TAR04,
                                                USLG05, SLDIN05, VAL05, NACH05, PERER05, SPAY05, CLCSUM05, TAR05, USLG06, SLDIN06, VAL06, NACH06, PERER06, SPAY06, CLCSUM06, TAR06,
                                                AOGUID, HOUSEGUID, HOUSN, FLATN) VALUES " +
                                                "('" + DRaeder["POSTCODE"].ToString()+"', '"+DRaeder["ADDRESS"].ToString()+"', "+DRaeder["EPDNUM"].ToString()+", '"+DRaeder["LIC"].ToString() +"', " +
                                                "'" + DRaeder["FIO"].ToString() + "', " + DRaeder["MNT"].ToString() + ", "+DRaeder["YER"].ToString()+", "+DRaeder["NREG"].ToString()+", "+
                                                DRaeder["SFLAT"].ToString() + ", '" + DRaeder["MPDATEH1"].ToString() +"', '" + DRaeder["MPDATEH2"].ToString() +"','" + DRaeder["MPDATEH3"].ToString() +"', " +
                                                "'" + DRaeder["MPDATEG1"].ToString() +"', '" + DRaeder["MPDATEG2"].ToString() +"','" + DRaeder["MPDATEG3"].ToString() +"', " +
                                                "'" + DRaeder["NPU"].ToString() +"', '" + DRaeder["VX1"].ToString() +"', '" + DRaeder["VX2"].ToString() +"', '" + DRaeder["VX3"].ToString() +"', " +
                                                "'" + DRaeder["VG1"].ToString() +"', '" + DRaeder["VG2"].ToString() +"', '" + DRaeder["VG3"].ToString() +"', " +
                                                DRaeder["SLDIN"].ToString() + ", " + DRaeder["NACH"].ToString() + ", " + DRaeder["PERER"].ToString() + ", " + DRaeder["SPAY"].ToString() + ", " +
                                                DRaeder["SALDOTTL"].ToString() + ", '" + DRaeder["LINECODE1"].ToString() + "', '" + DRaeder["LINECODE2"].ToString() + "', '" + DRaeder["LINECODE3"].ToString() + "', '" + DRaeder["LINECODE1"].ToString() + "', " +
                                                "'" + DRaeder["USLG01"].ToString() + "', " + DRaeder["SLDIN01"].ToString() + ", " + DRaeder["VAL01"].ToString() + ", " + DRaeder["NACH01"].ToString() + ", " + DRaeder["PERER01"].ToString() + ", " + DRaeder["SPAY01"].ToString() + ", " + DRaeder["CLCSUM01"].ToString() + ", " + DRaeder["TAR01"].ToString() + "," +
                                                "'" + DRaeder["USLG02"].ToString() + "', " + DRaeder["SLDIN02"].ToString() + ", " + DRaeder["VAL02"].ToString() + ", " + DRaeder["NACH02"].ToString() + ", " + DRaeder["PERER02"].ToString() + ", " + DRaeder["SPAY02"].ToString() + ", " + DRaeder["CLCSUM02"].ToString() + ", " + DRaeder["TAR02"].ToString() + "," +
                                                "'" + DRaeder["USLG03"].ToString() + "', " + DRaeder["SLDIN03"].ToString() + ", " + DRaeder["VAL03"].ToString() + ", " + DRaeder["NACH03"].ToString() + ", " + DRaeder["PERER03"].ToString() + ", " + DRaeder["SPAY03"].ToString() + ", " + DRaeder["CLCSUM03"].ToString() + ", " + DRaeder["TAR03"].ToString() + "," +
                                                "'" + DRaeder["USLG04"].ToString() + "', " + DRaeder["SLDIN04"].ToString() + ", " + DRaeder["VAL04"].ToString() + ", " + DRaeder["NACH04"].ToString() + ", " + DRaeder["PERER04"].ToString() + ", " + DRaeder["SPAY04"].ToString() + ", " + DRaeder["CLCSUM04"].ToString() + ", " + DRaeder["TAR04"].ToString() + "," +
                                                "'" + DRaeder["USLG05"].ToString() + "', " + DRaeder["SLDIN05"].ToString() + ", " + DRaeder["VAL05"].ToString() + ", " + DRaeder["NACH05"].ToString() + ", " + DRaeder["PERER05"].ToString() + ", " + DRaeder["SPAY05"].ToString() + ", " + DRaeder["CLCSUM05"].ToString() + ", " + DRaeder["TAR05"].ToString() + "," +
                                                "'" + DRaeder["USLG06"].ToString() + "', " + DRaeder["SLDIN06"].ToString() + ", " + DRaeder["VAL06"].ToString() + ", " + DRaeder["NACH06"].ToString() + ", " + DRaeder["PERER06"].ToString() + ", " + DRaeder["SPAY06"].ToString() + ", " + DRaeder["CLCSUM06"].ToString() + ", " + DRaeder["TAR06"].ToString() + "," +
                                                "'" + DRaeder["AOGUID"].ToString() + "', '" + DRaeder["HOUSEGUID"].ToString() + "', '" + DRaeder["HOUSN"].ToString() + "', '" + DRaeder["FLATN"].ToString() + "')";
                            oCmd.ExecuteNonQuery();
                        }
                OdbcCon.Close();
                OdbcCon.Dispose();
        }

        private void перерасчетToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void начислениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCalc frmC = new frmCalc();
            frmC.MdiParent = this;
            frmC.Show();
        }

        private void онлайнКассаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmOnlineCash OnlineCash = new frmOnlineCash();
            OnlineCash.MdiParent = this;
            OnlineCash.Show();
            //VirtualCash VC;
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("28.06.2019"), "922", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("29.06.2019"), "922", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("30.06.2019"), "922", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("28.06.2019"), "921", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("29.06.2019"), "921", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("30.06.2019"), "921", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("28.06.2019"), "956", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("29.06.2019"), "956", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("28.06.2019"), "955", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("01.07.2019"), "278", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("28.06.2019"), "0013255", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("29.06.2019"), "0013256", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("30.06.2019"), "0013257", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
            //VC = new VirtualCash(frmMain.db_con, Convert.ToDateTime("30.06.2019"), "0013258", new DateTime(2019, 7, 1, 23, 0, 0));
            //VC.MakeFile();
        }

    }
}

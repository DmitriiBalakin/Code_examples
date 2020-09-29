using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace water
{
    class Pays
    {
        public double Summa;
        public DateTime PayDate;
        public Pays(double summa, DateTime paydate)
        {
            Summa = summa;
            PayDate = paydate;
        }
    }

    class PeniRepire
    {
        public string lic;
        public string period;
        public int skvagina;
        public int lgkan;
        public double ostat;
        public double razn;
        public double saldo;
        public PeniRepire(string Lic, string Period, int Skvagina, int Lgkan, double Ostat, double Razn, double Saldo)
        {
            lic = Lic;
            period = Period;
            skvagina = Skvagina;
            lgkan = Lgkan;
            ostat = Ostat;
            razn = Razn;
            saldo = Saldo;
        }
    }

    class CalculatePeni
    {
        private double A = 0;
        private double B = 0;
        private double C = 0;
        private double D = 0;
        private double P = 0;
        public double Peni = 0;
        private double Dolg = 0;
        private List<Pays> pays;
        private string Lic;
        private string Per;
        private DateTime D40;
        private DateTime D100;
        private DateTime PeriodBegin;
        const double StavRefinans = 0.07;
        const double BigPeniKoefficient = 1 / 130.0 * StavRefinans;
        const double SmallPeniKoefficient = 1 / 300.0 * StavRefinans;
        const string Bricket27 = "27"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом
        const string Bricket57 = "57"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом
        const string Bricket571 = "571"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом
        private double X1 = BigPeniKoefficient;
        private double X2 = SmallPeniKoefficient;
        private double X3 = SmallPeniKoefficient;
        private double X4 = 0;
        string CRT = "";

        public CalculatePeni(string lic, string per)
        {
            Per = per;
            Lic = lic;
            RepairDeletedPeni();
//            return;
            CRT = CRT + " 0) " + DateTime.Now.ToString();
            string sqlstr =
                @"-- январь --
                    SELECT 1601 as per, SUM(a.Saldo + a.SaldoODN) as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                    FROM abon.dbo.abonservsaldo201601 a INNER JOIN abon.dbo.abonent201601 b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                    WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                    UNION ALL
                    SELECT 1601 as per, SUM(a.Saldo + a.SaldoODN) as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                    FROM abonuk.dbo.abonservsaldo201601 a INNER JOIN abonuk.dbo.abonent201601 b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                    WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
            string i = "201601";
            while (i != GetPrevPeriod(Per, 4))
            {
                sqlstr = sqlstr +
                @"  UNION ALL  
                    SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                    FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                    WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                    UNION ALL
                    SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                    FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                    WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
                i = GetNextPeriod(i);
            }
            // - 40-100 дней просрочки
            i = GetPrevPeriod(Per, 4);
            sqlstr = sqlstr +
            @"  UNION ALL  
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                UNION ALL
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
            // - 40 дней просрочки
            i = GetPrevPeriod(Per, 3);
            sqlstr = sqlstr +
            @"  UNION ALL  
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                UNION ALL
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as C, 0 as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
            // - 0 - 40 дней просрочки
            i = GetPrevPeriod(Per, 2);
            sqlstr = sqlstr +
            @"  UNION ALL  
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                UNION ALL
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, SUM(a.Charge + a.ChargeODN + a.CorrCharge + a.CorrODNCharge) as D, SUM(a.Pay + a.PayODN) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
            // - только поступления и отрицательные сторно прошлого периода
            i = GetPrevPeriod(Per, 1);
            sqlstr = sqlstr +
            @"  UNION ALL  
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN + CASE WHEN a.corrCharge + a.corrodncharge < 0 THEN - (a.corrCharge + a.corrodncharge) ELSE 0 END) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                UNION ALL
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(a.Pay + a.PayODN + CASE WHEN a.corrCharge + a.corrodncharge < 0 THEN - (a.corrCharge + a.corrodncharge) ELSE 0 END) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg ";
            // - только отрицательные сторно текущего периода
            i = Per;
            sqlstr = sqlstr +
            @"  UNION ALL  
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(CASE WHEN a.corrCharge + a.corrodncharge < 0 THEN - (a.corrCharge + a.corrodncharge) ELSE 0 END) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abon.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 0  and a.Lic = '1" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                UNION ALL
                SELECT " + i.Substring(2, 4) + @" as per, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, SUM(CASE WHEN a.corrCharge + a.corrodncharge < 0 THEN - (a.corrCharge + a.corrodncharge) ELSE 0 END) as pos, c.ID, b.KodVedom, b.sdolgbeg
                FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic INNER JOIN abonuk.dbo.SpVedomstvo c ON b.kodvedom = c.ID
                WHERE abonserv_code IN (1, 2, 3, 7) and c.bUK = 1  and a.Lic = '2" + Lic.Substring(1, 9) + @"'  GROUP BY c.ID, b.KodVedom, b.sdolgbeg 
                ORDER BY per desc";
            SqlCommand cmd = new SqlCommand(sqlstr, frmMain.db_con);
            CRT = sqlstr;
            cmd.CommandTimeout = 0;
            bool b = true;
//            CRT = CRT + " 1) " + DateTime.Now.ToString();
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
//                    CRT = CRT + " 2) " + DateTime.Now.ToString();
                    while (DRaeder.Read() && b)
                    {
                        if (Convert.ToDouble(DRaeder["sdolgbeg"]) <= 0 || Convert.ToInt32(DRaeder["KodVedom"]) == 211)
                        {
                            A += Convert.ToDouble(DRaeder["sdolgbeg"]);
                            b = false;
                        }
                        if (Convert.ToInt32(DRaeder["KodVedom"]) != 211)
                        {
                            if (Convert.ToDouble(DRaeder["dolg"]) < 0)
                                P += Convert.ToDouble(DRaeder["dolg"]);
                            else
                                Dolg += Convert.ToDouble(DRaeder["dolg"]);
                            A += Convert.ToDouble(DRaeder["A"]);
                            B += Convert.ToDouble(DRaeder["B"]);
                            C += Convert.ToDouble(DRaeder["C"]);
                            D += Convert.ToDouble(DRaeder["D"]);
                            P += Convert.ToDouble(DRaeder["pos"]);
                        }
                    }
                }
            }
//            CRT = CRT + " 3) " + DateTime.Now.ToString();
            if (Math.Round(A + B + C + D + Dolg - P, 2) <= 0)
            {
                A = 0; B = 0; C = 0; D = 0; return;
            }
            if (A < 0)
            {
                P -= A; A = 0;
            }
            if (B < 0)
            {
                P -= B; B = 0;
            }
            if (C < 0)
            {
                P -= C; C = 0;
            }
            if (D < 0)
            {
                P -= D; D = 0;
            }
            if (P < 0)
                P = 0;
            if (Dolg > P)
            {
                Dolg -= P; P = 0;
            }
            else
            {
                P -= Dolg; Dolg = 0;
            }
            if (A > P)
            {
                A -= P; P = 0;
            }
            else
            {
                P -= A; A = 0;
            }
            if (B > P)
            {
                B -= P; P = 0;
            }
            else
            {
                P -= B; B = 0;
            }
            if (C > P)
            {
                C -= P; P = 0;
            }
            else
            {
                P -= C; C = 0;
            }
            if (D > P)
            {
                D -= P; P = 0;
            }
            else
            {
                P -= D; D = 0;
            }
//            CRT = CRT + " 4) " + DateTime.Now.ToString();
            sqlstr = @"SELECT tbl.Lic, tbl.data_p, tbl.Summa FROM
                            (SELECT Right(Lic, 9) as Lic, data_p, SUM(opl) as Summa FROM [Abon].[dbo].[Pos" + Per + @"] a
                            WHERE brik <> " + Bricket27 + @" and (brik <> " + Bricket57 + @" or pach <> " + Bricket571 + ") and Lic = '1" + Lic.Substring(1, 9) + @"' 
                            GROUP BY Lic, data_p
                            UNION ALL
                            SELECT Right(Lic, 9) as Lic, data_p, SUM(opl) as Summa FROM [AbonUK].[dbo].[Pos" + Per + @"] a
                            WHERE brik <> " + Bricket27 + @" and (brik <> " + Bricket57 + @" or pach <> " + Bricket571 + ") and Lic = '2" + Lic.Substring(1, 9) + @"' 
                            GROUP BY Lic, data_p) as tbl
                            ORDER BY tbl.Lic";
            cmd = new SqlCommand(sqlstr, frmMain.db_con);
            cmd.CommandTimeout = 0;
//            frmMain.Memo1.Text = sqlstr;

            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    pays = new List<Pays>();
                    while (DRaeder.Read())
                        pays.Add(new Pays(Convert.ToDouble(DRaeder["Summa"]), Convert.ToDateTime(DRaeder["data_p"])));
                }
                else
                    pays = null;
                DRaeder.Close();
            }
//            CRT = CRT + " 5) " + DateTime.Now.ToString();
            PeriodBegin = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1);
            D40 = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1).AddMonths(-1).AddDays(40);
            D100 = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1).AddMonths(-3).AddDays(100);
        }

        //private void SaveNachToAbonent()
        //{

        //}

        // -- Вызов расчета пени по лицевому считу --
        public int CalcPeniLic()
        {
//            CRT = CRT + " 6) " + DateTime.Now.ToString();
            if (A > 0 || B > 0 || C > 0 || D > 0)
            {
                CalcOnePeni();
            }
            else
            {
                return 0;
            }
//            CRT = CRT + " 7) " + DateTime.Now.ToString();
            string str_sql;
            if (Lic == "2333964227" || Lic == "1010315900")
                return 0;
            if (Peni > 0)
            {
                str_sql = @"UPDATE a SET Charge = @Charge
                        FROM Abon.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.Abonent" + Per + @" b ON a.lic = b.lic
                        INNER JOIN Abon.dbo.SpVedomstvo c ON b.KodVedom = c.id
                        WHERE a.lic = '1" + Lic.Substring(1, 9) + @"' AND a.AbonServ_Code = 6 and c.bUK = 0 and c.bPaketC = 1;
                            UPDATE a SET Charge = @Charge
                        FROM AbonUK.dbo.AbonServSaldo" + Per + @" a INNER JOIN AbonUK.dbo.Abonent" + Per + @" b ON a.lic = b.lic
                        INNER JOIN AbonUK.dbo.SpVedomstvo c ON b.KodVedom = c.id
                        WHERE a.lic = '2" + Lic.Substring(1, 9) + @"' AND a.AbonServ_Code = 6 and c.bUK = 1 and c.bPaketC = 1;";
                SqlCommand cmd = new SqlCommand(str_sql, frmMain.db_con);
                cmd.Parameters.AddWithValue("@Charge", Math.Round(Peni + 0.0001, 2));
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
//                CRT = CRT + " 8) " + DateTime.Now.ToString();
                //str_sql = "INSERT INTO TestPeni (Lic, A, B, C, D, P, Peni, Dolg, Cnt) values (@Lic, @A, @B, @C, @D, @P, @Peni, @Dolg, @Cnt)";
                //    cmd = new SqlCommand(str_sql, frmMain.db_con);
                //    cmd.Parameters.AddWithValue("@Lic", Lic.ToString());
                //    cmd.Parameters.AddWithValue("@A", Math.Round(A, 2));
                //    cmd.Parameters.AddWithValue("@B", Math.Round(B, 2));
                //    cmd.Parameters.AddWithValue("@C", Math.Round(C, 2));
                //    cmd.Parameters.AddWithValue("@D", Math.Round(D, 2));
                //    cmd.Parameters.AddWithValue("@P", Math.Round(P, 2));
                //    cmd.Parameters.AddWithValue("@Peni", Math.Round(Peni, 2));
                //    cmd.Parameters.AddWithValue("@Dolg", Math.Round(Dolg, 2));
                //    cmd.Parameters.AddWithValue("@Cnt", jjj);
                //    cmd.CommandTimeout = 0;
                //cmd.ExecuteNonQuery();

                string sqlstr =
                @"UPDATE c SET Peni = a.Charge
                        FROM Abon.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.Abonent" + Per + @" c ON a.lic = c.lic
                        WHERE a.Charge <> 0 and AbonServ_Code = 6 and a.lic = '1" + Lic.Substring(1, 9) + @"';
                    UPDATE c SET Peni = a.Charge
                        FROM AbonUK.dbo.AbonServSaldo" + Per + @" a INNER JOIN AbonUK.dbo.Abonent" + Per + @" c ON a.lic = c.lic
                        WHERE a.Charge <> 0 and AbonServ_Code = 6 and a.lic = '2" + Lic.Substring(1, 9) + "';";
                cmd = new SqlCommand(sqlstr, frmMain.db_con);
                cmd.ExecuteNonQuery();
                return 1;
//                CRT = CRT + " 9) " + DateTime.Now.ToString();
                //SaveNachToAbonent();
            }
            return 0;
        }

        // -- Рассчитываем пеню по одному счету --        
        private void CalcOnePeni()
        {
            for (int j = 0; j < DateTime.DaysInMonth(PeriodBegin.Year, PeriodBegin.Month); j++)
            {
                if (D40.CompareTo(PeriodBegin.AddDays(j)) == 0)
                    X4 = SmallPeniKoefficient;
                if (D100.CompareTo(PeriodBegin.AddDays(j)) == 0)
                    X2 = BigPeniKoefficient;
                double PayedSumma = GetSumm(Lic, PeriodBegin.AddDays(j));
                if (PayedSumma != 0)
                {
                    PaymentDistribution(PayedSumma);
                    if (A <= 0 && B <= 0 && C <= 0 && D <= 0)
                        return;
                }
                Peni += A * X1 + B * X2 + C * X3 + D * X4;
            }
        }

        // -- Распределяем платеж на нужном лицевом счете --
        private void PaymentDistribution(double payment)
        {
            A -= payment;
            if (A < 0)
            {
                payment = -A;
                A = 0;
                B -= payment;
                if (B < 0)
                {
                    payment = -B;
                    B = 0;
                    C -= payment;
                    if (C < 0)
                    {
                        payment = -C;
                        C = 0;
                        D -= payment;
                        if (D < 0)
                            D = 0;
                    }
                }
            }
        }

        // -- Берем сумму поступлений за выбранную дату по выбранному счету, Пока просто данные для теста --
        private double GetSumm(string lic, DateTime NowDate)
        {
            double SummaOfPayments = 0;
            if (pays != null)
            {
                List<Pays> payments = (from t in pays where t.PayDate == NowDate select t).ToList();
                if (payments.Count != 0)
                {
                    for (int i = 0; i < payments.Count; i++)
                        SummaOfPayments += payments[i].Summa;
                }
            }
            return SummaOfPayments;
        }

        // -- Уменьшение строки с датой на месяц --
        private string GetPrevPeriod(string Period, int CountMonths = 1)
        {
            for (int i = 1; i <= CountMonths; i++)
                Period = (Period.Substring(4, 2) == "01") ? (Convert.ToInt32(Period) - 89).ToString() : (Convert.ToInt32(Period) - 1).ToString();
            return Period;
        }

        // -- Увеличение строки с датой на месяц --
        private string GetNextPeriod(string Period, int CountMonths = 1)
        {
            for (int i = 1; i <= CountMonths; i++)
                Period = (Period.Substring(4, 2) == "12") ? (Convert.ToInt32(Period) + 89).ToString() : (Convert.ToInt32(Period) + 1).ToString();
            return Period;
        }

        public string GetResultTimes()
        {
            return CRT;
        }

        public void RepairDeletedPeni()
        {
            string i = "201604";
            string sqlstr = "";
            string Base;
            int Usluga;
            while (i != Per)
            {
                sqlstr = sqlstr +
                @" SELECT a.lic, " + i + @" as period, b.skvagina, b.lgkan, a.saldo
                   FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic WHERE a.abonserv_code = 6 and a.saldo < 0
                   UNION ALL 
                   SELECT a.lic, " + i + @" as period, b.skvagina, b.lgkan, a.saldo
                   FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic WHERE a.abonserv_code = 6 and a.saldo < 0 ";
                i = GetNextPeriod(i);
                if (i != Per)
                {
                    sqlstr = sqlstr + " UNION ALL ";
                }
            }
            SqlCommand cmd = new SqlCommand(sqlstr, frmMain.db_con);
            List<PeniRepire> peniRepire = new List<PeniRepire>();
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                        peniRepire.Add(new PeniRepire(DRaeder["Lic"].ToString(), DRaeder["Period"].ToString(), Convert.ToInt32(DRaeder["Skvagina"]),
                            Convert.ToInt32(DRaeder["Lgkan"]), 0, 0, Convert.ToDouble(DRaeder["Saldo"])));
                }
            }
            foreach (PeniRepire Pr in peniRepire)
            {
                Base = Pr.lic.Substring(0, 1) == "1" ? "abon.dbo." : "abonuk.dbo.";
                Usluga = Pr.skvagina == 0 ? 1 : 2;
                sqlstr = "UPDATE " + Base + "abonservsaldo" + Pr.period + " SET Saldo = 0 WHERE lic = @lic and abonserv_code = 6";
                cmd = new SqlCommand(sqlstr, frmMain.db_con);
                cmd.Parameters.AddWithValue("@lic", Pr.lic);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                sqlstr = "UPDATE " + Base + "abonservsaldo" + Pr.period + " SET Saldo = Saldo + @Saldo WHERE lic = @lic and abonserv_code = @Usluga";
                cmd = new SqlCommand(sqlstr, frmMain.db_con);
                cmd.Parameters.AddWithValue("@Saldo", Math.Round(Pr.saldo + 0.0001, 2));
                cmd.Parameters.AddWithValue("@lic", Pr.lic);
                cmd.Parameters.AddWithValue("@Usluga", Usluga);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
            i = "201603";
            sqlstr = "";
            while (i != Per)
            {
                sqlstr = sqlstr +
                @" SELECT a.lic, " + i + @" as period, b.skvagina, b.lgkan, a.saldo + a.charge as ostat, a.pay - (a.saldo + a.charge) as razn
                   FROM abonuk.dbo.abonservsaldo" + i + @" a INNER JOIN abonuk.dbo.abonent" + i + @" b ON a.lic = b.lic WHERE a.abonserv_code = 6 and a.pay > a.saldo + a.charge
                   UNION ALL
                   SELECT a.lic, " + i + @" as period, b.skvagina, b.lgkan, a.saldo + a.charge as ostat, a.pay - (a.saldo + a.charge) as razn
                   FROM abon.dbo.abonservsaldo" + i + @" a INNER JOIN abon.dbo.abonent" + i + @" b ON a.lic = b.lic WHERE a.abonserv_code = 6 and a.pay > a.saldo + a.charge";
                i = GetNextPeriod(i);
                if (i != Per)
                {
                    sqlstr = sqlstr + " UNION ALL ";
                }
            }
            cmd = new SqlCommand(sqlstr, frmMain.db_con);
            cmd.CommandTimeout = 0;
            peniRepire = new List<PeniRepire>();
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                        peniRepire.Add(new PeniRepire(DRaeder["Lic"].ToString(), DRaeder["Period"].ToString(), Convert.ToInt32(DRaeder["Skvagina"]),
                            Convert.ToInt32(DRaeder["Lgkan"]), Convert.ToDouble(DRaeder["Ostat"]), Convert.ToDouble(DRaeder["Razn"]), 0));
                }
            }
            foreach(PeniRepire Pr in peniRepire)
            {
                Base = Pr.lic.Substring(0, 1) == "1" ? "abon.dbo." : "abonuk.dbo.";
                Usluga = Pr.skvagina == 0 ? 1 : 2;
                sqlstr = "UPDATE " + Base + "abonservsaldo" + Pr.period + " SET Pay = @Pay WHERE lic = @lic and abonserv_code = 6";
                cmd = new SqlCommand(sqlstr, frmMain.db_con);
                cmd.Parameters.AddWithValue("@Pay", Math.Round(Pr.ostat + 0.0001, 2));
                cmd.Parameters.AddWithValue("@lic", Pr.lic);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                sqlstr = "UPDATE " + Base + "abonservsaldo" + Pr.period + " SET Pay = Pay + @Pay WHERE lic = @lic and abonserv_code = @Usluga";
                cmd = new SqlCommand(sqlstr, frmMain.db_con);
                cmd.Parameters.AddWithValue("@Pay", Math.Round(Pr.razn + 0.0001, 2));
                cmd.Parameters.AddWithValue("@lic", Pr.lic);
                cmd.Parameters.AddWithValue("@Usluga", Usluga);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }
    }
}

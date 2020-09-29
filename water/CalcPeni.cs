using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace water
{
    class Licsus
    {
        public double A, B, C, D, P;
        public string Lic;
        public Licsus(double a, double b, double c, double d, string lic)
        {
            Lic = lic;
            A = a;
            B = b;
            C = c;
            D = d;
            P = 0;
        }
    }

    class Payments
    {
        public double Summa;
        public string Lic;
        public DateTime PayDate;
        public Payments(double summa, string lic, DateTime paydate)
        {
            Lic = lic;
            Summa = summa;
            PayDate = paydate;
        }
    }

    class CalcPeni
    {
        private string Per;
        private DateTime D40, D100, PeriodBegin;
        private double X1, X2, X3, X4;
        private SqlConnection conn;
        public List<Licsus> licsus;
        private List<Payments> Payments;
        const double StavRefinans = 0.09;
        const double BigPeniKoefficient = 1 / 130.0 * StavRefinans;
        const double SmallPeniKoefficient = 1 / 300.0 * StavRefinans;
        const string Bricket27 = "27"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом
        const string Bricket57 = "57"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом
        const string Bricket571 = "571"; // -- брикет для переноса данных из одной базы данных в другую. не является платежом

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
        
// -- Конструктор, инициализирующий данные --
        public CalcPeni(int TypeCalc, string per, SqlConnection Conn, string Lic = "")
        {
            Per = per;
            conn = Conn;
// -- Иницилизация для начисления пени в заданном периоде -----------------------
            if (TypeCalc == 1)
            {
                // -- Выбираем все нужные долги и начисления --
                string One_Lic = (Lic == "" ? Lic : " and a.Lic = '1" + Lic.Substring(1, 9) + "' ");
                string Two_Lic = (Lic == "" ? Lic : " and a.Lic = '2" + Lic.Substring(1, 9) + "' ");
                string sqlstr =
                    @"  SELECT lic, SUM(A) as A, SUM(b) as B, SUM(C) as C, SUM(d) as D, SUM(Pos) as Pos, SUM(dolg) as Dolg 
                        FROM
                        (
                        -- январь --
                        SELECT RIGHT(a.lic, 9) as lic, (CASE WHEN a.SDOLGBEG > 0 THEN a.SDOLGBEG ELSE 0 END) as dolg,
                        (CASE WHEN a.SDOLGBEG < 0 THEN a.SDOLGBEG ELSE 0 END) as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent201601 a
                        INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos201601 WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0 " + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, (CASE WHEN a.SDOLGBEG > 0 THEN a.SDOLGBEG ELSE 0 END) as dolg,
                        (CASE WHEN a.SDOLGBEG < 0 THEN a.SDOLGBEG ELSE 0 END) as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent201601 a
                        INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos201601 WHERE brik <> 27" + Two_Lic.Replace("a.", " ") + @" GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + Two_Lic + @"
                        UNION ALL ";
                // -- Выбираем все долги изначально больше 100 дней --
                string i = "201601";
                while (i != GetPrevPeriod(Per, 4))
                {
                    sqlstr = sqlstr +
                    @"  SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, a.Nachisl + a.Poliv as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent" + i + @" a INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos" + i + @" WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0 " + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, a.Nachisl + a.Poliv as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent" + i + @" a INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos" + i + @" WHERE brik <> 27" + Two_Lic.Replace("a.", " ") + @" GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0 " + Two_Lic + @"
                        UNION ALL ";
                    i = GetNextPeriod(i);
                }
                sqlstr = sqlstr + @" -- 40 - 100 дней --
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, a.Nachisl + a.Poliv as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent" + GetPrevPeriod(Per, 4) + @" a
                        INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos" + GetPrevPeriod(Per, 4) + @" WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, a.Nachisl + a.Poliv as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent" + GetPrevPeriod(Per, 4) + @" a
                        INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos" + GetPrevPeriod(Per, 4) + @" WHERE brik <> 27 " + Two_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + Two_Lic + @"
                        UNION ALL
                        -- 40 дней -- 
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, a.Nachisl + a.Poliv as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent" + GetPrevPeriod(Per, 3) + @" a
                        INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos" + GetPrevPeriod(Per, 3) + @" WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, a.Nachisl + a.Poliv as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent" + GetPrevPeriod(Per, 3) + @" a
                        INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos" + GetPrevPeriod(Per, 3) + @" WHERE brik <> 27 " + Two_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + Two_Lic + @"
                        UNION ALL
                        -- 0 - 40 дней -- 
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, 0 as C, a.Nachisl + a.Poliv as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent" + GetPrevPeriod(Per, 2) + @" a
                        INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos" + GetPrevPeriod(Per, 2) + @" WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, 0 as C, a.Nachisl + a.Poliv as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent" + GetPrevPeriod(Per, 2) + @" a
                        INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos" + GetPrevPeriod(Per, 2) + @" WHERE brik <> 27 " + Two_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + Two_Lic + @"
                        UNION ALL

                        -- предыдущий период (только поступления) --
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abon.dbo.abonent" + GetPrevPeriod(Per, 1) + @" a
                        INNER JOIN abon.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abon.dbo.pos" + GetPrevPeriod(Per, 1) + @" WHERE brik <> 27 " + One_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 0 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(a.lic, 9) as lic, 0 as dolg, 0 as A, 0 as B, 0 as C, 0 as D, ISNULL(c.Pos, 0) as pos
                        FROM abonuk.dbo.abonent" + GetPrevPeriod(Per, 1) + @" a
                        INNER JOIN abonuk.dbo.spvedomstvo b ON a.kodvedom = b.id
                        LEFT JOIN (SELECT lic, SUM(Opl + Poliv) as pos FROM abonuk.dbo.pos" + GetPrevPeriod(Per, 1) + @" WHERE brik <> 27 " + Two_Lic.Replace("a.", " ") + @"GROUP BY lic) c ON a.lic = c.lic
                        WHERE b.bUK = 1 and b.bPaketC = 1 and a.prochee = 0 and a.gruppa3 = 0" + Two_Lic + @"
                        UNION ALL ";
// -- сторно за периоды больше 100 дней --
                i = "201601";
//                string j = "201601";
                if (One_Lic != "")
                    One_Lic = "WHERE " + One_Lic.Replace("and a.", " ");
                if (Two_Lic != "")
                    Two_Lic = "WHERE " + Two_Lic.Replace("and a.", " "); ;
                while (i != Per)
                {
                    string SelectText = "";
                    if (i == GetPrevPeriod(Per, 2))
                        SelectText = " 0 as A, 0 as B, 0 as C, oplata as D ";
                    else if (i == GetPrevPeriod(Per, 3))
                        SelectText = " 0 as A, 0 as B, oplata as C, 0 as D ";
                    else if (i == GetPrevPeriod(Per, 4))
                        SelectText = " 0 as A, oplata as B, 0 as C, 0 as D ";
                    else
                        SelectText = " oplata as A, 0 as B, 0 as C, 0 as D ";
                    if(i != "201601")
                        sqlstr = sqlstr + " UNION ALL ";
                    sqlstr = sqlstr +
                        @"SELECT RIGHT(lic, 9) as lic, 0 as dolg, " + SelectText + @", 0 as pos
                        FROM abon.dbo.abonent" + i + @" " + One_Lic + @"
                        UNION ALL
                        SELECT RIGHT(lic, 9) as lic, 0 as dolg, " + SelectText + @", 0 as pos
                        FROM abonuk.dbo.abonent" + i + @" " + Two_Lic; 
                    i = GetNextPeriod(i);

//                    string SelectText = "";
//                    if (i == GetPrevPeriod(Per, 2))
//                        SelectText = " 0 as A, 0 as B, 0 as C, oplata as D ";
//                    else if (i == GetPrevPeriod(Per, 3))
//                        SelectText = " 0 as A, 0 as B, oplata as C, 0 as D ";
//                    else if (i == GetPrevPeriod(Per, 4))
//                        SelectText = " 0 as A, oplata as B, 0 as C, 0 as D ";
//                    else
//                        SelectText = " oplata as A, 0 as B, 0 as C, 0 as D ";
//                    if (i != "201601" || j != "201601")
//                        sqlstr = sqlstr + " UNION ALL ";
//                    sqlstr = sqlstr +
//                        @"SELECT RIGHT(lic, 9) as lic, 0 as dolg, " + SelectText + @", 0 as pos
//                        FROM abon.dbo.oplata" + i + @" WHERE perN = '" + j + @"'" + One_Lic + @"
//                        UNION ALL
//                        SELECT RIGHT(lic, 9) as lic, 0 as dolg, " + SelectText + @", 0 as pos
//                        FROM abonuk.dbo.oplata" + i + @" WHERE perN = '" + j + @"'" + Two_Lic + @"
//                        UNION ALL 
//                        SELECT RIGHT(lic, 9) as lic, CASE WHEN oplata < 0 THEN oplata ELSE 0 END as dolg, 0 as A, 0 as B, 0 as C, 0 as D, 0 as pos
//                        FROM abon.dbo.oplata" + i + @" WHERE perN < 201601" + One_Lic + @"
//                        UNION ALL
//                        SELECT RIGHT(lic, 9) as lic, CASE WHEN oplata < 0 THEN oplata ELSE 0 END as dolg, 0 as A, 0 as B, 0 as C, 0 as D, 0 as pos
//                        FROM abonuk.dbo.oplata" + i + @" WHERE perN < 201601" + Two_Lic;
//                    if (i == Per)
//                    {
//                        j = GetNextPeriod(j);
//                        i = j;
//                    }
//                    else
//                        i = GetNextPeriod(i);
                }
//                        -- сторно за предыдущие периоды --
                sqlstr = sqlstr +
                        @") as tbl
                        GROUP BY lic
                        HAVING SUM(a+b+c+d-(CASE WHEN Pos > dolg THEN Pos - dolg ELSE 0 END)) > 0
                        ORDER BY Lic";



                SqlCommand cmd = new SqlCommand(sqlstr, conn);
                cmd.CommandTimeout = 0;
                using (SqlDataReader DRaeder = cmd.ExecuteReader())
                {
                    if (DRaeder.HasRows)
                    {
                        licsus = new List<Licsus>();
                        while (DRaeder.Read())
                        {
                            Double RealPos = Convert.ToDouble(DRaeder["Dolg"]) > Convert.ToDouble(DRaeder["Pos"]) ? 0 : Convert.ToDouble(DRaeder["Pos"]) - Convert.ToDouble(DRaeder["Dolg"]);
                            double A = Convert.ToDouble(DRaeder["A"]);
                            double B = Convert.ToDouble(DRaeder["B"]);
                            double C = Convert.ToDouble(DRaeder["C"]);
                            double D = Convert.ToDouble(DRaeder["D"]);
                            if (A + B + C + D > RealPos && A + B + C + D > 0)
                            {
                                licsus.Add(new Licsus(A, B, C, D, DRaeder["lic"].ToString()));
                                PaymentDistribution(licsus[licsus.Count - 1], RealPos);
                            }
                        }
                    }
                    else
                    {
                        licsus = null;
                    }
                    DRaeder.Close();
                }
                if (licsus != null)
                {
                    sqlstr = @"SELECT tbl.Lic, tbl.data_p, tbl.Summa FROM
                            (SELECT Right(Lic, 9) as Lic, data_p, SUM(opl) as Summa FROM [Abon].[dbo].[Pos" + Per + @"] a
                            WHERE brik <> " + Bricket27 + @" and (brik <> " + Bricket57 + @" or pach <> " + Bricket571 + ") " + One_Lic.Replace("WHERE", " and ") + @"
                            GROUP BY Lic, data_p
                            UNION ALL
                            SELECT Right(Lic, 9) as Lic, data_p, SUM(opl) as Summa FROM [AbonUK].[dbo].[Pos" + Per + @"] a
                            WHERE brik <> " + Bricket27 + @" and (brik <> " + Bricket57 + @" or pach <> " + Bricket571 + ") " + Two_Lic.Replace("WHERE", " and ") + @"
                            GROUP BY Lic, data_p) as tbl
                            ORDER BY tbl.Lic";
                    cmd = new SqlCommand(sqlstr, conn);
                    using (SqlDataReader DRaeder = cmd.ExecuteReader())
                    {
                        if (DRaeder.HasRows)
                        {
                            Payments = new List<Payments>();
                            while (DRaeder.Read())
                            {
                                Payments.Add(new Payments(Convert.ToDouble(DRaeder["Summa"]), DRaeder["Lic"].ToString(), Convert.ToDateTime(DRaeder["data_p"])));
                            }
                        }
                        else
                        {
                            Payments = null;
                        }
                        DRaeder.Close();
                    }
                }
                if (licsus != null)
                {
                    X1 = BigPeniKoefficient;
                    X2 = SmallPeniKoefficient;
                    X3 = SmallPeniKoefficient;
                    X4 = 0;
                    PeriodBegin = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1);
                    D40 = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1).AddMonths(-1).AddDays(40);
                    D100 = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1).AddMonths(-3).AddDays(100);
                }
            }
// --------- Иницилизация для переноса неначисляемых остстков на новый период ---------------------
            else if (TypeCalc == 2)
            {
//                SaveOldMoneyToPeniCalc(Lic);
            }
// --------- Сохраняем начисленное за период пени из PeniCalc в AbonServSaldo ---------------------
            else if (TypeCalc == 3)
            {
//                SaveNachToAbonServSaldo(Lic);
            }
// --------- Сохраняем начисленное за период пени из AbonServSaldo в Абонент ----------------------
            else if (TypeCalc == 4)
            {
                SaveNachToAbonent(Lic);
            }
            else 
            {
                // -- Если не 1, 2, 3, то ничего не делаем при инициализации, кроме присвоения даты и соединения
            }
        }

        public void ClearData(string lic = "")
        {
            //if (lic != "")
            //    lic = " and lic = '" + lic.Substring(1, 9) + "'";
            //string sqlstr = "DELETE FROM Abon.dbo.PeniCalc WHERE per = '" + Per.Substring(2, 4) + "'" + lic + ";";
            //SqlCommand cmd = new SqlCommand(sqlstr, conn);
            //cmd.ExecuteNonQuery();
        }
        
        private void SaveOldMoneyToPeniCalc(string lic = "")
        {
//            SqlCommand cmd;
//            string PredPer = (Per.Substring(4, 2) == "01" ? (Convert.ToInt32(Per) - 89) : (Convert.ToInt32(Per) - 1)).ToString();
//            ClearData(lic);
//            if (lic != "")
//                lic = " and a.lic = '" + lic + "'";
//            string sqlstr =
//            @"INSERT INTO Abon.dbo.PeniCalc
//                (lic, per, MoneyForPeni)
//                (SELECT ttt.lic, ttt.per, ttt.MoneyForPeni - ISNULL(ttt.pos, 0) as dolg FROM
//                (SELECT tbl.lic as lic, tbl.per, tbl.MoneyForPeni, SUM(tbl.pos) as pos FROM
//                (SELECT b.lic, '" + Per.Substring(2, 4) + @"' as per, b.MoneyForPeni, SUM(a.Opl + a.Poliv) as pos
//                FROM Abon.dbo.PeniCalc b LEFT JOIN Abon.dbo.Pos" + PredPer + @" a ON RIGHT(a.lic, 9) = b.lic and a.brik <> 27" + lic + @"
//                WHERE b.per = '" + PredPer.Substring(2, 4) + @"'
//                GROUP BY b.lic, b.MoneyForPeni
//                UNION
//                SELECT b.lic, '" + Per.Substring(2, 4) + @"' as per, b.MoneyForPeni, SUM(a.Opl + a.Poliv) as pos
//                FROM Abon.dbo.PeniCalc b LEFT JOIN AbonUK.dbo.Pos" + PredPer + @" a ON RIGHT(a.lic, 9) = b.lic and a.brik <> 27" + lic + @"
//                WHERE b.per = '" + PredPer.Substring(2, 4) + @"'
//                GROUP BY b.lic, b.MoneyForPeni) as tbl
//                GROUP BY tbl.lic, tbl.per, tbl.MoneyForPeni) as ttt
//                WHERE ttt.MoneyForPeni - ISNULL(ttt.pos, 0) > 0)";
//            cmd = new SqlCommand(sqlstr, conn);
//            cmd.ExecuteNonQuery();
        }
       

        private void SaveNachToAbonServSaldo(string lic = "")
        {
//            string PredPer = (Convert.ToInt32(Per.Substring(4, 2)) == 01 ? (Convert.ToInt32(Per) - 89) : (Convert.ToInt32(Per) - 1)).ToString();
//            string sqlstr;
//            string Lic = lic == "" ? lic : " and lic = '" + lic + "'";
//            SqlCommand cmd;
//            sqlstr = @"UPDATE Abon.dbo.AbonServSaldo" + Per + @" SET Charge = 0 WHERE AbonServ_Code = 6" + Lic + @";
//                       UPDATE AbonUK.dbo.AbonServSaldo" + Per + @" SET Charge = 0 WHERE AbonServ_Code = 6" + Lic + ";";
//            cmd = new SqlCommand(sqlstr, conn);
//            cmd.ExecuteNonQuery();
//            Lic = lic == "" ? lic : " and a.lic = '" + lic + "'";
//            sqlstr =
//            @"UPDATE Abon.dbo.AbonServSaldo" + Per + @" SET Charge = b.Nach
//                FROM Abon.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.PeniCalc b ON Right(a.lic, 9) = b.lic
//                INNER JOIN Abon.dbo.Abonent" + Per + @" c ON a.lic = c.lic INNER JOIN Abon.dbo.SpVedomstvo d ON c.KodVedom = d.id
//                WHERE AbonServ_Code = 6 and d.bUK = 0 and d.bPaketC = 1 and b.Nach <> 0 " + Lic + @";
//              UPDATE AbonUK.dbo.AbonServSaldo" + Per + @" SET Charge = b.Nach
//                FROM AbonUK.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.PeniCalc b ON Right(a.lic, 9) = b.lic
//                INNER JOIN AbonUK.dbo.Abonent" + Per + @" c ON a.lic = c.lic INNER JOIN AbonUK.dbo.SpVedomstvo d ON c.KodVedom = d.id
//                WHERE AbonServ_Code = 6 and d.bUK = 1 and d.bPaketC = 1 and b.Nach <> 0" + Lic + ";";
//            cmd = new SqlCommand(sqlstr, conn);
//            cmd.ExecuteNonQuery();
        }

        private void SaveNachToAbonent(string lic = "")
        {
            string sqlstr;
            SqlCommand cmd;
            string Lic;
            Lic = lic == "" ? lic : " WHERE lic = '" + lic + "'";
            sqlstr = @"UPDATE Abon.dbo.Abonent" + Per + @" SET Peni = 0 " + Lic + @";
                       UPDATE AbonUK.dbo.Abonent" + Per + @" SET Peni = 0 " + Lic;
            cmd = new SqlCommand(sqlstr, conn);
            cmd.ExecuteNonQuery();
            Lic = lic == "" ? lic : " and a.lic = '" + lic + "'";
            sqlstr =
            @"UPDATE c SET Peni = a.Charge
                FROM Abon.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.Abonent" + Per + @" c ON a.lic = c.lic
                WHERE a.Charge <> 0 and AbonServ_Code = 6" + Lic + @";
              UPDATE c SET Peni = a.Charge
                FROM AbonUK.dbo.AbonServSaldo" + Per + @" a INNER JOIN AbonUK.dbo.Abonent" + Per + @" c ON a.lic = c.lic
                WHERE a.Charge <> 0 and AbonServ_Code = 6" + Lic + @";";
            cmd = new SqlCommand(sqlstr, conn);
            cmd.ExecuteNonQuery();
        }


// -- Вызов расчета пени по всем выбранным считам --
        public int CalcPeniAll()
        {
//            SqlCommand cmd = new SqlCommand("UPDATE Abon.dbo.PeniCalc SET nach = 0 WHERE per = '" + Per.Substring(2, 4) + "';", conn);
//            cmd.ExecuteNonQuery();
            string str_sql;
            int nn = 0;
            if (licsus != null)
            {
                str_sql = @"UPDATE Abon.dbo.AbonServSaldo" + Per + @" SET Charge = 0 WHERE AbonServ_Code = 6;
                            UPDATE AbonUK.dbo.AbonServSaldo" + Per + @" SET Charge = 0 WHERE AbonServ_Code = 6;";
                SqlCommand cmd = new SqlCommand(str_sql, conn);
                cmd.ExecuteNonQuery();
                for (int i = 0; i < licsus.Count; i++)
                {
                    if (licsus[i].Lic == "2333964227")
                        continue;
                    Licsus lics = licsus[i];
//                    if (lics.Lic == "303423200")
//                       nn = 1;
                    if (lics.A > 0 || lics.B > 0 || lics.C > 0 || lics.D > 0)
                        CalcOnePeni(lics);
                    if (lics.P > 0)
                    {
                        str_sql = @"UPDATE a SET Charge = @Charge
                                FROM Abon.dbo.AbonServSaldo" + Per + @" a INNER JOIN Abon.dbo.Abonent" + Per + @" b ON a.lic = b.lic
                                INNER JOIN Abon.dbo.SpVedomstvo c ON b.KodVedom = c.id
                                WHERE a.lic = '1" + lics.Lic + @"' AND a.AbonServ_Code = 6 and c.bUK = 0 and c.bPaketC = 1;
                                    UPDATE a SET Charge = @Charge
                                FROM AbonUK.dbo.AbonServSaldo" + Per + @" a INNER JOIN AbonUK.dbo.Abonent" + Per + @" b ON a.lic = b.lic
                                INNER JOIN AbonUK.dbo.SpVedomstvo c ON b.KodVedom = c.id
                                WHERE a.lic = '2" + lics.Lic + @"' AND a.AbonServ_Code = 6 and c.bUK = 1 and c.bPaketC = 1;";
                        cmd = new SqlCommand(str_sql, conn);
                        cmd.Parameters.AddWithValue("@Charge", Math.Round(lics.P + 0.0001, 2).ToString());
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                return 0;
            }
            return nn;//licsus.Count;
        }

// -- Рассчитываем пеню по одному счету --        
        private void CalcOnePeni(Licsus lics)
        {
            X1 = BigPeniKoefficient;
            X2 = SmallPeniKoefficient;
            X3 = SmallPeniKoefficient;
            X4 = 0;
            for (int j = 0; j < DateTime.DaysInMonth(PeriodBegin.Year, PeriodBegin.Month); j++)
            {
                if(D40.CompareTo(PeriodBegin.AddDays(j)) == 0)
                    X4 = SmallPeniKoefficient;
                if(D100.CompareTo(PeriodBegin.AddDays(j)) == 0)
                    X2 = BigPeniKoefficient;
                double PayedSumma = GetSumm(lics.Lic, PeriodBegin.AddDays(j));
                if (PayedSumma != 0)
                {
                    PaymentDistribution(lics, PayedSumma);
                    if (lics.A <= 0 && lics.B <= 0 && lics.C <= 0 && lics.D <= 0)
                        return;
                }
                lics.P += lics.A * X1 + lics.B * X2 + lics.C * X3 + lics.D * X4;
            }
        }

// -- Берем сумму поступлений за выбранную дату по выбранному счету, Пока просто данные для теста --
        private double GetSumm(string lic, DateTime NowDate)
        {
            double SummaOfPayments = 0;
            if (Payments != null)
            {
                List<Payments> payments = (from t in Payments where t.Lic == lic && t.PayDate == NowDate select t).ToList();
                if (payments.Count != 0)
                {
                    for (int i = 0; i < payments.Count; i++)
                        SummaOfPayments += payments[i].Summa;
                }
            }
            return SummaOfPayments;
        }


// -- Если какая-то из сумм меньше 0, то возвращаем эту сумму для распределения --
        private double FixSum(Licsus lics)
        {
            double payment = 0;
            if(lics.A < 0)
            {
                payment -= lics.A;
                lics.A = 0;
            }
            if (lics.B < 0)
            {
                payment -= lics.B;
                lics.B = 0;
            }
            if (lics.C < 0)
            {
                payment -= lics.C;
                lics.C = 0;
            }
            if (lics.D < 0)
            {
                payment -= lics.D;
                lics.D = 0;
            }
            return payment;
        }

 // -- Распределяем платеж на нужном лицевом счете --
        private void PaymentDistribution(Licsus lics, double payment)
        {
            //payment = payment < 0 ? 0 : payment;
            payment = FixSum(lics) + (payment < 0 ? 0 : payment);
            lics.A -= payment;
            if (lics.A < 0)
            {
                payment = -lics.A;
                lics.A = 0;
                lics.B -= payment;
                if (lics.B < 0)
                {
                    payment = -lics.B;
                    lics.B = 0;
                    lics.C -= payment;
                    if (lics.C < 0)
                    {
                        payment = -lics.C;
                        lics.C = 0;
                        lics.D -= payment;
                        if (lics.D < 0)
                        {
                            lics.D = 0;
                        }
                    }
                }
            }
        }

    }
}






// --------------------- Перенос неначисляемых остатков в PeniCalc на ----------------
// --------------------- следующий период --------------------------------------------
//SqlConnection conn = new SqlConnection(db_con.ConnectionString);
//conn.Open();
//DateTime TheTime = DateTime.Now;
//CalcPeni calcPeni = new CalcPeni(2, Per, conn);
//MessageBox.Show(DateTime.Now.Subtract(TheTime).TotalMinutes.ToString() + " минут", "Все");

// --------------------- Для начисления пени в PeniCalc ------------------------------
//SqlConnection conn = new SqlConnection(db_con.ConnectionString);
//conn.Open();
//DateTime TheTime = DateTime.Now;
//CalcPeni calcPeni = new CalcPeni(1, Per, conn);
//calcPeni.CalcPeniAll();
//MessageBox.Show(DateTime.Now.Subtract(TheTime).TotalMinutes.ToString() + " минут", "Все");

// ---------------------- Для сохранения начисленного пени в AbonServSaldo -----------
//SqlConnection conn = new SqlConnection(db_con.ConnectionString);
//conn.Open();
//DateTime TheTime = DateTime.Now;
//CalcPeni calcPeni = new CalcPeni(3, Per, conn);
//MessageBox.Show(DateTime.Now.Subtract(TheTime).TotalMinutes.ToString() + " минут", "Все");

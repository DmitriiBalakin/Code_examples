using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace water
{
    class SetMeanCube
    {
        class CubeC
        {
            public int RowID;
            public int KubH1;
            public int KubH2;
            public int KubH3;
            public int KubG1;
            public int KubG2;
            public int KubG3;
            public double CntM;
            public bool MeanCube;
            public CubeC (int RowID, int KubH1, int KubH2, int KubH3, int KubG1,
                               int KubG2, int KubG3, int CntM, bool MeanCube)
            {
                this.RowID = RowID;
                this.KubH1 = KubH1;
                this.KubH2 = KubH2;
                this.KubH3 = KubH3;
                this.KubG1 = KubG1;
                this.KubG2 = KubG2;
                this.KubG3 = KubG3;
                this.CntM = CntM;
                this.MeanCube = MeanCube;
            }
        }

        class InsertionInfo
        {
            public  int KubH1n;
            public  int KubH1s;
            public  int KubH2n;
            public  int KubH2s;
            public  int KubH3n;
            public  int KubH3s;
            public  int KubG1n;
            public  int KubG1s;
            public  int KubG2n;
            public  int KubG2s;
            public  int KubG3n;
            public  int KubG3s;
            public InsertionInfo(int KubH1n, int KubH1s, int KubH2n, int KubH2s, int KubH3n, int KubH3s,
                                 int KubG1n, int KubG1s, int KubG2n, int KubG2s, int KubG3n, int KubG3s)
            {
                this.KubH1n = KubH1n;
                this.KubH1s = KubH1s;
                this.KubH2n = KubH2n;
                this.KubH2s = KubH2s;
                this.KubH3n = KubH3n;
                this.KubH3s = KubH3s;
                this.KubG1n = KubG1n;
                this.KubG1s = KubG1s;
                this.KubG2n = KubG2n;
                this.KubG2s = KubG2s;
                this.KubG3n = KubG3n;
                this.KubG3s = KubG3s;
            }
        }

        private string Per;
        private SqlConnection conn;
        private string Lic;
        private string CurrentBase;
        int baseName;
        private string NotCurrentBase;
        private string PerOpl;
        DateTime MaxCurDate;
        DateTime Date3M;                                                // -- Дата за три месяца до текущего периода (первое число месяца) --
        DateTime Date6M;                                                // -- Дата за двенадцать месяцев до текущего периода (первое число месяца) --

        // -- Уменьшение строки с датой на месяц --
        public string GetPrevPeriod(string Period, int CountMonths = 1)
        {
            for (int i = CountMonths; i > 0; i--)
              Period = (Period.Substring(4, 2) == "01") ? (Convert.ToInt32(Period) - 89).ToString() : (Convert.ToInt32(Period) - 1).ToString();
            return Period;
        }

        // ----------------------------------------------------------------------------
        // -- Обновление среднего по счету. Удаление старого и запись инвентаризации --
        // ----------------------------------------------------------------------------
        private int ClearMean()
        {
            // -------------------------------------------------------------------------------
            // -- Удаляем все показания по среднему за указанный период по указанному счету --
            // -------------------------------------------------------------------------------
            string sql = "DELETE FROM " + CurrentBase + @"PosVod WHERE MeanCube = 1 AND PerOpl = @PerOpl AND Lic = @Lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@PerOpl", SqlDbType.NVarChar).Value = PerOpl;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.ExecuteNonQuery();
            //if (cmd.ExecuteNonQuery() > 0)
            //{
            //    return 1;
            //}
            //else
            //{
            //    return 0;
            //}

            // ---------------------------------------------------------
            // -- Запись данных по инвенторизации (это не я придумал) --
            // ---------------------------------------------------------
            sql = "INSERT INTO " + CurrentBase + @"PosVod ([Lic], [KubH1n], [KubH1s], [KubH2n], [KubH2s], [KubH3n], [KubH3s], [KubG1n], [KubG1s], " +
                  "[KubG2n], [KubG2s], [KubG3n], [KubG3s], [PerOpl], [PosCur], [LastPer], [ModiRec], [ModiRecR], [MeanCube], [BrikPach], [VodKod]) " +
                  "SELECT  @Lic, ISNULL(vi.h1, 0) as h1, mp.KubH1n, ISNULL(vi.h2, 0) as h2, mp.KubH2n, ISNULL(vi.h3, 0) as h3, mp.KubH3n, " +
                  "ISNULL(vi.g1, 0) as g1, mp.KubG1n, ISNULL(vi.g2, 0) as g2, mp.KubG2n, ISNULL(vi.g3, 0) as g3, mp.KubG3n, " +
                  "@PerOpl as PerOpl, 'Pos" + Per + "' as PosCur, @Per as LastPer, @MaxCurDate, @MaxCurDate, 1, 19, a.VodKod " +
                  "FROM " + CurrentBase + "MaxPosVod mp " +
                  "INNER JOIN " + CurrentBase + "MaxVodomerInv vi ON mp.Lic = vi.lic " +
                  "INNER JOIN " + CurrentBase + "abonent" + Per + " a ON a.Lic = mp.Lic " +
                  "LEFT JOIN " + CurrentBase + "GetAbon(@Per) ab ON ab.Lic = a.lic " +
                  "WHERE " + CurrentBase + "DateToPer(DateInv, 0) = CAST(@Per as varchar(7)) AND " +
                  CurrentBase + "DateToPer(mp.ModiRec, 0) < CAST(@Per as varchar(7)) AND " +
                  "a.gruppa3 = 0 AND a.Vvodomer = 1 and a.VodKod = 0 AND (a.Prochee = 0) AND (a.NotLive = 0) AND (a.Fam <> 'У') AND " +
                  "(mp.KubH1n + mp.KubH2n + mp.KubH3n < ISNULL(vi.h1, 0) + ISNULL(vi.h2,0) + ISNULL(vi.h3,0) OR " +
                  "mp.KubG1n + mp.KubG2n + mp.KubG3n < ISNULL(vi.g1,0) + ISNULL(vi.g2,0) + ISNULL(vi.g3,0)) AND " +
                  "(mp.KubH1n - ISNULL(vi.h1, 0) <= 0 AND mp.KubH2n - ISNULL(vi.h2, 0) <= 0 AND mp.KubH3n - ISNULL(vi.h3, 0) <= 0 AND " +
                  "mp.KubG1n - ISNULL(vi.g1, 0) <= 0 AND mp.KubG2n - ISNULL(vi.g2, 0) <= 0 AND mp.KubG3n - ISNULL(vi.g3, 0) <= 0) AND " +
                  "(ab.dtNotCalc IS NULL OR " + CurrentBase + "DateToPer(ab.dtNotCalc, 0) < CAST(@Per as varchar(7))) " +
                  "AND a.Lic = @Lic";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@PerOpl", SqlDbType.NVarChar).Value = PerOpl;
            cmd.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Per);
            cmd.Parameters.Add("@MaxCurDate", SqlDbType.DateTime).Value = MaxCurDate;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            if (cmd.ExecuteNonQuery() > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

/// <summary>
///  Начисляем предыдущие средние показания по счету, если молчание меньше 6 месяцев, текущих показаний нет и есть среднее в прошлом месяце
/// </summary>
/// <param name="Lic"></param>
/// <param name="Period"></param>
/// <returns></returns>

        private bool RepeatMean(string lic, string Period)
        {
            Lic = lic;
            Per = Period;                                                                          // -- Период для начисления среднего --
            CurrentBase = (Lic.Substring(0, 1) == "1") ? "Abon.dbo." : "AbonUK.dbo.";           // -- Префикс для текущей базы --
            NotCurrentBase = (Lic.Substring(0, 1) == "2") ? "AbonUK.dbo." : "Abon.dbo.";        // -- Префикс для другой базы --
            PerOpl = Per.Substring(2, 4);                                                       // -- Строка с периодом в фотмате YYMM --
            Date3M = new DateTime(Convert.ToInt32(Per.Substring(Per.Length - 6, 4)), Convert.ToInt32(Per.Substring(Per.Length - 2, 2)), 1).AddMonths(-3);
            Date6M = new DateTime(Convert.ToInt32(Per.Substring(Per.Length - 6, 4)), Convert.ToInt32(Per.Substring(Per.Length - 2, 2)), 1).AddMonths(-6);
            MaxCurDate = Date3M.AddMonths(4).AddDays(-1);
            // -- Очищаем от старого среднего базу по лс --
            if (ClearMean() == 1)
                return true;
            CubeC CC = GetMeanCounters(GetPrevPeriod(Period), Lic);
            if (!CC.MeanCube)
                return CC.MeanCube;
            double KubH1 = CC.KubH1;
            double KubH2 = CC.KubH2;
            double KubH3 = CC.KubH3;
            double KubG1 = CC.KubG1;
            double KubG2 = CC.KubG2;
            double KubG3 = CC.KubG3;
            // -- Выбираем данные для вставки --
            string sql = @"SELECT
                           case when vd.H1 IS NULL then 0 else mpv.KubH1n + @KubH1 end as KubH1n,
		                   case when vd.H1 IS NULL then 0 else mpv.KubH1n  end as KubH1s,
		                   case when vd.H2 IS NULL then 0 else mpv.KubH2n + @KubH2 end as KubH2n,
		                   case when vd.H2 IS NULL then 0 else mpv.KubH2n  end as KubH2s,
		                   case when vd.H3 IS NULL then 0 else (case when a.Pvodomer = 1 then mpv.KubH3n else mpv.KubH3n + @KubH3 end) end as KubH3n,
		                   case when vd.H3 IS NULL then 0 else mpv.KubH3n  end as KubH3s,
		                   case when vd.G1 IS NULL then 0 else mpv.KubG1n + @KubG1 end as KubG1n,
		                   case when vd.G1 IS NULL then 0 else mpv.KubG1n  end as KubG1s,
		                   case when vd.G2 IS NULL then 0 else mpv.KubG2n + @KubG2 end as KubG2n,
		                   case when vd.G2 IS NULL then 0 else mpv.KubG2n  end as KubG2s,
		                   case when vd.G3 IS NULL then 0 else mpv.KubG3n + @KubG3 end as KubG3n,
		                   case when vd.G3 IS NULL then 0 else mpv.KubG3n  end as KubG3s
	                       FROM " + CurrentBase + @"MaxPosVod mpv
		                   INNER JOIN " + CurrentBase + @"VodomerDate" + Per + @" vd ON vd.Lic = mpv.Lic
		                   INNER JOIN " + CurrentBase + @"Abonent" + Per + @" a ON a.Lic = mpv.lic
		                   where mpv.Lic = @Lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.Parameters.Add("@KubH1", SqlDbType.Int).Value = KubH1;
            cmd.Parameters.Add("@KubH2", SqlDbType.Int).Value = KubH2;
            cmd.Parameters.Add("@KubH3", SqlDbType.Int).Value = KubH3;
            cmd.Parameters.Add("@KubG1", SqlDbType.Int).Value = KubG1;
            cmd.Parameters.Add("@KubG2", SqlDbType.Int).Value = KubG2;
            cmd.Parameters.Add("@KubG3", SqlDbType.Int).Value = KubG3;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    InsertionInfo InsInf;
                    sqlReader.Read();
                    InsInf = new InsertionInfo(Convert.ToInt32(sqlReader["KubH1n"]), Convert.ToInt32(sqlReader["KubH1s"]),
                                               Convert.ToInt32(sqlReader["KubH2n"]), Convert.ToInt32(sqlReader["KubH2s"]),
                                               Convert.ToInt32(sqlReader["KubH3n"]), Convert.ToInt32(sqlReader["KubH3s"]),
                                               Convert.ToInt32(sqlReader["KubG1n"]), Convert.ToInt32(sqlReader["KubG1s"]),
                                               Convert.ToInt32(sqlReader["KubG2n"]), Convert.ToInt32(sqlReader["KubG2s"]),
                                               Convert.ToInt32(sqlReader["KubG3n"]), Convert.ToInt32(sqlReader["KubG3s"]));
                    sqlReader.Close();
                    sql = @"INSERT INTO " + CurrentBase + @"PosVod (Lic, KubH1n, KubH1s, KubH2n, KubH2s, KubH3n, KubH3s,
                            KubG1n, KubG1s, KubG2n, KubG2s, KubG3n, KubG3s, PerOpl, PosCur, LastPer, ModiRec, MeanCube)
                            VALUES (@Lic, @KubH1n, @KubH1s, @KubH2n, @KubH2s, @KubH3n, @KubH3s, @KubG1n, @KubG1s,
                            @KubG2n, @KubG2s, @KubG3n, @KubG3s, @PerOpl, @PosCur, @LastPer, @ModiRec, 1)";
                    cmd = new SqlCommand(sql, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
                    cmd.Parameters.Add("@KubH1n", SqlDbType.Int).Value = InsInf.KubH1n;
                    cmd.Parameters.Add("@KubH2n", SqlDbType.Int).Value = InsInf.KubH2n;
                    cmd.Parameters.Add("@KubH3n", SqlDbType.Int).Value = InsInf.KubH3n;
                    cmd.Parameters.Add("@KubG1n", SqlDbType.Int).Value = InsInf.KubG1n;
                    cmd.Parameters.Add("@KubG2n", SqlDbType.Int).Value = InsInf.KubG2n;
                    cmd.Parameters.Add("@KubG3n", SqlDbType.Int).Value = InsInf.KubG3n;
                    cmd.Parameters.Add("@KubH1s", SqlDbType.Int).Value = InsInf.KubH1s;
                    cmd.Parameters.Add("@KubH2s", SqlDbType.Int).Value = InsInf.KubH2s;
                    cmd.Parameters.Add("@KubH3s", SqlDbType.Int).Value = InsInf.KubH3s;
                    cmd.Parameters.Add("@KubG1s", SqlDbType.Int).Value = InsInf.KubG1s;
                    cmd.Parameters.Add("@KubG2s", SqlDbType.Int).Value = InsInf.KubG2s;
                    cmd.Parameters.Add("@KubG3s", SqlDbType.Int).Value = InsInf.KubG3s;
                    cmd.Parameters.Add("@PerOpl", SqlDbType.NVarChar).Value = PerOpl;
                    cmd.Parameters.Add("@PosCur", SqlDbType.NVarChar).Value = "Pos" + Per;
                    cmd.Parameters.Add("@LastPer", SqlDbType.NVarChar).Value = Per;
                    cmd.Parameters.Add("@ModiRec", SqlDbType.NVarChar).Value = MaxCurDate;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        // ----------------------------------------------------------------------------------------------------
        // -- Проверяется, нужно ли начислять среднее (установлен водомер, нет оплаты в этом месяце, и т.д.) --
        // ----------------------------------------------------------------------------------------------------

        private bool IsCalculateMean()
        {
            string sql = @"SELECT a.Lic FROM " + CurrentBase + "abonent" + Per + @" AS a 
                         LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic 
                         INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                         WHERE a.Vvodomer = 1 and a.VodKod = 0 AND sv.bPaketC = 1 and
                         ((LEFT(a.lic, 1) = '1' and sv.bUK = 0) OR (LEFT(a.lic, 1) = '2' and sv.bUK = 1))
                         AND a.gruppa3 = 0 AND a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND
                         (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                         a.Lic = @Lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Per);
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            bool Result;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                Result = sqlReader.HasRows;
                sqlReader.Close();
            }
            return Result;
        }        
        
        // -----------------------------------------------------------------------
        // -- Проверяется более ли трех месяцев прошло после установки счетчика --
        // -----------------------------------------------------------------------
        private bool MoreThan3Months()
        {
            string sql = "SELECT Lic FROM " + CurrentBase + "VodomerDate" + Per +
                            " WHERE Lic = @Lic AND ISNULL(datvvod, @Date3M) < @Date3M ";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Date3M", SqlDbType.DateTime).Value = Date3M;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            bool Result;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                Result = sqlReader.HasRows;
                sqlReader.Close();
            }
            return Result;
        }


        // -------------------------------------------------------------------------------
        // -- Берем показания счетчиков если счетчик установлен больше 3 месяцев назад --
        // -------------------------------------------------------------------------------
        private CubeC GetMeanCounters(string Period, string Lic)
        {
            string sql = @"SELECT RowID, KubH1n - KubH1s as KubH1, KubH2n - KubH2s as KubH2, KubH3n - KubH3s as KubH3,
                    KubG1n - KubG1s as KubG1, KubG2n - KubG2s as KubG2, KubG3n - KubG3s as KubG3, 1 as CntM, MeanCube, PerOpl
                    FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PerOplPrev AND Lic = @Lic AND MeanCube = 1 AND BrikPach IS NULL
                    UNION
                    SELECT RowID, KubH1n - KubH1s as KubH1, KubH2n - KubH2s as KubH2, KubH3n - KubH3s as KubH3,
                    KubG1n - KubG1s as KubG1, KubG2n - KubG2s as KubG2, KubG3n - KubG3s as KubG3, 1 as CntM, MeanCube, PerOpl
                    FROM " + NotCurrentBase + @"PosVod WHERE PerOpl = @PerOplPrev AND Lic = @Lic AND MeanCube = 1 AND BrikPach IS NULL ";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.BigInt).Value = Convert.ToInt64(Lic);
            cmd.Parameters.Add("@PerOplPrev", SqlDbType.Int).Value = Convert.ToInt32(Period.Substring(2, 4));
            CubeC CC;
            using (SqlDataReader L = cmd.ExecuteReader())
            {
                if (L.HasRows)
                {
                    L.Read();
                    CC = new CubeC(0, Convert.ToInt32(L["KubH1"]), Convert.ToInt32(L["KubH2"]), Convert.ToInt32(L["KubH3"]),
                                         Convert.ToInt32(L["KubG1"]), Convert.ToInt32(L["KubG2"]), Convert.ToInt32(L["KubG3"]), 1, true);
                }
                else
                {
                    CC = new CubeC(0, 0, 0, 0, 0, 0, 0, 1, false);
                }
                L.Close();
            }
            return CC;
        }
        
        // -------------------------------------------------------------------------------
        // -- Берем показания счетчиков если счетчик установлен больше 3 месяцев назад --
        // -------------------------------------------------------------------------------
        private List<CubeC> GetCounters()
        {
            List<CubeC> cubeC = new List<CubeC>();
            string sql = @"SELECT p.RowID, p.KubH1n - p.KubH1s as KubH1, p.KubH2n - p.KubH2s as KubH2, p.KubH3n - p.KubH3s as KubH3,
                    p.KubG1n - p.KubG1s as KubG1, p.KubG2n - p.KubG2s as KubG2, p.KubG3n - p.KubG3s as KubG3, 
                    case when vd.datvvod <= @Date6M then 6 else DATEDIFF(month, vd.datvvod, @MaxCurDate) - 1 end as CntM,
                    p.MeanCube, p.PerOpl
                    FROM " + CurrentBase + @"PosVod AS p INNER JOIN " + CurrentBase + @"VodomerDate" + Per + @" vd ON vd.Lic = p.Lic
                    WHERE p.Lic = @Lic AND NOT (p.PerOpl IS NULL) AND p.PerOpl >= @PerOpl
                    UNION
                    SELECT p.RowID, p.KubH1n - p.KubH1s as KubH1, p.KubH2n - p.KubH2s as KubH2, p.KubH3n - p.KubH3s as KubH3,
                    p.KubG1n - p.KubG1s as KubG1, p.KubG2n - p.KubG2s as KubG2, p.KubG3n - p.KubG3s as KubG3, 
                    case when vd.datvvod <= @Date6M then 6 else DATEDIFF(month, vd.datvvod, @MaxCurDate) - 1 end as CntM,
                    p.MeanCube, p.PerOpl
                    FROM " + NotCurrentBase + @"PosVod AS p INNER JOIN " + CurrentBase + @"VodomerDate" + Per + @" vd ON RIGHT(vd.Lic, 9) = RIGHT(p.Lic, 9)
                    WHERE p.Lic = @OldLic AND NOT (p.PerOpl IS NULL) AND p.PerOpl >= @PerOpl
                    ORDER BY p.PerOpl ";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.Parameters.Add("@OldLic", SqlDbType.NVarChar).Value = (Lic.Substring(0, 1) == "1" ? "2" : "1") + Lic.Substring(1, 9);
            cmd.Parameters.Add("@MaxCurDate", SqlDbType.DateTime).Value = MaxCurDate;
            cmd.Parameters.Add("@Date6M", SqlDbType.DateTime).Value = Date6M;
            cmd.Parameters.Add("@PerOpl", SqlDbType.Int).Value = GetPrevPeriod(Per, 6).Substring(2, 4);

            using (SqlDataReader L = cmd.ExecuteReader())
            {
                if (L.HasRows)
                {
                    string PerOplat = null;
                    int[] CountsOfCube = {0, 0, 0, 0, 0, 0};
                    while (L.Read())
                    {
                        if (PerOplat == null || PerOplat != L["PerOpl"].ToString() ||
                            CountsOfCube[0] != Convert.ToInt32(L["KubH1"]) || CountsOfCube[1] != Convert.ToInt32(L["KubH2"]) ||
                            CountsOfCube[2] != Convert.ToInt32(L["KubH3"]) || CountsOfCube[3] != Convert.ToInt32(L["KubG1"]) ||
                            CountsOfCube[4] != Convert.ToInt32(L["KubG2"]) || CountsOfCube[5] != Convert.ToInt32(L["KubG3"]))
                        {
                            cubeC.Add(new CubeC(Convert.ToInt32(L["RowID"]), Convert.ToInt32(L["KubH1"]), Convert.ToInt32(L["KubH2"]),
                                                Convert.ToInt32(L["KubH3"]), Convert.ToInt32(L["KubG1"]), Convert.ToInt32(L["KubG2"]),
                                                Convert.ToInt32(L["KubG3"]), Convert.ToInt32(L["CntM"]),
                                                Convert.ToInt32(L["MeanCube"]) == 1));
                            CountsOfCube[0] = Convert.ToInt32(L["KubH1"]);
                            CountsOfCube[1] = Convert.ToInt32(L["KubH2"]);
                            CountsOfCube[2] = Convert.ToInt32(L["KubH3"]);
                            CountsOfCube[3] = Convert.ToInt32(L["KubG1"]);
                            CountsOfCube[4] = Convert.ToInt32(L["KubG2"]);
                            CountsOfCube[5] = Convert.ToInt32(L["KubG3"]);
                            PerOplat = L["PerOpl"].ToString();
                        }
                    }
                }
                L.Close();
            }
            return cubeC;
        }

        public SetMeanCube(SqlConnection conn)
        {
            this.conn = conn;                                               // -- Соединение с базой --
        }

        // -- Выбираем данные для вставки среднего показания если дата установки счетчика больше, чем 3 месяца --
        private InsertionInfo InsertMoreThan3Months(List<CubeC> cubeC)
        {
            // -- Находим сумму показаний по каждому счетчику -- 
            double KubH1 = 0;
            double KubH2 = 0;
            double KubH3 = 0;
            double KubG1 = 0;
            double KubG2 = 0;
            double KubG3 = 0;
            if (cubeC.Count > 0)
            {
                foreach (CubeC cC in cubeC)
                {
                    KubH1 += cC.KubH1;
                    KubH2 += cC.KubH2;
                    KubH3 += cC.KubH3;
                    KubG1 += cC.KubG1;
                    KubG2 += cC.KubG2;
                    KubG3 += cC.KubG3;
                }
                // -- Находим средние показания --
                KubH1 = (KubH1 > 0 && KubH1 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubH1 / cubeC[0].CntM + 0.001);
                KubH2 = (KubH2 > 0 && KubH2 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubH2 / cubeC[0].CntM + 0.001);
                KubH3 = (KubH3 > 0 && KubH3 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubH3 / cubeC[0].CntM + 0.001);
                KubG1 = (KubG1 > 0 && KubG1 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubG1 / cubeC[0].CntM + 0.001);
                KubG2 = (KubG2 > 0 && KubG2 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubG2 / cubeC[0].CntM + 0.001);
                KubG3 = (KubG3 > 0 && KubG3 / cubeC[0].CntM < 1) ? 1 : Math.Round(KubG3 / cubeC[0].CntM + 0.001);
            }

            // -- Выбираем данные для вставки --
            string sql = @"SELECT
                           case when vd.H1 IS NULL then 0 else mpv.KubH1n + @KubH1 end as KubH1n,
		                   case when vd.H1 IS NULL then 0 else mpv.KubH1n  end as KubH1s,
		                   case when vd.H2 IS NULL then 0 else mpv.KubH2n + @KubH2 end as KubH2n,
		                   case when vd.H2 IS NULL then 0 else mpv.KubH2n  end as KubH2s,
		                   case when vd.H3 IS NULL then 0 else (case when a.Pvodomer = 1 then mpv.KubH3n else mpv.KubH3n + @KubH3 end) end as KubH3n,
		                   case when vd.H3 IS NULL then 0 else mpv.KubH3n  end as KubH3s,
		                   case when vd.G1 IS NULL then 0 else mpv.KubG1n + @KubG1 end as KubG1n,
		                   case when vd.G1 IS NULL then 0 else mpv.KubG1n  end as KubG1s,
		                   case when vd.G2 IS NULL then 0 else mpv.KubG2n + @KubG2 end as KubG2n,
		                   case when vd.G2 IS NULL then 0 else mpv.KubG2n  end as KubG2s,
		                   case when vd.G3 IS NULL then 0 else mpv.KubG3n + @KubG3 end as KubG3n,
		                   case when vd.G3 IS NULL then 0 else mpv.KubG3n  end as KubG3s
	                       FROM " + CurrentBase + @"MaxPosVod mpv
		                   INNER JOIN " + CurrentBase + @"VodomerDate" + Per + @" vd ON vd.Lic = mpv.Lic
		                   INNER JOIN " + CurrentBase + @"Abonent" + Per + @" a ON a.Lic = mpv.lic
		                   where mpv.Lic = @Lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.Parameters.Add("@KubH1", SqlDbType.Int).Value = KubH1;
            cmd.Parameters.Add("@KubH2", SqlDbType.Int).Value = KubH2;
            cmd.Parameters.Add("@KubH3", SqlDbType.Int).Value = KubH3;
            cmd.Parameters.Add("@KubG1", SqlDbType.Int).Value = KubG1;
            cmd.Parameters.Add("@KubG2", SqlDbType.Int).Value = KubG2;
            cmd.Parameters.Add("@KubG3", SqlDbType.Int).Value = KubG3;
            InsertionInfo InsInf;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    InsInf = new InsertionInfo(Convert.ToInt32(sqlReader["KubH1n"]), Convert.ToInt32(sqlReader["KubH1s"]),
                                               Convert.ToInt32(sqlReader["KubH2n"]), Convert.ToInt32(sqlReader["KubH2s"]),
                                               Convert.ToInt32(sqlReader["KubH3n"]), Convert.ToInt32(sqlReader["KubH3s"]),
                                               Convert.ToInt32(sqlReader["KubG1n"]), Convert.ToInt32(sqlReader["KubG1s"]),
                                               Convert.ToInt32(sqlReader["KubG2n"]), Convert.ToInt32(sqlReader["KubG2s"]),
                                               Convert.ToInt32(sqlReader["KubG3n"]), Convert.ToInt32(sqlReader["KubG3s"]));
                }
                else
                {
                    InsInf = null;
                }
                sqlReader.Close();
            }
            return InsInf;
        }

        // -- Выбираем данные для вставки среднего показания если дата установки счетчика не больше, чем 3 месяца --
        private InsertionInfo InsertLessThan3Months()
        {
            string sql = @" SELECT case when vd.H1 IS NULL then 0 else mpv.KubH1n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end)) 
                            ) end as KubH1n,
                            case when vd.H1 IS NULL then 0 else mpv.KubH1n end as KubH1s,
                            case when vd.H2 IS NULL then 0 else mpv.KubH2n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end))
                            ) end as KubH2n,
                            case when vd.H2 IS NULL then 0 else mpv.KubH2n end as KubH2s,
                            case when vd.H3 IS NULL OR a.Pvodomer = 1 then 0 else mpv.KubH3n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end))
                            ) end as KubH3n,
                            case when vd.H3 IS NULL OR a.Pvodomer = 1 then 0 else mpv.KubH3n  end as KubH3s,
                            case when vd.G1 IS NULL then 0 else mpv.KubG1n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end))
                            ) end as KubG1n, 
                            case when vd.G1 IS NULL then 0 else mpv.KubG1n end as KubG1s, 
                            case when vd.G2 IS NULL then 0 else mpv.KubG2n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end))
                            ) end as KubG2n,
                            case when vd.G2 IS NULL then 0 else mpv.KubG2n end as KubG2s,
                            case when vd.G3 IS NULL then 0 else mpv.KubG3n + Floor(((a.Liver - a.Vibilo - a.Vibilo2) *
                            (case when ISNULL(vd.NormaV,0) >= ISNULL(vd.NormaK,0) then ISNULL(vd.NormaV,0) else ISNULL(vd.NormaK,0) end)) /
                            ((case when vd.H1 IS NULL then 0 else 1 end) + (case when vd.H2 IS NULL then 0 else 1 end) + (case when vd.H3 IS NULL then 0 else 1 end) + 
                            (case when vd.G1 IS NULL then 0 else 1 end) + (case when vd.G2 IS NULL then 0 else 1 end) + (case when vd.G3 IS NULL then 0 else 1 end))
                            ) end as KubG3n,
                            case when vd.G3 IS NULL then 0 else mpv.KubG3n end as KubG3s
                            FROM " + CurrentBase + @"VodomerDate" + Per + @" AS vd INNER JOIN " + CurrentBase + @"MaxPosVod AS mpv ON vd.Lic = mpv.Lic 
                            INNER JOIN " + CurrentBase + @"abonent" + Per + @" a ON a.Lic = vd.Lic
                            WHERE a.Lic = @Lic and ISNULL(vd.datvvod, '01.01.2000') < @FirstDate";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.Parameters.Add("@FirstDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(Per.Substring(0, 4)), Convert.ToInt32(Per.Substring(4, 2)), 1);
            InsertionInfo InsInf;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    InsInf = new InsertionInfo(Convert.ToInt32(sqlReader["KubH1n"]), Convert.ToInt32(sqlReader["KubH1s"]),
                                               Convert.ToInt32(sqlReader["KubH2n"]), Convert.ToInt32(sqlReader["KubH2s"]),
                                               Convert.ToInt32(sqlReader["KubH3n"]), Convert.ToInt32(sqlReader["KubH3s"]),
                                               Convert.ToInt32(sqlReader["KubG1n"]), Convert.ToInt32(sqlReader["KubG1s"]),
                                               Convert.ToInt32(sqlReader["KubG2n"]), Convert.ToInt32(sqlReader["KubG2s"]),
                                               Convert.ToInt32(sqlReader["KubG3n"]), Convert.ToInt32(sqlReader["KubG3s"]));
                }
                else
                {
                    InsInf = null;
                }
                sqlReader.Close();
            }
            return InsInf;
        }

        // -- Вставляем получившиеся данные в базу данных --
        private bool InsertIntoPosVod(InsertionInfo InsInf)
        {
            string sql = @"INSERT INTO " + CurrentBase + @"PosVod (Lic, KubH1n, KubH1s, KubH2n, KubH2s, KubH3n, KubH3s,
                            KubG1n, KubG1s, KubG2n, KubG2s, KubG3n, KubG3s, PerOpl, PosCur, LastPer, ModiRec, ModiRecR, MeanCube, VodKod)
                            VALUES (@Lic, @KubH1n, @KubH1s, @KubH2n, @KubH2s, @KubH3n, @KubH3s, @KubG1n, @KubG1s,
                            @KubG2n, @KubG2s, @KubG3n, @KubG3s, @PerOpl, @PosCur, @LastPer, @ModiRec, @ModiRec, 1, 0)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
            cmd.Parameters.Add("@KubH1n", SqlDbType.Int).Value = InsInf.KubH1n;
            cmd.Parameters.Add("@KubH2n", SqlDbType.Int).Value = InsInf.KubH2n;
            cmd.Parameters.Add("@KubH3n", SqlDbType.Int).Value = InsInf.KubH3n;
            cmd.Parameters.Add("@KubG1n", SqlDbType.Int).Value = InsInf.KubG1n;
            cmd.Parameters.Add("@KubG2n", SqlDbType.Int).Value = InsInf.KubG2n;
            cmd.Parameters.Add("@KubG3n", SqlDbType.Int).Value = InsInf.KubG3n;
            cmd.Parameters.Add("@KubH1s", SqlDbType.Int).Value = InsInf.KubH1s;
            cmd.Parameters.Add("@KubH2s", SqlDbType.Int).Value = InsInf.KubH2s;
            cmd.Parameters.Add("@KubH3s", SqlDbType.Int).Value = InsInf.KubH3s;
            cmd.Parameters.Add("@KubG1s", SqlDbType.Int).Value = InsInf.KubG1s;
            cmd.Parameters.Add("@KubG2s", SqlDbType.Int).Value = InsInf.KubG2s;
            cmd.Parameters.Add("@KubG3s", SqlDbType.Int).Value = InsInf.KubG3s;
            cmd.Parameters.Add("@PerOpl", SqlDbType.NVarChar).Value = PerOpl;
            cmd.Parameters.Add("@PosCur", SqlDbType.NVarChar).Value = "Pos" + Per;
            cmd.Parameters.Add("@LastPer", SqlDbType.NVarChar).Value = Per;
            cmd.Parameters.Add("@ModiRec", SqlDbType.NVarChar).Value = MaxCurDate;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CalculateMean(string lic, string per)
        {
            Per = per;                                                                          // -- Период для начисления среднего --
            Lic = lic;                                                                          // -- Лицевой счет для которого происходит начисление --
            CurrentBase = (Lic.Substring(0, 1) == "1") ? "Abon.dbo." : "AbonUK.dbo.";           // -- Префикс для текущей базы --
            NotCurrentBase = (Lic.Substring(0, 1) == "2") ? "AbonUK.dbo." : "Abon.dbo.";        // -- Префикс для другой базы --
            PerOpl = Per.Substring(2, 4);                                                       // -- Строка с периодом в фотмате YYMM --
            Date3M = new DateTime(Convert.ToInt32(Per.Substring(Per.Length - 6, 4)), Convert.ToInt32(Per.Substring(Per.Length - 2, 2)), 1).AddMonths(-3);
            Date6M = new DateTime(Convert.ToInt32(Per.Substring(Per.Length - 6, 4)), Convert.ToInt32(Per.Substring(Per.Length - 2, 2)), 1).AddMonths(-6);
            MaxCurDate = Date3M.AddMonths(4).AddDays(-1);

            // ----------------------------------------------------------------------------
            // -- Обновление среднего по счету. Удаление старого и запись инвентаризации --
            // ----------------------------------------------------------------------------
            if (ClearMean() == 0)
            {
                // -- Вставляем полученную информацию в базу --
                if (IsCalculateMean())
                {
                    return InsertIntoPosVod((MoreThan3Months()) ? InsertMoreThan3Months(GetCounters()) : InsertLessThan3Months());
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Установка галки водомер в 0 у тех, кто не передавал показания больше 6 месяцев
        /// </summary>
        /// <param name="Period"></param>
        /// <returns></returns>
        public int ClearVvodomerMoreThanSixMonths(string Period, int BaseName)
        {
            CurrentBase = (BaseName == 0) ? "Abon.dbo." : "AbonUK.dbo.";           // -- Префикс для текущей базы --
            string sql = @"UPDATE " + CurrentBase + @"Abonent" + Period + @"
                            SET vVodomer = 0, pVodomer = 0, N_Vl = b.NormaV, N_Kl = b.NormaK 
                            FROM " + CurrentBase + @"Abonent" + Period + @" ab INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" b ON ab.lic = b.lic 
                            WHERE ab.lic IN
                            (SELECT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                            INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                            INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" as vd ON a.Lic = vd.Lic
                            LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic
                            WHERE sv.bPaketC = 1 AND a.gruppa3 = 0 AND (a.Vvodomer = 1 OR a.pVodomer = 1) and a.VodKod = 0 AND
                            a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND vd.datvvod < @BorderDate AND
                            (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod
                            WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl is NULL and ModiRecR >= @BorderDate)))";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Period);
            cmd.Parameters.Add("@PerOplLast", SqlDbType.Int).Value = Convert.ToInt32(GetPrevPeriod(Period, 6).Substring(2, 4));
            cmd.Parameters.Add("@BorderDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(GetPrevPeriod(Period, 6).Substring(0, 4)), Convert.ToInt32(GetPrevPeriod(Period, 6).Substring(4, 2)), 1);
            int counts = -1;
            try
            {
                counts = cmd.ExecuteNonQuery();
                return counts;
            }
            catch
            {
                return counts;
            }
        }

        public delegate void ShowProgress(int TotalCount, int CurrectRecord, string lic);


        /// <summary>
        /// Расчет среднего по всем лицевым счетам в базе (BaseName) за период (Period). Для отслеживания прогресса есть делегат ShowProgress.
        /// </summary>
        /// <returns> Возвращает True если все расчитано нормально, и False если была ошибка. </returns>
        public bool CalculateAllMean(ShowProgress showProgress, int BaseName, string Period, bool ODNOnly = false)
        {
            CurrentBase = (BaseName == 0) ? "Abon.dbo." : "AbonUK.dbo.";           // -- Префикс для текущей базы --
            NotCurrentBase = (BaseName == 0) ? "AbonUK.dbo." : "Abon.dbo.";        // -- Префикс для не текущей базы --
            baseName = BaseName;
            string Period3month = GetPrevPeriod(Period, 3);
            bool Result = true;
            List<string> Lics;
            string AdditionOption;
            if (ODNOnly)
            {
                AdditionOption = @" AND a.Lic IN (SELECT DISTINCT x.Lic
                                        FROM [Common].[dbo].[spHouses] a, [Common].[dbo].[HousesData] b, [Common].[dbo].[SpStreets] c,
                                        [Common].[dbo].[HouseCounterAdres] d, [Common].[dbo].[HouseCounters] e,
                                        [Common].[dbo].[CountersIndication] f, " + CurrentBase + @"abonent" + Period + @" x, " + CurrentBase + @"SpVedomstvo z 
                                        WHERE b.PerEnd = 0 and b.House_code = a.id_house and x.KodVedom = z.ID and z.bPaketC = 1 and
                                        a.Street_Code = c.Id_Street and a.Id_House = d.House_Code and d.Id_Adres = e.Adres_Code and
                                        f.HouseCounter_Code = e.Id_Counter and x.Str_code = c.Code_Yl and x.dom = cast(a.numhouse as nvarchar(10)) + a.LitHouse
                                        and b.IsCalcODN = 1) ";
            }
            else
            {
//                Period = "201701";
//                AdditionOption = " and a.str_code = 'Гру1' and a.dom = '123'";
                AdditionOption = "";
            }
            string sql;
            SqlCommand cmd;

            // -- Те, кто не оплатил в текущем месяце но есть платежи в последние 6 месяцев (для начисления среднего) и у кого не просрочены счетчики, и нет начисления среднего в предыдущем месяце --
            sql = @"SELECT DISTINCT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" a 
                            LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                            WHERE a.Vvodomer = 1 and a.VodKod = 0 AND  sv.bPaketC = 1 AND a.gruppa3 = 0 AND a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND
                            (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                            NOT (a.Lic IN (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PrevPeriod and MeanCube = 1 and BrikPach IS NULL )) AND
                            (Not (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PerOpl and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) AND
                            ((a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) OR
                            ('" + ((BaseName == 0) ? "2" : "1") + @"' + RIGHT(a.Lic, 9) in (SELECT Lic FROM " + NotCurrentBase + @"PosVod WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19))))) AND 
                            NOT a.Lic IN (SELECT Lic FROM " + CurrentBase + @"VodomerDate" + Period + @" WHERE (datvvod >= @CurentDate AND NOT datvvod IS NULL) OR
                            ((NOT H1 IS NULL AND H1 < @Date3month) OR (NOT H2 IS NULL AND H2 < @Date3month) OR (NOT H3 IS NULL AND H3 < @Date3month) OR
                            (NOT G1 IS NULL AND G1 < @Date3month) OR (NOT G2 IS NULL AND G2 < @Date3month) OR (NOT G3 IS NULL AND G3 < @Date3month)))) " + AdditionOption + @"
                            UNION
                            SELECT DISTINCT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                            LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic
                            INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                            INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" vd ON a.Lic = vd.Lic
                            WHERE a.Vvodomer = 1 and a.VodKod = 0 AND  sv.bPaketC = 1 AND a.gruppa3 = 0 AND 
                            a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND vd.datvvod < @CurentDate AND NOT vd.datvvod IS NULL AND 
                            (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                            NOT (a.Lic IN (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PrevPeriod and MeanCube = 1 and BrikPach IS NULL )) AND
                            (((H1 >= @BorderDate AND H1 < @Date3month) OR (H2 >= @BorderDate AND H2 < @Date3month) OR (H3 >= @BorderDate AND H3 < @Date3month)) AND
                            (G1 IS NULL OR G1 < @Date3month) AND (G2 IS NULL OR G2 < @Date3month) AND (G3 IS NULL OR G3 < @Date3month)) " + AdditionOption;
            cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Period);
            cmd.Parameters.Add("@PrevPeriod", SqlDbType.Int).Value = Convert.ToInt32(GetPrevPeriod(Period).Substring(2, 4));
            cmd.Parameters.Add("@PerOpl", SqlDbType.Int).Value = Convert.ToInt32(Period.Substring(2, 4));
            cmd.Parameters.Add("@PerOplLast", SqlDbType.Int).Value = Convert.ToInt32(GetPrevPeriod(Period, 6).Substring(2, 4));
            cmd.Parameters.Add("@CurentDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(Period.Substring(0, 4)), Convert.ToInt32(Period.Substring(4, 2)), 1);
            cmd.Parameters.Add("@Date3month", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(Period3month.Substring(0, 4)), Convert.ToInt32(Period3month.Substring(4, 2)), 1);
            cmd.Parameters.Add("@BorderDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(GetPrevPeriod(Period, 3).Substring(0, 4)), Convert.ToInt32(GetPrevPeriod(Period, 3).Substring(4, 2)), 1);
            cmd.CommandTimeout = 0;
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    Lics = new List<string>();
                    while (sqlReader.Read())
                    {
                        Lics.Add(sqlReader["Lic"].ToString());
                    }
                }
                else
                {
                    sqlReader.Close();
                    return false;
                }
                sqlReader.Close();
            }
            for (int i = 0; i < Lics.Count; i++)
            {
                showProgress(Lics.Count, i + 1, Lics[i]);
//                if (Lics[i] != "1013328300")
                    CalculateMean(Lics[i], Period);
            }
            Lics.Clear();

// -- Те, кто не оплатил в текущем месяце но есть платежи в последние 6 месяцев (для начисления среднего) и у кого не просрочены счетчики, и есть начисление среднего в предыдущем месяце --
            sql =   @"SELECT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                    LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                    WHERE a.Vvodomer = 1 and a.VodKod = 0 AND sv.bPaketC = 1 AND a.gruppa3 = 0 AND a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND
                    (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                    Not (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PerOpl and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) AND
                    ((a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) OR
                    ('" + ((BaseName == 0) ? "2" : "1") + @"' + RIGHT(a.Lic, 9) in (SELECT Lic FROM " + NotCurrentBase +
                    @"PosVod WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19))))) AND 
                    NOT a.Lic IN (SELECT Lic FROM " + CurrentBase + @"VodomerDate" + Period +
                    @" WHERE ((NOT H1 IS NULL AND H1 < @Date3month) OR (NOT H2 IS NULL AND H2 < @Date3month) OR (NOT H3 IS NULL AND H3 < @Date3month) OR
                    (NOT G1 IS NULL AND G1 < @Date3month) OR (NOT G2 IS NULL AND G2 < @Date3month) OR (NOT G3 IS NULL AND G3 < @Date3month))) AND
                    a.Lic IN (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PrevPeriod and MeanCube = 1 and BrikPach is NULL) " + AdditionOption + @"
                    UNION
                    SELECT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                    LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic
                    INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                    INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" vd ON a.Lic = vd.Lic
                    WHERE sv.bPaketC = 1 AND a.gruppa3 = 0 AND a.Vvodomer = 1 and a.VodKod = 0 AND
                    (a.Prochee = 0) AND (a.NotLive = 0) AND (a.Fam <> 'У') AND
                    (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                    ((H1 >= @BorderDate AND H1 < @Date3month) OR (H2 >= @BorderDate AND H2 < @Date3month) OR (H3 >= @BorderDate AND H3 < @Date3month)) AND
                    (G1 IS NULL OR G1 < @Date3month) AND (G2 IS NULL OR G2 < @Date3month) AND (G3 IS NULL OR G3 < @Date3month) AND
                    a.Lic IN (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl = @PrevPeriod and MeanCube = 1 and BrikPach is NULL) " + AdditionOption;
            cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Period);
            cmd.Parameters.Add("@PrevPeriod", SqlDbType.Int).Value = Convert.ToInt32(GetPrevPeriod(Period).Substring(2, 4));
            cmd.Parameters.Add("@PerOpl", SqlDbType.Int).Value = Convert.ToInt32(Period.Substring(2, 4));
            cmd.Parameters.Add("@PerOplLast", SqlDbType.Int).Value = Convert.ToInt32(GetPrevPeriod(Period, 6).Substring(2, 4));
            cmd.Parameters.Add("@Date3month", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(Period3month.Substring(0, 4)), Convert.ToInt32(Period3month.Substring(4, 2)), 1);
            cmd.Parameters.Add("@BorderDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(GetPrevPeriod(Period, 3).Substring(0, 4)), Convert.ToInt32(GetPrevPeriod(Period, 3).Substring(4, 2)), 1);
            using (SqlDataReader sqlReader = cmd.ExecuteReader())
            {
                if (sqlReader.HasRows)
                {
                    Lics = new List<string>();
                    while (sqlReader.Read())
                    {
                        Lics.Add(sqlReader["Lic"].ToString());
                    }
                }
                else
                {
                    sqlReader.Close();
                    return false;
                }
                sqlReader.Close();
            }
            for (int i = 0; i < Lics.Count; i++)
            {
                showProgress(Lics.Count, i + 1, Lics[i]);
                RepeatMean(Lics[i], Period);
            }
            Lics.Clear();
            return Result;
        }
    }
}

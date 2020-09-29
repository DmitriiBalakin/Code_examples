using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace water
{
/// <summary>
/// Рассчет ОДН с учетом промышленных помещений.
/// Сделано на основе хранимой процедуры common.dbo.calchouseodn
/// </summary>
    class CalcODN
    {
        class House
        {
            public int HouseCode;
            public string Street;
            public string Dom;
            public double ColdCounter;
            public double HotCounter;
            public double CirculateCounter;
            public double HabitationArea;
            public double NotHabitationArea;
            public int BaseName;
            public House(int HouseCode, string Street, string Dom, double ColdCounter, double HotCounter,
                          double CirculateCounter, double HabitationArea, double NotHabitationArea)
            {
                this.HouseCode = HouseCode;
                this.Street = Street;
                this.Dom = Dom;
                this.ColdCounter = ColdCounter;
                this.HotCounter = HotCounter;
                this.CirculateCounter = CirculateCounter;
                this.HabitationArea = HabitationArea;
                this.NotHabitationArea = NotHabitationArea;
                this.BaseName = 0;
            }
        }

        private List<House> Houses; // -- Список домов --
        private int HouseCount;     // -- 0 - все дома, >0 - определенный дом --
        private string Period;      // -- период начисления --

        // -- Увеличение строки с датой на месяц --
        private string GetNextPeriod(string Period)
        {
            return (Period.Substring(4, 2) == "12") ? (Convert.ToInt32(Period) + 89).ToString() : (Convert.ToInt32(Period) + 1).ToString();
        }

        // -- Уменьшение строки с датой на месяц --
        private string GetPrevPeriod(string Period)
        {
            return (Period.Substring(4, 2) == "01") ? (Convert.ToInt32(Period) - 89).ToString() : (Convert.ToInt32(Period) - 1).ToString();
        }

        // -- Обнуление ОДН по лицевым счетам перед расчетом --
        private void NulificateODN() 
        {
            string QueryOptions = "";
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                QueryOptions = " WHERE Str_Code = @Street and dom = @Dom ";
            }
            string sql = "UPDATE Abon.dbo.Abonent" + Period + " SET OverCubeV = 0, OverCubeK = 0, OverNV_full = 0, OverNK_full = 0 " + QueryOptions + ";" +
                         "UPDATE AbonUK.dbo.Abonent" + Period + " SET OverCubeV = 0, OverCubeK = 0, OverNV_full = 0, OverNK_full = 0 " + QueryOptions + ";";
            SqlCommand SqlQuery = new SqlCommand(sql, frmMain.db_con);
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                SqlQuery.Parameters.Add("@Street", SqlDbType.NVarChar).Value = Houses[0].Street;
                SqlQuery.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = Houses[0].Dom;
            }
            SqlQuery.ExecuteNonQuery();
        }

        // -- Удаляем из таблицы Volume предыдущие начисления --
        private void DeleteFromVolume()
        {
            string QueryOptions = "";
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                /*QueryOptions = " AND v.House_Code = @HouseCode";*/
                QueryOptions = " AND House_Code = @HouseCode";
            }
            /*string sql = "DELETE FROM Common.dbo.Volume FROM  Common.dbo.SpStreets AS s " +
			             "INNER JOIN Common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code " +
			             "INNER JOIN Common.dbo.HousesData AS hd ON h.Id_House = hd.House_Code " +
			             "INNER JOIN Common.dbo.Volume AS v  ON v.House_Code = h.Id_House " +
                         "WHERE hd.PerBeg <= @PerCurent AND " +
                         "(case when hd.PerEnd = 0 then @PerCurent else " +
                         "hd.PerEnd end) >= @PerCurent AND " +
			             "v.PerSave = @PerCurent AND h.IsManyFlats = 0 " + QueryOptions;*/
            string sql = "DELETE FROM Common.dbo.Volume Where percalc = @PerCurent " + QueryOptions;
            SqlCommand SqlQuery = new SqlCommand(sql, frmMain.db_con);
            SqlQuery.Parameters.Add("@PerCurent", SqlDbType.NVarChar).Value = Period;
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = Houses[0].HouseCode;
            }
            SqlQuery.ExecuteNonQuery();
        }

        // -- Расчет ОДН для конкретного дома --
        private void SetCubeODN(House house)
        {
            string PerProm = GetNextPeriod(Period);
            string PerCurProm = Period.Substring(2, 2) + " " + Period.Substring(4, 2);
            string BaseName = (house.BaseName == 1) ? "Abon" : "AbonUK";
            // -- Удаляем из Oplata201507 все записи с кодом 42(начисления по ОДН)
            string sql = "DELETE FROM " + BaseName + ".dbo.oplata" + Period +
                         " WHERE BasisRecID = 42 and Lic IN (SELECT Lic FROM " + BaseName + ".dbo.Abonent" + Period +
                         " WHERE Str_code = @StrCode and Dom = @Dom)";
            SqlCommand SqlQuery = new SqlCommand(sql, frmMain.db_con);
            SqlQuery.CommandType = CommandType.Text;
            SqlQuery.Parameters.Add("@StrCode", SqlDbType.NVarChar).Value = house.Street;
            SqlQuery.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = house.Dom;
            SqlQuery.ExecuteNonQuery();

             // -- Походу удаление ОДН в промышленности. надо будет сделать вызов из нашей процедуры --
            SqlQuery = new SqlCommand("Common.dbo.DelODNNach", frmMain.db_con);
            SqlQuery.CommandType = CommandType.StoredProcedure;
            SqlQuery.Parameters.Add("@PerProm", SqlDbType.NVarChar).Value = PerProm;
            SqlQuery.Parameters.Add("@CodeHome", SqlDbType.Int).Value = house.HouseCode;
            SqlQuery.ExecuteNonQuery();

            // -- Выбираем какие-то данные из базы --
            SqlQuery = new SqlCommand("Common.dbo.GetVolume", frmMain.db_con);
            SqlQuery.CommandType = CommandType.StoredProcedure;
            SqlQuery.Parameters.Add("@Per", SqlDbType.NVarChar).Value = Period;
            SqlQuery.Parameters.Add("@DB", SqlDbType.NVarChar).Value = BaseName;
            SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = house.HouseCode;
            SqlQuery.Parameters.Add("@CubeVJ", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeVJ"].Direction = ParameterDirection.Output; 
            SqlQuery.Parameters.Add("@CubeKJ", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeKJ"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@CubeV3", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeV3"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@CubeK3", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeK3"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@CubeHousCntrV", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeHousCntrV"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@CubeHousCntrK", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@CubeHousCntrK"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@ArJil", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@ArJil"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@Ar3g", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@Ar3g"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@ArAll", SqlDbType.Decimal).Value = 0;
            SqlQuery.Parameters["@ArAll"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@bNorm", SqlDbType.Bit).Value = 0;
            SqlQuery.Parameters["@bNorm"].Direction = ParameterDirection.Output;
            SqlQuery.Parameters.Add("@AllLiver", SqlDbType.Int).Value = 0;
            SqlQuery.Parameters["@AllLiver"].Direction = ParameterDirection.Output;
            SqlQuery.CommandTimeout = 0;
            SqlQuery.ExecuteNonQuery();
            double CubeJilV = Convert.ToDouble(SqlQuery.Parameters["@CubeVJ"].Value);
            double CubeJilK = Convert.ToDouble(SqlQuery.Parameters["@CubeKJ"].Value);
            double Cube3gV = Convert.ToDouble(SqlQuery.Parameters["@CubeV3"].Value);
            double Cube3gK = Convert.ToDouble(SqlQuery.Parameters["@CubeK3"].Value);
            double CubeHousCntrV = Convert.ToDouble(SqlQuery.Parameters["@CubeHousCntrV"].Value);
            double CubeHousCntrK = Convert.ToDouble(SqlQuery.Parameters["@CubeHousCntrK"].Value);
            double ArJil = Convert.ToDouble(SqlQuery.Parameters["@ArJil"].Value);
            double Ar3g = Convert.ToDouble(SqlQuery.Parameters["@Ar3g"].Value);
            double ArAll = Convert.ToDouble(SqlQuery.Parameters["@ArAll"].Value);
            bool bNorm = Convert.ToInt32(SqlQuery.Parameters["@bNorm"].Value) == 1;
            int AllLiver = Convert.ToInt32(SqlQuery.Parameters["@AllLiver"].Value);
            // -- Прекращение начисления по дому, если нет площади жилых помещений   --
            // -- или общая площадь по жилым и нежилым помещениям меньше или равна 0 --
            if (ArJil == 0 || ArJil + Ar3g <= 0)
            {
                return;
            }
            if (!bNorm)
            {
                // -- Прекращаем начисления если счетчик на дом внесен некорректно (меньше 0) --
	        	if (CubeHousCntrV < 0 || CubeHousCntrK< 0)
                {
                    return;
                }
                double CV = CubeHousCntrV - CubeJilV - Cube3gV;
                double CK = 0;
                double Ar = ArJil + Ar3g;
                // -- Сохраняем в OplataYYYYMM --
                SetODNj(BaseName, Period, Period, house.HouseCode, CV, CK, Ar, ArAll, bNorm, AllLiver);
                SqlQuery = new SqlCommand("Common.dbo.SetODN3", frmMain.db_con);
                SqlQuery.CommandType = CommandType.StoredProcedure;
                SqlQuery.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = Period;
                SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = house.HouseCode;
                SqlQuery.Parameters.Add("@CubeV", SqlDbType.Decimal).Value = CV;
                SqlQuery.Parameters.Add("@CubeK", SqlDbType.Decimal).Value = CK;
                SqlQuery.Parameters.Add("@ArJil", SqlDbType.Decimal).Value = ArJil;
                SqlQuery.Parameters.Add("@Ar3g", SqlDbType.Decimal).Value = Ar3g;
                SqlQuery.Parameters.Add("@ArAll", SqlDbType.Decimal).Value = ArAll;
                SqlQuery.ExecuteNonQuery();
            }

            string AdditionQuery = " ISNULL((SELECT SUM(Vodn) FROM GEN.Prom.dbo.OdnNach AS o INNER JOIN GEN.Prom.dbo.Podabonent AS pa ON o.KodObj = pa.Код_подабонента INNER JOIN " +
                                   " GEN.Prom.dbo.Pomehenia AS p ON pa.Код_подабонента = p.код_подабонент WHERE (pa.код_дома = @CodeHome) AND (pa.дата_начало <= @PerCurProm or pa.дата_начало is null) " +
                                   " AND (ISNULL(pa.дата_конец, '99 01') >= @PerCurProm) AND IsNull(pa.вода, 0) = 1 AND o.Period = @PerProm), 0) ";
            sql = "INSERT INTO Common.dbo.Volume ([TypeService_Code],[House_Code],[HomeVolume],[HomeVolumeODN],[OrgVolume],[OrgVolumeODN],[PerSave],[PerCalc],HouseVolume) " +
                  "SELECT 1 , @CodeHome, @CubeJilV, (SELECT SUM(OverCubeV) FROM " + BaseName + ".dbo.abonent" + Period + " WHERE Str_code  = @Street AND dom = @Dom ), " +
                  "@Cube3gV, " + AdditionQuery + ", @PerCur, @PerCur, @CubeHousCntrV";
            SqlQuery = new SqlCommand(sql, frmMain.db_con);
            SqlQuery.CommandType = CommandType.Text;
            SqlQuery.Parameters.Add("@CodeHome", SqlDbType.Int).Value = house.HouseCode;
            SqlQuery.Parameters.Add("@CubeJilV", SqlDbType.Decimal).Value = CubeJilV;
            SqlQuery.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = house.Dom;
            SqlQuery.Parameters.Add("@Street", SqlDbType.NVarChar).Value = house.Street;
            SqlQuery.Parameters.Add("@Cube3gV", SqlDbType.Decimal).Value = Cube3gV;
            SqlQuery.Parameters.Add("@PerCurProm", SqlDbType.NVarChar).Value = PerCurProm;
            SqlQuery.Parameters.Add("@PerProm", SqlDbType.NVarChar).Value = PerProm;
            SqlQuery.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = Period;
            SqlQuery.Parameters.Add("@CubeHousCntrV", SqlDbType.Decimal).Value = CubeHousCntrV;
            SqlQuery.ExecuteNonQuery();
        }

        private void SetODNj(string DB, string PerCur, string PerCalc, int HouseCode,
                             double CubeV, double CubeK, double Ar, double ArAll, bool bNorm, int AllLiver)
        {
            string StrFcur;
            string StrW;
            string sql;
            StrFcur = "FROM  Common.dbo.SpStreets AS s INNER JOIN Common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code " +
                      " INNER JOIN " + DB + ".dbo.abonent" + PerCur + " AS a ON s.Code_Yl = a.Str_code AND " +
                      " CAST(h.NumHouse AS NVARCHAR(20)) + h.LitHouse = a.dom INNER JOIN " + 
                      DB + ".dbo.abonent" + PerCalc + " AS aa ON aa.lic = a.lic";
            StrW = " WHERE h.Id_House = @HouseCode AND a.gruppa3 = 0 ";
            SqlCommand SqlQuery;
            if (!bNorm)
            {
                if (CubeV > 0)
                {
                    sql = "UPDATE a SET OverCubeV = (CASE WHEN a.skvagina = 1 THEN 0 ELSE (@CubeV/@Ar*a.Area) END)" + StrFcur + StrW;
                    SqlQuery = new SqlCommand(sql, frmMain.db_con);
                    SqlQuery.CommandType = CommandType.Text;
                    SqlQuery.Parameters.Add("@CubeV", SqlDbType.Decimal).Value = CubeV;
                    SqlQuery.Parameters.Add("@Ar", SqlDbType.Decimal).Value = Ar;
                    SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = HouseCode;
                    SqlQuery.ExecuteNonQuery();
                }
                else
                {
                    sql = "INSERT INTO " + DB + ".dbo.oplata" + PerCur + " (per, perN, lic, kv, oplata, cv, BasisRecID) " +
                          "SELECT DATENAME(month, convert(datetime, '01.' + RIGHT(@PerCalc, 2) + '.' + LEFT(@PerCalc, 4), 104)) + LEFT(CAST(@PerCalc AS NVARCHAR(6)), 4) AS MN, " +
                          "@PerCalc, " +
                          "aa.Lic, " + 
                          "case WHEN ABS(floor(100 * (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver *(aa.Liver - aa.Vibilo - aa.Vibilo2) * st.vsum END) + 0.5) * 0.01) > aa.NvFull " +
                                " THEN -1 * aa.NvFull / st.vsum " +
                                " ELSE (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver * (aa.Liver - aa.Vibilo - aa.Vibilo2) END) END, " +
                          "case WHEN ABS(floor(100 * (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver * (aa.Liver - aa.Vibilo - aa.Vibilo2) * st.vsum END) + 0.5) * 0.01) > aa.NvFull " + 
                                "THEN -1 * aa.NvFull " + 
                                "ELSE floor(100 * (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver * (aa.Liver-aa.Vibilo-aa.Vibilo2) * st.vsum END) + 0.5) * 0.01 END, " +
                          "case WHEN ABS(floor(100 * (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver * (aa.Liver - aa.Vibilo - aa.Vibilo2) * st.vsum END) + 0.5) * 0.01) > aa.NvFull " +
                                " THEN -1 * aa.NvFull " + 
                                "ELSE floor(100 * (case WHEN aa.skvagina = 1 THEN 0 ELSE @CubeV / @AllLiver * (aa.Liver - aa.Vibilo - aa.Vibilo2) * st.vsum END) + 0.5) * 0.01 END, " +
                          "42 " +
                          StrFcur + " INNER JOIN " + DB + ".dbo.street ss on ss.cod_yl = a.Str_code INNER JOIN " + DB + ".dbo.SpTarif st on st.IdTarif = ss.TarifKod " +
                          StrW + " and st.Per = @PerCalc ";
                    SqlQuery = new SqlCommand(sql, frmMain.db_con);
                    SqlQuery.CommandType = CommandType.Text;
                    SqlQuery.Parameters.Add("@PerCalc", SqlDbType.NVarChar).Value = PerCalc;
                    SqlQuery.Parameters.Add("@CubeV", SqlDbType.Decimal).Value = CubeV;
                    SqlQuery.Parameters.Add("@AllLiver", SqlDbType.Decimal).Value = AllLiver;
                    SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = HouseCode;
                    SqlQuery.ExecuteNonQuery();
                    sql = "DELETE FROM " + DB + ".dbo.oplata" + PerCur + " WHERE BasisRecID = 42 and oplata = 0 and " +
                          "Lic in (SELECT aa.Lic " + StrFcur + StrW + ")";
                    SqlQuery = new SqlCommand(sql, frmMain.db_con);
                    SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = HouseCode;
                    SqlQuery.CommandType = CommandType.Text;
                    SqlQuery.ExecuteNonQuery();
                }
                sql = "UPDATE a SET a.OverNV_full = floor(100 * a.OverCubeV * st.psum + 0.5) * 0.01, a.OverNK_full = 0, a.OverCubeK = 0 " +
			          StrFcur + " INNER JOIN " + DB + ".dbo.street ss on ss.cod_yl = a.Str_code INNER JOIN " + DB + ".dbo.SpTarif st on st.IdTarif = ss.TarifKod " +
			          StrW + " and st.Per = @PerCalc";
                SqlQuery = new SqlCommand(sql, frmMain.db_con);
                SqlQuery.CommandType = CommandType.Text;
                SqlQuery.Parameters.Add("@PerCalc", SqlDbType.NVarChar).Value = PerCalc;
                SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = HouseCode;
                SqlQuery.ExecuteNonQuery();
            }
        }


        // -- Удаляем все начисления по счетчикам перед расчетом ОДН --
        private void DeleteFromCountersIndication()
        {
            string QueryOptions = "";
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                QueryOptions = " AND hd.House_Code = @HouseCode";
            }
            string sql = "DELETE FROM Common.dbo.CountersIndication FROM  Common.dbo.SpStreets AS s " +
                         "INNER JOIN Common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code " +
                         "INNER JOIN Common.dbo.HousesData AS hd ON h.Id_House = hd.House_Code " +
                         "INNER JOIN Common.dbo.HouseCounterAdres ha on ha.House_Code = hd.House_Code " +
                         "INNER JOIN Common.dbo.HouseCounters AS hc ON hc.Adres_Code  = ha.Id_Adres " +
                         "INNER JOIN Common.dbo.CountersIndication ci ON ci.HouseCounter_Code = hc.Id_Counter " +
                         "WHERE hd.PerBeg <= @PerCurent AND " +
                         "(case when hd.PerEnd = 0 then @PerCurent else hd.PerEnd end) >= @PerCurent AND " +
                         "ci.bMeanReading = 1 AND ci.Per = @Per AND h.IsManyFlats = 0 " + QueryOptions;
            SqlCommand SqlQuery = new SqlCommand(sql, frmMain.db_con);
            SqlQuery.Parameters.Add("@PerCurent", SqlDbType.NVarChar).Value = Period;
            SqlQuery.Parameters.Add("@Per", SqlDbType.NVarChar).Value = Convert.ToInt32(GetPrevPeriod(Period));
            if (this.HouseCount != 0 && this.Houses.Count > 0)
            {
                SqlQuery.Parameters.Add("@HouseCode", SqlDbType.Int).Value = Houses[0].HouseCode;
            }
            SqlQuery.ExecuteNonQuery();
        }

        // -- Начисление ОДН --
        public void CalculateODN(ProgressBarCounter progressBarCounter = null)
        {
            if (Houses.Count == 0)                                   // -- Если список с домами пустой, то ничего не начислять
                return;
            NulificateODN();                                       // -- Обнуляем все начисления, если начисление происходит по всей базе
            DeleteFromVolume();                                   // -- Удаляем из таблицы Volume результаты предыдущего начисления
            DeleteFromCountersIndication();                      // -- Удаляем все начисления по счетчикам перед расчетом ОДН
            if (Houses.Count > 0)
            {
                for (int i = 0; i < Houses.Count; i++)
                {
                    House house = Houses[i];
                    if (house.BaseName != 0)
                    {
                        SetCubeODN(house);
                        if (progressBarCounter != null)
                        {
                            progressBarCounter(Houses.Count, i + 1);
                        }
                    }
                }
            }
        }

        // -- Метод, возвращающий количество домов выбранных при инициализации объекта --
        public int GetHouseCount()
        {
            return this.Houses.Count;
        }

        // -- Делегат для вызова из другого класса.
        public delegate void ProgressBarCounter(int HouseCount, int HousesComplite);

        // -- Инициализирующий данные конструктор --
        public CalcODN(string CurentPeriod, int HouseCode = 0)
        {
            Period = CurentPeriod;
            this.HouseCount = HouseCode;
            // -- Выбираем все дома со счетчиками и начислением по ОДН -- 
            string Insertion1 = (HouseCode == 0) ? "" : " a.id_house = @HouseCode and ";
            string Insertion2 = (HouseCode == 0) ? "" : " b.House_code = @HouseCode and ";
            string sql = @"SELECT DISTINCT e.Circulation, e.ColdHot, c.Code_Yl, cast(a.numhouse as nvarchar(10)) + a.LitHouse as dom, 
                            b.AreaHabitation, b.AreaNotHabitation, f.cube, a.id_house 
                            FROM Common.dbo.spHouses a, Common.dbo.HousesData b, Common.dbo.SpStreets c, 
                            Common.dbo.HouseCounterAdres d, Common.dbo.HouseCounters e, Common.dbo.CountersIndication f 
                            WHERE b.PerBeg <= @PerCur and " + Insertion1 +
                            @"(case when b.PerEnd = 0 then @PerCur else b.PerEnd end) >= @PerCur and " + Insertion2 +
                            @"a.Street_Code = c.Id_Street and a.Id_House = d.House_Code and d.Id_Adres = e.Adres_Code and
                            f.HouseCounter_Code = e.Id_Counter and f.Per = @PerCur and f.Cube <> 0 and 
                            a.id_house = b.House_code and b.isCalcOdn = 1
                            ORDER BY Id_House ";
            SqlCommand cmd = new SqlCommand(sql, frmMain.db_con);
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = Period;
            if (HouseCode != 0)
            {
                cmd.Parameters.Add("@HouseCode", SqlDbType.Int).Value = HouseCode;
            }
            this.Houses = new List<House>();
            using (SqlDataReader readHouses = cmd.ExecuteReader())
            {
                if (readHouses.HasRows)
                {
                    while (readHouses.Read())
                    {
                        List<House> SingleHouse = (from t in Houses
                                                   where t.HouseCode == Convert.ToInt32(readHouses["id_house"])
                                                   select t).ToList();
                        if (SingleHouse.Count == 0)
                        {
                            House NewHouse = new House(Convert.ToInt32(readHouses["id_house"]), readHouses["Code_Yl"].ToString(), readHouses["Dom"].ToString(),
                                                       0, 0, 0, Convert.ToDouble(readHouses["AreaHabitation"]), Convert.ToDouble(readHouses["AreaNotHabitation"]));
                            NewHouse.ColdCounter = (Convert.ToByte(readHouses["Circulation"]) + Convert.ToByte(readHouses["ColdHot"]) == 0) ? Convert.ToDouble(readHouses["Cube"]) : NewHouse.ColdCounter;
                            NewHouse.HotCounter = (Convert.ToByte(readHouses["Circulation"]) == 0 && Convert.ToByte(readHouses["ColdHot"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : NewHouse.HotCounter;
                            NewHouse.CirculateCounter = (Convert.ToByte(readHouses["Circulation"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : NewHouse.CirculateCounter;
                            Houses.Add(NewHouse);
                        }
                        else
                        {
                            House OldHouse = SingleHouse[0];
                            OldHouse.ColdCounter = (Convert.ToByte(readHouses["Circulation"]) + Convert.ToByte(readHouses["ColdHot"]) == 0) ? Convert.ToDouble(readHouses["Cube"]) : OldHouse.ColdCounter;
                            OldHouse.HotCounter = (Convert.ToByte(readHouses["Circulation"]) == 0 && Convert.ToByte(readHouses["ColdHot"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : OldHouse.HotCounter;
                            OldHouse.CirculateCounter = (Convert.ToByte(readHouses["Circulation"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : OldHouse.CirculateCounter;
                        }
                    }
                    readHouses.Close();
                    for (int i = 0; i < Houses.Count; i++)
                    {
                        sql = "SELECT DISTINCT 1 as BaseCode FROM [Abon].[dbo].[Abonent" + Period + "] a, [Abon].[dbo].[SpVedomstvo] b " +
                            "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and b.bUK = 0 " +
                            "UNION  " +
                            "SELECT DISTINCT 2 as BaseCode FROM [AbonUK].[dbo].[Abonent" + Period + "] a, [AbonUK].[dbo].[SpVedomstvo] b " +
                            "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and bUK = 1";
                        cmd = new SqlCommand(sql, frmMain.db_con);
                        House house = Houses[i];
                        cmd.Parameters.Add("@Street", SqlDbType.NVarChar).Value = house.Street;
                        cmd.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = house.Dom;
                        using (SqlDataReader readHouse = cmd.ExecuteReader())
                        {
                            if (readHouse.HasRows)
                            {
                                while (readHouse.Read())
                                {
                                    house.BaseName = Convert.ToInt32(readHouse["BaseCode"]);
                                    break;
                                }
                            }
                            readHouse.Close();
                        }
                    }
                }
                else
                {
                    readHouses.Close();
                    return;
                }
            }
        }
    }
}

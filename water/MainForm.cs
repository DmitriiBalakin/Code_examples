using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;

namespace CalculateWater
{
    public class MainForm
    {
        //public SqlConnection conn;
        const double AvDayInMonth = 30.4;           // Среднее количество дней в месяце для рассчетов
        const int norma_min = 75;                   // норма после которой идет какое-то дополнительное начисление по санпин
        public const byte Abon = 0;                 // Начисления в базе Жилье
        public const byte AbonUK = 1;               // Начисление в базе Управляющие компании
        private const double EcoKoefficient = 0.5;  // Коэффициент для начисления по нормам
        LicNachislenie licNach;
        Conf conf;
        public int cntbg = 0;
        private StringBuilder SaveAbonNach;
        private StringBuilder SaveAbonent;


// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Класс для начислений по счетчику ------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        class rsNachL
        {
            public double normaV;
            public double normaK;
            public DateTime DatVvod;
            public rsNachL(double pnormaV, double pnormaK, DateTime pDatVvod)
            {
                this.normaV = pnormaV;
                this.normaK = pnormaK;
                this.DatVvod = pDatVvod;
            }
        }


// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод возвращающий начало месяца из периода типа строки "201507" ----------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private DateTime GetBeginOfMonth(string Period)
        {
            int FirstDay = 1;
            int Years = Convert.ToInt32(Period.Substring(0, 4));
            int Months = Convert.ToInt32(Period.Substring(4, 2));
            return new DateTime(Years, Months, FirstDay);
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод возвращающий конец месяца из периода типа строки "201507" -----------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private DateTime GetEndOfMonth(string Period)
        {
            int LastDay = DateTime.DaysInMonth(Convert.ToInt32(Period.Substring(0, 4)), Convert.ToInt32(Period.Substring(4, 2)));
            int Years = Convert.ToInt32(Period.Substring(0, 4));
            int Months = Convert.ToInt32(Period.Substring(4, 2));
            return new DateTime(Years, Months, LastDay);
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Тут начисляется расход по нормам если счетчик установлен в текущем периоде. Высчитывается сколько дней вода шла по нормам -------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CalcCubeF(string lic, int iDayCnt, SqlConnection conn)
        {
            string Base = (lic.Substring(0, 1) == "1") ? "Abon.dbo." : "AbonUK.dbo.";
            SqlCommand cmd = new SqlCommand("SELECT DateFit, N_Vl, N_Kl " + 
                "FROM " + Base + "FitIn WHERE lic = '" + lic + "' And Year(DateFit) = @Years AND Month(DateFit) = @Months " +
                "ORDER BY DateFit DESC", conn);
            cmd.Parameters.Add("@Years", SqlDbType.Int).Value = Convert.ToInt32(conf.PerCur.Substring(0, 4));
            cmd.Parameters.Add("@Months", SqlDbType.Int).Value = Convert.ToInt32(conf.PerCur.Substring(4, 2));
            double tempdata;
            using (SqlDataReader rsNach = cmd.ExecuteReader())
            {
                if (rsNach.HasRows)
                {
                    while (rsNach.Read())
                    {
                        if (Convert.ToDateTime(rsNach["DateFit"]).Day <= iDayCnt)
                        {
                            tempdata = (iDayCnt - Convert.ToDateTime(rsNach["DateFit"]).Day);
                            licNach.AllCubeK = licNach.AllCubeK + Convert.ToDouble(rsNach["n_kl"]) * tempdata / AvDayInMonth;
                            licNach.AllCubeV = licNach.AllCubeV + Convert.ToDouble(rsNach["n_vl"]) * tempdata / AvDayInMonth;
                            iDayCnt = Convert.ToDateTime(rsNach["DateFit"]).Day;
                        }
                    }
                    rsNach.Close();
                    return true;
                }
                else
                {
                    rsNach.Close();
                    return false;
                }
                
            }
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод обнуляющий начисления по счету, если на нем есть выгребная яма (канализация обнуляется) или скважина (обнуляется холодная вода) -------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void SkvKan(SelectFirst rs)
        {
            if (rs.LgKan > 0)
            {
                licNach.AllCubeK = 0;
                licNach.AllNormaK = 0;
                licNach.NkFull = 0;
                rs.N_Kl = 0;
            }
            if (rs.Skvagina > 0)
            {
                licNach.AllCubeV = 0;
                licNach.AllNormaV = 0;
                licNach.NvFull = 0;
                rs.N_VL = 0;
                rs.OverNV_Full = 0;
            }
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод, обнуляющий все данные в AbobnentNach -------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void NullificateAbonentCalculation(byte bUK, SqlConnection conn ,string Lic = "")
        {
            string Base = (bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.";
            string Lc = (Lic == "") ? Lic : " WHERE lic = " + Lic;
            SqlCommand cmd = new SqlCommand("UPDATE " + Base + "AbonentNach" + conf.PerCur + " SET Norma = 0, Cube = 0, Nachisl = 0 " + Lc, conn);
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("UPDATE " + Base + "Abonent" + conf.PerCur + " SET nv = 0, nvk = 0, nk = 0, nvfull = 0, nvkfull = 0, nkfull = 0, nachisl = 0 " + Lc, conn);
            cmd.ExecuteNonQuery();
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод, возвращающий все счета для начислений ------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private List<SelectFirst> GetPersonalAccounts(int bUK, SqlConnection conn, string Lic = "")
        {
            string Base = ((bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.");
            string Lc = "";
            if (Lic != "")
            {
                Lc = " a.Lic = " + Lic;
            }
            else
            {
                Lc = " gruppa3 = 0 AND KodVedom IN (SELECT ID FROM " + Base + "SpVedomstvo WHERE bPaketC = 1 and bUK = " + bUK.ToString() + ")";
            }
            string sql = @"SELECT a.Lic, Str_code, Liver, Vibilo, VibiloData, Vibilo2, VibiloData2, Vvodomer, 
                    Pvodomer, LgKan, N_VL, N_Kl, Nv, Nk, NvFull, NkFull, KodVedom, skvagina, gruppa3, 
                    TempNorm, TempNormDate, Sebestoim, AllN_vl, AllN_kl, VodKod, SkvPol, OverNV_Full, ISNULL(b.DevCombination_Code, 0) as DevCombination_Code, ISNULL(a.ToRemoval, 0) as ToRemoval,
                    CASE WHEN a.ImpossibleCounter IS NULL THEN 0 ELSE 1 END as ImpossibleCounter
                    FROM " + Base + "Abonent" + conf.PerCur + " a LEFT JOIN " + Base + @"Abon b ON a.lic = b.lic 
                    WHERE a.lic >= 1000000000 and " + Lc + "  and b.perend = 0 ORDER BY a.lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
//            cmd.Parameters.Add("@bUK", SqlDbType.Bit).Value = bUK;
            List<SelectFirst> PersonalAccounts = new List<SelectFirst>();
            using (SqlDataReader PersonalAccount = cmd.ExecuteReader())
            {
                if (PersonalAccount.HasRows)
                {
                    while (PersonalAccount.Read())
                    {
                        bool b1 = Convert.ToDouble(PersonalAccount["N_VL"]) == 1.52;
                        bool b2 = Convert.ToDouble(PersonalAccount["N_VL"]) == 1.824;
                        bool b3 = Convert.ToInt32(PersonalAccount["DevCombination_Code"]) == 44;
                        bool b4 = Convert.ToInt32(PersonalAccount["DevCombination_Code"]) == 45;
                        bool b5 = Convert.ToInt32(PersonalAccount["DevCombination_Code"]) == 47;
                        bool b6 = Convert.ToByte(PersonalAccount["Vvodomer"]) == 1;
                        bool b7 = bUK == 1;
                        bool b8 = Convert.ToInt32(PersonalAccount["ImpossibleCounter"]) == 1;
                        bool b9 = Convert.ToInt32(PersonalAccount["ToRemoval"]) != 0;

                        DateTime VibiloData = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("VibiloData")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("VibiloData")));
                        DateTime VibiloData2 = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("VibiloData2")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("VibiloData2")));
                        DateTime TempNormDate = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("TempNormDate")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("TempNormDate")));
                        PersonalAccounts.Add(new SelectFirst(PersonalAccount["Lic"].ToString(), PersonalAccount["Str_code"].ToString(), Convert.ToByte(PersonalAccount["Liver"]),
                            Convert.ToInt16(PersonalAccount["Vibilo"]), VibiloData, Convert.ToInt16(PersonalAccount["Vibilo2"]), VibiloData2, Convert.ToByte(PersonalAccount["Vvodomer"]),
                            Convert.ToByte(PersonalAccount["Pvodomer"]), Convert.ToByte(PersonalAccount["LgKan"]), Convert.ToDouble(PersonalAccount["N_VL"]), Convert.ToDouble(PersonalAccount["N_KL"]),
                            Convert.ToInt32(PersonalAccount["KodVedom"]), Convert.ToByte(PersonalAccount["Skvagina"]), Convert.ToDouble(PersonalAccount["TempNorm"]), TempNormDate, Convert.ToByte(PersonalAccount["Sebestoim"]),
                            Convert.ToDouble(PersonalAccount["AllN_vl"]), Convert.ToDouble(PersonalAccount["AllN_kl"]), Convert.ToInt32(PersonalAccount["VodKod"]), Convert.ToByte(PersonalAccount["SkvPol"]),
                            Convert.ToDouble(PersonalAccount["OverNV_Full"]), !(b1 || b2 || b3 || b4 || b5 || b6 || b7 || b8 || b9)));
                    }
                }
                PersonalAccount.Close();
            }
            return PersonalAccounts;
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Выбираем все начисления по начисляемым счетам из таблицы AbonNach ------------------------  ---------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private List<rsNachisl> GetAbonentNach(int bUK, SqlConnection conn, string Lic)
        {
            string Base = ((bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.");
            string Lc = "";
            if (Lic != "")
            {
                Lc = "Lic = '" + Lic + "'";
            }
            else
            {
                Lc = "Lic IN (SELECT Lic FROM " + Base + "Abonent" + conf.PerCur + " WHERE gruppa3 = 0 AND KodVedom IN (SELECT ID FROM " + Base + "SpVedomstvo WHERE bPaketC = 1 and bUK = " + bUK.ToString() + "))";
            }
            SqlCommand cmd = new SqlCommand("SELECT Id, Lic, GranServiceID, Amount, Norma, Cube, Nachisl From " + Base + "AbonentNach" + conf.PerCur + " WHERE " + Lc, conn);
            List<rsNachisl> rsN = new List<rsNachisl>();
            using (SqlDataReader rsNach = cmd.ExecuteReader())
            {
                if (rsNach.HasRows)
                {
                    while (rsNach.Read())
                    {
                        rsN.Add(new rsNachisl(Convert.ToInt32(rsNach["ID"]), rsNach["Lic"].ToString(), Convert.ToInt16(rsNach["GranServiceID"]),
                                              Convert.ToDouble(rsNach["Amount"]), Convert.ToDouble(rsNach["Norma"]),
                                              Convert.ToDouble(rsNach["Cube"]), Convert.ToDouble(rsNach["Nachisl"])));
                    }
                }
                rsNach.Close();
            }
            return rsN;
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Выбираем Данные из хранимки -----------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private List<CountersData> GetCountersData(int bUK, SqlConnection conn, string Lic, int VodKod)
        {
            string Base = ((bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.");
            SqlCommand cmd = new SqlCommand(Base + "GetVodRead", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = conf.PerCur;
            cmd.Parameters.Add("@GV_Lic", SqlDbType.NVarChar).Value = Lic;
            if(VodKod != 0)
                cmd.Parameters.Add("@VodKod", SqlDbType.Int).Value = VodKod;

            List<CountersData> CD = new List<CountersData>();
            cmd.CommandTimeout = 0;
            using (SqlDataReader rsNach = cmd.ExecuteReader())
            {
                if (rsNach.HasRows)
                {
                    while (rsNach.Read())
                    {
                        CD.Add(new CountersData(rsNach.IsDBNull(rsNach.GetOrdinal("PerOpl")) ? "" : rsNach["PerOpl"].ToString(),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubH12V")) ? 0 : Convert.ToInt32(rsNach["KubH12V"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubH3V")) ? 0 : Convert.ToInt32(rsNach["KubH3V"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubGV")) ? 0 : Convert.ToInt32(rsNach["KubGV"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubH12K")) ? 0 : Convert.ToInt32(rsNach["KubH12K"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubH3K")) ? 0 : Convert.ToInt32(rsNach["KubH3K"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("KubGK")) ? 0 : Convert.ToInt32(rsNach["KubGK"]),
                                                rsNach.IsDBNull(rsNach.GetOrdinal("Liver")) ? 0 : Convert.ToInt32(rsNach["Liver"])));
                    }
                }
                rsNach.Close();
            }
            return CD;
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод для начисления расхода по конкретному счету -------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Nach(byte bUK, SelectFirst PersonalDataForAccount, List<rsNachisl> rsnachisl, SqlConnection conn)
        {
            string Base = ((bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.");
            bool NotHotCounter = (PersonalDataForAccount.Lic == "2306695200" || PersonalDataForAccount.Lic == "1305366900" || PersonalDataForAccount.Lic == "1303762600" ||
                                  PersonalDataForAccount.Lic == "1306458600" || PersonalDataForAccount.Lic == "2032654700" || PersonalDataForAccount.Lic == "1304499100" ||
                                  PersonalDataForAccount.Lic == "1313492200" || PersonalDataForAccount.Lic == "1313492210" || PersonalDataForAccount.Lic == "1307451800" ||
                                  PersonalDataForAccount.Lic == "1001569210" || PersonalDataForAccount.Lic == "1032338300" || PersonalDataForAccount.Lic == "1818066943" ||
                                  PersonalDataForAccount.Lic == "1818085717" || PersonalDataForAccount.Lic == "1818087636" || PersonalDataForAccount.Lic == "1818114074");
// ------ Создаем объект для данного кокретного счета для дальнейших начислений -----------------------
            licNach = new LicNachislenie();
            SqlCommand cmd;
// ------ Выбираем все виды начисления по данному лицевому счету из таблицы AbonentNach ---------------
            List<rsNachisl> rsN = GetAbonentNach(bUK, conn, PersonalDataForAccount.Lic);
//            List<rsNachisl> rsN =  (from t in rsnachisl where t.Lic == PersonalDataForAccount.Lic select t).ToList();
// ------ Забираем тарифы из базы. В зависимости от сроков или из истории (Street) или ----------------
// ------ из текущего справочника тарифов (SpTarif) ---------------------------------------------------

            Tarifs Tar = new Tarifs(conf.PerCur, conf.LastPer, PersonalDataForAccount.Str_code, conn, (byte)bUK);

// ------ Количество проживающих ----------------------------------------------------------------------
            int CoutnLiver = Math.Max(PersonalDataForAccount.Liver - PersonalDataForAccount.Vibilo - PersonalDataForAccount.Vibilo2, 0); // Количество проживающих должно быть не меньше 0
            int CountDays = -1;
// ------ Если у лицевого счета есть водомер на воду, полив или указан код счетчика, то... ------------
            if ((PersonalDataForAccount.Vvodomer != 0) || (PersonalDataForAccount.VodKod != 0) || (PersonalDataForAccount.Pvodomer != 0))
            {
// ------ Выбираем все водомеры по текущему лицевому счету и зпихиваем в список rsNachList ------------

                cmd = new SqlCommand("SELECT normaV, normaK, DatVvod FROM " + Base + "VodomerDate" + conf.PerCur + " WHERE Lic= @lic", conn);
                cmd.Parameters.Add("@lic", SqlDbType.BigInt).Value = PersonalDataForAccount.Lic;
                List<rsNachL> rsNachList = new List<rsNachL>();
                using (SqlDataReader rsNach = cmd.ExecuteReader())
                {
                    if (rsNach.HasRows)
                        while (rsNach.Read())
                        {
                            rsNachList.Add(new rsNachL(Convert.ToDouble(rsNach["normaV"]), Convert.ToDouble(rsNach["normaK"]),
                                                      (rsNach["DatVvod"] is DBNull) ? DateTime.MinValue : Convert.ToDateTime(rsNach["DatVvod"])));
                        }                 
                    rsNach.Close();
                }
// ------ Проходим по списку счетчиков для начисления по ним денег ----------------------------------
                foreach (rsNachL rsNL in rsNachList)
                {
// ------ Если дата ввода счетчика в рассчетном периоде, то высчитываем за сколько дней ему надо начислять по счетчикам ------                    
                    if ((rsNL.DatVvod.Date.CompareTo(GetBeginOfMonth(conf.PerCur).Date) >= 0) &&
                       (rsNL.DatVvod.Date.CompareTo(GetEndOfMonth(conf.PerCur).Date) <= 0))
                            CountDays = rsNL.DatVvod.Day - 1;
                    if ((CountDays > 0) && (!CalcCubeF(PersonalDataForAccount.Lic, CountDays + 1, conn)))
                    {
                        licNach.AllCubeK = rsNL.normaK * CountDays / AvDayInMonth;
                        licNach.AllCubeV = rsNL.normaV * CountDays / AvDayInMonth;
                    }
                    licNach.NkFull = Math.Round(licNach.AllCubeK * Tar.TarK * CoutnLiver, 2, MidpointRounding.AwayFromZero);
                    licNach.NvFull = Math.Round(licNach.AllCubeV * Tar.TarV * CoutnLiver, 2, MidpointRounding.AwayFromZero);
                    licNach.AllNormaK = rsNL.normaK;
                    licNach.AllNormaV = rsNL.normaV;
                    licNach.AllCubeK = licNach.AllCubeK * CoutnLiver;
                    licNach.AllCubeV = licNach.AllCubeV * CoutnLiver;
                    SkvKan(PersonalDataForAccount);
                    licNach.NkvFull = PersonalDataForAccount.DevCombination ? Math.Round(licNach.NvFull * EcoKoefficient, 2, MidpointRounding.AwayFromZero) : 0;
                }

// -- Выбирает все показания счетчика по лицевому счетчику и начисляет в том числе и полив --
                List<CountersData> CD = GetCountersData((int)bUK, conn, PersonalDataForAccount.Lic, PersonalDataForAccount.VodKod);

                List<rsNachisl> rsNachPoliv = (from t in rsN
                                               where t.GranServiceID < 8
                                               select t).ToList();

                double PropLiver = 1;
                if (CD.Count > 0)
                {
                    for (int i = 0; i < CD.Count; i++)
                    {
//                        Tar = new Tarifs(CD[i].PerOpl, conf.LastPer, PersonalDataForAccount.Str_code, conn, (byte)bUK);
                        CountersData SingleCD = CD[i];
                        if (PersonalDataForAccount.VodKod > 0)
                            if (SingleCD.Liver > 0)
                                PropLiver = CoutnLiver / Convert.ToDouble(SingleCD.Liver);
                            else
                                PropLiver = 0;
                        Tar = new Tarifs("20" + SingleCD.PerOpl, conf.LastPer, PersonalDataForAccount.Str_code, conn, (byte)bUK);
// -- начисляем кубы по полученым показаниям счетчиков ---------------------
                        int KubH12V = SingleCD.KubH12V;
                        int KubH3V = SingleCD.KubH3V;
                        int KubGV = SingleCD.KubGV;
                        int KubH12K = SingleCD.KubH12K;
                        int KubH3K = SingleCD.KubH3K;
                        int KubGK = SingleCD.KubGK;
                        string perOpl = SingleCD.PerOpl;

// -- начисляем сумму по полученым показаниям счетчиков ---------------------

                        if (PersonalDataForAccount.Skvagina == 0)
                        {
                            licNach.AllCubeV = licNach.AllCubeV + (((NotHotCounter ? 0 : KubH12V) + KubGV) * PropLiver);
                            licNach.NvFull = licNach.NvFull + ((NotHotCounter ? 0 : KubH12V) + KubGV) * PropLiver * Tar.TarV;
                            if (PersonalDataForAccount.Pvodomer != 0 && Convert.ToInt32(perOpl.Substring(2, 2)) >= 5
                                            && Convert.ToInt32(perOpl.Substring(2, 2)) <= 9 && rsNachPoliv.Count() > 0)
                            {
                                rsNachPoliv[0].Cube = rsNachPoliv[0].Cube + KubH3V;
                                rsNachPoliv[0].Nachisl = rsNachPoliv[0].Nachisl + KubH3V * Tar.TarP;
                                licNach.NPoliv = licNach.NPoliv + KubH3V * Tar.TarP;
                            }
                            else
                            {
                                licNach.AllCubeV = licNach.AllCubeV + KubH3V * PropLiver;
                                licNach.NvFull = licNach.NvFull + KubH3V * PropLiver * Tar.TarV;
                            }
                        }
                        if (PersonalDataForAccount.LgKan == 0)
                        {
                            licNach.AllCubeK = licNach.AllCubeK + (((NotHotCounter ? 0 : KubH12K) + KubGK) * PropLiver);
                            licNach.NkFull = licNach.NkFull + ((NotHotCounter ? 0 : KubH12K) + KubGK) * PropLiver * Tar.TarK;
                            if (PersonalDataForAccount.Pvodomer == 0 ||
                                ((PersonalDataForAccount.Pvodomer != 0) &&
                                ((Convert.ToInt32(perOpl.Substring(2, 2)) < 5) || (Convert.ToInt32(perOpl.Substring(2, 2)) > 9))))
                            {
                                licNach.AllCubeK = licNach.AllCubeK + KubH3K * PropLiver;
                                licNach.NkFull = licNach.NkFull + KubH3K * PropLiver * Tar.TarK;
                            }
                        }
                    }
                }
                CD = null;
                rsNachPoliv = null;
                // -- Обработка лицевых с неработающими счетчиками горячей воды, но с работающими холодной
                if (PersonalDataForAccount.Lic == "1330237900" || PersonalDataForAccount.Lic == "1334693678" || PersonalDataForAccount.Lic == "1320253100" ||
                    PersonalDataForAccount.Lic == "1012871100" || PersonalDataForAccount.Lic == "1013100100" || PersonalDataForAccount.Lic == "1818107010" ||
                    PersonalDataForAccount.Lic == "1032333300" || PersonalDataForAccount.Lic == "1818115286" || PersonalDataForAccount.Lic == "1818109832" ||
                    PersonalDataForAccount.Lic == "1032334600" || PersonalDataForAccount.Lic == "1032336700" || PersonalDataForAccount.Lic == "1032337000" ||
                    PersonalDataForAccount.Lic == "1032337200" || PersonalDataForAccount.Lic == "1032338800" || PersonalDataForAccount.Lic == "1032339900" ||
                    PersonalDataForAccount.Lic == "1309400600" || PersonalDataForAccount.Lic == "1335538800" || PersonalDataForAccount.Lic == "1818065125" ||
                    PersonalDataForAccount.Lic == "1818070276" || PersonalDataForAccount.Lic == "1818070478" || PersonalDataForAccount.Lic == "1818071286" ||
                    PersonalDataForAccount.Lic == "1818073912" || PersonalDataForAccount.Lic == "1818078552" || PersonalDataForAccount.Lic == "1818084101" ||
                    PersonalDataForAccount.Lic == "1818084606" || PersonalDataForAccount.Lic == "1818088040" || PersonalDataForAccount.Lic == "1818089751" ||
                    PersonalDataForAccount.Lic == "1818091973" || PersonalDataForAccount.Lic == "1818092377" || PersonalDataForAccount.Lic == "1818100855" ||
                    PersonalDataForAccount.Lic == "1818105697" || PersonalDataForAccount.Lic == "1032621500")
                {
                    if (licNach.AllNormaV == 9.12)
                    {
                        licNach.AllCubeV = licNach.AllCubeV + 3.187 * CoutnLiver;
                        licNach.NvFull = licNach.NvFull + 3.187 * CoutnLiver * Tar.TarV;
                    }
                    licNach.AllCubeK = licNach.AllCubeK + 3.187 * CoutnLiver;
                    licNach.NkFull = licNach.NkFull + 3.187 * CoutnLiver * Tar.TarK;
                }
                // -- Обработка лицевых с неработающими счетчиками холодной воды, но с работающими горячей
                if (CountDays != -1)
                {
                    licNach.AllCubeV = licNach.AllCubeV + (NotHotCounter ? 5.933 * CoutnLiver * (AvDayInMonth - CountDays) / AvDayInMonth : 0);
                    licNach.AllCubeK = licNach.AllCubeK + (NotHotCounter ? 5.933 * CoutnLiver * (AvDayInMonth - CountDays) / AvDayInMonth : 0);
                    licNach.NvFull = licNach.NvFull + (NotHotCounter ? 5.933 * CoutnLiver * (AvDayInMonth - CountDays) / AvDayInMonth : 0) * Tar.TarV;
                    licNach.NkFull = licNach.NkFull + (NotHotCounter ? 5.933 * CoutnLiver * (AvDayInMonth - CountDays) / AvDayInMonth : 0) * Tar.TarK; PersonalDataForAccount.N_VL = licNach.AllCubeV;
                }
                else
                {
                    licNach.AllCubeV = licNach.AllCubeV + (NotHotCounter ? 5.933 * CoutnLiver : 0);
                    licNach.AllCubeK = licNach.AllCubeK + (NotHotCounter ? 5.933 * CoutnLiver : 0);
                    licNach.NvFull = licNach.NvFull + (NotHotCounter ? 5.933 * CoutnLiver : 0) * Tar.TarV;
                    licNach.NkFull = licNach.NkFull + (NotHotCounter ? 5.933 * CoutnLiver : 0) * Tar.TarK; PersonalDataForAccount.N_VL = licNach.AllCubeV;
                }
                
                PersonalDataForAccount.N_Kl = licNach.AllCubeK;
            }
// ------ Если у лицевого счета нет ни водомера на воду, на полив и не указан код счетчика, то... ------------
            else
            {
// ------ Проверяем, был ли установлен счетчик по данному лицевому счету в текущем месяце. Если был, то начисляем за время до установки счетчика -----
                if (!CalcCubeF(PersonalDataForAccount.Lic, GetEndOfMonth(conf.PerCur).Day + 1, conn))
                {
// ------ Если в рассчетном месяце счетчик не устанавливался, то начисляем кубы по норме ---------------------
                    licNach.AllCubeK = PersonalDataForAccount.N_Kl;
                    licNach.AllCubeV = PersonalDataForAccount.N_VL;
                }
                licNach.NkFull = Math.Round(licNach.AllCubeK * Tar.TarK * CoutnLiver, 2, MidpointRounding.AwayFromZero);
                licNach.NvFull = Math.Round(licNach.AllCubeV * Tar.TarV * CoutnLiver, 2, MidpointRounding.AwayFromZero);
                licNach.AllNormaK = PersonalDataForAccount.N_Kl;
                licNach.AllNormaV = PersonalDataForAccount.N_VL;
                licNach.AllCubeK = licNach.AllCubeK * CoutnLiver;
                licNach.AllCubeV = licNach.AllCubeV * CoutnLiver;
                SkvKan(PersonalDataForAccount);
                licNach.NkvFull = PersonalDataForAccount.DevCombination ? Math.Round(licNach.NvFull * EcoKoefficient, 2, MidpointRounding.AwayFromZero) : 0;
            }

// ------ Походу дела начисление дополнительных плюшек для комфорта, как вторая ванная, бассейн ------------
            Tar = new Tarifs(conf.PerCur, conf.LastPer, PersonalDataForAccount.Str_code, conn, bUK);
            double SanPinNV = licNach.AllNormaV;
            double SanPinNK = licNach.AllNormaK;
            double DaysCalc = (CountDays >= 0) ? CountDays : AvDayInMonth;
            DateTime perOt;
            DateTime perDo;
// ------ Перебираем все дополнительные начисления ---------------------------------------------------------
            foreach (rsNachisl rsNach in rsN)
            {
// ------ Берем из базы все данные по конкретному виду начисления ------------------------------------------
                GrantServices m_rsGS = new GrantServices(conn, rsNach.GranServiceID, bUK);
// ------ Берем период начисления от и до ------------------------------------------------------------------
                perOt = new DateTime(Convert.ToInt32(conf.PerCur.Substring(0, 4)),
                                        (m_rsGS.PeriodOt == "") ? 1 : Convert.ToInt32(m_rsGS.PeriodOt.Substring(3, 2)),
                                        (m_rsGS.PeriodOt == "") ? 1 : Convert.ToInt32(m_rsGS.PeriodOt.Substring(0, 2)));
                perDo = new DateTime(Convert.ToInt32(conf.PerCur.Substring(0, 4)),
                                        (m_rsGS.PeriodDo == "") ? 1 : Convert.ToInt32(m_rsGS.PeriodDo.Substring(3, 2)),
                                        (m_rsGS.PeriodDo == "") ? 1 : Convert.ToInt32(m_rsGS.PeriodDo.Substring(0, 2)));
// ------ Начисляем только на 3,4,5,6 типы начислений, т.е. все выбранные ----------------------------------
                switch (m_rsGS.TypeChargeID)
                {
                    case 3:
                        double AmountDays = 0;
                        bool bCalc;
                        bCalc = ((PersonalDataForAccount.N_VL >= 1.824 || PersonalDataForAccount.TempNorm >= 1.824) ||
                                (PersonalDataForAccount.Vvodomer != 0 && CountDays > 0) ||
                                ((PersonalDataForAccount.N_VL == 1.52 || PersonalDataForAccount.TempNorm == 1.52) && PersonalDataForAccount.SkvPol == 0));
                        bCalc = bCalc && (PersonalDataForAccount.VodKod == 0);
                        bCalc = bCalc && !(PersonalDataForAccount.Vvodomer != 0 && CountDays == 0);
                        bCalc = bCalc && !(PersonalDataForAccount.Vvodomer == 0 && PersonalDataForAccount.N_VL == 0 && PersonalDataForAccount.TempNorm == 0);
                        bCalc = bCalc && PersonalDataForAccount.Pvodomer == 0;
                        if (bCalc)
                        {
                            // сравниваем текущий период с периодом начисления услуги
                            if ((Convert.ToInt32(perOt.ToString("yyyyMM", CultureInfo.InvariantCulture)) <= Convert.ToInt32(conf.PerCur)) &&
                               (Convert.ToInt32(perDo.ToString("yyyyMM", CultureInfo.InvariantCulture)) >= Convert.ToInt32(conf.PerCur)))
                            {
                                if (!(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0))
                                {
                                    if (GetBeginOfMonth(conf.PerCur).CompareTo(perOt) < 0)
                                    {
                                        // высчитываем дни между началом поливочного сезона и концом текущего месяца
                                        AmountDays = GetEndOfMonth(conf.PerCur).Subtract(perOt).Days + 1;
                                    }
                                    else if (GetEndOfMonth(conf.PerCur).CompareTo(perDo) > 0)
                                    {
                                        // высчитываем дни между концом поливочного сезона и началом текущего месяца
                                            AmountDays = perDo.Subtract(GetBeginOfMonth(conf.PerCur)).Days;
                                    }
                                    else
                                    {
                                        AmountDays = GetEndOfMonth(conf.PerCur).Day;
                                    }
                                }
                                else
                                {
                                    AmountDays = CountDays;
                                    // Сравниваем месяц начала полива с текущим месяцем
                                    if (Convert.ToInt32(perOt.ToString("yyyyMM", CultureInfo.InvariantCulture)) == Convert.ToInt32(conf.PerCur))
                                    {
                                        AmountDays = CountDays - perOt.Day + 1;
                                    }
                                    // Сравниваем месяц конца полива с текущим месяцем
                                    if (Convert.ToInt32(perDo.ToString("yyyyMM", CultureInfo.InvariantCulture)) == Convert.ToInt32(conf.PerCur))
                                    {
                                        if (CountDays > perDo.Day)
                                        {
                                            AmountDays = perDo.Day;
                                        }
                                    }
                                }
                                if (AmountDays < 0)
                                {
                                    AmountDays = 0;
                                }
                                rsNach.Cube = m_rsGS.ConsumptionRate * (AmountDays / AvDayInMonth) * ((PersonalDataForAccount.N_VL == 1.52 ||
                                            (PersonalDataForAccount.TempNorm == 1.52 && PersonalDataForAccount.N_VL == 0))
                                            && PersonalDataForAccount.SkvPol == 0 ? ((rsNach.Amount > 1 ? 1 : rsNach.Amount)) : rsNach.Amount);
                                rsNach.Norma = m_rsGS.ConsumptionRate;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarP, 2, MidpointRounding.AwayFromZero);
                                if (rsNach.GranServiceID >= 46 && rsNach.GranServiceID <= 55)
                                {
                                    licNach.NvFull = licNach.NvFull + rsNach.Nachisl;
                                }
                                else
                                {
                                    licNach.NPoliv = licNach.NPoliv + rsNach.Nachisl;
                                }
                            }
                        }
                        break;
                    case 4:
                        // нормы по санпину
                        if (m_rsGS.TypeServicesID == 1)
                        {
                            if ((PersonalDataForAccount.Skvagina == 0 && !(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0)) ||
                                (!(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && PersonalDataForAccount.N_VL > norma_min) ||
                                ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = SanPinNV * DaysCalc / AvDayInMonth * m_rsGS.Coefficient * rsNach.Amount;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarV, 2, MidpointRounding.AwayFromZero) * CoutnLiver;
                                rsNach.Cube = rsNach.Cube * CoutnLiver;
                                licNach.AllCubeV = licNach.AllCubeV + rsNach.Cube;
                                licNach.NvFull = licNach.NvFull + rsNach.Nachisl;
                            }
                            rsNach.Norma = m_rsGS.Coefficient * rsNach.Amount * SanPinNV;
                            licNach.AllNormaV = licNach.AllNormaV + rsNach.Norma;
                        }
                        if (m_rsGS.TypeServicesID == 2)
                        {
                            if ((PersonalDataForAccount.LgKan == 0 && !(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0)) ||
                                (!(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && PersonalDataForAccount.N_Kl > norma_min) ||
                                ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = SanPinNK * DaysCalc / AvDayInMonth * m_rsGS.Coefficient * rsNach.Amount;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarK, 2, MidpointRounding.AwayFromZero) * CoutnLiver;
                                rsNach.Cube = rsNach.Cube * CoutnLiver;
                                licNach.AllCubeK = licNach.AllCubeK + rsNach.Cube;
                                licNach.NkFull = licNach.NkFull + rsNach.Nachisl;
                            }
                            rsNach.Norma = m_rsGS.Coefficient * rsNach.Amount * SanPinNK;
                            licNach.AllNormaK = licNach.AllNormaK + rsNach.Norma;
                        }
                        break;
                    case 5:
                        if (m_rsGS.TypeServicesID == 1)
                        {
                            if ((PersonalDataForAccount.Skvagina == 0 && !(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0)) || ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = m_rsGS.ConsumptionRate * DaysCalc / AvDayInMonth * rsNach.Amount;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarV, 2);
                                licNach.AllCubeV = licNach.AllCubeV + rsNach.Cube;
                                licNach.NvFull = licNach.NvFull + rsNach.Nachisl;
                            }
                            if (PersonalDataForAccount.Skvagina == 0)
                            {
                                rsNach.Norma = m_rsGS.ConsumptionRate * rsNach.Amount;
                                licNach.AllNormaV = licNach.AllNormaV + rsNach.Norma;
                            }
                        }
                        if (m_rsGS.TypeServicesID == 2)
                        {
                            if ((PersonalDataForAccount.LgKan == 0 && !(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0)) || ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = m_rsGS.ConsumptionRate * DaysCalc / AvDayInMonth * rsNach.Amount;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarK, 2, MidpointRounding.AwayFromZero);
                                licNach.AllCubeK = licNach.AllCubeK + rsNach.Cube;
                                licNach.NkFull = licNach.NkFull + rsNach.Nachisl;
                            }
                            if (PersonalDataForAccount.LgKan == 0)
                            {
                                rsNach.Norma = m_rsGS.ConsumptionRate * rsNach.Amount;
                                licNach.AllNormaK = licNach.AllNormaK + rsNach.Norma;
                            }
                        }
                        break;
                    case 6:
                        if (m_rsGS.TypeServicesID == 1)
                        {
                            if (PersonalDataForAccount.Skvagina == 0 && !(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) || ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = (rsNach.Amount > 2.28 ? 2.28 : rsNach.Amount) * DaysCalc / AvDayInMonth;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarV, 2, MidpointRounding.AwayFromZero);
                                licNach.AllCubeV = licNach.AllCubeV + rsNach.Cube;
                                licNach.NvFull = licNach.NvFull + rsNach.Nachisl;
                            }
                            rsNach.Norma = (rsNach.Amount > 2.28) ? 2.28 : rsNach.Amount;
                            licNach.AllNormaV = licNach.AllNormaV + rsNach.Norma;
                        }
                        break;
                }
// -- Сохраняем обработанное начисление ------------------------------------------------------------
                rsNach.SaveNach(conf.PerCur, conn);
            }
//            licNach.NvFull = licNach.NvFull + PersonalDataForAccount.OverNV_Full;
            PersonalDataForAccount.SetNewDate(licNach.NvFull, licNach.NkvFull, licNach.NkFull, licNach.NPoliv,
                                              licNach.AllCubeV, licNach.AllCubeK, licNach.AllNormaV, licNach.AllNormaK);
            PersonalDataForAccount.Save(conf.PerCur, conn);
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Делегат для обновления прогрессбара в другом классе -------------------------------------- ---------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void ProgressBarCounter(int HouseCount, int HousesComplite, string lic);

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Делегат для обновления прогрессбара в другом классе -------------------------------------- ---------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void InformationMessage(string TextMessage);

        private class bgClass : BackgroundWorker
        {
            public byte bUK;
            public SelectFirst SingleLic;
            public SqlConnection conn;
            public bgClass() : base()
            {
            }

            public bgClass(byte bUK, SelectFirst SingleLic, SqlConnection conn) : base()
            {

                this.bUK = bUK;
                this.SingleLic = SingleLic;
                this.conn = conn;
            }
        }

        int k;
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Выбираем счета, которые нужно рассчитать и потом для каждого вызываем функцию расчета -------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Query(byte bUK, string CalculatedPeriod, string Lic = "", ProgressBarCounter progressBarCounter = null, InformationMessage informationMessage = null, SqlConnection con = null)
        {
            bool InnerConnection = con == null;
// -- Открываем соединение с базой -------------------------------------------------------------------
            if (con == null || con.State.ToString() == "Closed")
            {
                con = new SqlConnection("Data Source=SERVERAB;Initial Catalog=Abon;Integrated Security=True;Persist Security Info=False;User ID=SqlAbon;Connect Timeout=0;TrustServerCertificate=False");
                con.Open();
            }
// -- Забираем настройки текущего пользователя --------------------------------------------------------
            conf = new Conf(water.frmMain.MaxCurPer, "", water.frmMain.MaxCurPer);


// ---------------------------------------------------------------------
// ----------- Убрать --------------------------------------------------
// ---------------------------------------------------------------------
            //CalculatedPeriod = "201602";
            conf.PerCur = CalculatedPeriod;
            conf.LastPer = CalculatedPeriod;
// ---------------------------------------------------------------------
// ---------------------------------------------------------------------
// ---------------------------------------------------------------------


            if (Lic == "" && informationMessage != null) informationMessage(DateTime.Now.ToString() + " Время начала обработки");

// ------ Обнуляем начисления по данному лицевому счету в таблице AbonentNach -------------------------
            NullificateAbonentCalculation(bUK, con, Lic);
// ------ Выбираем список лицевых счетов для начисления -----------------------------------------------
            List<SelectFirst> PersonalAccounts = GetPersonalAccounts(bUK, con, Lic);
// ------ Выбираем все виды начислений для ранее выбранных счетов -------------------------------------            
            if (Lic == "" && informationMessage != null) informationMessage(DateTime.Now.ToString() + " Выбрано записей для пересчета " + PersonalAccounts.Count.ToString());
//            List<rsNachisl> rsN = null;
//            rsN = GetAbonentNach(bUK, con, Lic);

            if (PersonalAccounts.Count > 0)
            {

                bool PB = progressBarCounter != null;
//                SaveAbonNach = new StringBuilder("");
//                SaveAbonent = new StringBuilder("");
                int j = PersonalAccounts.Count;
                k = 0;
//                int z = 0;
//                int maxcount = 0;
                List<SelectFirst> PA;
                DateTime BeginDate = DateTime.Now;
                for (int i = 0; i < 10; i++)
                {
                    PA = (from t in PersonalAccounts where t.Lic.ToString().Substring(7, 1) == i.ToString() select t).ToList();
                    if (Lic == "" && informationMessage != null && PA.Count > 0) informationMessage(BeginDate.ToString() + " Обрабатываются л/с на " + i.ToString());
                    if (PA.Count > 0)
                    {
//                        Thread thread;
                        for (int l = 0; l < PA.Count; l++)
                        {
                            if (k % 10 == 0)
                            {
                                if (Lic == "" && informationMessage != null && PA.Count > 0) progressBarCounter(j, k, "");
                            }
//                            thread = new Thread(CalcInThread);
//                            thread.Start(bUK);
                            Application.DoEvents();
                            Nach(bUK, PA[l], null, con);
                            k++;
                        }
                    }
                }
            }
            if (InnerConnection)
                con.Close();
//            if (Lic == "" && informationMessage != null) informationMessage(DateTime.Now.ToString() + " Начисления произведены. Вычисляем сальдо");
//            if (Lic == "" && informationMessage != null) informationMessage(DateTime.Now.ToString() + " Вычисление сальдо закончено");
            if (Lic == "" && informationMessage != null) informationMessage(DateTime.Now.ToString() + " Все начисления выполнены!");
        }
    
        public void CalcInThread(byte pbUK, string pCalculatedPeriod, List<SelectFirst> pPA, SqlConnection pcon)
        {
            SqlConnection con = new SqlConnection(pcon.ConnectionString);
            con.Open();
            byte bUK = pbUK;
            string CalculatedPeriod = pCalculatedPeriod;
            List<SelectFirst> PA = pPA;
            for (int i = 0; i < PA.Count; i++)
            {
                Nach(bUK, PA[i], null, con);
            }
            k--;
        }
    }
}

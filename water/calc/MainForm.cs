using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace CalculateWater
{
    public partial class MainForm : Form
    {
        public SqlConnection conn;
//        string connStr;
        const double AvDayInMonth = 30.4;    // Среднее количество дней в месяце для рассчетов
        const int norma_min = 75;            // норма после которой идет какое-то дополнительное начисление по санпин
        const byte Abon = 0;                 // Начисления в базе Жилье
        const byte AbonUK = 1;               // Начисление в базе Управляющие компании

        LicNachislenie licNach;
        Conf conf;

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

        public MainForm()
        {
            InitializeComponent();
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

        private bool CalcCubeF(string lic, int iDayCnt)
        {
            SqlCommand cmd = new SqlCommand("SELECT DateFit, N_Vl, N_Kl " + 
                "FROM FitIn WHERE lic = '" + lic + "' And Year(DateFit) = @Years AND Month(DateFit) = @Months " +
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
                rs.OverCubeK = 0;
                rs.OverNK_full = 0;
            }
            if (rs.Skvagina > 0)
            {
                licNach.AllCubeV = 0;
                licNach.AllNormaV = 0;
                licNach.NvFull = 0;
                rs.N_VL = 0;
                rs.OverCubeV = 0;
                rs.OverNV_full = 0;
            }
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод, обнуляющий все данные в AbobnentNach -------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void NullificateAbonentCalculation()
        {
            SqlCommand cmd = new SqlCommand("UPDATE AbonentNach" + conf.PerCur + " SET Norma = 0, Cube = 0, Nachisl = 0", conn);
            cmd.ExecuteNonQuery();
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Метод, возвращающий все счета для начислений ------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private List<SelectFirst> GetPersonalAccounts(int bUK, string Lic = "")
        {
            string Lc = "";
            if (Lic != "")
            {
                Lc = " Lic = " + Lic;
            }
            else
            {
                Lc = " gruppa3 = 0 AND KodVedom IN (SELECT ID FROM SpVedomstvo WHERE bPaketC = 1 and bUK = " + bUK.ToString() + ")";
            }
            string sql = "SELECT Lic, Str_code, Liver, Vibilo, VibiloData, Vibilo2, VibiloData2, Vvodomer, " +
                    "Pvodomer, LgKan, N_VL, N_Kl, Nv, Nk, NvFull, NkFull, KodVedom, skvagina, gruppa3, " +
                    "TempNorm, TempNormDate, Sebestoim, AllN_vl, AllN_kl, VodKod, sodset, poliv, " +
                    "SkvPol FROM Abonent" + conf.PerCur + " WHERE " + Lc + " ORDER BY lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@bUK", SqlDbType.Bit).Value = bUK;
            List<SelectFirst> PersonalAccounts = new List<SelectFirst>();
            using (SqlDataReader PersonalAccount = cmd.ExecuteReader())
            {
                if (PersonalAccount.HasRows)
                {
                    while (PersonalAccount.Read())
                    {
                        DateTime VibiloData = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("VibiloData")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("VibiloData")));
                        DateTime VibiloData2 = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("VibiloData2")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("VibiloData2")));
                        DateTime TempNormDate = (PersonalAccount.IsDBNull(PersonalAccount.GetOrdinal("TempNormDate")) ? DateTime.MinValue : PersonalAccount.GetDateTime(PersonalAccount.GetOrdinal("TempNormDate")));
                        PersonalAccounts.Add(new SelectFirst(PersonalAccount["Lic"].ToString(), PersonalAccount["Str_code"].ToString(), Convert.ToByte(PersonalAccount["Liver"]),
                            Convert.ToInt16(PersonalAccount["Vibilo"]), VibiloData, Convert.ToInt16(PersonalAccount["Vibilo2"]), VibiloData2, Convert.ToByte(PersonalAccount["Vvodomer"]),
                            Convert.ToByte(PersonalAccount["Pvodomer"]), Convert.ToByte(PersonalAccount["LgKan"]), Convert.ToDouble(PersonalAccount["N_VL"]), Convert.ToDouble(PersonalAccount["N_KL"]),
                            Convert.ToDouble(PersonalAccount["Nv"]), Convert.ToDouble(PersonalAccount["Nk"]), Convert.ToDouble(PersonalAccount["NvFull"]), Convert.ToDouble(PersonalAccount["NkFull"]), 
                            Convert.ToInt32(PersonalAccount["KodVedom"]), Convert.ToByte(PersonalAccount["Skvagina"]), Convert.ToDouble(PersonalAccount["TempNorm"]), TempNormDate, Convert.ToByte(PersonalAccount["Sebestoim"]),
                            Convert.ToDouble(PersonalAccount["AllN_vl"]), Convert.ToDouble(PersonalAccount["AllN_kl"]), Convert.ToInt32(PersonalAccount["VodKod"]), Convert.ToByte(PersonalAccount["SkvPol"])));
                    }
                }
                PersonalAccount.Close();
            }
            return PersonalAccounts;
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Выбираем все начисления по начисляемым счетам из таблицы AbonNach ---------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private List<rsNachisl> GetAbonentNach(int bUK, string Lic = "")
        {
            string Lc = "";
            if (Lic != "")
            {
                Lc = "Lic = " + Lic;
            }
            else
            {
                Lc = "Lic IN (SELECT Lic FROM Abonent" + conf.PerCur + " WHERE gruppa3 = 0 AND KodVedom IN (SELECT ID FROM SpVedomstvo WHERE bPaketC = 1 and bUK = " + bUK.ToString() + "))";
            }
            SqlCommand cmd = new SqlCommand("SELECT Id, Lic, GranServiceID, Amount, Norma, Cube, Nachisl From AbonentNach" + conf.PerCur + " WHERE " + Lc + " ORDER BY lic", conn);
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

        private List<CountersData> GetCountersData(int bUK, string Lic = "0")
        {
            SqlCommand cmd = new SqlCommand("[dbo].[GetVodReadLong]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = conf.PerCur;
            List<CountersData> CD = new List<CountersData>();
            cmd.CommandTimeout = 0;
            using (SqlDataReader rsNach = cmd.ExecuteReader())
            {
                if (rsNach.HasRows)
                {
                    while (rsNach.Read())
                    {
                        CD.Add(new CountersData(rsNach["lic"].ToString(),
                            rsNach.IsDBNull(rsNach.GetOrdinal("PerOpl")) ? "" : rsNach["PerOpl"].ToString(),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Gl")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Gl"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("H1Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["H1Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("H2Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["H2Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("H3Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["H3Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubH1V")) ? 0 : Convert.ToInt32(rsNach["KubH1V"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubH2V")) ? 0 : Convert.ToInt32(rsNach["KubH2V"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubH3V")) ? 0 : Convert.ToInt32(rsNach["KubH3V"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Id_House")) ? 0 : Convert.ToInt32(rsNach["Id_House"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("id_HWorg")) ? 0 : Convert.ToInt32(rsNach["id_HWorg"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("HotWaterAnother")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["HotWaterAnother"]), 
                            rsNach.IsDBNull(rsNach.GetOrdinal("Circulation")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Circulation"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("G1Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["G1Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("G2Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["G2Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("G3Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["G3Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubGV1")) ? 0 : Convert.ToInt32(rsNach["KubGV1"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubGV2")) ? 0 : Convert.ToInt32(rsNach["KubGV2"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("KubGV3")) ? 0 : Convert.ToInt32(rsNach["KubGV3"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Hk1Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Hk1Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Hk2Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Hk2Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Hk3Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Hk3Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Gk1Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Gk1Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Gk2Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Gk2Another"]),
                            rsNach.IsDBNull(rsNach.GetOrdinal("Gk3Another")) ? Convert.ToByte(0) : Convert.ToByte(rsNach["Gk3Another"]),
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
        private void Nach(int bUK, SelectFirst PersonalDataForAccount, List<rsNachisl> rsNch, List<CountersData> CDFull)
        {
// ------ Создаем объект для данного кокретного счета для дальнейших начислений -----------------------
            licNach = new LicNachislenie();
            SqlCommand cmd;
// ------ Выбираем все виды начисления по данному лицевому счету из таблицы AbonentNach ---------------
            List<rsNachisl> rsN = (from t in rsNch
                                   where t.Lic == PersonalDataForAccount.Lic
                                   select t).ToList();
// ------ Забираем тарифы из базы. В зависимости от сроков или из истории (Street) или ----------------
// ------ из текущего справочника тарифов (SpTarif) ---------------------------------------------------
            Tarifs Tar = new Tarifs(conf.PerCur, conf.LastPer, PersonalDataForAccount.Str_code, conn);
// ------ Количество проживающих ----------------------------------------------------------------------
            int CoutnLiver = Math.Max(PersonalDataForAccount.Liver - PersonalDataForAccount.Vibilo - PersonalDataForAccount.Vibilo2, 0); // Количество проживающих должно быть не меньше 0
            int CountDays = -1;
// ------ Если у лицевого счета есть водомер на воду, полив или указан код счетчика, то... ------------
            if ((PersonalDataForAccount.Vvodomer != 0) || (PersonalDataForAccount.VodKod != 0) || (PersonalDataForAccount.Pvodomer != 0))
            {
// ------ Выбираем все водомеры по текущему лицевому счету и зпихиваем в список rsNachList ------------
                cmd = new SqlCommand("SELECT normaV, normaK, DatVvod FROM VodomerDate" + conf.PerCur + " WHERE Lic= @lic", conn);
                cmd.Parameters.Add("@lic", SqlDbType.BigInt).Value = PersonalDataForAccount.Lic;
                List<rsNachL> rsNachList = new List<rsNachL>();
                using (SqlDataReader rsNach = cmd.ExecuteReader())
                {
                    if (rsNach.HasRows)
                    {
                        while (rsNach.Read())
                        {
                            rsNachList.Add(new rsNachL(Convert.ToDouble(rsNach["normaV"]), Convert.ToDouble(rsNach["normaK"]),
                                                      (rsNach["DatVvod"] is DBNull) ? DateTime.MinValue : Convert.ToDateTime(rsNach["DatVvod"])));
                        }                 
                    }
                    rsNach.Close();
                }
// ------ Проходим по списку счетчиков для начисления по ним денег ----------------------------------
                foreach (rsNachL rsNL in rsNachList)
                {
// ------ Если дата ввода счетчика в рассчетном периоде, то высчитываем за сколько дней ему надо начислять по счетчикам ------                    
                    if ((rsNL.DatVvod.Date.CompareTo(GetBeginOfMonth(conf.PerCur).Date) >= 0) &&
                       (rsNL.DatVvod.Date.CompareTo(GetEndOfMonth(conf.PerCur).Date) <= 0))
                    {
                        CountDays = rsNL.DatVvod.Day - 1;
                    }
                    if ((CountDays > 0) && (!CalcCubeF(PersonalDataForAccount.Lic, CountDays + 1)))
                    {
                        licNach.AllCubeK = rsNL.normaK * CountDays / AvDayInMonth;
                        licNach.AllCubeV = rsNL.normaV * CountDays / AvDayInMonth;
                    }
                    licNach.NkFull = Math.Round(licNach.AllCubeK * Tar.TarK, 2) * CoutnLiver;
                    licNach.NvFull = Math.Round(licNach.AllCubeV * Tar.TarV, 2) * CoutnLiver;
                    licNach.AllNormaK = rsNL.normaK;
                    licNach.AllNormaV = rsNL.normaV;
                    licNach.AllCubeK = licNach.AllCubeK * CoutnLiver;
                    licNach.AllCubeV = licNach.AllCubeV * CoutnLiver;
                    SkvKan(PersonalDataForAccount);
                }
// -- Выбирает все показания счетчика по лицевому счетчику и начисляет в том числе и полив --
                List<CountersData> CD = (from t in CDFull
                                         where t.lic == PersonalDataForAccount.Lic
                                         select t).ToList();
                List<rsNachisl> rsNachPoliv = (from t in rsN
                                               where t.GranServiceID < 8
                                               select t).ToList();
                double PropLiver = 1;
                if (CD.Count > 0)
                {
                    for (int i = 0; i < CD.Count; i++)
                    {
                        CountersData SingleCD = CD[i];
                        if (PersonalDataForAccount.VodKod > 0)
                        {
                            if (SingleCD.Liver > 0)
                            {
                                PropLiver = CoutnLiver / Convert.ToDouble(SingleCD.Liver);
                            }
                            else
                            {
                                PropLiver = 0;
                            }
                        }
                        Tar = new Tarifs("20" + SingleCD.PerOpl, conf.LastPer, PersonalDataForAccount.Str_code, conn);
                        int KubH12V = (SingleCD.KubH1V * (1 - SingleCD.H1Another)) +
                            ((SingleCD.Gl == 0) ? SingleCD.KubH2V * (1 - SingleCD.H2Another) : 0);
                        int KubH12K = (SingleCD.KubH1V * (1 - SingleCD.Hk1Another)) +
                            ((SingleCD.Gl == 0) ? SingleCD.KubH2V * (1 - SingleCD.Hk2Another) : 0);

                        int KubH3V = ((SingleCD.Gl == 0) ? (SingleCD.KubH3V * (1 - SingleCD.H3Another)) : 0);
                        int KubH3K = ((SingleCD.Gl == 0) ? (SingleCD.KubH3V * (1 - SingleCD.Hk3Another)) : 0);
                        int TempGV = SingleCD.KubGV1 * (1 - SingleCD.G1Another) + SingleCD.KubGV3 * (1 - SingleCD.G3Another);
                        int AltTempGV = SingleCD.KubGV2 * (1 - SingleCD.G2Another);
                        int KubGV = (SingleCD.Id_House != 0 && SingleCD.id_HWorg > 0) || SingleCD.HotWaterAnother == 1 ?
                                    0 : TempGV + AltTempGV * (SingleCD.Circulation == 0 || SingleCD.Gk1Another == 0 ? 1 : -1);
                        int TempGK = SingleCD.KubGV1 * (1 - SingleCD.Gk1Another) +
                                     SingleCD.KubGV3 * (1 - SingleCD.Gk3Another);
                        int AltTempGK = SingleCD.KubGV2 * (1 - SingleCD.Gk2Another);
                        int KubGK = TempGV + AltTempGV * (SingleCD.Circulation == 0 || SingleCD.Gk1Another == 0 ? 1 : -1);
                        string perOpl = SingleCD.PerOpl;

                        if (PersonalDataForAccount.Skvagina == 0)
                        {
                            licNach.AllCubeV = licNach.AllCubeV + ((KubH12V + KubGV) * PropLiver);
                            licNach.NvFull = licNach.NvFull + (KubH12V + KubGV) * PropLiver * Tar.TarV;
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
                            licNach.AllCubeK = licNach.AllCubeK + ((KubH12K + KubGK) * PropLiver);
                            licNach.NkFull = licNach.NkFull + (KubH12K + KubGK) * PropLiver * Tar.TarK;
                            if (PersonalDataForAccount.Pvodomer == 0 ||
                                ((PersonalDataForAccount.Pvodomer != 0) &&
                                ((Convert.ToInt32(perOpl.Substring(2, 2)) < 5) || (Convert.ToInt32(perOpl.Substring(2, 2)) > 9))))
                            {
                                licNach.AllCubeK = licNach.AllCubeK + KubH3K * PropLiver;
                                licNach.NkFull = licNach.NkFull + KubH3K * PropLiver * Tar.TarK;
                            }
                        }
                    }
                        if (PersonalDataForAccount.Lic == "1330237900")
                        {
                            licNach.AllCubeK = licNach.AllCubeK + 3.187 * CoutnLiver;
                            licNach.NkFull = licNach.NkFull + 3.187 * CoutnLiver * Tar.TarK;
                        }
                }
                PersonalDataForAccount.N_VL = licNach.AllCubeV;
                PersonalDataForAccount.N_Kl = licNach.AllCubeK;
            }
// ------ Если у лицевого счета нет ни водомера на воду, на полив и не указан код счетчика, то... ------------
            else
            {
// ------ Проверяем, бал ли установлен счетчик по данному лицевому счету в текущем месяце. Если был, то начисляем за время до установки счетчика -----
                if (!CalcCubeF(PersonalDataForAccount.Lic, GetEndOfMonth(conf.PerCur).Day + 1))
                {
// ------ Если в рассчетном месяце счетчик не устанавливался, то начисляем кубы по норме ---------------------
                    licNach.AllCubeK = PersonalDataForAccount.N_Kl;
                    licNach.AllCubeV = PersonalDataForAccount.N_VL;
                }
                licNach.NkFull = Math.Round(licNach.AllCubeK * Tar.TarK, 2) * CoutnLiver;
                licNach.NvFull = Math.Round(licNach.AllCubeV * Tar.TarV, 2) * CoutnLiver;
                licNach.AllNormaK = PersonalDataForAccount.N_Kl;
                licNach.AllNormaV = PersonalDataForAccount.N_VL;
                licNach.AllCubeK = licNach.AllCubeK * CoutnLiver;
                licNach.AllCubeV = licNach.AllCubeV * CoutnLiver;
                SkvKan(PersonalDataForAccount);
            }
// ------ Походу дела начисление дополнительных плюшек для комфорта, как вторая ванная, бассейн ------------
            Tar = new Tarifs(conf.PerCur, conf.LastPer, PersonalDataForAccount.Str_code, conn);
            double SanPinNV = licNach.AllNormaV;
            double SanPinNK = licNach.AllNormaK;
            double DaysCalc = (CountDays >= 0) ? CountDays : AvDayInMonth;
            DateTime perOt;
            DateTime perDo;
// ------ Перебираем все дополнительные начисления ---------------------------------------------------------
            foreach (rsNachisl rsNach in rsN)
            {
// ------ Берем из базы все данные по конкретному виду начисления ------------------------------------------
                GrantServices m_rsGS = new GrantServices(conn, rsNach.GranServiceID);
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
                                        if (rsNach.GranServiceID >= 46 && rsNach.GranServiceID <= 55)
                                        {
                                            AmountDays = DaysCalc;
                                        }
                                        else
                                        {
                                            AmountDays = GetEndOfMonth(conf.PerCur).Day;
                                        }
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
                                rsNach.Cube = m_rsGS.ConsumptionRate * (AmountDays / AvDayInMonth) * ((PersonalDataForAccount.N_VL == 1.52 || (PersonalDataForAccount.TempNorm == 1.52 && PersonalDataForAccount.N_VL == 0))
                                              && PersonalDataForAccount.SkvPol == 0 ? ((rsNach.Amount > 1 ? 1 : rsNach.Amount)) : rsNach.Amount);
                                rsNach.Norma = m_rsGS.ConsumptionRate;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarP, 2);
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
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarV, 2) * CoutnLiver;
                                rsNach.Cube = rsNach.Cube * CoutnLiver;
                                licNach.AllCubeV = licNach.AllCubeV + rsNach.Cube;
                                licNach.NvFull = licNach.NvFull + rsNach.Nachisl;
                            }
                            rsNach.Norma = m_rsGS.Coefficient * rsNach.Amount * SanPinNV;
                            licNach.AllNormaV = licNach.AllNormaV + rsNach.Norma;
                        }
                        if (m_rsGS.TypeServicesID == 2)
                        {
                            if ((PersonalDataForAccount.LgKan == 0 && !(PersonalDataForAccount.Vvodomer !=0 || PersonalDataForAccount.VodKod != 0)) ||
                                (!(PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && PersonalDataForAccount.N_Kl > norma_min) ||
                                ((PersonalDataForAccount.Vvodomer != 0 || PersonalDataForAccount.VodKod != 0) && CountDays > 0))
                            {
                                rsNach.Cube = SanPinNK * DaysCalc / AvDayInMonth * m_rsGS.Coefficient * rsNach.Amount;
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarK, 2) * CoutnLiver;
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
                            if (PersonalDataForAccount.Skvagina != 0)
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
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarK, 2);
                                licNach.AllCubeK = licNach.AllCubeK + rsNach.Cube;
                                licNach.NkFull = licNach.NkFull + rsNach.Nachisl;
                            }
                            if (PersonalDataForAccount.LgKan == 0)
                            {
                                rsNach.Norma = (m_rsGS.ConsumptionRate * rsNach.Amount);
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
                                rsNach.Nachisl = Math.Round(rsNach.Cube * Tar.TarV, 2);
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
            licNach.NvFull = licNach.NvFull + PersonalDataForAccount.OverNV_full;
            licNach.NkFull = licNach.NkFull + PersonalDataForAccount.OverNK_full;
            PersonalDataForAccount.SetNewDate(licNach.NvFull, licNach.NkFull, licNach.NPoliv,
                                              licNach.AllCubeV, licNach.AllCubeK, licNach.AllNormaV, licNach.AllNormaK);
            PersonalDataForAccount.Save(conf.PerCur, conn);

        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Нажатие на кнопку расчета -------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnOK_Click(object sender, EventArgs e)
        {
            informationBox.Items.Add(DateTime.Now.ToString() + " Нaчало расчета");
            Query(Abon, "201506");
            informationBox.Items.Add(DateTime.Now.ToString() + " Расчет окончен");
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Выбираем счета, которые нужно рассчитать и потом для каждого вызываем функцию расчета -------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Query(byte bUK, string CalculatedPeriod, SqlConnection pconn = null, string Lic = "")
        {
            System.IO.File.AppendAllText(@"info.log", "----------------------------------------------------------------------------\n\r");
// -- Открываем соединение с базой -------------------------------------------------------------------
            if (pconn == null)
            {
                if ((conn = ConnectToDatabase(bUK)) == null)
                {
                    System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Не удалось соединиться с базой данных " + (bUK == 0 ? "Abon" : "AbonUK") + "\n\r");
                    return;
                }
            }
            else
            {
                conn = pconn;
            }
// -- Забираем настройки текущего пользователя --------------------------------------------------------
            GetConfiguration();
            if (conf == null)
            {
                System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Отсутствуют настройки текущего пользователя в базе\n\r");
                return;
            }
            conf.PerCur = CalculatedPeriod;
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Время начала обработки\n\r");
// ------ Обнуляем начисления по данному лицевому счету в таблице AbonentNach -------------------------
            NullificateAbonentCalculation();
// ------ Выбираем список лицевых счетов для начисления -----------------------------------------------
            List<SelectFirst> PersonalAccounts = GetPersonalAccounts(bUK);
// ------ Выбираем все виды начислений для ранее выбранных счетов -------------------------------------            
            List<rsNachisl> rsN = GetAbonentNach(bUK);
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Выбираем показания счетчиков\n\r");
            Application.DoEvents();
            List<CountersData> CDFull = GetCountersData(bUK);
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Показания счетчиков выбраны\n\r ");
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Выбрано записей для пересчета " + PersonalAccounts.Count.ToString() + "\n\r");
            SelectFirst SingleLic;
            if (PersonalAccounts.Count > 0)
            {
                for (int i = 0; i < PersonalAccounts.Count; i++)
                {
                    if (i % 500 == 0)
                    {
                        System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Обработано: " + i.ToString() + "\n\r");
                        Application.DoEvents();
                    }
                    SingleLic = PersonalAccounts[i];
                    Nach(bUK, SingleLic, rsN, CDFull);
                }
            }
        }

        private void CalculateODN(string CalculatePeriod, SqlConnection connTemp, string LastPer)
        {
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Начало начисления ОДН\n\r");
            ODNCalculate calc = new ODNCalculate(connTemp, CalculatePeriod, LastPer);
            calc = null;
            System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " ОДН  начислен\n\r");
            
        }

// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// ------ Расчет ОДН -------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            CalculateODN("201506", conn, conf.LastPer);
        }

// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// -- Забираем конфигурацию пользователя ------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
        private void GetConfiguration()
        {
            SqlCommand cmd = new SqlCommand("select PerCur, HostName, LastPer from Config where HostName=(Host_Name())", conn);
            using (SqlDataReader rsIn = cmd.ExecuteReader())
            {
                if (rsIn.HasRows)
                {
                    while (rsIn.Read())
                    {
                        conf = new Conf(rsIn["PerCur"].ToString(), rsIn["HostName"].ToString(), rsIn["LastPer"].ToString());
                        return;
                    }
                }
                rsIn.Close();
            }
        }

// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// -- Открываем соединение с базой данных ----------------------------------------------------------
// -------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------

        private SqlConnection ConnectToDatabase(byte bUK)
        {
            string connStr = "Data Source=localhost; Initial Catalog=" + (bUK == 0 ? "Abon" : "AbonUK") + "; Integrated Security=True";
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                //пробуем подключится
                conn.Open();
                return conn;
            }
            catch
            {
                return null;
            }
        }


// ---------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------
// ------- Расчет среднего ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------
        
        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}

/*
    Чтобы пересчитать весь период
    Query(Abon, "201506", conn);
    
 
    Чтобы пересчитать 1 л/с
    Query(0, "201506", conn, "2323736700"); // для Abon
    или
    Query(1, "201506", conn, "2323736700"); // для AbonUK
 
    Чтобы начислить ODN
    CalculateODN("201506", conn)            // указать только период для начисления

  
*/
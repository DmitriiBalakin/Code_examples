using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace water
{
    class clsRecalcPos
    {
        class Pays
        {
            public int Usluga;
            public double Saldo;
            public double SaldoODN;
            public double Charge;
            public double ChargeODN;
            public double CorrCharge;
            public double CorrChargeODN;
            public double Pay;
            public double PayODN;
            public double PayOld;
            public double PayOldODN;
            public Pays(int usluga, double saldo, double saldoODN, double charge, double chargeODN, double corrCharge, double corrChargeODN, double pay, double payODN)
            {
                Usluga = usluga;
                Saldo = saldo;
                SaldoODN = saldoODN;
                Charge = charge;
                ChargeODN = chargeODN;
                CorrCharge = corrCharge;
                CorrChargeODN = corrChargeODN;
                Pay = 0;
                PayODN = 0;
                PayOld = pay;
                PayOldODN = payODN;
            }
        }

        private int GetUsl(string Lic, SqlConnection conn, string Per)
        {
            int U = 0;
            string BaseName = Lic.Substring(0, 1) == "1" ? "abon.dbo." : "abonuk.dbo.";
            string sql_str = "SELECT * FROM " + BaseName + "abonent" + Per + " WHERE Lic = " + Lic + " and skvagina = 0";
            SqlCommand cmd = new SqlCommand(sql_str, conn);
            using (SqlDataReader DR = cmd.ExecuteReader())
            {
                if (DR.HasRows)
                    U += 1;
                DR.Close();
            }
            sql_str = "SELECT * FROM " + BaseName + "abonent" + Per + " WHERE Lic = " + Lic + " and LgKan = 0";
            cmd = new SqlCommand(sql_str, conn);
            using (SqlDataReader DR = cmd.ExecuteReader())
            {
                if (DR.HasRows)
                    U += 2;
                DR.Close();
            }
            return U;
        }

        private double GetPayment (double ToPay, double Summa)
        {
            if(Summa >= 0 && ToPay >= 0)
                return (Summa <= ToPay) ? Summa : ToPay;
            else
                return 0;
        }

// -- Пересчет всех поступлений за период по одному лицевому счету (со строкой соединения) --
        public void RecalcPos(string Lic, string ConnString, string Period)
        {
//            SqlConnection conn = new SqlConnection("Data Source=SERVERAB;Initial Catalog=Abon;Integrated Security=True;Persist Security Info=False;User ID=SqlAbon;Connect Timeout=0;TrustServerCertificate=False");
            SqlConnection conn = new SqlConnection(ConnString);
            conn.Open();
            RecalcPos(Lic, conn, Period);
        }

// -- Пересчет всех поступлений за период по одному лицевому счету (с готовым соединением) --
        public void RecalcPos(string Lic, SqlConnection conn, string Period)
        {
            List<Pays> Spisok = new List<Pays>();
            string BaseName = Lic.Substring(0, 1) == "1" ? "abon.dbo." : "abonuk.dbo.";
            double PaySumm = 0;             // Сумма платежей
            double SumSaldo = 0;            // Сумма положительной задолженности без ОДН
            double SumNachisl = 0;          // Сумма начислений без ОДН
// -- Выбираем платежи в текущем периоде --
            string commstr = @"SELECT SUM(opl + poliv) as Pays FROM " + BaseName + @"Pos" + Period + @" WHERE Lic = " + Lic + " and brik <> 27";
            SqlCommand cmd = new SqlCommand(commstr, conn);
            using (SqlDataReader PA = cmd.ExecuteReader())
            {
                if (PA.HasRows)
                    while (PA.Read())
                        PaySumm += Convert.ToDouble(PA["Pays"]);
                PA.Close();
            }
// -- Выбираем долги и начисления в текущем периоде --            
            commstr = @"SELECT lic, AbonServ_Code, SALDO, Charge, CorrCharge, SaldoODN, ChargeODN, CorrODNCharge, Pay, PayODN
                            FROM " + BaseName + @"AbonServSaldo" + Period + @" WHERE lic = " + Lic + @" ORDER BY AbonServ_Code";
            cmd = new SqlCommand(commstr, conn);
            using (SqlDataReader PA = cmd.ExecuteReader())
            {
                if (PA.HasRows)
                {
                    while (PA.Read())
                    {
                        Spisok.Add(new Pays(Convert.ToInt32(PA["AbonServ_Code"]), Convert.ToDouble(PA["Saldo"]), Convert.ToDouble(PA["SaldoODN"]),
                            Convert.ToDouble(PA["Charge"]), Convert.ToDouble(PA["ChargeODN"]), Convert.ToDouble(PA["CorrCharge"]),
                            Convert.ToDouble(PA["CorrODNCharge"]), Convert.ToDouble(PA["Pay"]), Convert.ToDouble(PA["PayODN"])));
                    }
                }
                PA.Close();
            }
// -- Сумма всех долгов и начислений без ОДН и отдельно по ОДН
            foreach (Pays usl in Spisok)
            {
                SumSaldo += usl.Saldo + usl.SaldoODN > 0 ? usl.Saldo + usl.SaldoODN : 0;
                usl.Pay += usl.Saldo < 0 ? usl.Saldo : 0;
                PaySumm -= usl.Saldo < 0 ? usl.Saldo : 0;
                usl.PayODN += usl.SaldoODN < 0 ? usl.SaldoODN : 0;
                PaySumm -= usl.SaldoODN < 0 ? usl.SaldoODN : 0;
                
                SumNachisl += usl.Charge + usl.CorrCharge + usl.ChargeODN + usl.CorrChargeODN > 0 ?
                    usl.Charge + usl.CorrCharge + usl.ChargeODN + usl.CorrChargeODN : 0;
                usl.Pay += usl.Charge + usl.CorrCharge < 0 ? usl.Charge + usl.CorrCharge : 0;
                PaySumm -= usl.Charge + usl.CorrCharge < 0 ? usl.Charge + usl.CorrCharge : 0;
                usl.PayODN += usl.ChargeODN + usl.CorrChargeODN < 0 ? usl.ChargeODN + usl.CorrChargeODN : 0;
                PaySumm -= usl.ChargeODN + usl.CorrChargeODN < 0 ? usl.ChargeODN + usl.CorrChargeODN : 0;
            }
// -- Распределяем поступления на долги --
            double PayedSaldo = 0;
            if (PaySumm > 0)
            {
                foreach (Pays saldo in Spisok)
                {
                    saldo.Pay += Math.Round(GetPayment((saldo.Saldo > 0 ? saldo.Saldo * (SumSaldo > PaySumm ? PaySumm / SumSaldo : 1) : 0), PaySumm) + 0.00001, 2);
                    PayedSaldo += Math.Round(GetPayment((saldo.Saldo > 0 ? saldo.Saldo * (SumSaldo > PaySumm ? PaySumm / SumSaldo : 1) : 0), PaySumm) + 0.00001, 2);
                    saldo.PayODN += Math.Round(GetPayment((saldo.SaldoODN > 0 ? saldo.SaldoODN * (SumSaldo > PaySumm ? PaySumm / SumSaldo : 1) : 0), PaySumm) + 0.00001, 2);
                    PayedSaldo += Math.Round(GetPayment((saldo.SaldoODN > 0 ? saldo.SaldoODN * (SumSaldo > PaySumm ? PaySumm / SumSaldo : 1) : 0), PaySumm) + 0.00001, 2);
                }
                PaySumm -= Math.Round(PayedSaldo + 0.0001, 2);
            }
// -- Распределяем поступления на начисления --
            if (PaySumm > 0)
            {
                PayedSaldo = 0;
                foreach (Pays saldo in Spisok)
                {
                    saldo.Pay += Math.Round(GetPayment((saldo.Charge + saldo.CorrCharge > 0 ?
                                (saldo.Charge + saldo.CorrCharge) * (SumNachisl > PaySumm ? PaySumm / SumNachisl : 1) : 0), PaySumm) + 0.00001, 2);
                    PayedSaldo += Math.Round(GetPayment((saldo.Charge + saldo.CorrCharge > 0 ?
                                (saldo.Charge + saldo.CorrCharge) * (SumNachisl > PaySumm ? PaySumm / SumNachisl : 1) : 0), PaySumm) + 0.00001, 2);
                    saldo.PayODN += Math.Round(GetPayment((saldo.ChargeODN + saldo.CorrChargeODN > 0 ?
                                (saldo.ChargeODN + saldo.CorrChargeODN) * (SumNachisl > PaySumm ? PaySumm / SumNachisl : 1) : 0), PaySumm) + 0.00001, 2);
                    PayedSaldo += Math.Round(GetPayment((saldo.ChargeODN + saldo.CorrChargeODN > 0 ?
                                (saldo.ChargeODN + saldo.CorrChargeODN) * (SumNachisl > PaySumm ? PaySumm / SumNachisl : 1) : 0), PaySumm) + 0.00001, 2);
                }
                PaySumm -= Math.Round(PayedSaldo + 0.0001, 2);
            }
//-- Распределяем остаток на минусовые платежи --
            if (PaySumm > 0)
            {
                foreach (Pays saldo in Spisok)
                {
                    if (saldo.Pay < 0)
                    {
                        PayedSaldo = Math.Round(-saldo.Pay > PaySumm ? PaySumm : -saldo.Pay + 0.00001, 2);
                        saldo.Pay += Math.Round(-saldo.Pay > PaySumm ? PaySumm : -saldo.Pay + 0.00001, 2);
                        PaySumm -= PayedSaldo;
                    }
                    if (PaySumm > 0)
                    {
                        if (saldo.PayODN < 0)
                        {
                            PayedSaldo = Math.Round(-saldo.PayODN > PaySumm ? PaySumm : -saldo.PayODN + 0.00001, 2);
                            saldo.Pay += Math.Round(-saldo.PayODN > PaySumm ? PaySumm : -saldo.PayODN + 0.00001, 2);
                            PaySumm -= PayedSaldo;
                        }
                    }

                }
            }
//-- распределение остатка если переплата --
//-- Смотрим на наличие скважины и выгребной ямы и распределяем остаток в зависимости от результата - на воду, на канализацию или напополам на обе услуги --
            if (PaySumm != 0)
            {
                int Usl = GetUsl(Lic, conn, Period);
                foreach (Pays saldo in Spisok)
                {
                    if (saldo.Usluga == 1)
                    {
                        if (Usl == 1)
                        {
                            saldo.Pay += PaySumm;
                            PaySumm = 0;
                        }
                        else if (Usl == 0 || Usl == 3)
                        {
                            saldo.Pay += Math.Round(PaySumm / 2, 2);
                            PaySumm -= Math.Round(PaySumm / 2, 2);
                        }
                    }
                    if (saldo.Usluga == 2)
                    {
                        if (Usl == 2)
                        {
                            saldo.Pay += PaySumm;
                            PaySumm = 0;
                        }
                        else if (Usl == 0 || Usl == 3)
                        {
                            saldo.Pay += Math.Round(PaySumm, 2);
                            PaySumm = 0;
                        }
                    }
                }
            }
//--сохранение распределения платежей по услугам--
            foreach (Pays saldo in Spisok)
            {
                cmd.Parameters.Clear();
                commstr = @"UPDATE " + BaseName + @"AbonServSaldo" + Period + @" SET Pay = @Pay, PayODN = @PayODN WHERE Lic = @Lic and AbonServ_Code = @AbonServ_Code";
                cmd = new SqlCommand(commstr, conn);
                cmd.Parameters.Add("@Pay", SqlDbType.Decimal).Value = Math.Round(saldo.Pay + 0.0001, 2);
                cmd.Parameters.Add("@PayODN", SqlDbType.Decimal).Value = Math.Round(saldo.PayODN + 0.0001, 2);
                cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Lic;
                cmd.Parameters.Add("@AbonServ_Code", SqlDbType.Int).Value = saldo.Usluga;
                if (Math.Round(saldo.Pay + 0.0001, 2) != Math.Round(saldo.PayOld + 0.0001, 2) || Math.Round(saldo.PayODN + 0.0001, 2) != Math.Round(saldo.PayOldODN + 0.0001, 2))
                {
                      cmd.ExecuteNonQuery();
//                    string text = Lic + "   :   " + saldo.PayOld.ToString() + "   :   " + saldo.Pay.ToString() + "   :   " + saldo.PayOldODN.ToString() + "   :   " + saldo.PayODN.ToString();
//                    System.IO.File.AppendAllText(@"C:\Users\Фурив.WATER\Documents\Changes.txt", text + "\n");
                }
            }
        }

        private List<string> GetPersonalAccounts(int bUK, SqlConnection conn, string Per)
        {
            string Base = ((bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.");
            string sql = @"SELECT DISTINCT Lic FROM " + Base + "Pos" + Per + " WHERE opl + poliv <> 0 and brik <> 27 ORDER BY lic";
            SqlCommand cmd = new SqlCommand(sql, conn);
            List<string> PersonalAccounts = new List<string>();
            using (SqlDataReader PersonalAccount = cmd.ExecuteReader())
            {
                if (PersonalAccount.HasRows)
                    while (PersonalAccount.Read())
                        PersonalAccounts.Add(PersonalAccount["Lic"].ToString());
                PersonalAccount.Close();
            }
            return PersonalAccounts;
        }

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Делегат для обновления прогрессбара в другом классе -------------------------------------- ---------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void ProgressBarCounter(int LicCount, int LicComplite, string lic);

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ------ Делегат для обновления прогрессбара в другом классе -------------------------------------- ---------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void InformationMessage(string TextMessage);


// -- Пересчет всех поступлений за период по одной базе --
        public void RecalcAllPos(string ConnString, int bUK, string Period, ProgressBarCounter progressBarCounter = null, InformationMessage informationMessage = null)
        {
            SqlConnection conn = new SqlConnection(ConnString);
            conn.Open();
            List<string> Lics = GetPersonalAccounts(bUK, conn, Period);

            int i = 0;
            if (Lics.Count > 0)
                foreach (string L in Lics)
                {
                    i++;
                    RecalcPos(L, conn, Period);
//                    System.IO.File.WriteAllText(@"C:\Users\Фурив.WATER\Documents\Counter.txt", i.ToString() + " из " + Lics.Count.ToString());
                    if (progressBarCounter != null) progressBarCounter(Lics.Count, i, L);

                }
            if (informationMessage != null) informationMessage(DateTime.Now.ToString() + " Платежи распределены.");
            conn.Close();
        }
    }
}

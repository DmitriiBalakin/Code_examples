using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using System.IO;
namespace water
{
    class Verification
    {
        private int i = 0;
        private SqlConnection conn;
        private string Per;
        private string DataBase;
        private string DataBaseInt;
        private SqlCommand cmd;
        private List<string> VerificationResult;
        private string sql;
        private int PredPer;
        private string DataBaseinv;
        private string DataTimeStr;
        public Verification(SqlConnection conn, string per, int database)
        {

            this.conn = conn;                                               // -- Соединение с базой --
            this.Per = per;
            this.DataBase = database == 0 ? "abon.dbo." : "abonuk.dbo.";
            this.DataBaseInt = database.ToString();
            this.DataBaseinv = database == 0 ? "1" : "0";
            PredPer = Convert.ToInt32(Per);
            this.DataTimeStr = DateTime.Today.ToString();
            this.DataTimeStr = DataTimeStr.Replace(".", "");
            if (Per.EndsWith("01") == true)
            {
                PredPer = PredPer - 89;
                PredPer.ToString();

            }
            else
            {
                PredPer = PredPer - 1;
                PredPer.ToString();
            }
        }

        public List<string> MakeVerification()
        {
            string Select = "SELECT a.KodVedom, a.lic,c.fam AS cFIO ";
            string From = " FROM " + DataBase + "abonent" + Per + @" AS a 
                INNER JOIN " + DataBase + @"Controler AS c ON a.Kontr = c.KOD
                INNER JOIN " + DataBase + @"SpVedomstvo AS sv ON a.KodVedom = sv.ID ";
            string Order = " ORDER BY c.fam,a.lic ";
            VerificationResult = new List<string>();
            // -- 1.Проверка индексов --
            sql = " SELECT su.NAME_UL, su.PREF_UL, a.dom, LEFT(su.UCH, 6) AS ind FROM " +
                   DataBase + "abonent" + Per + " AS a INNER JOIN " +
                   DataBase + "SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN " +
                   DataBase + @"StrUch AS su ON a.Str_code = su.cod_yl AND a.dom = su.dom 
                   WHERE     (sv.bPaketC = 1) AND (LEFT(su.UCH, 3) <> '302') 
                   GROUP BY su.NAME_UL, su.PREF_UL, a.dom, LEFT(su.UCH, 6)";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Проверка индексов --");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add("Неверный индекс по адресу " + DRaeder["ind"].ToString() + " " + DRaeder["pref_ul"].ToString() + " " +
                            DRaeder["name_ul"].ToString() + " " + DRaeder["dom"].ToString());
                    }
                }
            }
            // -- 2.Проверка наличия контролера --
            sql = Select + From + "WHERE  kontr=0 AND Lic NOT IN (1008301500,1009059900,1009517900,1009555900,1009777900,1009888900,1008153000) AND kodvedom <> 211 AND buk = " + DataBaseInt.ToString() + @" AND bpaketc = 1 ";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Проверка наличия контролера --");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add("Счет " + DRaeder["lic"].ToString() + " Не выбран контролер. Необходимо определить к какому контролеру относится." + DRaeder["KodVedom"].ToString());
                    }
                }
            }
            // -- 3.Проверка на наличие у контролеров ведомства --
            sql = Select + From + @" WHERE (Kontr < 10 or Kontr > 16) and Kontr <> 0 and Kontr <> 20 AND 
                     (Vedom = 'нет данных' or KodVedom = 57 or KodVedom = 0) " + Order;
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Проверка на наличие у контролеров ведомства --");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add("Контролер " + DRaeder["cFIO"].ToString() + " счет " + DRaeder["lic"].ToString() + " У контролера муниц. и вед. жилья ведомство <нет данных>.");
                    }
                }
            }
            // -- 4.Несоответствие контролера 3-й группы и лицевых с третьей группой --
            sql = Select + From + @" WHERE (Kontr <> 20) AND (gruppa3 = 1) " + Order;
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Несоответствие контролера 3-й группы и лицевых с третьей группой --");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add("Контролер " + DRaeder["cFIO"].ToString() + " счет " + DRaeder["lic"].ToString() + " 3 группа, а контролер не 3-й группы.");
                    }
                }
            }
            sql = @"select b.lic, b.Charge, a.nv, a.NvFull -- Проверка воды
                    from " + DataBase + @"abonent" + Per + @" a 
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom, AbonServSaldo" + Per + @" b 
                    where b.AbonServ_Code = 1 and b.lic = a.Lic and a.nv <> b.Charge and a.nv <> a.NvFull  and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Не совпадают начисления по воде -----------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Не совпадают начисления по воде " + DRaeder["Charge"].ToString() + " " + DRaeder["nv"].ToString() + " " + DRaeder["NvFull"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.pos, summ, propusk = 1, errore = '2'-- поступления
                    from " + DataBase + @"abonent" + Per + @" a inner join
                    (SELECT lic, SUM(opl) as summ FROM pos" + Per + @" b group by b.lic ) b on a.Lic = b.lic
                    inner join(select buk, ID, bPaketC from spVedomstvo ) s on s.ID = a.KodVedom
                    where a.pos <> b.[summ] and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в поступлениях -------------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в поступлениях " + DRaeder["pos"].ToString() + " " + DRaeder["summ"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.Oplata, summ,propusk = 1, errore = '3' -- сторнирование начислений
                    from " + DataBase + @"abonent" + Per + @" a inner join
                    (SELECT lic, SUM(b.CorrCharge + b.CorrODNCharge) as summ FROM AbonServSaldo" + Per + @" b group by b.lic ) b on a.Lic = b.lic 
                    inner join(select buk, ID, bPaketC  from spVedomstvo m ) s on s.ID = a.KodVedom 
                    where a.Oplata <> b.[summ] and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Сторнирование начислений не верно --------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Сторнирование начислений не верно " + DRaeder["Oplata"].ToString() + " " + DRaeder["summ"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.SDolgBeg, summ,propusk = 1, errore = '4' --долг начальный
                    from " + DataBase + @"abonent" + Per + @" a inner join
                    (SELECT lic, SUM(b.SALDO + b.SaldoODN) as summ FROM AbonServSaldo" + Per + @" b group by b.lic ) b on a.Lic = b.lic
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom
                    where a.SDolgBeg <> b.[summ] and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в начальном долге ------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в начальном долге " + DRaeder["SDolgBeg"].ToString() + " " + DRaeder["summ"].ToString());
                    }
                }
            }
            sql = @"select b.lic, b.Charge,a.OverNV_full,propusk = 1, errore = '5' -- ОДН
                    from " + DataBase + @"abonent" + Per + @" a inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom ,
                     abon.dbo.AbonServSaldo" + Per + @" b 
                    where b.AbonServ_Code = 1 and b.lic = a.Lic and a.OverNV_full <> b.ChargeODN and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в ОДН ----------------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в ОДН " + DRaeder["Charge"].ToString() + " " + DRaeder["OverNV_full"].ToString());
                    }
                }
            }
            sql = @"select b.lic, b.Charge,a.Peni,propusk = 1, errore = '6' --пеня 
                    from " + DataBase + @"abonent" + Per + @" a 
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom, abon.dbo.AbonServSaldo" + Per + @" b 
                    where b.AbonServ_Code = 6 and b.lic = a.Lic and a.Peni <> b.Charge and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в Пени -------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в Пени " + DRaeder["Charge"].ToString() + " " + DRaeder["Peni"].ToString());
                    }
                }
            }
            sql = @"select b.lic, b.Charge, a.nk, a.NkFull, errore = '7' -- канализация
                     from " + DataBase + @"abonent" + Per + @" a 
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom, abon.dbo.AbonServSaldo" + Per + @" b 
                    where b.AbonServ_Code = 2 and b.lic = a.Lic and a.nk <> b.Charge and a.nk <> a.NkFull and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в начислении канализации -----------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в начислении канализации " + DRaeder["Charge"].ToString() + " " + DRaeder["nk"].ToString() + " " + DRaeder["NkFull"].ToString());
                    }
                }
            }
            sql = @"select b.lic, b.Charge, a.nvk, a.NvkFull, errore = '8' -- коэф вода
                    from " + DataBase + @"abonent" + Per + @" a
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom, abon.dbo.AbonServSaldo" + Per + @" b 
                    where b.AbonServ_Code = 7 and b.lic = a.Lic and a.nv <> b.Charge and a.nvk <> a.NvkFull and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в коэф воды ---------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в коэф воды" + DRaeder["Charge"].ToString() + " " + DRaeder["nvk"].ToString() + " " + DRaeder["NvkFull"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.skvagina, summ, AbonServ_Code, errror = '9' --скважина начисление
                    from " + DataBase + @"abonent" + Per + @" a inner
                    join (SELECT lic,AbonServ_Code,  SUM(b.SALDO + b.SALDO + b.CorrCharge + b.Pay) as summ FROM AbonServSaldo" + Per + @" b group by b.lic, AbonServ_Code ) b on a.Lic = b.lic 
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom
                    where 0 <> b.[summ] and a.skvagina <> 0 and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1 and (AbonServ_Code = 1 or AbonServ_Code = 7)";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в начислении аоды из-за скважины --------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в начислении аоды из-за скважины " + DRaeder["skvagina"].ToString() + " " + DRaeder["summ"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.LgKan, summ, AbonServ_Code, errror = '10' -- яма начисление
                    from " + DataBase + @"abonent" + Per + @" a inner join
                    (SELECT lic,AbonServ_Code,  SUM(b.SALDO + b.SALDO + b.CorrCharge + b.Pay) as summ FROM AbonServSaldo" + Per + @" b group by b.lic, AbonServ_Code ) b on a.Lic = b.lic 
                    inner join(select buk, ID, bPaketC from spVedomstvo m) s on s.ID = a.KodVedom
                    where 0 <> b.[summ] and a.LgKan <> 0 and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1  and AbonServ_Code = 2";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в начислении ямы -----------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в начислении ямы " + DRaeder["LgKan"].ToString() + " " + DRaeder["summ"].ToString());
                    }
                }
            }
            sql = @"select b.lic, a.SDolgBeg, b.SDolgEnd, propusk = 1, errore = '11'--конец с качалом
                    from " + DataBase + @"abonent" + Per + @" a 
                    inner join (SELECT lic,Sdolgend, KodVedom  FROM " + DataBase + "abonent" + PredPer + @" b group by b.lic, b.SDolgEnd, KodVedom) b on a.Lic = b.lic
                    inner join(select buk, ID, bPaketC from spVedomstvo ) s on s.ID = a.KodVedom
                    where a.SDolgBeg <> b.SDolgEnd and a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseInt.ToString() + " and s.bPaketC = 1 and a.KodVedom = b.KodVedom";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Ошибка в начислении ямы -----------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Ошибка в начислении ямы " + DRaeder["SDolgBeg"].ToString() + " " + DRaeder["SDolgEnd"].ToString());
                    }
                }
            }
            sql = @"select b.lic, b.opl, b.brik,b.Prim, b.data_p
                    from " + DataBase + @"abonent" + Per + @" a inner join
                    (SELECT lic,b.opl, b.brik,b.Prim, b.data_p FROM " + DataBase + "Pos" + Per + @"  b) b on a.Lic = b.lic
                    inner join(select buk, ID, bPaketC from spVedomstvo) s on s.ID = a.KodVedom
                    where  a.gruppa3 = 0 and a.Prochee = 0 and s.bUK = " + DataBaseinv + " and s.bPaketC = 1";
            cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 0;
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    VerificationResult.Add("------------- Начисления в другую базу -------------------------");
                    while (DRaeder.Read())
                    {
                        VerificationResult.Add(" счет " + DRaeder["lic"].ToString() + " Начисления в другую базу " + DRaeder["opl"].ToString() + " " + DRaeder["brik"].ToString() + " " + DRaeder["Prim"].ToString() + " " + DRaeder["data_p"].ToString());
                    }
                }
            }

            StreamWriter file = new StreamWriter(".\\Проверка.txt");
            while (i < VerificationResult.Count)
            {

                file.WriteLine(VerificationResult[i]);
                i++;


            }
            file.Close();

            return VerificationResult;
        }


    }
}

/*
Public Sub TestDataBase()
Dim File As String 'Eiy oaeea
Dim sql As String
Dim Nach As clsNach 'Nachisl
''Dim rsLg As ADODB.Recordset
Dim sSelect As String
Dim sFrom As String
Dim sWhere As String
Dim sOrder As String

Me.MousePointer = 11

''On Error GoTo err

Set Nach = New clsNach
With rs
    If Val(Right(LastPeriod, 2)) <> Month(CDate(DataS)) Then
        MsgBox "Au ia ii?aoa i?iaiaeou i?iaa?eo, ?aaioay a yoii ianyoa!", vbOKOnly, Right(Trim(LastPeriod), 6)
        Exit Sub
    End If
    sSelect = "SELECT a.lic,c.fam AS cFIO "
    sFrom = "FROM " & LastPeriod & " AS a" & vbCrLf & _
           "      INNER JOIN Controler AS c ON a.Kontr = c.KOD" & vbCrLf & _
           "      INNER JOIN SpVedomstvo AS sv ON a.KodVedom = sv.ID" & vbCrLf
    sWhere = "WHERE sv.bPaketC = 1 AND a.gruppa3 = 0 AND "
    sOrder = "ORDER BY c.fam,a.lic"
    If DEAbo.admin.State = adStateOpen Then DEAbo.admin.Close
    DEAbo.admin.ConnectionTimeout = 120
    DEAbo.admin.CursorLocation = adUseServer
    If DEAbo.admin.State = adStateClosed Then DEAbo.admin.Open
    lblTest = "Ia?aei o?aicaeoee..."
    DEAbo.admin.IsolationLevel = adXactSerializable
    'DEAbo.admin.BeginTrans
''    .ActiveConnection = DEAbo.admin
    .CursorLocation = adUseClient
''    .CursorType = adOpenStatic
''    .LockType = adLockOptimistic
    
    sql = _
    "UPDATE [abonent" & DataSYM & "]" & _
       "Set [n_vl] = 0 " & _
          ",[N_Kl] = 0 " & _
          ",[nv] = 0 " & _
          ",[nk] = 0 " & _
          ",[Nachisl] = 0 " & _
          ",[LgotaV] = 0 " & _
          ",[LgotaK] = 0 " & _
          ",[NvFull] = 0 " & _
          ",[NkFull] = 0 " & _
          ",[CubeV] = 0 " & _
          ",[CubeK] = 0 " & _
          ",[AllN_vl] = case when vvodomer=0 then 0 else AllN_vl end " & _
          ",[AllN_kl] = case when vvodomer=0 then 0 else AllN_kl end " & _
    "FROM abonent" & DataSYM & " a INNER JOIN " & _
        "SpVedomstvo AS sv ON a.KodVedom = sv.ID " & _
     "Where  (sv.bPaketC = 0)  and " & _
               "(nv+nk+Nachisl+LgotaV+LgotaK+NvFull+NkFull<>0 or CubeV+CubeK<>0 or N_Vl+N_Kl<>0)"
    DEAbo.admin.Execute sql

    sql = _
        "IF EXISTS" & vbCrLf & _
        "    (" & vbCrLf & _
        "  SELECT *" & vbCrLf & _
        "  FROM" & vbCrLf & _
        "     (SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) > 1.52 And an.GranServiceID = 2" & vbCrLf & _
        "      UNION ALL" & vbCrLf & _
        "      SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) > 1.52 And  an.GranServiceID = 1) aa" & vbCrLf & _
        "  GROUP BY Lic" & vbCrLf & _
        "  HAVING COUNT(*)>1" & vbCrLf & _
        "      )" & vbCrLf
    sql = sql & _
        "DELETE AbonentNach" & DataSYM & "" & vbCrLf & _
        "WHERE GranServiceID = 1 AND Lic IN" & vbCrLf & _
        "      (" & vbCrLf & _
        "  SELECT Lic" & vbCrLf & _
        "  FROM" & vbCrLf & _
        "     (SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) > 1.52 And an.GranServiceID = 2" & vbCrLf & _
        "      UNION ALL" & vbCrLf & _
        "      SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) > 1.52 And  an.GranServiceID = 1) aa" & vbCrLf & _
        "  GROUP BY Lic" & vbCrLf & _
        "  HAVING COUNT(*)>1" & vbCrLf & _
        "      )"
    DEAbo.admin.Execute sql

    sql = _
        "IF EXISTS" & vbCrLf & _
        "    (" & vbCrLf & _
        "  SELECT *" & vbCrLf & _
        "  FROM" & vbCrLf & _
        "     (SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) = 1.52 And an.GranServiceID = 2" & vbCrLf & _
        "      UNION ALL" & vbCrLf & _
        "      SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) = 1.52 And  an.GranServiceID = 1) aa" & vbCrLf & _
        "  GROUP BY Lic" & vbCrLf & _
        "  HAVING COUNT(*)>1" & vbCrLf & _
        "      )" & vbCrLf
    sql = sql & _
        "DELETE AbonentNach" & DataSYM & "" & vbCrLf & _
        "WHERE GranServiceID = 2 AND Lic IN" & vbCrLf & _
        "      (" & vbCrLf & _
        "  SELECT Lic" & vbCrLf & _
        "  FROM" & vbCrLf & _
        "     (SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) = 1.52 And an.GranServiceID = 2" & vbCrLf & _
        "      UNION ALL" & vbCrLf & _
        "      SELECT an.lic" & vbCrLf & _
        "      FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "           AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "           VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "      Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) = 1.52 And  an.GranServiceID = 1) aa" & vbCrLf & _
        "  GROUP BY Lic" & vbCrLf & _
        "  HAVING COUNT(*)>1" & vbCrLf & _
        "      )"
    DEAbo.admin.Execute sql

    sql = _
        "DECLARE @Lic numeric(10,0), @sql varchar(8000)" & vbCrLf & _
        "DECLARE PostTables CURSOR" & vbCrLf & _
        "READ_ONLY" & vbCrLf & _
        "FOR" & vbCrLf & _
        "  SELECT an.lic" & vbCrLf & _
        "    FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "         AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "         VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "    Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) > 1.52 And an.GranServiceID = 1" & vbCrLf & _
        "OPEN PostTables" & vbCrLf & _
        "FETCH NEXT FROM PostTables INTO @Lic" & vbCrLf & _
        "WHILE (@@fetch_status = 0)" & vbCrLf & _
        "BEGIN" & vbCrLf & _
        "  SET @sql='UPDATE AbonentNach" & DataSYM & " SET [GranServiceID] = 2 WHERE GranServiceID = 1 AND lic='+CAST(@Lic as varchar(10))" & vbCrLf & _
        "  EXEC (@Sql)" & vbCrLf & _
        "  FETCH NEXT FROM PostTables INTO @Lic" & vbCrLf & _
        "END" & vbCrLf & _
        "CLOSE PostTables" & vbCrLf & _
        "DEALLOCATE PostTables"
    DEAbo.admin.Execute sql

    sql = _
        "DECLARE @Lic numeric(10,0), @sql varchar(8000)" & vbCrLf & _
        "DECLARE PostTables CURSOR" & vbCrLf & _
        "READ_ONLY" & vbCrLf & _
        "FOR" & vbCrLf & _
        "  SELECT an.lic" & vbCrLf & _
        "    FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "         AbonentNach" & DataSYM & " AS an  ON a.Lic = an.Lic LEFT JOIN" & vbCrLf & _
        "         VodomerDate" & DataSYM & " AS vd ON vd.Lic =a.Lic" & vbCrLf & _
        "    Where (CASE WHEN a.Vvodomer =0 THEN a.n_vl ELSE vd.NormaV END) = 1.52 And an.GranServiceID = 2" & vbCrLf & _
        "OPEN PostTables" & vbCrLf & _
        "FETCH NEXT FROM PostTables INTO @Lic" & vbCrLf & _
        "WHILE (@@fetch_status = 0)" & vbCrLf & _
        "BEGIN" & vbCrLf & _
        "  SET @sql='UPDATE AbonentNach" & DataSYM & " SET [GranServiceID] = 1 WHERE GranServiceID = 2 AND lic='+CAST(@Lic as varchar(10))" & vbCrLf & _
        "  EXEC (@Sql)" & vbCrLf & _
        "  FETCH NEXT FROM PostTables INTO @Lic" & vbCrLf & _
        "END" & vbCrLf & _
        "CLOSE PostTables" & vbCrLf & _
        "DEALLOCATE PostTables"
    DEAbo.admin.Execute sql
    lblTest = "Ioe?uoea oaeea aey caiene..."
    Open txtFullPatch For Output As #1 ' Open file for output.
    Write #1, "Aaiiaio OE"

    lblTest = "I?iaa?ea eiaaenia..."
    Me.Refresh
    sql = " SELECT su.NAME_UL, su.PREF_UL, a.dom, LEFT(su.UCH, 6) AS ind " & vbCrLf & _
          " FROM abonent" & DataSYM & " AS a INNER JOIN " & vbCrLf & _
          "      SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN " & vbCrLf & _
          "      StrUch AS su ON a.Str_code = su.cod_yl AND a.dom = su.dom " & vbCrLf & _
          " WHERE     (sv.bPaketC = 1) AND (LEFT(su.UCH, 3) <> '302') " & vbCrLf & _
          " GROUP BY su.NAME_UL, su.PREF_UL, a.dom, LEFT(su.UCH, 6)"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Aieiaiea: iaaa?iue eiaaen ii aa?ano " & rs!Ind & " " & rs!pref_ul & " " & rs!NAME_UL & " a." & rs!dom; vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

     '--------------I?iaa?ea iaee?ey eiio?iea?a---------------------
    lblTest = "I?iaa?ea iaee?ey eiio?iea?a..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & "WHERE  kontr=0 AND Lic<>2000888800"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Aieiaiea: N?ao " & !lic & " - Iaecaanoai eiio?iea?. Iaiaoiaeii ii?aaaeeou, e eaeiio eiio?iea?o ii ioiineony." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '-------------I?iaa?ea ia iaee?ea o eiio?iea?ia ioieo. e aaa. ?eeuy aaaiinoaa --------
    lblTest = "I?iaa?ea ia iaee?ea o eiio?iea?ia ioieo. e aaa. ?eeuy aaaiinoaa..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "WHERE (Kontr < 10 or Kontr >16) and Kontr<>0 and Kontr<>20 AND " & vbCrLf & _
          "      (Vedom = 'iao aaiiuo' or KodVedom=57 or KodVedom=0)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - O eiio?iea?a ioieo. e aaa. ?eeuy aaaiinnoai <iao aaiiuo>." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '----------------3 a?oiia, a eiio?iea? ia 20
    lblTest = "I?iaa?ea ia iaee?ea o eiio?iea?ia ioieo. e aaa. ?eeuy aaaiinoaa..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & " WHERE (Kontr <> 20) AND (gruppa3 = 1)" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - 3 a?oiia, a eiio?iea? ia < ianaeaiea 3 a?>." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '----------------ia?aiooaiu iieacaiey
    lblTest = "Ia?aiooaiu iieacaiey"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & Conf!PerCur & " AS vd ON a.Lic = vd.Lic" & vbCrLf & _
          "     INNER JOIN MaxPosVod AS pv ON a.Lic = pv.Lic " & vbCrLf & _
          sWhere & vbCrLf & " a.Vvodomer =1 AND (pv.LastPer is not null) AND " & _
            "       ((vd.H1 IS NULL AND pv.KubH1s+pv.KubH1n <> 0) OR " & vbCrLf & _
            "        (vd.H2 IS NULL AND pv.KubH2s+pv.KubH2n <> 0) OR " & vbCrLf & _
            "        (vd.H3 IS NULL AND pv.KubH3s+pv.KubH3n <> 0) OR " & vbCrLf & _
            "        (vd.G1 IS NULL AND pv.KubG1s+pv.KubG1n <> 0) OR " & vbCrLf & _
            "        (vd.G2 IS NULL AND pv.KubG2s+pv.KubG2n <> 0) OR " & vbCrLf & _
            "        (vd.G3 IS NULL AND pv.KubG3s+pv.KubG3n <> 0)) "

    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & "aaoa(aaou) iiaa?ee e iieacaiey ianiioaaonoao?o!"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '--------------400---------------------
    lblTest = "400e e anou ANI..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN AbonentNach" & Conf!PerCur & " AS an ON a.Lic = an.Lic " & vbCrLf & _
          "     INNER JOIN VodomerDate" & Conf!PerCur & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          sWhere & " (an.GranServiceID > 7 AND an.GranServiceID < 14) " & vbCrLf & _
          " AND ((CASE WHEN a.vvodomer=0 THEN a.N_VL ELSE vd.NormaV END) =12.16 OR" & vbCrLf & _
          "      (CASE WHEN a.vvodomer=0 THEN a.N_KL ELSE vd.NormaK END) =12.16 ) " & vbCrLf & _
          " GROUP BY a.lic,c.fam " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Ii?ia 12.16 e anou aii.nai.i?eai?" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '--------------I?iaa?ea ia iiaaaaou a  noa ai---------------------
    lblTest = "ia iiaaaaou a noa ai..."
    Me.Refresh
    sql = "DELETE From DateBringCourt WHERE DateBringCourt<CONVERT(datetime, '" + "01." & Format(DataS, "mm.yyyy") + "', 104)"
    DEAbo.admin.Execute sql

        '------------------------I?iaa?ea ia?eneaiey----------------------
    lblTest = "I?iaa?ea aieuoiai ia?eneaiey..."
    Me.Refresh
    sql = sSelect & ",a.Nachisl,ISNULL(p.pos,0) as pos,a.liver,i.md,a.vvodomer" & vbCrLf & _
        sFrom & vbCrLf & _
        " LEFT JOIN abonent" & PredPer(DataSYM) & " aa on a.lic=aa.lic AND (a.vvodomer=0 or (a.vvodomer=1 AND isnull(aa.vvodomer,0)=1)) " & _
        "   LEFT OUTER JOIN (SELECT SUM(opl+poliv) pos, lic  from Pos" & DataSYM & " WHERE Brik<>27 GROUP BY lic ) p ON a.Lic = p.lic" & vbCrLf & _
        "   LEFT OUTER JOIN (SELECT lic,MAX(MaxData) md" & vbCrLf & _
        "                    FROM(SELECT Lic,MaxData FROM invakt" & vbCrLf & _
        "                         Union All" & vbCrLf & _
        "                         SELECT Lic,MaxData FROM invaktpoliv" & vbCrLf & _
        "                         Union All" & vbCrLf & _
        "                         SELECT Lic,MaxData FROM invinv" & vbCrLf & _
        "                         Union All" & vbCrLf & _
        "                         SELECT lic,MaxData FROM InvVod" & vbCrLf & _
        "                        ) ii" & vbCrLf & _
        "                    GROUP BY lic) i ON i.lic=a.lic" & vbCrLf & _
        "  LEFT OUTER JOIN (select lic from abonentNAch" & DataSYM & " WHERE nachisl>0 GROUP BY lic) an on a.lic=an.lic AND an.Lic is NULL" & vbCrLf & _
        sWhere & vbCrLf & _
        "  a.Nachisl > p.pos+100 and a.SDolgBeg+a.Nachisl+a.Oplata+a.Poliv-ISNULL(p.Pos,0)-a.Spisan>0 AND  " & vbCrLf & _
        "((a.CubeV/CASE WHEN a.liver-a.vibilo-a.vibilo2<=0 Then 1 Else (a.liver-a.vibilo-a.vibilo2) END )>50 or" & vbCrLf & _
        " (a.CubeK/CASE WHEN a.liver-a.vibilo-a.vibilo2<=0 Then 1 Else (a.liver-a.vibilo-a.vibilo2) END )>50 )" & vbCrLf & _
        sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Neeoeii aieuoia  ia?eneaiea " & !Nachisl & " ?oa. " & _
            " i?i?." & !liver & " ?ae." & IIf(!vvodomer <> 0, " anou aiaiia?, iinooieei " & !pos & "", "") & " Aaoa eia - " & Nz(rs!md, "iao") & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '------------------------I?iaa?ea io?eoaoaeuiiai ia?eneaiey.
    lblTest = "I?iaa?ea io?eoaoaeuiiai ia?eneaiey..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & sWhere & " nachisl<0" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - ioeaea: Ia?eneaiea iaiuoa 0" & vbCrLf
         PB.Value = PB.Value + 1
        .MoveNext
    Wend
    .Close

     '--Neaa?eia, neeaiay yia, iao ia?eneaiey e anou iinooieaiea
     lblTest = "Neaa?eia, neeaiay yia, iao ia?eneaiey e anou iinooieaiea..."
     Me.Refresh
     sql = sSelect & vbCrLf & sFrom & vbCrLf & _
           "       INNER JOIN Pos" & DataSYM & " p ON a.Lic = p.lic" & vbCrLf & _
           "Where (a.skvagina <> 0) And (a.LgKan <> 0) And (a.Nachisl = 0) And (p.opl+p.poliv > 0) AND a.sud=0 AND a.SDolgBeg<=0 " & vbCrLf & _
           "GROUP BY a.Lic,c.fam" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Neaa?eia, neeaiay yia, iao ia?eneaiey e anou iinooieaiea" & vbCrLf
         PB.Value = PB.Value + 1
        .MoveNext
    Wend
    .Close

     '---------------------------I?iaa?ea niaa??aiey naoae------------------------
    'Oae?aai io 1 ?enea oae. ianyoa e ie?a
    lblTest = "I?iaa?ea niaa??aiey naoae..."
    Me.Refresh
    sql = "SELECT a.*, c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " sodset<>0 and socdo<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104)" & vbCrLf & _
          sOrder
    .Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Caeii?eeinu niaa??aiea naoae" & vbCrLf
        !sodset = 0
        !socdo = Null
        !Sodsetdn = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '--------------------Ia?aeinu niaa??aiea naoae ii n??oo
    sql = "SELECT a.*, c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " sodset = 0 and sodsetdn<convert(datetime,'" + DataS + "',104) and (socdo>convert(datetime,'" + DataS + "',104) or socdo is null)" & vbCrLf & _
          sOrder
    .Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        If IsDate(!Sodsetdn) Then
            If !sodset = 0 And IsNull(!socdo) Then
                !Sodsetdn = Null
            Else
                Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Ia?aeinu niaa??aiea naoae" & vbCrLf
                !sodset = 1
            End If
            Nach.Nach rs, True
        End If
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

        '------------------Anou aiaiia? e ii?ia auoa nioeaeuiie------------------------
    lblTest = "Anou aiaiia? e ii?ia auoa nioeaeuiie..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & " (a.Vvodomer <> 0) AND (vd.NormaV > 12.16 OR vd.NormaK > 12.16)" & vbCrLf & _
          sOrder
    .Open sql
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Anou aiaiia? e ii?ia auoa nioeaeuiie. "
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

 '------------I?iaa?ea ia ionoo? caienu a noaa
    lblTest = "I?iaa?ea ia ionoo? caienu a noaa..."
    Me.Refresh
    sql = "SELECT Lic, RegNum From SudReestrT WHERE IskSumma = 0"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        '31,01,2007  anee ia iiiaaiaeony auaiaeou ia ye?ai e a aaeuiaeoai, oi
        ' iaai caiaieou ia DEAbo.admin.Execute "DELETE FROM [SudReestrT]  WHERE IskSumma = 0"
        'Write #1, "Aieiaiea: N?ao " + str(!lic) + " Iiaaii a noa, a noiia enea = 0. Caienu oaaeaia." + vbCrLf
        DEAbo.admin.Execute "DELETE FROM SudReestrT  WHERE lic=" & !lic & " and RegNum= " + Replace(!RegNum, ",", ".")
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

     '------------Iaee?ea aaoo acaeiienee??a?o oneoa ii iieeao
    lblTest = "Iaee?ea aaoo acaeiienee??a?o oneoa ii iieeao..."
    Me.Refresh
    sql = "SELECT a.lic,c.fam AS cFIO" & vbCrLf & _
          "FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
          "     Controler AS c ON a.Kontr = c.KOD INNER JOIN" & vbCrLf & _
          "     (SELECT Lic" & vbCrLf & _
          "      FROM AbonentNach" & DataSYM & vbCrLf & _
          "      Where GranServiceID = 2  And GranServiceID = 1" & vbCrLf & _
          "      GROUP BY Lic) an ON an.lic=a.lic" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - aaa oneoae ii iieeao Ioe?uoue a?oio, aiainiaa?aiea ec oee?iie e ec aiaicaai?iie eieiiee" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '----------Ia?eia iinooieaiey aieuoa 3 eaoiae aaaiinoe
    lblTest = "Ia?eia iinooieaiey aieuoa 3 eaoiae aaaiinoe..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN pos" & DataSYM & " AS p ON p.lic=a.lic" & vbCrLf & _
          "WHERE  cast(left(peropl,4) as int)<" & CInt(Left(DataSYM, 4)) - 3 & vbCrLf & _
          "GROUP BY a.lic,c.fam" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " ia?eia iinooieaiey aieuoa 3 eaoiae aaaiinoe." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '------------------------Ai?y?ay aiaa ia iaoa
    lblTest = "Ai?y?ay aiaa ii aiainiaa?aie? ia iaoa ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          "     INNER JOIN" & vbCrLf & _
          "      (SELECT a.lic" & vbCrLf & _
          "       FROM common.dbo.SpStreets AS s INNER JOIN" & vbCrLf & _
          "            common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code INNER JOIN" & vbCrLf & _
          "            abonent" & DataSYM & " AS a ON s.Code_Yl = a.Str_code AND CAST(h.NumHouse AS nvarchar(20)) + h.LitHouse = a.dom INNER JOIN" & vbCrLf & _
          "            common.dbo.SpHWorg AS hw ON h.HWorg_Code = hw.id_HWorg INNER JOIN" & vbCrLf & _
          "            SpVedomstvo sv ON sv.ID = a.KodVedom" & vbCrLf & _
          "       WHERE hw.id_HWorg=0) k ON k.Lic =a.Lic" & vbCrLf & _
          sWhere & "  (a.Vvodomer = 1) AND (vd.HotWaterAnother = 0) AND " & vbCrLf & _
          "        (vd.NormaV <> 0) AND (vd.NormaK <> 0) AND (vd.NormaV <> vd.NormaK)  AND k.Lic IS NULL "
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " ii?iu ii aiaa ?acee?a?ony ii aaeea """" Ai?y?ay aiaa ii aiainiaa?aie? ia iaoa """" ia noieo " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '--- ainnoaiiaeou ii?io iaiuoa oaeouae
    lblTest = "Ainnoaiiaeou ii?io"
    Me.Refresh
    sql = sSelect & ", TempNorm, TempNormDate, N_Vl, N_Kl " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.Vvodomer=0  And a.VodKod=0 AND ((N_Vl > TempNorm) AND (TempNorm > 0) OR " & _
          "            (N_Kl >= TempNorm) AND (TempNorm > 0))"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " Anou ii?ia aiaa " & !N_VL & " ,eaiaeecaoey " & !n_kl & " e ii?ia aey ainnoaiiaeaiey " & !TempNorm
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    '---------------i?e eaea. aiaiia?a anou iinooieaiea  e iao ia?eneaiee
    lblTest = "i?e eiaeaea. aiaiia?a anou iinooieaiea  e iao ia?eneaiee..."
    Me.Refresh
    sql = sSelect & ",brik, data_p" & vbCrLf & sFrom & vbCrLf & _
        "       INNER JOIN (SELECT SUM(opl + poliv) AS pos, lic, brik, data_p" & vbCrLf & _
        "                   FROM Pos" & DataSYM & vbCrLf & _
        "                   WHERE brik <> 27" & vbCrLf & _
        "                   GROUP BY lic, brik, data_p) AS p ON a.Lic = p.lic" & vbCrLf & _
        "       INNER JOIN abonent" & strPrePer & " aa ON aa.Lic=a.Lic " & vbCrLf & _
    sWhere & " (a.Nachisl = 0) And (p.pos > 10) And a.Vvodomer <> 0 AND" & vbCrLf & _
    "       (a.SDolgBeg+a.Nachisl+a.Oplata+a.Poliv-ISNULL(p.Pos,0)-a.Spisan)<0 And aa.Vvodomer <> 0" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Anou eiaeaea. aiaiia?,a oaeouai ia?eiaa anou iinooieaiey ii iao ia?eneaiey" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
     
     '--------------I?iaa?ea a?aiaiiuo ii?i---------------------
    lblTest = "I?iaa?ea a?aiaiiuo ii?i..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
         sWhere & "Prochee=0 AND  TempNorm>0 and TempNormDate<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104) And vvodomer=0" & vbCrLf & _
         sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Ainnoaiiaeaiu ii?iu , " + !fam + ", " + !yl_name + ", " + !dom + _
                " / " + !flat + vbCrLf
        If !lgkan = 0 And !TempNorm >= 2.28 Then
            !n_kl = !TempNorm
            If !TempNorm = 5.933 Then !n_kl = 9.12
            If !TempNorm = 4.644 Then !n_kl = 7.144
            If !TempNorm = 3.56 Then !n_kl = 5.472
            If !TempNorm = 2.966 Then !n_kl = 4.56
        End If
        If !Skvagina = 0 Then !N_VL = !TempNorm
        !TempNorm = 0
        !TempNormDate = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    '--Oaaeyai Iai?i?eaa?o AI
    lblTest = "Oa?aii iai?i?eaa?o ai"
    Me.Refresh
    sql = sSelect & ", a.Liver,a.NotLive,ab.dtNotLive" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (a.NotLive = 1) AND (ab.dtNotLive < CONVERT(DATETIME, '01." & Format(DateAdd("m", 1, DataS), "mm.yyyy") & "', 104)) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Oa?aii ia i?i?eaa?o AI." + vbCrLf
        !NotLive = 0
        !dtNotLive = Null
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '--Oaaeyai i?i?aa AI
    lblTest = "Oa?aii i?i?aa AI..."
    Me.Refresh
    sql = sSelect & ", a.Prochee,ab.dtOther" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (Prochee = 1) AND (dtOther < CONVERT(DATETIME, '01." & Format(DateAdd("m", 1, DataS), "mm.yyyy") & "', 104)) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Oa?aii i?i?aa AI." + vbCrLf
        !Prochee = 0
        !dtOther = Null
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    
    '--------------Anee anou ia?eneaiea oi oaaeyai i?i?aa---------------------
    lblTest = "Oa?aii i?i?aa..."
    Me.Refresh
    sql = sSelect & ", a.Prochee" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (Prochee = 1) AND (dtOther IS NULL) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Oa?aii i?i?aa." + vbCrLf
        !Prochee = 0
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
   
    
    lblTest = "I?iaa?ea a?aiaiiuo ii?i..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          sWhere & "Prochee<>0 AND TempNorm>0 and TempNormDate<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104) And vvodomer=0" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Oa?aii i?i?aa Ainnoaiiaeaiu ii?iu, " + !fam + ", " + !yl_name + ", " + !dom + _
                " / " + !flat + vbCrLf
        If !lgkan = 0 And !TempNorm >= 2.28 Then
            !n_kl = !TempNorm
            If !TempNorm = 5.933 Then !n_kl = 9.12
            If !TempNorm = 4.644 Then !n_kl = 7.144
            If !TempNorm = 3.56 Then !n_kl = 5.472
            If !TempNorm = 2.966 Then !n_kl = 4.56
        End If
        If !Skvagina = 0 Then !N_VL = !TempNorm
        !TempNorm = 0
        !TempNormDate = Null
        !Prochee = 0
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Onoaiiaeai aiaiia?, ainnoaiiaeou ii?io"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " (TempNorm > 0) AND (Vvodomer = 1) AND (TempNormDate < convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104))" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " Onoaiiaeai aiaiia?, ainnoaiiaeou ii?io"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
       '-----------------I?iaa?ea a?aiaiii auauaoeo-------------------------
   'Oae?aai io 1 ?enea oae. ianyoa e ie?a
    lblTest = "I?iaa?ea a?aiaiii auauaoeo..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          sWhere & "Vibilo<>0 and VibiloData<=convert(datetime,'" + "15." & Format(DataS, "mm.yyyy") + "',104)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
'''        Write #1, " Ainnoaiiaeaii eiee?anoai i?i?eaa?ueo ii n?aoo "  & !lic &  " " + !fam + " " + !yl_name + " " + !Dom + _
'''                " " + !Flat + " eiio?iea? " + !FIO + vbCrLf
        !Vibilo = 0
        !VibiloData = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          sWhere & "Vibilo2<>0 and VibiloData2<=convert(datetime,'" + "15." & Format(DataS, "mm.yyyy") + "',104)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
'''        Write #1, " Ainnoaiiaeaii eiee?anoai i?i?eaa?ueo ii n?aoo "  & !lic &  " " + !fam + " " + !yl_name + " " + !Dom + _
'''                " " + !Flat + " eiio?iea? " + !FIO + vbCrLf
        !vibilo2 = 0
        !VibiloData2 = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '------------------------------------------------
    lblTest = "I?iaa?ea a?aiaiiuo ii?i (n?ua ieiiau)..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO, vi.DateInv " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          "     LEFT OUTER JOIN  VodInv AS vi ON a.Lic = vi.lic" & vbCrLf & _
          sWhere & "TempNorm>0 and TempNormDate<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104) " & vbCrLf & _
          " And vvodomer=1" & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " Aue aiaiia? e  ainnoaiiaeaiu ii?iu, " & !fam & ", " & !yl_name & ", " & !dom & _
                " / " & !flat & " oae.: " & !numtel & ", eiaaioa?ecaoey: " & !DateInv & vbCrLf
        If !lgkan = 0 And !TempNorm >= 2.28 Then
            !n_kl = !TempNorm
            If !TempNorm = 5.933 Then !n_kl = 9.12
            If !TempNorm = 4.644 Then !n_kl = 7.144
            If !TempNorm = 3.56 Then !n_kl = 5.472
            If !TempNorm = 2.966 Then !n_kl = 4.56
        End If
        If !Skvagina = 0 Then !N_VL = !TempNorm
        !TempNorm = 0
        !TempNormDate = Null
        !vvodomer = 0
        Nach.Nach rs, True
        If !Nachisl = 0 And !liver - !Vibilo - !vibilo2 = 0 Then
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " iinea ainnoaiiaeaiey ii?iu ia?eneaiea e eie-ai i?i?. = 0 " & vbCrLf
        End If
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
   lblTest = "Iao aiaiia?a ia iieea..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          sWhere & " (NOT (vd.H3 IS NULL)) AND (a.Pvodomer = 0) AND (a.Kontr > 9) AND (a.Kontr < 17) AND a.VVodomer=1 and vd.Gl=0" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " iao aaeee aiaiia? ia iieea e anou aaoa iiaa?ee O3 " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
        
    lblTest = "Iinoaaeou aaeeo neeaiay yia"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & sWhere & _
    " (a.Kontr > 9 AND a.Kontr < 17) AND a.vvodomer=0 AND LgKan = 0 AND N_Kl=0 AND N_Vl <> 0"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " iao ioiaoee Neeaiay/aua?aaiay yia " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
        '-------------------------------------
    lblTest = "AAoa iiaa?ee ionoonoaoao eee onoa?aea"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT OUTER JOIN  VodomerDate" & Conf!PerCur & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          sWhere & " a.gruppa3=0 AND a.fam<>'O' AND (a.Vvodomer = 1) AND  (sv.bPaketC = 1) AND ( " & vbCrLf & _
          "  ((vd.H1 IS NULL) AND (vd.H2 IS NULL) AND (vd.H3 IS NULL) AND " & vbCrLf & _
          "(vd.G1 IS NULL) AND (vd.G2 IS NULL) AND (vd.G3 IS NULL)))" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " Aaoa(aaou) iiaa?ee ionoonoao?o"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Ii?iaoea 1.52 "
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.vvodomer=0 AND N_Vl = 1.52 AND N_KL=1.52" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " ii?iaoea ii aiaa e eai-oee 1.52" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    lblTest = "Ii?iaoea 1.52 "
    Me.Refresh
    sql = sSelect & ",N_KL " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.vvodomer=0 AND N_Vl = 1.52 AND N_KL>1.52" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " ii?iaoea ii aiaa1.52, ii eai-oee " & !n_kl & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    lblTest = "Ionoonoaoao ii?iaoea ii aiaiia?o "
    Me.Refresh
    sql = sSelect & ",N_KL " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & " (a.Vvodomer = 1) And (a.gruppa3 = 0) AND (SV.bPaketC = 1) And (vd.NormaV + vd.NormaK = 0)" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " ionoonoaoao ii?iaoea ii aiaiia?o " & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
 
    lblTest = "I?iaa?ea aaou iiaa?ee aiain?ao?eeia..."
    Me.Refresh
    sql = "SELECT a.*, c.fam AS cFIO, v.* " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " v ON a.lic=v.lic " & vbCrLf & _
          sWhere & " (sv.bPaketC = 1) AND (a.vvodomer<>0 or a.VodKod<>0) and (" & _
                    " (v.h1<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)) OR (v.h2<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)) OR " & _
                    " (v.h3<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)) OR (v.G1<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)) OR " & _
                    " (v.G2<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)) OR (v.G3<=convert(datetime,'" & "15." & Format(DataS, "mm.yyyy") & "',104)))  " & vbCrLf & _
          sOrder
    .Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        !vvodomer = 0
        !pvodomer = 0
        !VodKod = 0
        !N_VL = !NormaV
        !n_kl = !NormaK
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    lblTest = "I?iaa?ea aaou iiaa?ee iaueo aiain?ao?eeia..."
    Me.Refresh
    sql = "SELECT a.*, c.fam AS cFIO,ccln.n_vl as NormV,ccln.n_kl as NormK" & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN CommonCountersData AS ccd ON a.VodKod = ccd.IdCommonAdress " & vbCrLf & _
          "     INNER JOIN CommonCountersLicNorm ccln ON a.Lic = ccln.lic " & vbCrLf & _
          sWhere & " (a.VodKod <> 0) AND (sv.bPaketC = 1) AND (ccd.Refusal = 0) AND (ccd.NonWorking = 0) AND (ccd.DateVerify < CONVERT(datetime, '" & "15." & Format(DataS, "mm.yyyy") & "', 104)) " & vbCrLf & _
          sOrder & ",ccd.DateVerify "
    .Open sql
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        !vvodomer = 0
        !pvodomer = 0
        !VodKod = 0
        !N_VL = !normV
        !n_kl = !normK
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
 '----------Iinooieaiey ii eoaai n enoaeoei n?ieii iiaa?ee
    lblTest = "Iinooieaiey ii eoaai n enoaeoei n?ieii iiaa?ee..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "INNER JOIN abonent" & strPrePer & " AS aa ON aa.Lic = a.Lic" & vbCrLf & _
          "INNER JOIN MaxPosVod AS mp ON a.Lic=mp.Lic " & vbCrLf & _
          sWhere & " (a.Vvodomer = 0) And (aa.Vvodomer = 1) And cast('20'+mp.PerOpl as int)<>mp.LastPer AND mp.LastPer=" & strPrePer & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " Anou iinooieaiey ii eoaai, ii n?ie iiaa?ee enoae. Aiia?eneeou i?aauaouee ianyo" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
'----ia ia?aei ianyoa
    If chkBegMonth.Value = 1 Then
        lblTest = "eaeoaioee ..."
        Me.Refresh
        sql = "  SELECT o.lic " & vbCrLf & _
               " FROM abonent" & DataSYM & " AS a INNER JOIN " & vbCrLf & _
               "           (SELECT     lic, SUM(oplata) AS opl " & vbCrLf & _
               "             From oplata" & DataSYM & " " & vbCrLf & _
               "             GROUP BY lic) AS o ON a.Lic = o.lic AND a.Nachisl = - o.opl"
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, !lic & " Noi?ii ?aaii ia?eneaie?, i?iaa?uoa eoiaiao? noiio a eaeoaioee" + vbCrLf
            PB.Value = PB.Value + 1
            .MoveNext
        Wend
        .Close
                 
        '---------------------------I?iaa?ea i?i?eaa?ueo
        lblTest = "I?iaa?ea i?i?eaa?ueo ..."
        Me.Refresh
        sql = "SELECT a.*, c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
              "INNER JOIN abonent" & strPrePer & " AS aa ON aa.Lic = a.Lic" & vbCrLf & _
              sWhere & "(aa.Liver > 0) AND (a.Liver = 0) AND (a.Fam NOT IN ('O', ' ', '.','.O','O.','I')) and " & vbCrLf & _
              " a.Gruppa3=0 and a.prochee=0 AND a.Vibilo+a.Vibilo2>=0" & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " I?i?eaa?ueo noaei 0 e iao a?ai i?ea" + vbCrLf
            If !Kontr > 9 And !Kontr < 18 Then
                !liver = 1
                !Vibilo = 0
                !vibilo2 = 0
                Nach.Nach rs, True
            End If
            PB.Value = PB.Value + 1
            .MoveNext
        Wend
        .Close
            
            '-----------------------I?iaa?ea ioeaaiai ia?eneaiey
        lblTest = "I?iaa?ea ioeaaiai ia?eneaiey..."
        Me.Refresh
        sql = "SELECT c.fam as cFIO, s.yl_name, a.dom, a.flat, a.Lic,a.fam " & vbCrLf & sFrom & vbCrLf & _
              "     INNER JOIN    street AS s ON a.Str_code = s.cod_yl  " & vbCrLf & _
              sWhere & "(sv.bPaketC = 1) And (a.Vvodomer = 0) And (a.VodKod = 0) And (a.n_vl + a.N_Kl <> 0) And " & vbCrLf & _
              "     (a.Nachisl <= 0) And (a.gruppa3 = 0) And (a.Prochee = 0) AND" & vbCrLf & _
              "     (a.Liver - a.Vibilo - a.Vibilo2 > 0 OR (a.liver=0 and a.vibilo+a.vibilo2=0)) " & vbCrLf & _
              " ORDER BY c.fam, s.yl_name"
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " " & !fam & " " & !yl_name & " " & !dom & " " & !flat & " Ia?eneaiea=0, ainnoaieou ia?eneaiea." & vbCrLf
             PB.Value = PB.Value + 1
            .MoveNext
        Wend
        .Close
            
        lblTest = "Iao aiaiia?a e ii?ia ia niioaaonaoao naiieio ..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
              sWhere & " Vvodomer = 0 AND Pvodomer = 0 AND VodKod = 0 AND" & vbCrLf & _
        "    ((N_Vl not in (0,1.52,1.824,2.28,3.648,4.56,5.472,7.144,7.6,9.12,12.16,5.933,4.644,3.56,2.967)) OR" & vbCrLf & _
        "     (N_Kl not in (0,1.52,1.824,2.28,3.648,4.56,5.472,7.144,7.6,9.12,12.16,5.933,4.644,3.56,2.967)))" & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " iao aaeee aiaiia?a e ii?ia ia niioaaonaoao naiieio " + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        .Close
        
        lblTest = "enoae n?ie iiaa?ee o a\n e iao iineaaieo iieacaiee ..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
              "     INNER JOIN abonent" & strPrePer & " AS aa ON a.Lic = aa.Lic " & vbCrLf & _
              "     LEFT OUTER JOIN MaxPosVod AS mpv ON a.Lic = mpv.Lic" & vbCrLf & _
              sWhere & " a.Vvodomer = 0 AND aa.Vvodomer = 1 AND ISNULL(mpv.LastPer," & strPrePer & ")<" & strPrePer & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - enoae n?ie iiaa?ee o a\n e iao iineaaieo iieacaiee" + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        rs.Close

            '---------------i?e iauai aiaiia?a anou iinooieaiea  e iao ia?eneaiee
        lblTest = "i?e iauai aiaiia?a anou iinooieaiea  e iao ia?eneaiee..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
            "       INNER JOIN (SELECT SUM(opl + poliv) AS pos, lic" & vbCrLf & _
            "                   FROM Pos" & DataSYM & vbCrLf & _
            "                   WHERE brik <> 27" & vbCrLf & _
            "                   GROUP BY lic) AS p ON a.Lic = p.lic" & vbCrLf & _
        sWhere & " (a.Nachisl = 0) And (p.pos > 10) And vodkod<>0 AND" & vbCrLf & _
        "       (SDolgBeg+Nachisl+Oplata+Poliv-ISNULL(p.Pos,0)-Spisan)<=0 " & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Anou iauee aiaiia?,a oaeouai ia?eiaa anou iinooieaiey ii iao ia?eneaiey" + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        .Close
    End If
    
    lblTest = "ioiineony e ?eeu?, iao aieaa, ii anou iieaoa..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
        "     LEFT OUTER JOIN" & vbCrLf & _
        "      (SELECT SUM(opl + poliv) AS pos, lic" & vbCrLf & _
        "        FROM Pos" & DataSYM & vbCrLf & _
        "        WHERE brik <> 27" & vbCrLf & _
        "        GROUP BY lic) AS p ON a.Lic = p.lic" & vbCrLf & _
        "WHERE a.SDolgBeg+a.Nachisl+Poliv+a.Oplata-ISNULL(p.pos,0)- a.Spisan<0 AND" & vbCrLf & _
        "      ISNULL(p.pos,0)>0 AND sv.bPaketC = 0" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - ioiineony e ?eeu?, iao aieaa, ii anou iieaoa." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
       
    If CInt(Right(DataSYM, 2)) >= 3 And CInt(Right(DataSYM, 2)) <= 9 Then
        lblTest = "Caeii?eeny n?ie iiaa?ee A\N e o aaiiaioa anou neaa?eia..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
              "     INNER JOIN abonent" & strPrePer & " AS aa ON a.Lic = aa.Lic" & vbCrLf & _
              sWhere & "a.skvagina = 1 AND ((a.Vvodomer=0 AND aa.Vvodomer =1) OR" & vbCrLf & _
            "                          (a.Pvodomer=0 AND aa.Pvodomer=1) OR" & vbCrLf & _
            "                          (a.VodKod=0 AND aa.VodKod<>0))    " & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Caeii?eeny n?ie iiaa?ee A\N e o aaiiaioa anou neaa?eia." + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        rs.Close
    End If
   
    lblTest = "Ionoonoaoao aiaiia?, ii aaoa iiaa?ee aieuoa oaeouaai ianyoa ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & "a.Vvodomer=0 AND a.Pvodomer=0 AND a.Nachisl=0 AND a.Vodkod=0 AND a.Fam<>'O' AND a.Fam<>'o' AND " & vbCrLf & _
          "(dbo.DateToPer(ISNULL(vd.H1,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & " AND" & vbCrLf & _
          " dbo.DateToPer(ISNULL(vd.H2,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & " AND" & vbCrLf & _
          " dbo.DateToPer(ISNULL(vd.H3,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & " AND" & vbCrLf & _
          " dbo.DateToPer(ISNULL(vd.G1,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & " AND" & vbCrLf & _
          " dbo.DateToPer(ISNULL(vd.G2,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & " AND" & vbCrLf & _
          " dbo.DateToPer(ISNULL(vd.G3,CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)),0)>=" & strAfterPer & ")" & vbCrLf & _
          "AND NOT (vd.H1 IS NULL AND vd.H2 IS NULL AND vd.H3 IS NULL AND vd.G1 IS NULL AND vd.G2 IS NULL AND vd.G3 IS NULL)" & vbCrLf & _
           sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - ionoonoaoao aiaiia?, ii aaoa iiaa?ee aieuoa oaeouaai ianyoa." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

'''    lblTest = "Oaaeaiea eeoaauo..."
'''    Me.Refresh
'''    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
'''          "LEFT OUTER JOIN" & vbCrLf & _
'''          "    (SELECT SUM(abs(opl) + abs(poliv)) AS pos, lic" & vbCrLf & _
'''          "     FROM Pos" & DataSYM & vbCrLf & _
'''          "     GROUP BY lic) AS p ON a.Lic = p.lic" & vbCrLf & _
'''          "WHERE (a.Fam = 'O' OR a.Fam = 'o') AND" & vbCrLf & _
'''          "       a.SDolgBeg+a.Nachisl+Poliv+a.Oplata-ISNULL(p.pos,0)- a.Spisan=0 AND" & vbCrLf & _
'''          "       a.sndeb-a.snkred+a.oplata-a.spisan-a.pos+a.Nachisl+a.Poliv=0 AND" & vbCrLf & _
'''          "       a.pos=0 AND a.Oplata =0 AND a.Spisan =0 AND a.Poliv=0 AND ISNULL(p.pos,0)=0 AND" & vbCrLf & _
'''          "       a.SDolgBeg=0 AND a.sndeb=0 AND a.snkred=0    " & vbCrLf & sOrder
'''    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
'''    PB.Max = .RecordCount + 1
'''    PB.Value = 0
'''    While Not .EOF
'''        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Oaaeai." + vbCrLf
'''        .MoveNext
'''        PB.Value = PB.Value + 1
'''    Wend
'''    rs.Close
''    sql = _
''        "DELETE FROM abonent" & DataSYM & vbCrLf & _
''        "FROM abonent" & DataSYM & " AS a LEFT OUTER JOIN" & vbCrLf & _
''        "    (SELECT SUM(abs(opl) + abs(poliv)) AS pos, lic" & vbCrLf & _
''        "     FROM Pos" & DataSYM & vbCrLf & _
''        "     GROUP BY lic) AS p ON a.Lic = p.lic" & vbCrLf & _
''        "WHERE (a.Fam = 'O' OR a.Fam = 'o') AND" & vbCrLf & _
''        "       a.SDolgBeg+a.Nachisl+Poliv+a.Oplata-ISNULL(p.pos,0)- a.Spisan=0 AND" & vbCrLf & _
''        "       a.sndeb-a.snkred+a.oplata-a.spisan-a.pos+a.Nachisl+a.Poliv=0 AND" & vbCrLf & _
''        "       a.pos=0 AND a.Oplata =0 AND a.Spisan =0 AND a.Poliv=0 AND ISNULL(p.pos,0)=0 AND" & vbCrLf & _
''        "       a.SDolgBeg=0 AND a.sndeb=0 AND a.snkred=0"
''    DEAbo.admin.Execute sql
    sql = ""
    
    lblTest = "o aaiiaioa ii?ia ia niaiaaaao n ii?iie ia aii ..."
    Me.Refresh
    sql = _
        "SELECT c.fam as cFIO, aaa.lic" & vbCrLf & _
        "FROM abonent" & DataSYM & " aaa INNER JOIN" & vbCrLf & _
        "     (" & vbCrLf & _
        "    SELECT COUNT(*) cntt,nn.koeff*10 as k,aa.Str_code, aa.dom," & vbCrLf & _
        "           CASE WHEN aa.Vvodomer =1 THEN vv.NormaV ELSE aa.N_Vl END AS NormaV ," & vbCrLf & _
        "           CASE WHEN aa.Vvodomer =1 THEN vv.NormaK ELSE aa.N_Kl END AS NormaK" & vbCrLf & _
        "    FROM abonent" & DataSYM & " aa INNER JOIN" & vbCrLf & _
        "        (" & vbCrLf & _
        "        SELECT Str_code, dom,  CAST(min(cnt) AS NUMERIC(18,10))/CAST(max(cnt) AS NUMERIC(18,10))*100 AS koeff" & vbCrLf & _
        "        FROM" & vbCrLf & _
        "            (" & vbCrLf & _
        "            SELECT COUNT(*) cnt,a.Str_code, a.dom" & vbCrLf & _
        "                ,CASE WHEN a.Vvodomer =1 THEN v.NormaV ELSE a.N_Vl END AS NormaV" & vbCrLf & _
        "                ,CASE WHEN a.Vvodomer =1 THEN v.NormaK ELSE a.N_Kl END AS NormaK" & vbCrLf & _
        "            FROM abonent" & DataSYM & " AS a" & vbCrLf & _
        "                 LEFT JOIN  VodomerDate" & DataSYM & " AS v ON a.Lic = v.Lic" & vbCrLf & _
        "                 INNER JOIN SpVedomstvo AS sv ON a.KodVedom = sv.ID" & vbCrLf & _
        "            WHERE (a.gruppa3 = 0) AND (a.Prochee = 0) AND NOT (a.LgKan<>0 AND a.skvagina=1) AND" & vbCrLf & _
        "                  (sv.bPaketC = 1) AND a.Fam <> 'O' AND a.Fam <> 'o' AND a.Fam <> 'Y' AND a.Fam <> 'y' AND" & vbCrLf & _
        "                  (CASE WHEN a.Vvodomer =1 THEN v.NormaV ELSE a.N_Vl END = 7.6 OR" & vbCrLf & _
        "                   CASE WHEN a.Vvodomer =1 THEN v.NormaV ELSE a.N_Vl END = 9.12) AND" & vbCrLf & _
        "                  (CASE WHEN a.Vvodomer =1 THEN v.NormaK ELSE a.N_Kl END = 7.6 OR" & vbCrLf & _
        "                   CASE WHEN a.Vvodomer =1 THEN v.NormaK ELSE a.N_Kl END = 9.12)" & vbCrLf & _
        "            GROUP BY  a.Str_code, a.dom," & vbCrLf
    sql = sql & _
        "                      CASE WHEN a.Vvodomer =1 THEN v.NormaV ELSE a.N_Vl END," & vbCrLf & _
        "                      CASE WHEN a.Vvodomer =1 THEN v.NormaK ELSE a.N_Kl END" & vbCrLf & _
        "            HAVING COUNT(*)>1" & vbCrLf & _
        "            ) qq" & vbCrLf & _
        "        GROUP BY Str_code, dom" & vbCrLf & _
        "        HAVING COUNT(*)>1  AND CAST(min(cnt) AS NUMERIC(18,10))/CAST(max(cnt) AS NUMERIC(18,10))*100 <10" & vbCrLf & _
        "        ) nn ON aa.Str_code =nn.Str_code AND aa.dom = nn.dom" & vbCrLf & _
        "        LEFT JOIN  VodomerDate" & DataSYM & " AS vv ON aa.Lic = vv.Lic" & vbCrLf & _
        "    GROUP BY aa.Str_code, aa.dom,nn.koeff" & vbCrLf & _
        "            ,CASE WHEN aa.Vvodomer =1 THEN vv.NormaV ELSE aa.N_Vl END" & vbCrLf & _
        "            ,CASE WHEN aa.Vvodomer =1 THEN vv.NormaK ELSE aa.N_Kl END" & vbCrLf & _
        "    HAVING COUNT(*)>nn.koeff*10" & vbCrLf & _
        "    ) n ON n.Str_code = aaa.Str_code AND aaa.dom = n.dom" & vbCrLf & _
        "    INNER JOIN VodomerDate" & DataSYM & " as vd ON vd.Lic = aaa.Lic" & vbCrLf & _
        "    INNER JOIN Controler c ON c.KOD = aaa.Kontr" & vbCrLf & _
        "    INNER JOIN SpVedomstvo AS sv ON aaa.KodVedom = sv.ID" & vbCrLf & _
        "WHERE  sv.bPaketC =1 AND aaa.gruppa3 = 0 AND" & vbCrLf & _
        "       (CASE WHEN aaa.Vvodomer =1 THEN vd.NormaV ELSE aaa.N_Vl END <> n.NormaV  OR" & vbCrLf & _
        "        CASE WHEN aaa.Vvodomer =1 THEN vd.NormaK ELSE aaa.N_Kl END <> n.NormaK  )" & vbCrLf & _
        "ORDER BY c.fam,aaa.lic"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - ii?ia ia niaiaaaao n ii?iie ia aii " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "iinoaaeaia aaeea aiaiia? a oae ianyoa e n?ie iiaa?ee enoa?ao a oii ?a ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
         "      INNER JOIN VodomerDate" & DataSYM & " AS v ON a.Lic = v.Lic" & vbCrLf & _
         "      INNER JOIN abonent" & strPrePer & " AS aa ON a.Lic = aa.Lic" & vbCrLf & _
        sWhere & " a.Vvodomer = 1 AND aa.Vvodomer = 0 AND" & vbCrLf & _
        "((v.H1 IS NOT NULL AND v.H1<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)) OR" & vbCrLf & _
        " (v.H2 IS NOT NULL AND v.H2<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)) OR" & vbCrLf & _
        " (v.H3 IS NOT NULL AND v.H3<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)) OR" & vbCrLf & _
        " (v.G1 IS NOT NULL AND v.G1<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)) OR" & vbCrLf & _
        " (v.G2 IS NOT NULL AND v.G2<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104)) OR" & vbCrLf & _
        " (v.G3 IS NOT NULL AND v.G3<CONVERT(DATETIME,'01." & Right(strAfterPer, 2) & "." & Left(strAfterPer, 4) & "',104))  )" & vbCrLf & _
        sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - iinoaaeaia aaeea aiaiia? a oae ianyoa e n?ie iiaa?ee enoa?ao a oii ?a " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    lblTest = "ii iaiiio aiio onoaiiaeaiu ?aciua aaaiinoaa ..."
    Me.Refresh
    sql = "EXEC [dbo].[Get2Ved] '" & DataSYM & "'"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - onoaiiaeaiu ?aciua aaaiinoaa a iaiii e oii ?a aiia" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "oaeouee ia?eia iieacaiee iaiuoa i?aauaouaai ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN  MaxPosVod AS m ON a.Lic = m.Lic" & vbCrLf & _
          "     INNER JOIN  (SELECT pp.*" & vbCrLf & _
          "                  From" & vbCrLf & _
          "                     (" & vbCrLf & _
          "                     SELECT p.Lic, MAX(p.RowID ) AS M2RowId" & vbCrLf & _
          "                     FROM PosVod AS p INNER JOIN" & vbCrLf & _
          "                          MaxPosVod AS mm ON mm.Lic=p.Lic AND mm.RowID <> p.RowID" & vbCrLf & _
          "                     GROUP BY p.lic" & vbCrLf & _
          "                     ) m2 " & vbCrLf & _
          "     INNER JOIN   PosVod pp ON pp.RowID = m2.M2RowId ) mm ON mm.Lic = a.Lic" & vbCrLf & _
          sWhere & " (a.Vvodomer = 1) AND m.PerOpl IS NOT NULL AND mm.PerOpl IS NOT NULL AND" & vbCrLf & _
          "           m.PerOpl<mm.PerOpl AND RTRIM(LTRIM(m.PerOpl))<>'' AND RTRIM(LTRIM(m.PerOpl))<>''" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - oaeouee ia?eia iieacaiee iaiuoa i?aauaouaai" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "iieacaiey i?ea. o?aoa i?aa. ia?eiaa ia eiiao ian. ia niaiaaaao n oaeouae ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN  MaxPosVod AS m ON a.Lic = m.Lic" & vbCrLf & _
          "     INNER JOIN  (SELECT pp.*" & vbCrLf & _
          "                  From" & vbCrLf & _
          "                     (" & vbCrLf & _
          "                     SELECT p.Lic, MAX(p.RowID ) AS M2RowId" & vbCrLf & _
          "                     FROM PosVod AS p INNER JOIN" & vbCrLf & _
          "                          MaxPosVod AS mm ON mm.Lic=p.Lic AND mm.RowID <> p.RowID" & vbCrLf & _
          "                     GROUP BY p.lic" & vbCrLf & _
          "                     ) m2 " & vbCrLf & _
          "     INNER JOIN   PosVod pp ON pp.RowID = m2.M2RowId ) mm ON mm.Lic = a.Lic" & vbCrLf & _
          "     INNER JOIN abonent" & Format(DateAdd("m", -2, DataS), "yyyymm") & " aa ON a.lic=aa.lic" & vbCrLf & _
          "     INNER JOIN SpVedomstvo AS svv ON aa.KodVedom = svv.ID" & vbCrLf & _
          sWhere & " (a.Vvodomer = 1) AND ISNULL(m.PerOpl,'')<>'' AND ISNULL(mm.PerOpl,'')<>''" & vbCrLf & _
          "      AND (m.KubG1s<>mm.KubG1n or m.KubG2s<>mm.KubG2n or m.KubG3s<>mm.KubG3n or" & vbCrLf & _
          "           m.KubH1s<>mm.KubH1n or m.KubH2s<>mm.KubH2n or m.KubH3s<>mm.KubH3n)" & vbCrLf & _
          "      and svv.bPaketC = 1" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - iieacaiey i?ea. o?aoa i?aa. ia?eiaa ia eiiao ian. ia niaiaaaao n oaeouae" + vbCrLf
'        .MoveNext
'        PB.Value = PB.Value + 1
'    Wend
'    rs.Close
    
    lblTest = "noaaaiia i?iecaianoai iei?aii, ioeacaii ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN SudReestrT AS st ON a.Lic = st.Lic" & vbCrLf & _
          sWhere & " dbo.DateToPer(st.DataProizKon, 0) = " & DataSYM & " And st.Otkaz = 1" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - noaaaiia i?iecaianoai iei?aii, ioeacaii" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "a oaeouai ia?eiaa onoaiiaeaia aaeea aiaiia? e ia aaaaaiu iieacaiey ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN abonent" & strPrePer & " AS aa ON a.Lic=aa.Lic " & vbCrLf & _
          "     INNER JOIN MaxPosVod AS p ON a.Lic=p.lic" & vbCrLf & _
          sWhere & " a.Vvodomer =1 and aa.Vvodomer =0 AND a.KodVedom = aa.KodVedom AND dbo.DateToPer(p.ModiRec,0)<" & DataSYM & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - a oaeouai ia?eiaa onoaiiaeaia aaeea aiaiia? e ia aaaaaiu iieacaiey" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "eie-ai i?i?eaa?ueo iaiuoa 0 ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.Liver-a.Vibilo-a.Vibilo2<0 " & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - eie-ai i?i?eaa?ueo iaiuoa 0" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "anou a?ai. i?ea./oa. iao  aaou ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " ((a.Vibilo<>0 AND a.VibiloData IS NULL) OR (a.Vibilo2<>0 AND a.VibiloData2 IS NULL)) " & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - anou a?ai. i?ea./oa. iao  aaou" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Aaoa aaiaa aieuoa aaou iiaa?ee ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "  INNER JOIN VodomerDate" & DataSYM & vbCrLf & " AS v ON a.Lic = v.Lic" & vbCrLf & _
          sWhere & " a.Vvodomer = 1 AND v.datvvod IS NOT NULL AND" & vbCrLf & _
          "      ((v.H1 IS NOT NULL AND  v.H1 < v.datvvod) OR" & vbCrLf & _
          "       (v.H2 IS NOT NULL AND  v.H2 < v.datvvod) OR" & vbCrLf & _
          "       (v.H3 IS NOT NULL AND  v.H3 < v.datvvod) OR" & vbCrLf & _
          "       (v.G1 IS NOT NULL AND  v.G1 < v.datvvod) OR" & vbCrLf & _
          "       (v.G2 IS NOT NULL AND  v.G2 < v.datvvod) OR" & vbCrLf & _
          "       (v.G3 IS NOT NULL AND  v.G3 < v.datvvod))" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - Aaoa aaiaa aieuoa aaou iiaa?ee" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Ii?iaoea ii ai?. aiaa e anou n?ao?eee oieuei ii oieiaiie aiaa  ..."
    Me.Refresh
    sql = sSelect & ", ss.yl_name,a.dom,a.flat " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " v ON v.Lic=a.Lic " & vbCrLf & _
          "     INNER JOIN common.dbo.SpStreets AS s  ON s.Code_Yl = a.Str_code" & vbCrLf & _
          "     INNER JOIN common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code AND CAST(h.NumHouse AS nvarchar(20)) + h.LitHouse = a.dom" & vbCrLf & _
          "     INNER JOIN common.dbo.SpHWorg AS hw ON h.HWorg_Code = hw.id_HWorg" & vbCrLf & _
          "     INNER JOIN street AS ss ON a.Str_code = ss.cod_yl " & vbCrLf & _
          "     LEFT JOIN (SELECT lic From VodomerInv GROUP BY lic HAVING MAX(YEAR(DateInv))=" & Left(DataSYM, 4) & " ) vi ON a.lic=vi.lic" & vbCrLf & _
          sWhere & " vi.lic is null AND  hw.id_HWorg>0 AND (a.Vvodomer =1 or a.VodKod<>0) AND" & vbCrLf & _
          "    (v.NormaK = 9.12 or v.NormaK = 7.144 or v.NormaK = 5.472 or v.NormaK = 4.56) AND" & vbCrLf & _
          "    (v.G1 IS NULL AND v.G2 IS NULL AND v.G3 IS NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " aa?an " & !yl_name & ", " & !dom & ", " & !flat & " - Ii?iaoea ii ai?. aiaa e anou n?ao?eee oieuei ii oieiaiie aiaa" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    lblTest = "Ii?iaoea aac ai?. aiau e anou n?ao?eee e ii ai?. aiaa oi?a ..."
    Me.Refresh
    sql = sSelect & ", ss.yl_name,a.dom,a.flat " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " v ON v.Lic=a.Lic " & vbCrLf & _
          "     INNER JOIN common.dbo.SpStreets AS s  ON s.Code_Yl = a.Str_code" & vbCrLf & _
          "     INNER JOIN common.dbo.SpHouses AS h ON s.Id_Street = h.Street_Code AND CAST(h.NumHouse AS nvarchar(20)) + h.LitHouse = a.dom" & vbCrLf & _
          "     INNER JOIN common.dbo.SpHWorg AS hw ON h.HWorg_Code = hw.id_HWorg" & vbCrLf & _
          "     INNER JOIN street AS ss ON a.Str_code = ss.cod_yl " & vbCrLf & _
          sWhere & " hw.id_HWorg=0 AND (a.Vvodomer =1 or a.VodKod<>0) AND" & vbCrLf & _
          "      (v.NormaK <> 9.12 AND v.NormaK <> 7.144 AND v.NormaK <> 5.472 AND v.NormaK <> 4.56 AND v.NormaK <>0) AND " & vbCrLf & _
          "      (v.G1 IS NOT NULL OR v.G2 IS NOT NULL OR v.G3 IS NOT NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " aa?an " & !yl_name & ", " & !dom & ", " & !flat & " - Ii?iaoea aac ai?. aiau e anou n?ao?eee e ii ai?. aiaa oi?a" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
   
    lblTest = "O aaiiaioa iauee n?ao?ee, ii aaou iiaa?ee e onoaiiaee ionoonoao?o ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT JOIN VodomerDate" & DataSYM & " v ON v.Lic=a.Lic " & vbCrLf & _
          sWhere & " (a.VodKod > 0) AND (v.Lic IS NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - O aaiiaioa iauee n?ao?ee, ii aaou iiaa?ee e onoaiiaee ionoonoao?o" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "O aaiiaioa iauee n?ao?ee, ii ionoonoao?o iieacaiey..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT JOIN PosVod AS p ON a.Lic = p.Lic " & vbCrLf & _
          sWhere & " (a.VodKod > 0) AND (p.Lic IS NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - O aaiiaioa iauee n?ao?ee, ii ionoonoao?o iieacaiey" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Aaiiaio ioiineony e ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
            "INNER JOIN street st ON a.Str_code = st.cod_yl " & vbCrLf & _
          "WHERE (" & _
            "(st.PoselKod = 2 And a.KodVedom <> 103) or " & _
            "(st.PoselKod = 3 And a.KodVedom <> 124) AND  (st.PoselKod = 3 And a.KodVedom <> 163)  or " & _
            "((st.PoselKod = 4 And a.KodVedom <> 133) AND (st.PoselKod = 4 And a.KodVedom <> 125) ) or " & _
            "(st.PoselKod = 5 And a.KodVedom <> 106) or " & _
            "(st.PoselKod = 6 AND a.KodVedom <> 133) or " & _
            "(st.PoselKod = 7 AND a.KodVedom <> 145) )  " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - ia aa?iia aaaiinoai o aaiiaioa III " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Ainoaauee ia i?enaiai ..."
    Me.Refresh
    sql = _
        "SELECT su.NAME_UL,su.PREF_UL,su.dom, su.idVendor,COUNT(a.lic) cnt" & vbCrLf & _
        "FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
        "     SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN" & vbCrLf & _
        "     StrUch AS su ON a.Str_code = su.cod_yl AND a.dom = su.dom LEFT JOIN" & vbCrLf & _
        "     (SELECT     Id" & vbCrLf & _
        "      From SpVendor" & vbCrLf & _
        "      WHERE      (noReflect = 0) AND (Id <> 0)) AS sd ON su.idVendor = sd.Id LEFT JOIN " & vbCrLf & _
        "     VodomerDate" & DataSYM & " vd on vd.Lic=a.lic" & vbCrLf & _
        "WHERE (a.Prochee = 0) AND (a.gruppa3 = 0) AND (a.Fam <> N'O') AND (sv.bPaketC = 1) AND " & vbCrLf & _
        "      (case when(a.Vvodomer=0 AND a.VodKod=0 ) then a.n_kl ELSE vd.NormaK END +case when(a.Vvodomer=0 AND a.VodKod=0 ) then a.n_vl ELSE vd.NormaV END)<>0" & vbCrLf & _
        "  AND (sd.Id IS NULL)" & vbCrLf & _
        "GROUP BY su.NAME_UL,su.PREF_UL,su.dom, su.idVendor" & vbCrLf & _
        "ORDER BY su.NAME_UL,su.PREF_UL,su.dom"
''    sql = "SELECT su.NAME_UL, a.dom, su.PUNKT, su.PREF_UL" & vbCrLf & _
''           "FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
''           "     SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN" & vbCrLf & _
''           "     StrUch AS su ON a.Str_code = su.cod_yl AND a.dom = su.dom" & vbCrLf & _
''           "WHERE (su.idVendor = 0) AND (sv.bPaketC = 1) AND a.Fam<>'O' AND a.prochee=0 AND " & vbCrLf & _
''           "      ((a.gruppa3=0 and a.skvagina = 0) or (a.skvagina <> 0 AND a.N_Kl <> 0) OR " & vbCrLf & _
''           "      (a.gruppa3<>0 AND a.SDolgEnd > 0))AND a.Str_code<>'Aio1' AND a.Str_code<>'oii' " & vbCrLf & _
''           "GROUP BY su.NAME_UL, a.dom, su.PUNKT, su.PREF_UL" & vbCrLf & _
''           "ORDER BY su.PUNKT,su.NAME_UL"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Ii aa?ano " & !NAME_UL & ", " & !dom & ", " & !pref_ul & " ainoaauee ia i?enaiai" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "E\n? iiia?ai ia oaaeaiea..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " (a.Fam='O' OR a.Fam='o')" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Eiio?-? " & !cFIO & " n?ao " & !lic & " - E\n? iiia?ai ia oaaeaiea" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

End With
lblTest = "I?eiaiaiea o?aicaeoee..."
'DEAbo.admin.CommitTrans
DEAbo.admin.Close
lblTest = "Cae?uoea oaeea..."
Close #1   ' Close file.
Call DE_EXEC("exec [dbo].[SetAllChargeOnAbonServ] '" & DataSYM & "'", True)
lblTest = "I?iaa?ea ia ioeaee caeii?aia!"
CmdTest.Enabled = True

Me.MousePointer = 0
Exit Sub

err:
    Me.MousePointer = 0
    lblTest = "Ioiaia o?aicaeoee..."
 '   DEAbo.admin.RollbackTrans
    DEAbo.admin.Close
    Close #1
    MsgBox "I?iecioea ioeaea!" & err.Description & vbCrLf & "Nicaaiiue oaee ioeaie ia aa?ai!", vbCritical, "Error"
End Sub

*/
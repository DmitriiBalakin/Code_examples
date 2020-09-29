using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace water
{
    class SberbankReestr
    {
        const char Razd1 = ';';
        const char Razd2 = ':';
        const string RazdBlock = "[!]:";
        int PredPer, CurrPer;
        string SQLText1;
        string SQLText2;
        string SQLText3;
        string SQLText4;
        string SQLText5;
        string SQLText6;
        string SQLTextAll;
        string SQLTextAdres;
        string BlockHeader;
        string BlockVodomer;
        string BlockVodomer1, BlockVodomer2, BlockVodomer3, BlockVodomer4, BlockVodomer5, BlockVodomer6;
        string BlockUsl;
        int NumVodomer;
        decimal SumSaldo; // сумма по услуге Задолженность на начало периода
        decimal SumCharge; // сумма по услуге Начисления текущего периода
        decimal SumPeni; // сумма по услуге Пеня за просрочку
        decimal Summa; // общая сумма по всем услугам
        decimal Razn;
        string ResSvertki;

        public SberbankReestr()
        {

        }

        // -- Уменьшение строки с датой на месяц --
        private string GetPrevPeriod(string Period, int CountMonths = 1)
        {
            for (int i = 1; i <= CountMonths; i++)
                Period = (Period.Substring(4, 2) == "01") ? (Convert.ToInt32(Period) - 89).ToString() : (Convert.ToInt32(Period) - 1).ToString();
            return Period;
        }

        public void MakeReestr()
        {
            // Поехали
            // lblStatus.Text = "Выгрузка запущена...";
            // this.Refresh();
            // НЕ ЗАБЫВАТЬ МЕНЯТЬ ПЕРИОДЫ!!! (сделал поля на форме!)
            CurrPer = Convert.ToInt32(frmMain.MaxCurPer); //201707;
            PredPer = Convert.ToInt32(GetPrevPeriod(frmMain.MaxCurPer)); //201706;
            // ЕСЛИ НЕОБХОДИМО ДОВЫГРУЗИТЬ ТОЛЬКО ПО НОВЫМ ДОМАМ, 
            // КОТОРЫЕ ПОЯВИЛИСЬ ПОСЛЕ ОСНОВНОЙ ВЫГРУЗКИ, ТО ПИШЕМ АДРЕСА В ХВОСТ ЗАПРОСА (РАСКОММЕНТИРОВАТЬ И ПОПРАВИТЬ АДРЕСА)
            SQLTextAdres = "";
            // SQLTextAdres = @" 
            // and (
            // (a.Str_code = 'Мар1' and a.dom = '20')
            // or(a.Str_code = 'Цве1' and a.dom = '5а')
            // or(a.Str_code = 'Сил1' and a.dom = '1')
            // or(a.Str_code = 'Мос1' and a.dom = '145')
            // or(a.Str_code = 'Мос1' and a.dom = '158')
            // or(a.Str_code = 'Мос1' and a.dom = '158/1')
            // or(a.Str_code = 'Мос1' and a.dom = '158/2')
            // )";

            #region
            // ABON без водомеров
            SQLText1 = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{0}',2)+SUBSTRING('{0}',1,4) as BPer, 
	                          null as H1, null as H2, null as H3, null as G1, null as G2, null as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              null as H1Date, null as H2Date, null as H3Date, null as G1Date, null as G2Date, null as G3Date, a.dom, a.flat, 1 as t1
                        FROM ABON.dbo.abonent{0} AS a INNER JOIN
	                         ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN) as SumSaldo
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge-ass.Pay-ass.PayODN) as SumCharge
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code=6) zPeni ON zPeni.lic=a.Lic INNER JOIN
	                         ABON.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE (a.vvodomer = 0 and a.vodkod = 0 and sv.bUK = 0 and sv.bPaketC = 1 and a.gruppa3 = 0)
                           -- and not a.lic IN (SELECT a.lic FROM Abon.dbo.abonent202007 a inner join Abon.dbo.abonent202008 b on a.Lic = b.Lic WHERE a.KodVedom <> b.KodVedom)
                        ", CurrPer) + SQLTextAdres;

            // ------------------- ABON с водомерами (без перешедших в прямые в этом месяце) ------------------- 
            SQLText2 = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{1}',2)+SUBSTRING('{1}',1,4) as BPer,
	                          pv.KubH1n as H1, pv.KubH2n as H2, pv.KubH3n as H3, pv.KubG1n as G1, pv.KubG2n as G2, pv.KubG3n as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              v.H1 as H1Date, v.H2 as H2Date, v.H3 as H3Date, v.G1 as G1Date, v.G2 as G2Date, v.G3 as G3Date, a.dom, a.flat, 2 as t1
                        FROM ABON.dbo.abonent{0} AS a INNER JOIN
                             ABON.dbo.VodomerDate{0} v ON v.Lic=a.Lic INNER JOIN
	                         ABON.dbo.MaxPosVod pv on pv.Lic=a.Lic INNER JOIN
	                         ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN-ass.Pay-ass.PayODN) as SumSaldo
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge) as SumCharge
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code=6) zPeni ON zPeni.lic=a.Lic INNER JOIN
	                         ABON.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE ((a.vvodomer = 1 or a.vodkod <> 0) and (sv.bUK = 0 and sv.bPaketC = 1 and a.gruppa3 = 0)) 
                           -- and not a.lic IN (SELECT a.lic FROM Abon.dbo.abonent202007 a inner join Abon.dbo.abonent202008 b on a.Lic = b.Lic WHERE a.KodVedom <> b.KodVedom)
                        ", CurrPer, PredPer) + SQLTextAdres;

            // ------------------- ABONUK без водомеров ------------------- 
            SQLText3 = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{0}',2)+SUBSTRING('{0}',1,4) as BPer, 
	                          null as H1, null as H2, null as H3, null as G1, null as G2, null as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              null as H1Date, null as H2Date, null as H3Date, null as G1Date, null as G2Date, null as G3Date, a.dom, a.flat, 3 as t1
                        FROM ABONUK.dbo.abonent{0} AS a INNER JOIN
	                         ABONUK.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN) as SumSaldo
		                        from ABONUK.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge-ass.Pay-ass.PayODN) as SumCharge
		                        from ABONUK.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABONUK.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code = 6) zPeni ON zPeni.lic = a.Lic INNER JOIN
	                         ABONUK.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE (a.vvodomer = 0 and a.vodkod = 0) and sv.bPaketC = 1 and a.gruppa3 = 0
                           -- and not a.lic IN (SELECT a.lic FROM Abonuk.dbo.abonent202007 a inner join Abonuk.dbo.abonent202008 b on a.Lic = b.Lic WHERE a.KodVedom <> b.KodVedom)
                        ", CurrPer) + SQLTextAdres;

            // ------------------- ABONUK с водомерами ------------------- 
            SQLText4 = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{1}',2)+SUBSTRING('{1}',1,4) as BPer,
	                          pv.KubH1n as H1, pv.KubH2n as H2, pv.KubH3n as H3, pv.KubG1n as G1, pv.KubG2n as G2, pv.KubG3n as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              v.H1 as H1Date, v.H2 as H2Date, v.H3 as H3Date, v.G1 as G1Date, v.G2 as G2Date, v.G3 as G3Date, a.dom, a.flat, 4 as t1
                        FROM ABONUK.dbo.abonent{0} AS a INNER JOIN
                            --ABONUK.dbo.abonent{1} b ON a.Lic=b.Lic INNER JOIN                    
                             ABONUK.dbo.VodomerDate{0} v ON v.Lic=a.Lic INNER JOIN
	                         ABONUK.dbo.MaxPosVod pv on pv.Lic=a.Lic INNER JOIN
	                         ABONUK.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN-ass.Pay-ass.PayODN) as SumSaldo
		                        from ABONUK.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge) as SumCharge
		                        from ABONUK.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABONUK.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code = 6) zPeni ON zPeni.lic = a.Lic INNER JOIN
	                         ABONUK.dbo.Street s ON s.cod_yl = a.Str_code
                        WHERE ((a.vvodomer = 1 or a.vodkod <> 0) and sv.bPaketC = 1 and a.gruppa3 = 0)
                         --   and not a.lic IN (SELECT a.lic FROM Abonuk.dbo.abonent202007 a inner join Abonuk.dbo.abonent202008 b on a.Lic = b.Lic WHERE a.KodVedom <> b.KodVedom)
                        ", CurrPer, PredPer) + SQLTextAdres;
            // ------------------- новоперешедшие дома под прямое управление нормы -------------------
            string SQLTextNorma = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{0}',2)+SUBSTRING('{0}',1,4) as BPer, 
	                          null as H1, null as H2, null as H3, null as G1, null as G2, null as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              null as H1Date, null as H2Date, null as H3Date, null as G1Date, null as G2Date, null as G3Date, a.dom, a.flat, 1 as t1
                        FROM ABON.dbo.abonent{0} AS a INNER JOIN
	                         ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN) as SumSaldo
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge-ass.Pay-ass.PayODN) as SumCharge
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code=6) zPeni ON zPeni.lic=a.Lic INNER JOIN
	                         ABON.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE (a.vvodomer = 0 and a.vodkod = 0) and a.lic IN (SELECT b.Lic FROM Abon.dbo.abonent202008 a RIGHT JOIN abon.dbo.abonent202009 b ON a.lic = b.lic
                                                                               WHERE (a.KodVedom <> b.kodvedom  or a.Lic is NULL) and b.KodVedom = 231)
                        ", CurrPer) + SQLTextAdres;

            // ------------------- ABON с водомерами (без перешедших в прямые в этом месяце) ------------------- 
            string SQLTextVodomer = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom+(CASE WHEN a.flat='' THEN '' ELSE ', '+isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo+zCharge.SumCharge+zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{1}',2)+SUBSTRING('{1}',1,4) as BPer,
	                          pv.KubH1n as H1, pv.KubH2n as H2, pv.KubH3n as H3, pv.KubG1n as G1, pv.KubG2n as G2, pv.KubG3n as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              v.H1 as H1Date, v.H2 as H2Date, v.H3 as H3Date, v.G1 as G1Date, v.G2 as G2Date, v.G3 as G3Date, a.dom, a.flat, 2 as t1
                        FROM ABON.dbo.abonent{0} AS a INNER JOIN
                             ABON.dbo.VodomerDate{0} v ON v.Lic=a.Lic INNER JOIN
	                         ABON.dbo.MaxPosVod pv on pv.Lic=a.Lic INNER JOIN
	                         ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO+ass.SaldoODN-ass.Pay-ass.PayODN) as SumSaldo
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge+ass.CorrCharge+ass.ChargeODN+ass.CorrODNCharge) as SumCharge
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO+ass.Charge+ass.CorrCharge-ass.Pay as SumPeni
		                        from ABON.dbo.AbonServSaldo{1} AS ass
		                        where ass.AbonServ_Code=6) zPeni ON zPeni.lic=a.Lic INNER JOIN
	                         ABON.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE (a.vvodomer = 1 or a.vodkod <> 0) and (a.lic IN (SELECT b.Lic FROM Abon.dbo.abonent202008 a RIGHT JOIN abon.dbo.abonent202009 b ON a.lic = b.lic
                                                                               WHERE (a.KodVedom <> b.kodvedom  or a.Lic is NULL) and b.KodVedom = 231))
                        ", CurrPer, CurrPer) + SQLTextAdres;



        // ABON без водомеров Родзевича-Белевича 7
        string SQLText21 = String.Format(@"SELECT '' FIO, s.yl_name+', ' + a.dom + (CASE WHEN a.flat = '' THEN '' ELSE ', ' + isnull(a.flat,'') END) as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                        zSaldo.SumSaldo + zCharge.SumCharge + zPeni.SumPeni AS SUMMA,
	                        RIGHT ('{0}', 2) + SUBSTRING('{0}', 1, 4) as BPer,
	                        pv.KubH1n as H1, pv.KubH2n as H2, pv.KubH3n as H3, pv.KubG1n as G1, pv.KubG2n as G2, pv.KubG3n as G3,
	                        zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                            v.H1 as H1Date, v.H2 as H2Date, v.H3 as H3Date, v.G1 as G1Date, v.G2 as G2Date, v.G3 as G3Date, a.dom, a.flat, 7 as t1
                    FROM ABON.dbo.abonent{0} AS a INNER JOIN
                            ABON.dbo.VodomerDate{0} v ON v.Lic = a.Lic INNER JOIN
	                        ABON.dbo.MaxPosVod pv on pv.Lic = a.Lic INNER JOIN
	                        ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                        -- Сальдо (задолженность на начало периода)
	                        (select '1' + RIGHT(lic,9) as lic, SUM(SALDO + SaldoODN - Pay - PayODN) as SumSaldo
		                    from ABONuk.dbo.AbonServSaldo{0} 
		                    where AbonServ_Code <> 6
		                    group by lic) zSaldo ON zSaldo.lic = a.Lic INNER JOIN
	                        -- Начисления тек.периода (кроме пени)
	                        (select '1' + RIGHT(lic,9) as lic, SUM(Charge + CorrCharge + ChargeODN + CorrODNCharge) as SumCharge
		                    from ABONuk.dbo.AbonServSaldo{0}
		                    where AbonServ_Code <> 6
		                    group by lic) zCharge ON zCharge.lic = a.Lic INNER JOIN
	                        -- Пеня
	                        (select '1' + RIGHT(lic,9) as lic, SALDO + Charge + CorrCharge - Pay as SumPeni
		                    from ABONuk.dbo.AbonServSaldo{0}
		                    where AbonServ_Code = 6) zPeni ON zPeni.lic = a.Lic INNER JOIN
	                        ABON.dbo.Street s ON s.cod_yl = a.Str_code
                    WHERE (a.vvodomer = 1 or a.vodkod <> 0) and a.lic IN (SELECT lic FROM abon.dbo.abonent201902
                        WHERE (str_code = 'Раз1' and dom = '45') or (str_code = 'Гру1' and dom = '1') or (str_code = 'Рощ1' and dom = '37')
                        or (str_code = 'Мос1' and dom = '168') or (str_code = 'Бот1' and dom = '4') or (str_code = 'Лив1' and dom = '33')
                        or (str_code = 'Арт1' and dom = '8') or (str_code = 'Яго1' and dom = '2'))
                    ", CurrPer, PredPer) + SQLTextAdres;

        // -- Долг по Орелжилцентру --
        string SQLText20 = String.Format(@"SELECT '' FIO, s.yl_name + ', ' + a.dom + (CASE WHEN a.flat = '' THEN '' ELSE ', ' + isnull(a.flat,'') END) as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                        zSaldo.SumSaldo + zCharge.SumCharge + zPeni.SumPeni AS SUMMA,
	                        RIGHT ('{0}', 2) + SUBSTRING('{0}', 1, 4) as BPer, 
	                        null as H1, null as H2, null as H3, null as G1, null as G2, null as G3,
	                        zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                            null as H1Date, null as H2Date, null as H3Date, null as G1Date, null as G2Date, null as G3Date, a.dom, a.flat, 8 as t1
                    FROM ABON.dbo.abonent{0} AS a INNER JOIN
	                        ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                        -- Сальдо (задолженность на начало периода)
	                        (select ass.lic, SUM(ass.SALDO + ass.SaldoODN) as SumSaldo
		                    from ABON.dbo.AbonServSaldo{0} AS ass
		                    where ass.AbonServ_Code <> 6
		                    group by ass.lic) zSaldo ON zSaldo.lic = a.Lic INNER JOIN
	                        -- Начисления тек.периода (кроме пени)
	                        (select ass.lic, SUM(ass.Charge + ass.CorrCharge + ass.ChargeODN + ass.CorrODNCharge - ass.Pay - ass.PayODN) as SumCharge
		                    from ABON.dbo.AbonServSaldo{0} AS ass
		                    where ass.AbonServ_Code <> 6
		                    group by ass.lic) zCharge ON zCharge.lic = a.Lic INNER JOIN
	                        -- Пеня
	                        (select ass.lic, ass.SALDO + ass.Charge + ass.CorrCharge - ass.Pay as SumPeni
		                    from ABON.dbo.AbonServSaldo{0} AS ass
		                    where ass.AbonServ_Code = 6) zPeni ON zPeni.lic = a.Lic INNER JOIN
	                        ABON.dbo.Street s ON s.cod_yl = a.Str_code
                    WHERE (a.vvodomer = 0 and a.vodkod = 0) and a.lic IN (SELECT lic FROM abon.dbo.abonent201902
                        WHERE (str_code = 'Раз1' and dom = '45') or (str_code = 'Гру1' and dom = '1') or (str_code = 'Рощ1' and dom = '37')
                        or (str_code = 'Мос1' and dom = '168') or (str_code = 'Бот1' and dom = '4') or (str_code = 'Лив1' and dom = '33')
                        or (str_code = 'Арт1' and dom = '8') or (str_code = 'Яго1' and dom = '2'))
                    ", CurrPer) + SQLTextAdres;

        string SQLText25 = String.Format(@"SELECT '' FIO, s.yl_name+', '+a.dom + (CASE WHEN a.flat = '' THEN '' ELSE ', ' + isnull(a.flat,'') END)as Adr, CAST(a.LIC AS nvarchar(10)) AS LIC, 
	                          zSaldo.SumSaldo + zCharge.SumCharge + zPeni.SumPeni AS SUMMA,
	                          RIGHT ('{0}',2) + SUBSTRING('{0}',1,4) as BPer, 
	                          null as H1, null as H2, null as H3, null as G1, null as G2, null as G3,
	                          zSaldo.SumSaldo, zCharge.SumCharge, zPeni.SumPeni,
                              null as H1Date, null as H2Date, null as H3Date, null as G1Date, null as G2Date, null as G3Date, a.dom, a.flat, 9 as t1
                        FROM ABON.dbo.abonent{0} AS a INNER JOIN
	                         ABON.dbo.SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN
	                         -- Сальдо (задолженность на начало периода)
	                         (select ass.lic, SUM(ass.SALDO + ass.SaldoODN) as SumSaldo
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zSaldo ON zSaldo.lic=a.Lic INNER JOIN
	                         -- Начисления тек.периода (кроме пени)
	                         (select ass.lic, SUM(ass.Charge + ass.CorrCharge + ass.ChargeODN + ass.CorrODNCharge - ass.Pay - ass.PayODN) as SumCharge
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code<>6
		                        group by ass.lic) zCharge ON zCharge.lic=a.Lic INNER JOIN
	                         -- Пеня
	                         (select ass.lic, ass.SALDO + ass.Charge + ass.CorrCharge - ass.Pay as SumPeni
		                        from ABON.dbo.AbonServSaldo{0} AS ass
		                        where ass.AbonServ_Code=6) zPeni ON zPeni.lic=a.Lic INNER JOIN
	                         ABON.dbo.Street s ON s.cod_yl=a.Str_code
                        WHERE a.lic in (select lic from abon.dbo.abonent{0} where (str_code = 'Дуб1' and dom = '62'))
                        ", CurrPer) + SQLTextAdres;


//            SQLTextAll = SQLText1 + " UNION ALL " + SQLText2 + " UNION ALL " + SQLText3 + " UNION ALL " + SQLText4 + " order by adr";
            SQLTextAll = SQLTextNorma + " UNION ALL " + SQLTextVodomer + " order by adr";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = frmMain.db_con;
            cmd.CommandText = SQLTextAll;
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("@New", SqlDbType.Bit).Value = 1;
            #endregion

            List<String> CD = new List<String>();
            cmd.CommandTimeout = 0;
            Int32 SizeOfFile = 0;
            Int32 NumberOfFile = 1;
            using (SqlDataReader rsNach = cmd.ExecuteReader())
            {
                if (rsNach.HasRows)
                {
                    while (rsNach.Read())
                    {
                        //;Приборостроительная, 10, 1;1000064000;-127.96;042017;:[!]:1:Задолженность на начало периода:-127.96:2:Начисления текущего периода:0.00:3:Пеня за просрочку:0.00
                        //;Приборостроительная, 10, 2;1000064100;223.76;042017;1:Х1(кух):587:2:Х2(c/у):0:3:Х3:0:4:Г1(кух):460:5:Г2(c/у):0:6:Г3:0:[!]:1:Задолженность на начало периода:0.00:2:Начисления текущего периода:223.76:3:Пеня за просрочку:0.00

                        // считываем суммы
                        SumSaldo = Convert.ToDecimal(rsNach["SumSaldo"]);
                        SumCharge = Convert.ToDecimal(rsNach["SumCharge"]);
                        SumPeni = Convert.ToDecimal(rsNach["SumPeni"]);
                        Summa = Convert.ToDecimal(rsNach["Summa"]);

                        // обрабатываем в соответствии с условием Сбербанка:
                        // Минусовых сумм не должно быть, это значит переплата у плательщика, следовательно сумма должна быть ноль
                        ResSvertki = "";
                        if (Summa <= 0)
                        {
                            Summa = SumSaldo = SumCharge = SumPeni = 0;
                            ResSvertki = "отриц";
                        }
                        if (Summa > 0)
                        {
                            // 1) свертка Saldo+Charge и Peni
                            if (((SumSaldo + SumCharge) < 0 && SumPeni >= 0) || ((SumSaldo + SumCharge) >= 0 && SumPeni < 0))
                            {
                                Razn = SumSaldo + SumPeni;
                                SumPeni = 0;
                                SumSaldo = Razn;
                                ResSvertki = "свертка1";
                            }
                            // 2) свертка Saldo и Charge
                            if ((SumSaldo < 0 && SumCharge >= 0) || (SumSaldo >= 0 && SumCharge < 0))
                            {
                                Razn = SumSaldo + SumCharge;
                                SumCharge = 0;
                                SumSaldo = Razn;
                                ResSvertki = "свертка2";
                            }
                            // 3) свертка Saldo и Peni
                            if ((SumSaldo < 0 && SumPeni >= 0) || (SumSaldo >= 0 && SumPeni < 0))
                            {
                                Razn = SumSaldo + SumPeni;
                                SumPeni = 0;
                                SumSaldo = Razn;
                                ResSvertki = "свертка3";
                            }
                            // 4) контроль свертки
                            if (Summa != (SumSaldo + SumCharge + SumPeni) || (SumSaldo < 0) || (SumCharge < 0) || (SumPeni < 0))
                            {
                                ResSvertki = "BAD";
//                                MessageBox.Show("Ошибка при свёртке минусовых сумм! Формирование реестра прекращено. ЛС=" + rsNach["lic"].ToString(), "Ошибка");
                                break;
                            }
                        }
                        // блок-шапка
                        BlockHeader = rsNach["FIO"].ToString() + Razd1 + rsNach["adr"].ToString() + Razd1 + rsNach["lic"].ToString() + Razd1 +
                                        Convert.ToString(Summa).Replace(',', '.') + Razd1 + rsNach["bper"].ToString() + Razd1;
                        // блок счетчиков
                        NumVodomer = 0;
                        // для абонентов без счетчиков блок счетчиков будет пустой
                        if (rsNach["H1"] == DBNull.Value)
                        {
                            BlockVodomer = Razd2 + RazdBlock;
                        }
                        else
                        {
                            // для абонентов со счетчиками блок счетчиков формируется динамически
                            // т.е. показывать только счетчики, которые реально есть у абонента ()
                            if (rsNach["H1Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer1 = NumVodomer.ToString() + Razd2 + "Х1(кух)" + Razd2 + rsNach["H1"].ToString() + Razd2; }
                            else { BlockVodomer1 = ""; }
                            if (rsNach["H2Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer2 = NumVodomer.ToString() + Razd2 + "Х2(c/у)" + Razd2 + rsNach["H2"].ToString() + Razd2; }
                            else { BlockVodomer2 = ""; }
                            if (rsNach["H3Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer3 = NumVodomer.ToString() + Razd2 + "Х3" + Razd2 + rsNach["H3"].ToString() + Razd2; }
                            else { BlockVodomer3 = ""; }
                            if (rsNach["G1Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer4 = NumVodomer.ToString() + Razd2 + "Г1(кух)" + Razd2 + rsNach["G1"].ToString() + Razd2; }
                            else { BlockVodomer4 = ""; }
                            if (rsNach["G2Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer5 = NumVodomer.ToString() + Razd2 + "Г2(c/у)" + Razd2 + rsNach["G2"].ToString() + Razd2; }
                            else { BlockVodomer5 = ""; }
                            if (rsNach["G3Date"] != DBNull.Value)
                            { NumVodomer++; BlockVodomer6 = NumVodomer.ToString() + Razd2 + "Г3" + Razd2 + rsNach["G3"].ToString() + Razd2; }
                            else { BlockVodomer6 = ""; }

                            // общий блок счетчиков
                            BlockVodomer = BlockVodomer1 + BlockVodomer2 + BlockVodomer3 +
                                           BlockVodomer4 + BlockVodomer5 + BlockVodomer6 + RazdBlock;
                        }
                        // блок услуг
                        BlockUsl = "1" + Razd2 + "Долг на начало периода" + Razd2 + Convert.ToString(SumSaldo).Replace(',', '.') + Razd2 +
                                        "2" + Razd2 + "Текущие начисления" + Razd2 + Convert.ToString(SumCharge).Replace(',', '.') + Razd2 +
                                        "3" + Razd2 + "Пеня за просрочку" + Razd2 + Convert.ToString(SumPeni).Replace(',', '.');
                        CD.Add(/*Convert.ToInt32(rsNach["t1"]).ToString() + " " +*/ BlockHeader + BlockVodomer + BlockUsl + ";;1;");
//                        CD.Add(BlockHeader + BlockVodomer + BlockUsl);

//                        lblStatus.Text = rsNach["lic"].ToString();
//                        Application.DoEvents();
                        SizeOfFile = SizeOfFile + CD[CD.Count - 1].Length;
                        if (SizeOfFile > 20000000)
                        {
                            SaveToSber(NumberOfFile, CD);
                            NumberOfFile++;
                            CD.Clear();
                            SizeOfFile = 0;
                        }

                    }
                    if (CD.Count > 0)
                        SaveToSber(NumberOfFile, CD);
                }
                Console.WriteLine(SizeOfFile);
                rsNach.Close();
            }
            //lblStatus.Text = "Выгрузка завершена!";
            //MessageBox.Show("Выгрузка завершена!", "Всё");

        }

        private void SaveToSber(Int32 NumberOfFile, List<string> cd)
        {
            //FileInfo fi = new FileInfo("D:\\5701000368_40702810700400000278_00" + NumberOfFile.ToString() + "_0511.txt");
            //StreamWriter sw = fi.AppendText();
            //foreach (string lists in cd)
            //    sw.WriteLine(lists);
            //sw.Close();
            string MonthDay;

            MonthDay = DateTime.Today.ToString("MMdd");
            using (var sw = new StreamWriter("D:\\1_5701000368_40702810700400000278_" + NumberOfFile.ToString("000") + "_" + MonthDay + ".txt", false, Encoding.GetEncoding(1251)))
            //using (var sw = new StreamWriter("D:\\5701000368_40702810700400000278_002_0511.txt", true, Encoding.GetEncoding(1251)))
            {
                foreach (string lists in cd)
                    sw.WriteLine(lists);
            }

        }
    }
}

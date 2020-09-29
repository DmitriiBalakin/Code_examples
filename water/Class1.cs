using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace water
{
    class Verification
    {
    }
}

/*
Public Sub TestDataBase()
Dim File As String 'Èìÿ ôàéëà
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
        MsgBox "Âû íå ìîæåòå ïðîâîäèòü ïðîâåðêó, ðàáîòàÿ â ýòîì ìåñÿöå!", vbOKOnly, Right(Trim(LastPeriod), 6)
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
    lblTest = "Íà÷àëî òðàíçàêöèè..."
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
    lblTest = "Îòêðûòèå ôàéëà äëÿ çàïèñè..."
    Open txtFullPatch For Output As #1 ' Open file for output.
    Write #1, "Àáîíåíò ÓÊ"

    lblTest = "Ïðîâåðêà èíäåêñîâ..."
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
        Write #1, "Âíèìàíèå: íåâåðíûé èíäåêñ ïî àäðåñó " & rs!Ind & " " & rs!pref_ul & " " & rs!NAME_UL & " ä." & rs!dom; vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

     '--------------Ïðîâåðêà íàëè÷èÿ êîíòðîëåðà---------------------
    lblTest = "Ïðîâåðêà íàëè÷èÿ êîíòðîëåðà..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & "WHERE  kontr=0 AND Lic<>2000888800"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Âíèìàíèå: Ñ÷åò " & !lic & " - Íåèçâåñòåí êîíòðîëåð. Íåîáõîäèìî îïðåäåëèòü, ê êàêîìó êîíòðîëåðó îí îòíîñèòñÿ." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '-------------Ïðîâåðêà íà íàëè÷èå ó êîíòðîëåðîâ ìóíèö. è âåä. æèëüÿ âåäîìñòâà --------
    lblTest = "Ïðîâåðêà íà íàëè÷èå ó êîíòðîëåðîâ ìóíèö. è âåä. æèëüÿ âåäîìñòâà..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "WHERE (Kontr < 10 or Kontr >16) and Kontr<>0 and Kontr<>20 AND " & vbCrLf & _
          "      (Vedom = 'íåò äàííûõ' or KodVedom=57 or KodVedom=0)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ó êîíòðîëåðà ìóíèö. è âåä. æèëüÿ âåäîìññòâî <íåò äàííûõ>." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '----------------3 ãðóïïà, à êîíòðîëåð íå 20
    lblTest = "Ïðîâåðêà íà íàëè÷èå ó êîíòðîëåðîâ ìóíèö. è âåä. æèëüÿ âåäîìñòâà..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & " WHERE (Kontr <> 20) AND (gruppa3 = 1)" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - 3 ãðóïïà, à êîíòðîëåð íå < íàñåëåíèå 3 ãð>." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '----------------ïåðåïóòàíû ïîêàçàíèÿ
    lblTest = "Ïåðåïóòàíû ïîêàçàíèÿ"
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & "äàòà(äàòû) ïîâåðêè è ïîêàçàíèÿ íåñîîòâåòñòâóþò!"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '--------------400---------------------
    lblTest = "400ë è åñòü ÄÑÏ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Íîðìà 12.16 è åñòü äîï.ñàí.ïðèáîð" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    '--------------Ïðîâåðêà íå ïîäàâàòü â  ñóä äî---------------------
    lblTest = "íå ïîäàâàòü â ñóä äî..."
    Me.Refresh
    sql = "DELETE From DateBringCourt WHERE DateBringCourt<CONVERT(datetime, '" + "01." & Format(DataS, "mm.yyyy") + "', 104)"
    DEAbo.admin.Execute sql

        '------------------------Ïðîâåðêà íà÷èñëåíèÿ----------------------
    lblTest = "Ïðîâåðêà áîëüøîãî íà÷èñëåíèÿ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ñëèøêîì áîëüøîå  íà÷èñëåíèå " & !Nachisl & " ðóá. " & _
            " ïðîæ." & !liver & " ÷åë." & IIf(!vvodomer <> 0, " åñòü âîäîìåð, ïîñòóïèëî " & !pos & "", "") & " Äàòà èíâ - " & Nz(rs!md, "íåò") & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '------------------------Ïðîâåðêà îòðèöàòåëüíîãî íà÷èñëåíèÿ.
    lblTest = "Ïðîâåðêà îòðèöàòåëüíîãî íà÷èñëåíèÿ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & sWhere & " nachisl<0" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - îøèáêà: Íà÷èñëåíèå ìåíüøå 0" & vbCrLf
         PB.Value = PB.Value + 1
        .MoveNext
    Wend
    .Close

     '--Ñêâàæèíà, ñëèâíàÿ ÿìà, íåò íà÷èñëåíèÿ è åñòü ïîñòóïëåíèå
     lblTest = "Ñêâàæèíà, ñëèâíàÿ ÿìà, íåò íà÷èñëåíèÿ è åñòü ïîñòóïëåíèå..."
     Me.Refresh
     sql = sSelect & vbCrLf & sFrom & vbCrLf & _
           "       INNER JOIN Pos" & DataSYM & " p ON a.Lic = p.lic" & vbCrLf & _
           "Where (a.skvagina <> 0) And (a.LgKan <> 0) And (a.Nachisl = 0) And (p.opl+p.poliv > 0) AND a.sud=0 AND a.SDolgBeg<=0 " & vbCrLf & _
           "GROUP BY a.Lic,c.fam" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ñêâàæèíà, ñëèâíàÿ ÿìà, íåò íà÷èñëåíèÿ è åñòü ïîñòóïëåíèå" & vbCrLf
         PB.Value = PB.Value + 1
        .MoveNext
    Wend
    .Close

     '---------------------------Ïðîâåðêà ñîäåðæàíèÿ ñåòåé------------------------
    'Óáèðàåì îò 1 ÷èñëà òåê. ìåñÿöà è íèæå
    lblTest = "Ïðîâåðêà ñîäåðæàíèÿ ñåòåé..."
    Me.Refresh
    sql = "SELECT a.*, c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " sodset<>0 and socdo<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104)" & vbCrLf & _
          sOrder
    .Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Çàêîí÷èëîñü ñîäåðæàíèå ñåòåé" & vbCrLf
        !sodset = 0
        !socdo = Null
        !Sodsetdn = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '--------------------Íà÷àëîñü ñîäåðæàíèå ñåòåé ïî ñ÷¸òó
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
                Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Íà÷àëîñü ñîäåðæàíèå ñåòåé" & vbCrLf
                !sodset = 1
            End If
            Nach.Nach rs, True
        End If
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

        '------------------Åñòü âîäîìåð è íîðìà âûøå ñîöèàëüíîé------------------------
    lblTest = "Åñòü âîäîìåð è íîðìà âûøå ñîöèàëüíîé..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & " (a.Vvodomer <> 0) AND (vd.NormaV > 12.16 OR vd.NormaK > 12.16)" & vbCrLf & _
          sOrder
    .Open sql
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Åñòü âîäîìåð è íîðìà âûøå ñîöèàëüíîé. "
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

 '------------Ïðîâåðêà íà ïóñòóþ çàïèñü â ñóäå
    lblTest = "Ïðîâåðêà íà ïóñòóþ çàïèñü â ñóäå..."
    Me.Refresh
    sql = "SELECT Lic, RegNum From SudReestrT WHERE IskSumma = 0"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        '31,01,2007  åñëè íå ïîíàäîáèòñÿ âûâîäèòü íà ýêðàí è â äàëüíåéøåì, òî
        ' íàäî çàìåíèòü íà DEAbo.admin.Execute "DELETE FROM [SudReestrT]  WHERE IskSumma = 0"
        'Write #1, "Âíèìàíèå: Ñ÷åò " + str(!lic) + " Ïîäàíî â ñóä, à ñóììà èñêà = 0. Çàïèñü óäàëåíà." + vbCrLf
        DEAbo.admin.Execute "DELETE FROM SudReestrT  WHERE lic=" & !lic & " and RegNum= " + Replace(!RegNum, ",", ".")
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

     '------------Íàëè÷èå äâóõ âçàèìîèñêëþ÷àþõ óñëóã ïî ïîëèâó
    lblTest = "Íàëè÷èå äâóõ âçàèìîèñêëþ÷àþõ óñëóã ïî ïîëèâó..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - äâå óñëóãè ïî ïîëèâó Îòêðûòûé ãðóíò, âîäîñíàáæåíèå èç óëè÷íîé è èç âîäîçàáîðíîé êîëîíêè" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '----------Ïåðèîä ïîñòóïëåíèÿ áîëüøå 3 ëåòíåé äàâíîñòè
    lblTest = "Ïåðèîä ïîñòóïëåíèÿ áîëüøå 3 ëåòíåé äàâíîñòè..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN pos" & DataSYM & " AS p ON p.lic=a.lic" & vbCrLf & _
          "WHERE  cast(left(peropl,4) as int)<" & CInt(Left(DataSYM, 4)) - 3 & vbCrLf & _
          "GROUP BY a.lic,c.fam" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " ïåðèîä ïîñòóïëåíèÿ áîëüøå 3 ëåòíåé äàâíîñòè." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '------------------------Ãîðÿ÷àÿ âîäà íå íàøà
    lblTest = "Ãîðÿ÷àÿ âîäà ïî âîäîñíàáæåíèþ íå íàøà ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íîðìû ïî âîäå ðàçëè÷àþòñÿ íî ãàëêà """" Ãîðÿ÷àÿ âîäà ïî âîäîñíàáæåíèþ íå íàøà """" íå ñòîèò " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '--- âîññòàíîâèòü íîðìó ìåíüøå òåêóùåé
    lblTest = "Âîññòàíîâèòü íîðìó"
    Me.Refresh
    sql = sSelect & ", TempNorm, TempNormDate, N_Vl, N_Kl " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.Vvodomer=0  And a.VodKod=0 AND ((N_Vl > TempNorm) AND (TempNorm > 0) OR " & _
          "            (N_Kl >= TempNorm) AND (TempNorm > 0))"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Åñòü íîðìà âîäà " & !N_VL & " ,êàíàëèçàöèÿ " & !n_kl & " è íîðìà äëÿ âîññòàíîâëåíèÿ " & !TempNorm
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    '---------------ïðè èäèâ. âîäîìåðå åñòü ïîñòóïëåíèå  è íåò íà÷èñëåíèé
    lblTest = "ïðè èíäèâèä. âîäîìåðå åñòü ïîñòóïëåíèå  è íåò íà÷èñëåíèé..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Åñòü èíäèâèä. âîäîìåð,â òåêóùåì ïåðèîäå åñòü ïîñòóïëåíèÿ íî íåò íà÷èñëåíèÿ" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
     
     '--------------Ïðîâåðêà âðåìåííûõ íîðì---------------------
    lblTest = "Ïðîâåðêà âðåìåííûõ íîðì..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
         sWhere & "Prochee=0 AND  TempNorm>0 and TempNormDate<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104) And vvodomer=0" & vbCrLf & _
         sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Âîññòàíîâëåíû íîðìû , " + !fam + ", " + !yl_name + ", " + !dom + _
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
    '--Óäàëÿåì Íåïðîæèâàþò ÄÎ
    lblTest = "Óáðàíî íåïðîæèâàþò äî"
    Me.Refresh
    sql = sSelect & ", a.Liver,a.NotLive,ab.dtNotLive" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (a.NotLive = 1) AND (ab.dtNotLive < CONVERT(DATETIME, '01." & Format(DateAdd("m", 1, DataS), "mm.yyyy") & "', 104)) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Óáðàíî íå ïðîæèâàþò ÄÎ." + vbCrLf
        !NotLive = 0
        !dtNotLive = Null
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    '--Óäàëÿåì ïðî÷åå ÄÎ
    lblTest = "Óáðàíî ïðî÷åå ÄÎ..."
    Me.Refresh
    sql = sSelect & ", a.Prochee,ab.dtOther" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (Prochee = 1) AND (dtOther < CONVERT(DATETIME, '01." & Format(DateAdd("m", 1, DataS), "mm.yyyy") & "', 104)) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Óáðàíî ïðî÷åå ÄÎ." + vbCrLf
        !Prochee = 0
        !dtOther = Null
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    
    '--------------Åñëè åñòü íà÷èñëåíèå òî óäàëÿåì ïðî÷åå---------------------
    lblTest = "Óáðàíî ïðî÷åå..."
    Me.Refresh
    sql = sSelect & ", a.Prochee" & vbCrLf & sFrom & vbCrLf & _
          " INNER JOIN Abon ab ON ab.lic=a.lic AND ab.perend=0 " & vbCrLf & _
          sWhere & "(Nachisl > 0) AND (Prochee = 1) AND (dtOther IS NULL) " & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Óáðàíî ïðî÷åå." + vbCrLf
        !Prochee = 0
        .Update
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
   
    
    lblTest = "Ïðîâåðêà âðåìåííûõ íîðì..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          sWhere & "Prochee<>0 AND TempNorm>0 and TempNormDate<=convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104) And vvodomer=0" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Óáðàíî ïðî÷åå Âîññòàíîâëåíû íîðìû, " + !fam + ", " + !yl_name + ", " + !dom + _
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
    
    lblTest = "Óñòàíîâëåí âîäîìåð, âîññòàíîâèòü íîðìó"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " (TempNorm > 0) AND (Vvodomer = 1) AND (TempNormDate < convert(datetime,'" + "01." & Format(DataS, "mm.yyyy") + "',104))" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Óñòàíîâëåí âîäîìåð, âîññòàíîâèòü íîðìó"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
       '-----------------Ïðîâåðêà âðåìåííî âûáûâøèõ-------------------------
   'Óáèðàåì îò 1 ÷èñëà òåê. ìåñÿöà è íèæå
    lblTest = "Ïðîâåðêà âðåìåííî âûáûâøèõ..."
    Me.Refresh
    sql = "SELECT a.*, s.yl_name,c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN street AS s ON a.Str_code = s.cod_yl " & vbCrLf & _
          sWhere & "Vibilo<>0 and VibiloData<=convert(datetime,'" + "15." & Format(DataS, "mm.yyyy") + "',104)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
'''        Write #1, " Âîññòàíîâëåíî êîëè÷åñòâî ïðîæèâàþùèõ ïî ñ÷åòó "  & !lic &  " " + !fam + " " + !yl_name + " " + !Dom + _
'''                " " + !Flat + " êîíòðîëåð " + !FIO + vbCrLf
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
'''        Write #1, " Âîññòàíîâëåíî êîëè÷åñòâî ïðîæèâàþùèõ ïî ñ÷åòó "  & !lic &  " " + !fam + " " + !yl_name + " " + !Dom + _
'''                " " + !Flat + " êîíòðîëåð " + !FIO + vbCrLf
        !vibilo2 = 0
        !VibiloData2 = Null
        Nach.Nach rs, True
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    '------------------------------------------------
    lblTest = "Ïðîâåðêà âðåìåííûõ íîðì (ñðûâ ïëîìáû)..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Áûë âîäîìåð è  âîññòàíîâëåíû íîðìû, " & !fam & ", " & !yl_name & ", " & !dom & _
                " / " & !flat & " òåë.: " & !numtel & ", èíâåíòàðèçàöèÿ: " & !DateInv & vbCrLf
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
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " ïîñëå âîññòàíîâëåíèÿ íîðìû íà÷èñëåíèå è êîë-âî ïðîæ. = 0 " & vbCrLf
        End If
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
   lblTest = "Íåò âîäîìåðà íà ïîëèâ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          sWhere & " (NOT (vd.H3 IS NULL)) AND (a.Pvodomer = 0) AND (a.Kontr > 9) AND (a.Kontr < 17) AND a.VVodomer=1 and vd.Gl=0" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íåò ãàëêè âîäîìåð íà ïîëèâ è åñòü äàòà ïîâåðêè Õ3 " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
        
    lblTest = "Ïîñòàâèòü ãàëêó ñëèâíàÿ ÿìà"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & sWhere & _
    " (a.Kontr > 9 AND a.Kontr < 17) AND a.vvodomer=0 AND LgKan = 0 AND N_Kl=0 AND N_Vl <> 0"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íåò îòìåòêè Ñëèâíàÿ/âûãðåáíàÿ ÿìà " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
        '-------------------------------------
    lblTest = "ÄÀòà ïîâåðêè îòñóòñòâóåò èëè óñòàðåëà"
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT OUTER JOIN  VodomerDate" & Conf!PerCur & " AS vd ON a.Lic = vd.Lic " & vbCrLf & _
          sWhere & " a.gruppa3=0 AND a.fam<>'Ó' AND (a.Vvodomer = 1) AND  (sv.bPaketC = 1) AND ( " & vbCrLf & _
          "  ((vd.H1 IS NULL) AND (vd.H2 IS NULL) AND (vd.H3 IS NULL) AND " & vbCrLf & _
          "(vd.G1 IS NULL) AND (vd.G2 IS NULL) AND (vd.G3 IS NULL)))" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Äàòà(äàòû) ïîâåðêè îòñóòñòâóþò"
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Íîðìàòèâ 1.52 "
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.vvodomer=0 AND N_Vl = 1.52 AND N_KL=1.52" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íîðìàòèâ ïî âîäå è êàí-öèè 1.52" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
    lblTest = "Íîðìàòèâ 1.52 "
    Me.Refresh
    sql = sSelect & ",N_KL " & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.vvodomer=0 AND N_Vl = 1.52 AND N_KL>1.52" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íîðìàòèâ ïî âîäå1.52, ïî êàí-öèè " & !n_kl & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close

    lblTest = "Îòñóòñòâóåò íîðìàòèâ ïî âîäîìåðó "
    Me.Refresh
    sql = sSelect & ",N_KL " & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & " (a.Vvodomer = 1) And (a.gruppa3 = 0) AND (SV.bPaketC = 1) And (vd.NormaV + vd.NormaK = 0)" & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " îòñóòñòâóåò íîðìàòèâ ïî âîäîìåðó " & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
 
    lblTest = "Ïðîâåðêà äàòû ïîâåðêè âîäîñ÷åò÷èêîâ..."
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
    
    lblTest = "Ïðîâåðêà äàòû ïîâåðêè îáùèõ âîäîñ÷åò÷èêîâ..."
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
    
 '----------Ïîñòóïëåíèÿ ïî êóáàì ñ èñòåêøèì ñðîêîì ïîâåðêè
    lblTest = "Ïîñòóïëåíèÿ ïî êóáàì ñ èñòåêøèì ñðîêîì ïîâåðêè..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "INNER JOIN abonent" & strPrePer & " AS aa ON aa.Lic = a.Lic" & vbCrLf & _
          "INNER JOIN MaxPosVod AS mp ON a.Lic=mp.Lic " & vbCrLf & _
          sWhere & " (a.Vvodomer = 0) And (aa.Vvodomer = 1) And cast('20'+mp.PerOpl as int)<>mp.LastPer AND mp.LastPer=" & strPrePer & vbCrLf & sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Åñòü ïîñòóïëåíèÿ ïî êóáàì, íî ñðîê ïîâåðêè èñòåê. Äîíà÷èñëèòü ïðåäûäóùèé ìåñÿö" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    .Close
    
'----íà íà÷àëî ìåñÿöà
    If chkBegMonth.Value = 1 Then
        lblTest = "êâèòàíöèè ..."
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
            Write #1, !lic & " Ñòîðíî ðàâíî íà÷èñëåíèþ, ïðîâåðüòå èòîãîâóþ ñóììó â êâèòàíöèè" + vbCrLf
            PB.Value = PB.Value + 1
            .MoveNext
        Wend
        .Close
                 
        '---------------------------Ïðîâåðêà ïðîæèâàþùèõ
        lblTest = "Ïðîâåðêà ïðîæèâàþùèõ ..."
        Me.Refresh
        sql = "SELECT a.*, c.fam AS cFIO " & vbCrLf & sFrom & vbCrLf & _
              "INNER JOIN abonent" & strPrePer & " AS aa ON aa.Lic = a.Lic" & vbCrLf & _
              sWhere & "(aa.Liver > 0) AND (a.Liver = 0) AND (a.Fam NOT IN ('Ó', ' ', '.','.Ó','Ó.','Ï')) and " & vbCrLf & _
              " a.Gruppa3=0 and a.prochee=0 AND a.Vibilo+a.Vibilo2>=0" & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenStatic, adLockBatchOptimistic
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " Ïðîæèâàþùèõ ñòàëî 0 è íåò âðåì ïðèá" + vbCrLf
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
            
            '-----------------------Ïðîâåðêà íóëåâîãî íà÷èñëåíèÿ
        lblTest = "Ïðîâåðêà íóëåâîãî íà÷èñëåíèÿ..."
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
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " " & !fam & " " & !yl_name & " " & !dom & " " & !flat & " Íà÷èñëåíèå=0, âîññòàíèòü íà÷èñëåíèå." & vbCrLf
             PB.Value = PB.Value + 1
            .MoveNext
        Wend
        .Close
            
        lblTest = "Íåò âîäîìåðà è íîðìà íå ñîîòâåòñâóåò ñàíïèíó ..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
              sWhere & " Vvodomer = 0 AND Pvodomer = 0 AND VodKod = 0 AND" & vbCrLf & _
        "    ((N_Vl not in (0,1.52,1.824,2.28,3.648,4.56,5.472,7.144,7.6,9.12,12.16,5.933,4.644,3.56,2.967)) OR" & vbCrLf & _
        "     (N_Kl not in (0,1.52,1.824,2.28,3.648,4.56,5.472,7.144,7.6,9.12,12.16,5.933,4.644,3.56,2.967)))" & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " íåò ãàëêè âîäîìåðà è íîðìà íå ñîîòâåòñâóåò ñàíïèíó " + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        .Close
        
        lblTest = "èñòåê ñðîê ïîâåðêè ó â\ñ è íåò ïîñëåäíèõ ïîêàçàíèé ..."
        Me.Refresh
        sql = sSelect & vbCrLf & sFrom & vbCrLf & _
              "     INNER JOIN abonent" & strPrePer & " AS aa ON a.Lic = aa.Lic " & vbCrLf & _
              "     LEFT OUTER JOIN MaxPosVod AS mpv ON a.Lic = mpv.Lic" & vbCrLf & _
              sWhere & " a.Vvodomer = 0 AND aa.Vvodomer = 1 AND ISNULL(mpv.LastPer," & strPrePer & ")<" & strPrePer & vbCrLf & sOrder
        rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
        PB.Max = .RecordCount + 1
        PB.Value = 0
        While Not .EOF
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - èñòåê ñðîê ïîâåðêè ó â\ñ è íåò ïîñëåäíèõ ïîêàçàíèé" + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        rs.Close

            '---------------ïðè îáùåì âîäîìåðå åñòü ïîñòóïëåíèå  è íåò íà÷èñëåíèé
        lblTest = "ïðè îáùåì âîäîìåðå åñòü ïîñòóïëåíèå  è íåò íà÷èñëåíèé..."
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
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Åñòü îáùèé âîäîìåð,â òåêóùåì ïåðèîäå åñòü ïîñòóïëåíèÿ íî íåò íà÷èñëåíèÿ" + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        .Close
    End If
    
    lblTest = "îòíîñèòñÿ ê Æèëüþ, íåò äîëãà, íî åñòü îïëàòà..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - îòíîñèòñÿ ê Æèëüþ, íåò äîëãà, íî åñòü îïëàòà." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
       
    If CInt(Right(DataSYM, 2)) >= 3 And CInt(Right(DataSYM, 2)) <= 9 Then
        lblTest = "Çàêîí÷èëñÿ ñðîê ïîâåðêè Â\Ñ è ó àáîíåíòà åñòü ñêâàæèíà..."
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
            Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Çàêîí÷èëñÿ ñðîê ïîâåðêè Â\Ñ è ó àáîíåíòà åñòü ñêâàæèíà." + vbCrLf
            .MoveNext
            PB.Value = PB.Value + 1
        Wend
        rs.Close
    End If
   
    lblTest = "Îòñóòñòâóåò âîäîìåð, íî äàòà ïîâåðêè áîëüøå òåêóùåãî ìåñÿöà ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN VodomerDate" & DataSYM & " AS vd ON a.Lic = vd.Lic" & vbCrLf & _
          sWhere & "a.Vvodomer=0 AND a.Pvodomer=0 AND a.Nachisl=0 AND a.Vodkod=0 AND a.Fam<>'Ó' AND a.Fam<>'ó' AND " & vbCrLf & _
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - îòñóòñòâóåò âîäîìåð, íî äàòà ïîâåðêè áîëüøå òåêóùåãî ìåñÿöà." + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

'''    lblTest = "Óäàëåíèå ëèöåâûõ..."
'''    Me.Refresh
'''    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
'''          "LEFT OUTER JOIN" & vbCrLf & _
'''          "    (SELECT SUM(abs(opl) + abs(poliv)) AS pos, lic" & vbCrLf & _
'''          "     FROM Pos" & DataSYM & vbCrLf & _
'''          "     GROUP BY lic) AS p ON a.Lic = p.lic" & vbCrLf & _
'''          "WHERE (a.Fam = 'Ó' OR a.Fam = 'ó') AND" & vbCrLf & _
'''          "       a.SDolgBeg+a.Nachisl+Poliv+a.Oplata-ISNULL(p.pos,0)- a.Spisan=0 AND" & vbCrLf & _
'''          "       a.sndeb-a.snkred+a.oplata-a.spisan-a.pos+a.Nachisl+a.Poliv=0 AND" & vbCrLf & _
'''          "       a.pos=0 AND a.Oplata =0 AND a.Spisan =0 AND a.Poliv=0 AND ISNULL(p.pos,0)=0 AND" & vbCrLf & _
'''          "       a.SDolgBeg=0 AND a.sndeb=0 AND a.snkred=0    " & vbCrLf & sOrder
'''    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
'''    PB.Max = .RecordCount + 1
'''    PB.Value = 0
'''    While Not .EOF
'''        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Óäàëåí." + vbCrLf
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
''        "WHERE (a.Fam = 'Ó' OR a.Fam = 'ó') AND" & vbCrLf & _
''        "       a.SDolgBeg+a.Nachisl+Poliv+a.Oplata-ISNULL(p.pos,0)- a.Spisan=0 AND" & vbCrLf & _
''        "       a.sndeb-a.snkred+a.oplata-a.spisan-a.pos+a.Nachisl+a.Poliv=0 AND" & vbCrLf & _
''        "       a.pos=0 AND a.Oplata =0 AND a.Spisan =0 AND a.Poliv=0 AND ISNULL(p.pos,0)=0 AND" & vbCrLf & _
''        "       a.SDolgBeg=0 AND a.sndeb=0 AND a.snkred=0"
''    DEAbo.admin.Execute sql
    sql = ""
    
    lblTest = "ó àáîíåíòà íîðìà íå ñîâïàäàåò ñ íîðìîé íà äîì ..."
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
        "                  (sv.bPaketC = 1) AND a.Fam <> 'Ó' AND a.Fam <> 'ó' AND a.Fam <> 'Y' AND a.Fam <> 'y' AND" & vbCrLf & _
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - íîðìà íå ñîâïàäàåò ñ íîðìîé íà äîì " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "ïîñòàâëåíà ãàëêà âîäîìåð â òåê ìåñÿöå è ñðîê ïîâåðêè èñòå÷åò â òîì æå ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - ïîñòàâëåíà ãàëêà âîäîìåð â òåê ìåñÿöå è ñðîê ïîâåðêè èñòå÷åò â òîì æå " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    lblTest = "ïî îäíîìó äîìó óñòàíîâëåíû ðàçíûå âåäîìñòâà ..."
    Me.Refresh
    sql = "EXEC [dbo].[Get2Ved] '" & DataSYM & "'"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - óñòàíîâëåíû ðàçíûå âåäîìñòâà â îäíîì è òîì æå äîìå" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "òåêóùèé ïåðèîä ïîêàçàíèé ìåíüøå ïðåäûäóùåãî ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - òåêóùèé ïåðèîä ïîêàçàíèé ìåíüøå ïðåäûäóùåãî" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "ïîêàçàíèÿ ïðèá. ó÷åòà ïðåä. ïåðèîäà íà êîíåö ìåñ. íå ñîâïàäàåò ñ òåêóùåé ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - ïîêàçàíèÿ ïðèá. ó÷åòà ïðåä. ïåðèîäà íà êîíåö ìåñ. íå ñîâïàäàåò ñ òåêóùåé" + vbCrLf
'        .MoveNext
'        PB.Value = PB.Value + 1
'    Wend
'    rs.Close
    
    lblTest = "ñóäåáíîå ïðîèçâîäñòâî îêí÷åíî, îòêàçàíî ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     INNER JOIN SudReestrT AS st ON a.Lic = st.Lic" & vbCrLf & _
          sWhere & " dbo.DateToPer(st.DataProizKon, 0) = " & DataSYM & " And st.Otkaz = 1" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - ñóäåáíîå ïðîèçâîäñòâî îêí÷åíî, îòêàçàíî" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "â òåêóùåì ïåðèîäå óñòàíîâëåíà ãàëêà âîäîìåð è íå ââåäåíû ïîêàçàíèÿ ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - â òåêóùåì ïåðèîäå óñòàíîâëåíà ãàëêà âîäîìåð è íå ââåäåíû ïîêàçàíèÿ" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "êîë-âî ïðîæèâàþùèõ ìåíüøå 0 ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " a.Liver-a.Vibilo-a.Vibilo2<0 " & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - êîë-âî ïðîæèâàþùèõ ìåíüøå 0" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "åñòü âðåì. ïðèá./óá. íåò  äàòû ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " ((a.Vibilo<>0 AND a.VibiloData IS NULL) OR (a.Vibilo2<>0 AND a.VibiloData2 IS NULL)) " & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - åñòü âðåì. ïðèá./óá. íåò  äàòû" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Äàòà ââîäà áîëüøå äàòû ïîâåðêè ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Äàòà ââîäà áîëüøå äàòû ïîâåðêè" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Íîðìàòèâ ïî ãîð. âîäå è åñòü ñ÷åò÷èêè òîëüêî ïî õîëîäíîé âîäå  ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " àäðåñ " & !yl_name & ", " & !dom & ", " & !flat & " - Íîðìàòèâ ïî ãîð. âîäå è åñòü ñ÷åò÷èêè òîëüêî ïî õîëîäíîé âîäå" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

    lblTest = "Íîðìàòèâ áåç ãîð. âîäû è åñòü ñ÷åò÷èêè è ïî ãîð. âîäå òîæå ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " àäðåñ " & !yl_name & ", " & !dom & ", " & !flat & " - Íîðìàòèâ áåç ãîð. âîäû è åñòü ñ÷åò÷èêè è ïî ãîð. âîäå òîæå" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
   
    lblTest = "Ó àáîíåíòà îáùèé ñ÷åò÷èê, íî äàòû ïîâåðêè è óñòàíîâêè îòñóòñòâóþò ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT JOIN VodomerDate" & DataSYM & " v ON v.Lic=a.Lic " & vbCrLf & _
          sWhere & " (a.VodKod > 0) AND (v.Lic IS NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ó àáîíåíòà îáùèé ñ÷åò÷èê, íî äàòû ïîâåðêè è óñòàíîâêè îòñóòñòâóþò" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Ó àáîíåíòà îáùèé ñ÷åò÷èê, íî îòñóòñòâóþò ïîêàçàíèÿ..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          "     LEFT JOIN PosVod AS p ON a.Lic = p.Lic " & vbCrLf & _
          sWhere & " (a.VodKod > 0) AND (p.Lic IS NULL)" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ó àáîíåíòà îáùèé ñ÷åò÷èê, íî îòñóòñòâóþò ïîêàçàíèÿ" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Àáîíåíò îòíîñèòñÿ ê ..."
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
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - íå âåðíîå âåäîìñòâî ó àáîíåíòà ÎÎÎ " + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Äîñòàâùèê íå ïðèñâîåí ..."
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
        "WHERE (a.Prochee = 0) AND (a.gruppa3 = 0) AND (a.Fam <> N'Ó') AND (sv.bPaketC = 1) AND " & vbCrLf & _
        "      (case when(a.Vvodomer=0 AND a.VodKod=0 ) then a.n_kl ELSE vd.NormaK END +case when(a.Vvodomer=0 AND a.VodKod=0 ) then a.n_vl ELSE vd.NormaV END)<>0" & vbCrLf & _
        "  AND (sd.Id IS NULL)" & vbCrLf & _
        "GROUP BY su.NAME_UL,su.PREF_UL,su.dom, su.idVendor" & vbCrLf & _
        "ORDER BY su.NAME_UL,su.PREF_UL,su.dom"
''    sql = "SELECT su.NAME_UL, a.dom, su.PUNKT, su.PREF_UL" & vbCrLf & _
''           "FROM abonent" & DataSYM & " AS a INNER JOIN" & vbCrLf & _
''           "     SpVedomstvo AS sv ON a.KodVedom = sv.ID INNER JOIN" & vbCrLf & _
''           "     StrUch AS su ON a.Str_code = su.cod_yl AND a.dom = su.dom" & vbCrLf & _
''           "WHERE (su.idVendor = 0) AND (sv.bPaketC = 1) AND a.Fam<>'Ó' AND a.prochee=0 AND " & vbCrLf & _
''           "      ((a.gruppa3=0 and a.skvagina = 0) or (a.skvagina <> 0 AND a.N_Kl <> 0) OR " & vbCrLf & _
''           "      (a.gruppa3<>0 AND a.SDolgEnd > 0))AND a.Str_code<>'Àíò1' AND a.Str_code<>'òìï' " & vbCrLf & _
''           "GROUP BY su.NAME_UL, a.dom, su.PUNKT, su.PREF_UL" & vbCrLf & _
''           "ORDER BY su.PUNKT,su.NAME_UL"
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Ïî àäðåñó " & !NAME_UL & ", " & !dom & ", " & !pref_ul & " äîñòàâùèê íå ïðèñâîåí" & vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close
    
    lblTest = "Ë\ñ÷ ïîìå÷åí íà óäàëåíèå..."
    Me.Refresh
    sql = sSelect & vbCrLf & sFrom & vbCrLf & _
          sWhere & " (a.Fam='Ó' OR a.Fam='ó')" & vbCrLf & _
          sOrder
    rs.Open sql, DEAbo.admin, adOpenForwardOnly, adLockReadOnly
    PB.Max = .RecordCount + 1
    PB.Value = 0
    While Not .EOF
        Write #1, "Êîíòð-ð " & !cFIO & " ñ÷åò " & !lic & " - Ë\ñ÷ ïîìå÷åí íà óäàëåíèå" + vbCrLf
        .MoveNext
        PB.Value = PB.Value + 1
    Wend
    rs.Close

End With
lblTest = "Ïðèìåíåíèå òðàíçàêöèè..."
'DEAbo.admin.CommitTrans
DEAbo.admin.Close
lblTest = "Çàêðûòèå ôàéëà..."
Close #1   ' Close file.
Call DE_EXEC("exec [dbo].[SetAllChargeOnAbonServ] '" & DataSYM & "'", True)
lblTest = "Ïðîâåðêà íà îøèáêè çàêîí÷åíà!"
CmdTest.Enabled = True

Me.MousePointer = 0
Exit Sub

err:
    Me.MousePointer = 0
    lblTest = "Îòìåíà òðàíçàêöèè..."
 '   DEAbo.admin.RollbackTrans
    DEAbo.admin.Close
    Close #1
    MsgBox "Ïðîèçîøëà îøèáêà!" & err.Description & vbCrLf & "Ñîçäàííûé ôàéë îøèáîê íå âåðåí!", vbCritical, "Error"
End Sub

*/
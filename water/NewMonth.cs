using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace water
{
    class NewMonth
    {
        private SqlConnection con = new SqlConnection();

        public NewMonth(string constr = "")
        {
            if (constr.Length > 0) con.ConnectionString = constr;
            else con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch
            {
            }
        }

        public Boolean SetSaldo(Boolean base_)
        {
            Boolean result = false;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            try
            {
                con.Open();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public Boolean Saldo27(string str, string dom, Boolean deb, Boolean kred, string per=null)
        {
            Boolean result = false;
            if (per == null) per = frmMain.MaxCurPer;
            if (con.State == System.Data.ConnectionState.Open)
            {
                ///SqlTransaction tran = con.BeginTransaction("saldo27");
                //если счет в УК и забираем сальдо, то проверяем на наличие аналогичных счетов в другой базе
                List<string> lic = new List<string>();
                List<double> dolg = new List<double>();
                SqlCommand cmd = new SqlCommand();
                int NKUK = 0, NKA = 0;
                cmd.Connection = con;
                cmd.CommandTimeout = 0;
                cmd.CommandType = System.Data.CommandType.Text;

                string dolg_s = "(" + ((deb) ? "skdeb" : "") + ((kred) ? "-skkred" : "")+")";
                try
                {
                    cmd.CommandText = @"select lic,"+dolg_s+" as dolg from abon.dbo.abonent" + per + @" a inner join abon.dbo.spvedomstvo v on a.kodvedom=v.id where v.buk=0 and a.str_code=@str and a.dom=@dom 
                                    union all 
                                    select lic," + dolg_s + " as dolg from abonuk.dbo.abonent" + per + @" a inner join abonuk.dbo.spvedomstvo v on a.kodvedom=v.id where v.buk=1 and a.str_code=@str and a.dom=@dom ";
                    cmd.Parameters.AddWithValue("@str", str);
                    cmd.Parameters.AddWithValue("dom", dom);
                    using (SqlDataReader read = cmd.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            while (read.Read())
                            {
                                lic.Add(read["lic"].ToString().Trim());
                                dolg.Add(Convert.ToDouble(read["dolg"].ToString().Trim()));
                            }
                        }
                    }
                    cmd.Parameters.Clear();

                    if (deb || kred)
                    {
                        cmd.CommandText = @"INSERT INTO [Abon].[dbo].[abonent" + per + @"]
							   ([Lic],[Kontr],[Fam],[Str_code], [dom],[flat],[Liver],[Vibilo],[VibiloData],[Vibilo2],[VibiloData2],[Vvodomer],[Pvodomer],[LgKan]
							   ,[N_Vl],[N_Kl],  [KodVedom], [Vedom],[numtel],[skvagina],[gruppa3],  [SecHome], [Prochee],[TempNorm],[TempNormDate],[Sebestoim],[SkvPol]
							   ,[N_Vl1],[N_Kl1],[LSN],[NoGroundArea],[NotLive],[lsnEirc],flat_int,flat_str,Area)
					SELECT     '1'+Right(a.Lic,9), a.Kontr, a.Fam, a.Str_code, a.dom, a.flat, a.Liver, a.Vibilo, a.VibiloData, a.Vibilo2, a.VibiloData2,a.Vvodomer, a.Pvodomer, a.LgKan
								,a.N_Vl,a.N_Kl,a.KodVedom,a.Vedom, a.numtel, a.skvagina, a.gruppa3, a.SecHome, a.Prochee, a.TempNorm, a.TempNormDate, a.Sebestoim,  a.SkvPol
							  , a.N_Vl1, a.N_Kl1, a.LSN, a.NoGroundArea, a.NotLive,  a.lsnEirc,a.flat_int,a.flat_str,a.Area
					FROM AbonUK.dbo.abonent" + per + @" AS a LEFT OUTER JOIN
					     Abon.dbo.abonent" + per + @" AS aa ON RIGHT(a.Lic,9) = RIGHT(aa.Lic, 9) AND a.Str_code = aa.Str_code AND a.dom = aa.dom
					WHERE (aa.Lic IS NULL) AND(a.str_code=@str and a.dom=@dom)";
                        cmd.Parameters.AddWithValue("@str", str);
                        cmd.Parameters.AddWithValue("@dom", dom);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        cmd.CommandText = @"delete from abonuk.dbo.pos" + per + @" where brik='27' and pach='10' and lic in (select lic from abonuk.dbo.abonent"+per+@" where str_code=@str and dom=@dom)";
                        cmd.Parameters.AddWithValue("@str", str);
                        cmd.Parameters.AddWithValue("@dom", dom);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        cmd.CommandText = @"delete from abon.dbo.pos" + per + @" where brik='27' and pach='10' and lic in (select '1'+right(lic,9) from abonuk.dbo.abonent" + per + @" where str_code=@str and dom=@dom)";
                        cmd.Parameters.AddWithValue("@str", str);
                        cmd.Parameters.AddWithValue("@dom", dom);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        cmd.CommandText = @"select (isnull(MAX(NKvit),0)+1) as kvit FROM AbonUK.dbo.Pos" + per;
                        using (SqlDataReader read = cmd.ExecuteReader())
                        {
                            if (read.HasRows)
                            {
                                read.Read();
                                NKUK = Convert.ToInt32(read["kvit"].ToString());
                            }
                        }
                        cmd.CommandText = @"select (isnull(MAX(NKvit),0)+1) as kvit FROM Abon.dbo.Pos" + per;
                        using (SqlDataReader read = cmd.ExecuteReader())
                        {
                            if (read.HasRows)
                            {
                                read.Read();
                                NKA = Convert.ToInt32(read["kvit"].ToString());
                            }
                        }

                        for (int i = 0; i < lic.Count; i++)
                        {
                            if (dolg.ElementAt(i) != 0 && lic.ElementAt(i).Substring(0, 1) == "2")
                            {
                                    cmd.CommandText = @"INSERT INTO [AbonUK].[dbo].[Pos" +per + @"]
                                                   ([brik],[pach],[data_p],[opl],[lic],[Prim],[PerOpl],[NKvit],[PerOplReal],[poliv])
                                             VALUES
                                                   (27,10,convert(datetime,'" + DateTime.DaysInMonth(Convert.ToInt32(per.Substring(0, 4)), Convert.ToInt32(per.Substring(4,2))) + @"." + per.Substring(4, 2) + @"." + per.Substring(0, 4) + @"',104),@dolg,@lic,'переброска реального долга','" + per.Substring(0, 4) + @"/" + per.Substring(4, 2) + @"',@kvit,'" + per.Substring(0, 4) + @"/" + per.Substring(4, 2) + @"',0)";
                                    cmd.Parameters.AddWithValue("@lic", lic.ElementAt(i));
                                    cmd.Parameters.AddWithValue("@dolg", dolg.ElementAt(i));
                                    cmd.Parameters.AddWithValue("@kvit", NKUK);
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();
                                    NKUK++;
                                    cmd.CommandText = @"INSERT INTO [Abon].[dbo].[Pos" + per + @"]
                                                   ([brik],[pach],[data_p],[opl],[lic],[Prim],[PerOpl],[NKvit],[PerOplReal],[poliv])
                                             VALUES
                                                   (27,10,convert(datetime,'" + DateTime.DaysInMonth(Convert.ToInt32(per.Substring(0, 4)), Convert.ToInt32(per.Substring(4, 2))) + @"." + per.Substring(4, 2) + @"." + per.Substring(0, 4) + @"',104),-1 * @dolg,@lic,'переброска реального долга','" + per.Substring(0, 4) + @"/" + per.Substring(4, 2) + @"',@kvit,'" + per.Substring(0, 4) + @"/" + per.Substring(4, 2) + @"',0)";
                                    cmd.Parameters.AddWithValue("@lic", "1"+lic.ElementAt(i).Substring(1,9));
                                    cmd.Parameters.AddWithValue("@dolg", dolg.ElementAt(i));
                                    cmd.Parameters.AddWithValue("@kvit", NKA);
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();
                                    NKA++;
                                    Application.DoEvents();
                            }


                        }
                    }
                    result = true;
                    ///tran.Commit();
                }
                catch { ///if (tran == null) tran.Rollback(); 
                        result = false; }
            }

            return result;
        }

        public Boolean vodomer2norm(byte base_ = 0)
        {
            bool result = false;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
                SqlCommand com = new SqlCommand();
                com.Connection = con;
                SetMeanCube smc = new SetMeanCube(con);
                List<string> lic = new List<string>();
                string CurrentBase = "Abon.dbo.";
                string Period = frmMain.MaxCurPer;
                com.CommandText = @"SELECT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                            INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                            INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" as vd ON a.Lic = vd.Lic
                            LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic
                            WHERE sv.bPaketC = 1 AND a.gruppa3 = 0 AND (a.Vvodomer = 1 OR a.pVodomer = 1) AND
                            a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND vd.datvvod < @BorderDate AND
                            (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod
                            WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl is NULL and ModiRecR >= @BorderDate))";
                com.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Period);
                com.Parameters.Add("@PerOplLast", SqlDbType.Int).Value = Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(2, 4));
                com.Parameters.Add("@BorderDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(0, 4)), Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(4, 2)), 1);
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        while (read.Read())
                        {
                            lic.Add(read["lic"].ToString());
                        }
                    }
                }
                result = (smc.ClearVvodomerMoreThanSixMonths(frmMain.MaxCurPer, 0) >= 0);
                if ((lic.Count > 0 ) && result)
                {
                    CalculateWater.MainForm nach = new CalculateWater.MainForm();
                    for (int i = 0; i < lic.Count; i++)
                    {
                        nach.Query(0, frmMain.MaxCurPer, lic.ElementAt(i));
                        Application.DoEvents();
                    }
                }

                com.Parameters.Clear();
                lic.Clear();

                CurrentBase = "Abonuk.dbo.";
                com.CommandText = @"SELECT a.Lic FROM " + CurrentBase + @"abonent" + Period + @" AS a 
                            INNER JOIN " + CurrentBase + @"SpVedomstvo sv ON sv.ID = a.KodVedom
                            INNER JOIN " + CurrentBase + @"VodomerDate" + Period + @" as vd ON a.Lic = vd.Lic
                            LEFT JOIN " + CurrentBase + @"GetAbon(@Per) ab ON ab.Lic = a.lic
                            WHERE sv.bPaketC = 1 AND a.gruppa3 = 0 AND (a.Vvodomer = 1 OR a.pVodomer = 1) AND
                            a.Prochee = 0 AND a.NotLive = 0 AND a.Fam <> 'У' AND vd.datvvod < @BorderDate AND
                            (ab.dtNotCalc IS NULL OR " + CurrentBase + @"DateToPer(ab.dtNotCalc, 0) < @Per) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod
                            WHERE PerOpl >= @PerOplLast and (MeanCube = 0 or (MeanCube = 1 and BrikPach = 19)))) AND
                            NOT (a.Lic in (SELECT Lic FROM " + CurrentBase + @"PosVod WHERE PerOpl is NULL and ModiRecR >= @BorderDate))";
                com.Parameters.Add("@Per", SqlDbType.Int).Value = Convert.ToInt32(Period);
                com.Parameters.Add("@PerOplLast", SqlDbType.Int).Value = Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(2, 4));
                com.Parameters.Add("@BorderDate", SqlDbType.DateTime).Value = new DateTime(Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(0, 4)), Convert.ToInt32(smc.GetPrevPeriod(Period, 6).Substring(4, 2)), 1);
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        while (read.Read())
                        {
                            lic.Add(read["lic"].ToString());
                        }
                    }
                }
                result = (smc.ClearVvodomerMoreThanSixMonths(frmMain.MaxCurPer, 1) >= 0) && result;
                if ((lic.Count > 0) && result)
                {
                    CalculateWater.MainForm nach = new CalculateWater.MainForm();
                    for (int i = 0; i < lic.Count; i++)
                    {
                        nach.Query(1, frmMain.MaxCurPer, lic.ElementAt(i));
                        Application.DoEvents();
                    }
                }


            }
            catch { result = false; }
            return result;
        }
    }
}

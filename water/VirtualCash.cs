using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace water
{
    public class VirtualCash
    {
        private SqlConnection conn;
        private string Per;
        private string brik;
        private DateTime faktdate;
        private DateTime fdate;
        private bool FromFile;
        private string FromFileName;
        private string Pach;
        private string filenamepart;
        private bool tosell;
        public int FLCount = 0;
        public double FLSumma = 0;
        Excel.Application excel;
        Excel.Workbook workbook;
        Excel.Worksheet sheet;
        Excel.Range cells;


        public VirtualCash(SqlConnection connstr, string briket, DateTime FaktDate, string pach, string pref, string FileNamePart, string per, DateTime FDate, bool ToSell)
        {
            conn = connstr;
            brik = briket;
            Pach = pach;
            Per = per;
            faktdate = FaktDate;
            fdate = FDate.Date;
            filenamepart = FileNamePart;
            tosell = ToSell;
            if (brik.Substring(0, 3) == "000")
            {
//                FromFileName = "//serverab/Жилье/reestr sber/arhiv/" + pref.Substring(0, 4) + "_5701000368_40702810700400000278_" + pach + ".y" + pref.Substring(4, 2);
                FromFileName = "//serverab/Жилье/PGUK/POST/" + Per + Pach + ".xlsx";
            }
            //else if (brik.Substring(0, 3) == "_21")
            //{
            //    FromFileName = "//serverab/Жилье/SBER/3/post/3_" + pref.Substring(0, 4) + "_5701000368_40702810700400000278_" + pach + ".y" + pref.Substring(4, 2);
            //}
            //else if (brik.Substring(0, 3) == "_22")
            //{
            //    FromFileName = "//serverab/Жилье/SBER/4/post/4_" + pref.Substring(0, 4) + "_5701000368_40702810700400000278_" + pach + ".y" + pref.Substring(4, 2);
            //}
            //else if (brik.Substring(0, 3) == "_55")
            //{
            //    FromFileName = "//serverab/Жилье/MI/1/post/5701000368_40702810700400000278_00" + pach.Substring(0, 1) + "y" + pach.Substring(1, 2) + ".txt";
            //}
            //else if (brik.Substring(0, 3) == "_56")
            //{
            //    FromFileName = "//serverab/Жилье/MiNB/1/post/5701000368_40702810700400000278_0" + pach.Substring(0, 1) + "_" + pach.Substring(1, 2) + per.Substring(4, 2) + per.Substring(0, 4) + ".txt";
            //}
            else
            {
                FromFileName = "";
            }
        }

        public void MakeFile()
        {
            string OrganizationName = "МПП ВКХ <Орелводоканал>";
            string OrganizationPhone = "84862760515";
            string OrganizationINN = "5701000368";
            string BankName = "";
            string BankPhone = "";
            string BankWeb = "";
            string BankINN = "";
            SqlCommand cmd1;
            SqlConnection conect = new SqlConnection();
            conect.ConnectionString = conn.ConnectionString;
            conect.Open();
            string SQLText;

            
            string FileName = filenamepart;
            string DateToPay = "";

            if (brik == "276" || brik == "918")
            {
                DateToPay = " and data_p = @DP ";
            }
           
            List<String> FL = new List<String>();
            Int32 SizeOfFile = 0;
            Int32 NumberOfFile = 1;

            if (brik == "921" || brik == "922" || brik == "276" || brik == "918" || brik == "000" || brik == "001")
            {
                BankName = "'ПАО Сбербанк'";
                BankPhone = "'84955005550'";
                BankWeb = "'www.sberbank.ru'";
                BankINN = "'7707083893'";
            }
            if (brik == "955" || brik == "956")
            {
                BankName = "'ПАО Московский индустриальный банк'";
                BankPhone = "'88001007474'";
                BankWeb = "'www.minbank.ru'";
                BankINN = "'7725039953'";
            }

            if (FromFileName == "")
            {
                SQLText = String.Format(@" SELECT '1' + lic as lic, WPay as WaterCredit, KPay as KanalCredit, PPay as PolivCredit, KoPay as KoefCredit, APay as AvansItog, {1} as Prim, opl as summaitog,
                                           WPay + KPay + PPay + KoPay + APay as summatosend, '{2}' as brik, '{3}' as pach, '{4}' as data_p
                                           FROM abon.dbo.pos{0} WHERE brik = {2} and pach = {3} " + DateToPay +
                                     @"UNION ALL
                                           SELECT '1' + lic as lic, WPay as WaterCredit, KPay as KanalCredit, PPay as PolivCredit, KoPay as KoefCredit, APay as AvansItog, {1} as Prim, opl as summaitog,
                                           WPay + KPay + PPay + KoPay + APay as summatosend, '{2}' as brik, '{3}' as pach, '{4}' as data_p
                                           FROM abonuk.dbo.pos{0} WHERE brik = {2} and pach = {3} " + DateToPay, Per, BankName, brik, Pach, fdate);


                if (brik.Substring(0, 3) == "001")
                {
                    SQLText = String.Format(@"SELECT [договор-вх] as lic, 0 as vvodomer, [дата_поступ_документа] as data_p, ROUND([сумма] * 0.55, 2) as waterCredit, [сумма] - ROUND([сумма] * 0.55, 2) as KanalCredit, 0 as PolivCredit, 0 as KoefCredit, 0 as avansitog, [сумма] as SummaItog,
                                              'ПАО Сбербанк' as Prim, '+74955005550' as phone, 'www.sberbank.ru' as adress, '7707083893' as inn, '{0}' as PostName, '{1}' as PostPhone, '{2}' as PostINN
                                              FROM gen.prom.dbo.vadim WHERE [дата_поступ_документа] = @DP", OrganizationName, OrganizationPhone, OrganizationINN);
                }
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = SQLText;
                if (brik == "276" || brik == "918" || brik == "001")
                    cmd.Parameters.Add("@DP", SqlDbType.DateTime).Value = faktdate;
                cmd.CommandTimeout = 0;

                int i = 0;
                //string numbers;
                Random rnd = new Random();
                FLCount = 0;
                FLSumma = 0;
                using (SqlDataReader Cash = cmd.ExecuteReader())
                {
                    if (Cash.HasRows)
                    {
//---------------------- Сохраняем все данные в таблицу отчетов ----------------------------------------------------------------------------------------------------------------------------
                        if (tosell && brik != "000")
                        {
                            SQLText = String.Format(@" INSERT INTO abon.dbo.PrintChek_info (lic, WaterCredit, KanalCredit, PolivCredit, KoefCredit, AvansItog, Prim, summaitog, summatosend, brik, pach, data_p)
                                                (SELECT '1' + lic as lic, WPay as WaterCredit, KPay as KanalCredit, PPay as PolivCredit, KoPay as KoefCredit, APay as AvansItog, {1} as Prim, opl as summaitog,
                                                WPay + KPay + PPay + KoPay + APay as summatosend, '{2}' as brik, '{3}' as pach, '{4}' as data_p
                                                FROM abon.dbo.pos{0} WHERE brik = {2} and pach = {3} " + DateToPay +
                                                 @"UNION ALL
                                                 SELECT '1' + lic as lic, WPay as WaterCredit, KPay as KanalCredit, PPay as PolivCredit, KoPay as KoefCredit, APay as AvansItog, {1} as Prim, opl as summaitog,
                                                WPay + KPay + PPay + KoPay + APay as summatosend, '{2}' as brik, '{3}' as pach, '{4}' as data_p
                                                FROM abonuk.dbo.pos{0} WHERE brik = {2} and pach = {3} " + DateToPay + ")", Per, BankName, brik, Pach, fdate);
                            if (brik.Substring(0, 3) == "001")
                            {
                                SQLText = String.Format(@"INSERT INTO abon.dbo.PrintChek_info (lic, WaterCredit, KanalCredit, PolivCredit, KoefCredit, AvansItog, Prim, summaitog, summatosend, brik, pach, data_p)
                                              (SELECT [договор-вх] as lic, ROUND([сумма] * 0.55, 2) as waterCredit, [сумма] - ROUND([сумма] * 0.55, 2) as KanalCredit, 0 as PolivCredit, 0 as KoefCredit, 0 as avansitog, 'ПАО Сбербанк' as Prim,
                                              ROUND([сумма], 2) as summaitog, ROUND([сумма], 2) as summatosend, '001' as brik, '001' as pach, '{0}' as data_p
                                              FROM gen.prom.dbo.vadim WHERE [дата_поступ_документа] = @DP)", fdate);
                            }
                            cmd1 = new SqlCommand();
                            cmd1.Connection = conect;
                            cmd1.CommandText = SQLText;
                            if (brik == "276" || brik == "918" || brik == "001")
                                cmd1.Parameters.Add("@DP", SqlDbType.DateTime).Value = faktdate;
                            cmd1.CommandTimeout = 0;
                            cmd1.ExecuteNonQuery();
                        }
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
                        while (Cash.Read())
                        {
//----------------------------------------------------------------------------------------------------------------------------------------------------------------                    
                            i++;
                            FLCount++;
                            FLSumma = FLSumma + Math.Round(Convert.ToDouble(Cash["summaitog"]), 2);
                            // ----- Сохраняем поступление по воде -----
                            if (Convert.ToDouble(Cash["waterCredit"]) > 0 && tosell && brik != "000")
                            {
                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(Cash["waterCredit"]), 2), Math.Round(Convert.ToDouble(Cash["waterCredit"]) / 6, 2),
                                                           Math.Round(Convert.ToDouble(Cash["waterCredit"]), 2) - Math.Round(Convert.ToDouble(Cash["waterCredit"]) / 6, 2),
                                                           BankName, BankPhone, BankWeb, BankINN, "'ХОЛОДНОЕ ВОДОСНАБЖЕНИЕ'", "'Полный расчет'", "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());

                                cmd1 = new SqlCommand();
                                cmd1.Connection = conect;
                                cmd1.CommandText = SQLText;
                                cmd1.CommandTimeout = 0;
                                cmd1.ExecuteNonQuery();
                            }
                            if (Convert.ToDouble(Cash["KanalCredit"]) > 0 && tosell && brik != "000")
                            {
                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(Cash["KanalCredit"]), 2), Math.Round(Convert.ToDouble(Cash["KanalCredit"]) / 6, 2),
                                                           Math.Round(Convert.ToDouble(Cash["KanalCredit"]), 2) - Math.Round(Convert.ToDouble(Cash["KanalCredit"]) / 6, 2),
                                                           BankName, BankPhone, BankWeb, BankINN, "'ВОДООТВЕДЕНИЕ'", "'Полный расчет'", "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());
                                cmd1 = new SqlCommand();
                                cmd1.Connection = conect;
                                cmd1.CommandText = SQLText;
                                cmd1.CommandTimeout = 0;
                                cmd1.ExecuteNonQuery();
                            }
                            if (Convert.ToDouble(Cash["PolivCredit"]) > 0 && tosell && brik != "000")
                            {
                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(Cash["PolivCredit"]), 2), Math.Round(Convert.ToDouble(Cash["PolivCredit"]) / 6, 2),
                                                           Math.Round(Convert.ToDouble(Cash["PolivCredit"]), 2) - Math.Round(Convert.ToDouble(Cash["PolivCredit"]) / 6, 2),
                                                           BankName, BankPhone, BankWeb, BankINN, "'ПОЛИВ'", "'Полный расчет'", "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());
                                cmd1 = new SqlCommand();
                                cmd1.Connection = conect;
                                cmd1.CommandText = SQLText;
                                cmd1.CommandTimeout = 0;
                                cmd1.ExecuteNonQuery();
                            }
                            if (Convert.ToDouble(Cash["KoefCredit"]) > 0 && tosell && brik != "000")
                            {
                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(Cash["KoefCredit"]), 2), Math.Round(Convert.ToDouble(Cash["KoefCredit"]) / 6, 2),
                                                           Math.Round(Convert.ToDouble(Cash["KoefCredit"]), 2) - Math.Round(Convert.ToDouble(Cash["KoefCredit"]) / 6, 2),
                                                           BankName, BankPhone, BankWeb, BankINN, "'ПОВЫШАЮЩИЙ КОЭФФИЦИЕНТ'", "'Полный расчет'", "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());
                                cmd1 = new SqlCommand();
                                cmd1.Connection = conect;
                                cmd1.CommandText = SQLText;
                                cmd1.CommandTimeout = 0;
                                cmd1.ExecuteNonQuery();
                            }
                            if (Convert.ToDouble(Cash["avansitog"]) > 0 && tosell && brik != "000")
                            {
                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, {0}, 0, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(Cash["avansitog"]), 2), Math.Round(Convert.ToDouble(Cash["avansitog"]) / 6, 2),
                                                           Math.Round(Convert.ToDouble(Cash["avansitog"]), 2) - Math.Round(Convert.ToDouble(Cash["avansitog"]) / 6, 2),
                                                           BankName, BankPhone, BankWeb, BankINN, "'АВАНС'", "'Аванс'", "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());
                                cmd1 = new SqlCommand();
                                cmd1.Connection = conect;
                                cmd1.CommandText = SQLText;
                                cmd1.CommandTimeout = 0;
                                cmd1.ExecuteNonQuery();
                              }




//                            if (Convert.ToDouble(Cash["waterCredit"]) + Convert.ToDouble(Cash["KanalCredit"]) > 0 && tosell && brik == "000")
//                            {
//                                SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
//                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrise, UslCount, UslSumma, StavNDS,
//                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
//                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
//                                                          ", Math.Round(Convert.ToDouble(Cash["waterCredit"]) + Convert.ToDouble(Cash["KanalCredit"]), 2),
//                                                           Math.Round((Convert.ToDouble(Cash["waterCredit"]) + Convert.ToDouble(Cash["KanalCredit"])) / 6, 2),
//                                                           Math.Round(Convert.ToDouble(Cash["waterCredit"]) + Convert.ToDouble(Cash["KanalCredit"]), 2) -
//                                                           Math.Round((Convert.ToDouble(Cash["waterCredit"]) + Convert.ToDouble(Cash["KanalCredit"])) / 6, 2),
//                                                           BankName, BankPhone, BankWeb, BankINN, "'ХОЛОДНОЕ ВОДОСНАБЖЕНИЕ И ВОДООТВЕДЕНИЕ'", "'Полный расчет'",
//                                                           "'л/с " + Cash["Lic"].ToString() + "'", "'Услуга'", Cash["lic"].ToString());

//                                cmd1 = new SqlCommand();
//                                cmd1.Connection = conect;
//                                cmd1.CommandText = SQLText;
//                                cmd1.CommandTimeout = 0;
//                                cmd1.ExecuteNonQuery();
//                            }
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
                        }
                    }
                }
            }
            else
            {
/// ------------------------- Вставляем данные из файла ПГУК -------------------------------------
                //Создаём приложение.
                excel = new Microsoft.Office.Interop.Excel.Application(); //создаем COM-объект Excel
                //Открываем книгу.                                                                                                                                                        
                workbook = excel.Workbooks.Open(FromFileName, 0, false, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                //Выбираем таблицу(лист).
                sheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
                // В цыкле достаем данные.
                int i = 1;
                FLCount = 0;
                FLSumma = 0;
                double SummaPay;
                while (sheet.Cells[i+1, 1].Value != null)
                {
                    i++;
                    FLCount++;
                    FLSumma = FLSumma + Convert.ToDouble(sheet.Cells[i, 2].Value);
                    if (Convert.ToDouble(sheet.Cells[i, 7].Value) > 0 && tosell)
                    {
                        SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value), 2), Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value) / 6, 2),
                                                   Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value), 2) - Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value) / 6, 2),
                                                   BankName, BankPhone, BankWeb, BankINN, "'ХОЛОДНОЕ ВОДОСНАБЖЕНИЕ'", "'Полный расчет'", "'л/с " + sheet.Cells[i, 11].Value.ToString() + "'", "'Услуга'", sheet.Cells[i, 11].Value.ToString());
                        cmd1 = new SqlCommand();
                        cmd1.Connection = conect;
                        cmd1.CommandText = SQLText;
                        cmd1.CommandTimeout = 0;
                        cmd1.ExecuteNonQuery();
                    }
                    if (Convert.ToDouble(sheet.Cells[i, 8].Value) > 0 && tosell)
                    {
                        SQLText = String.Format(@" INSERT INTO abon.dbo.PrintCheck (SummaAllsNDS, SummaNal, SummaBeznal, SummaAvans, SummaPostoplata, SummaNDS, SummaBezNDS,
                                                           AgentName, AgentPhone, AgentSite, AgentINN, UslName, UslPrizSpos, UslPrizPred, UslPrice, UslCount, UslSumma, StavNDS,
                                                           Sended, Inform, SendDate, Phone, email, lic) VALUES
                                                           ({0}, 0, {0}, 0, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {10}, {0}, 1, {0}, 20, 0, {9}, NULL, NULL, NULL, {11}) 
                                                          ", Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value), 2), Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value) / 6, 2),
                                                   Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value), 2) - Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value) / 6, 2),
                                                   BankName, BankPhone, BankWeb, BankINN, "'ВОДООТВЕДЕНИЕ'", "'Полный расчет'", "'л/с " + sheet.Cells[i, 11].Value.ToString() + "'", "'Услуга'", sheet.Cells[i, 11].Value.ToString());
                        cmd1 = new SqlCommand();
                        cmd1.Connection = conect;
                        cmd1.CommandText = SQLText;
                        cmd1.CommandTimeout = 0;
                        cmd1.ExecuteNonQuery();
                    }
// -- записываем данные в таблицу с полной информацией -------------------------------------------------------------------------------------------------------------------------
                    if (tosell)
                    {
                        SQLText = String.Format(@" INSERT INTO abon.dbo.PrintChek_info (lic, WaterCredit, KanalCredit, PolivCredit, KoefCredit, AvansItog, Prim, summaitog, summatosend, brik, pach, data_p)
                                                VALUES ({0}, {1}, {2}, 0, 0, 0, {3}, {4}, {5}, {6}, {7}, '{8}')",
                                                    sheet.Cells[i, 11].Value.ToString(), Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value), 2), Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value), 2), BankName,
                                                    Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value), 2) + Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value), 2) + Math.Round(Convert.ToDouble(sheet.Cells[i, 9].Value), 2),
                                                    Math.Round(Convert.ToDouble(sheet.Cells[i, 7].Value), 2) + Math.Round(Convert.ToDouble(sheet.Cells[i, 8].Value), 2), "000", "000", fdate.ToString());
                        cmd1 = new SqlCommand();
                        cmd1.Connection = conect;
                        cmd1.CommandText = SQLText;
                        cmd1.CommandTimeout = 0;
                        cmd1.ExecuteNonQuery();
                    }
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                }
                excel.Quit();
                sheet = null;
                System.GC.Collect();
                excel = null;
            }
// ---------------------------------- Это вообще не надо            
//            SaveToFile(FileName, FL, FLCount, FLSumma);
        }

        private void SaveToFile(String FileName, List<string> fl, Int32 flcount, Double flsumma)
        {
            if (tosell)
            {
                string FilePath = "//serverab/Жилье/forcash/txt/" + FileName + ".txt";
                using (var sw = new StreamWriter(FilePath, false, Encoding.GetEncoding(65001)))
                {
                    foreach (string lists in fl)
                        sw.WriteLine(lists);
                    sw.WriteLine("--------------------------------");
                    sw.WriteLine("Количество: " + flcount.ToString());
                    sw.WriteLine("Сумма: " + flsumma.ToString());
                }
            }

        }
    }
}

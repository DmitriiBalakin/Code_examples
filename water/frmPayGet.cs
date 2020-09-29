using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace water
{
    public partial class frmPayGet : Form
    {
        public frmPayGet()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try{
                con.Open();
                }
            catch
            {
            }
        }
        SqlConnection con = new SqlConnection();
        SqlCommand db_com = new SqlCommand();
        SqlTransaction trans;
        string command_;
        string CurMonth = frmMain.CurPer.Substring(4, 2), CurYear = frmMain.CurPer.Substring(0, 4);

        private string MM(string m)
        {
            if (m.Length == 1) m = "0"+m;
            return m;
        }

        private Boolean CompareName(string Name, string mask)
        {
            string yy = frmMain.CurPer.Substring(2, 2), mm = frmMain.CurPer.Substring(4, 2);
           // mask = mask.Replace("{yy}", yy);
            //mask = mask.Replace("{mm}", mm);
            mask = mask.Replace("{dd}", "([0-3]{1}[0-9]{1})");
            mask = mask.Replace("{d}", @"(\d{1})");
            mask = mask.Replace("{c}", "(.{1})");
            mask = mask.Replace("{*}",@"(\({0,1}[0-9]{0,1}[0-9]{0,1}\){0,1})");

            string mask1 = mask.Replace("{mm}", mm); mask1 = mask1.Replace("{yy}", yy);
            string mask2 = mask.Replace("{mm}", (mm == "12" ? MM("1") : MM(Convert.ToString(Convert.ToInt16(mm) + 1)))); if (mm == "12") mask2 = mask2.Replace("{yy}", MM((Convert.ToInt32(yy)+1).ToString())); else mask2 = mask2.Replace("{yy}",yy);

            /*
            vb = mask.IndexOf("{d");
            while (vb != -1)
            {
                ve = mask.IndexOf("}");
                if (ve != 0)
                {
                    rep = mask.Substring(vb + 1, ve - vb - 1);
                    mask = mask.Remove(vb,ve-vb+1);
                    if (rep[0] == 'd')
                    {
                        rep = rep.Replace("d","");
                        try
                        {
                            string repc = "(\d{"+rep+"})";
                            mask = mask.Insert(vb,repc);
                        }
                        catch 
                        {
                            error = 1;
                        }
                    }
                }
                vb = mask.IndexOf("{d");
            }

            vb = mask.IndexOf("{c");
            while (vb != -1)
            {
                ve = mask.IndexOf("}");
                if (ve != 0)
                {
                    rep = mask.Substring(vb + 1, ve - vb - 1);
                    mask = mask.Remove(vb, ve - vb + 1);
                    if (rep[0] == 'c')
                    {
                        rep = rep.Replace("c", "");
                        try
                        {
                            string repc = "(.{" + rep + "})";
                            mask = mask.Insert(vb, repc);
                        }
                        catch
                        {
                            error = 1;
                        }
                    }
                }
                vb = mask.IndexOf("{c");
            }
            */
            int res = 0;
            Regex reg = new Regex(@mask1, RegexOptions.IgnoreCase);
            if (reg.IsMatch(Path.GetFileName(Name))) res = 1;
            Regex reg1 = new Regex(@mask2, RegexOptions.IgnoreCase);
            if (reg1.IsMatch(Path.GetFileName(Name))) res = 1;
            if (res == 1) return true; else return false;
        }

        private List<string> GetFileList(string dir)
        {
            List<string> sp = new List<string>();
            string[] flist = Directory.GetFiles(dir);
            sp = flist.ToList();
            return sp;
        }

        private void frmPayGet_Shown(object sender, EventArgs e)
        {
            panel5.Visible = true;
            CurYear = frmMain.MaxCurPer.Substring(0, 4);
            CurMonth = frmMain.MaxCurPer.Substring(4, 2);

            try
            {
                //// загрузка ведомств и архивов
                db_com.Connection = con;
                db_com.CommandType = CommandType.Text;
                db_com.CommandTimeout = 0;
                db_com.CommandText = "select * from abonuk.dbo.priemopt where active=1";

                List<string> files = new List<string>();
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        string path = "";
                        string arch = "";
                        while (db_reader.Read())
                        {
                            string pswd = db_reader["passwd"].ToString();
                            path = db_reader["path"].ToString();
                            if (path == "\\\\Serverab\\Жилье\\Sber\\3\\" || path == "\\\\Serverab\\Жилье\\Sber\\4\\" || path == "\\\\Serverab\\Жилье\\MI\\1\\")
                              continue;
                            if (path[path.Length-1] != '\\') path += "\\"; 
                            arch = db_reader["arch"].ToString();
                            files = GetFileList(path+"post\\");
                            int i = 0;
                            while (i < files.Count)
                            {
                                string n1 = files[i].ToString();
                                string n2 = db_reader["arch"].ToString();
                                if (CompareName(n1, n2)) 
                                {
                                    try
                                    {
                                        string FileExtension = path == "\\\\Serverab\\Жилье\\MiNB\\1\\" ? " *.txt " : " *.dbf ";
                                        ///path = path.Replace("\\\\", "\\");
                                        System.Diagnostics.Process MyProc = new System.Diagnostics.Process();
                                        if (pswd == "")
                                            MyProc.StartInfo.Arguments = "e -o- " + path + "post\\" + Path.GetFileName(files[i]) + FileExtension + path + "post\\";
                                        if (pswd.Length > 1)
                                            MyProc.StartInfo.Arguments = "e -o- -p" + pswd + " " + path + "post\\" + Path.GetFileName(files[i]) + FileExtension + path + "post\\";
                                        MyProc.StartInfo.FileName = @"winrar.exe";
                                        MyProc.Start();
                                        MyProc.WaitForExit();
                                        string dop = "";
                                        if (File.Exists(@"" + path + "arh\\" + Path.GetFileName(files[i]))) dop = "_";
                                        File.Move(@"" + path + "post\\" + Path.GetFileName(files[i]), @"" + path + "arh\\" + Path.GetFileName(files[i])+dop);
                                    }
                                    catch (Exception ex)
                                    {
                                        //MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else files[i] = "";
                                i++;
                            }
                        }
                    }
                    db_reader.Close();
                }
                files.Clear();
                //// поиск файлов на прием и вывод в list
                db_com.CommandText = "select * from abonuk.dbo.priemopt where active=1";
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        string path = "";
                        string arch = "";
                        gv_payfiles.Rows.Clear();
                        gv_payfiles.Refresh();
                        while (db_reader.Read())
                        {
                            DataTable tbl = new DataTable("tmp");
                            path = db_reader["path"].ToString();
                            if (path[path.Length - 1] != '\\') path += "\\";
                            arch = db_reader["file"].ToString();
                            files = GetFileList(path + "post\\");
                            int i = 0;
                            while (i < files.Count)
                            {
                                if (path == "\\\\Serverab\\Жилье\\Sber\\3\\" || path == "\\\\Serverab\\Жилье\\Sber\\4\\" || path == "\\\\Serverab\\Жилье\\MiNB\\1\\" || path == "\\\\Serverab\\Жилье\\MI\\1\\")
                                {
// -------------------------------------------------------------------------------------------------
                                    string fileName = files[i];
                                    string line;
                                    StreamReader file = new StreamReader(fileName);
                                        try
                                        {
                                            string DocDates = "";
                                            double summa = 0;
                                            int count_pay = 0;
                                            Boolean Errord = false;
                                            Boolean Errors = false;
                                            Boolean Errors0 = false;
                                            Boolean Errorl = false;
                                            while ((line = file.ReadLine()) != null)
                                            {
                                                if (line[0] != '=')
                                                {
                                                    // -- смотрим даты платежей --
                                                    if (DocDates == "") DocDates = line.Substring(0, 10);
                                                    if (DocDates != line.Substring(0, 10)) Errord = true; // -- есть платежи за разные даты
                                                    if (line.IndexOf("[!];;0,00;") != -1) Errors0 = true;   // -- имеются нулевые платежи
                                                    if (line.IndexOf("[!];;-") != -1) Errors = true;        // -- имеются отрицательные платежи
                                                    // Вырезаем сумму
                                                    int PosBegSum = line.IndexOf("[!];;");
                                                    string strSumma = line.Substring(PosBegSum + 5);
                                                    strSumma = strSumma.Substring(0, strSumma.IndexOf(";"));
                                                    if (Convert.ToInt32(line.Substring(3, 2)) == Convert.ToInt32(CurMonth))
                                                    {
                                                        summa += Convert.ToDouble(strSumma.Replace(',', '.'));
                                                        count_pay++;
                                                    }
                                                    string lic = line;
                                                    for (int j = 1; j <= 5;  j++)
                                                        lic = lic.Substring(lic.IndexOf(";") + 1, lic.Length - (lic.IndexOf(";") + 1));
                                                    if (lic[0] != 2 && lic[0] != 1 && lic.Substring(0, 10).IndexOf(";") != -1) Errorl = true; // -- лицевой счет начинается не с 1 и не с 2
                                                }
                                            }
                                            file.Close();
                                            file.Dispose();
                                            if (path == "\\\\Serverab\\Жилье\\Sber\\3\\")
                                            {
                                                string[] row_1 = { "False", Path.GetFileName(files[i]), db_reader["bric"].ToString(), count_pay.ToString(), Math.Round(summa, 2).ToString(),
                                                                db_reader["sys_name"].ToString(), fileName, Path.GetFileName(files[i]).Substring(39, 3)};
                                                gv_payfiles.Rows.Add(row_1);
                                            }
                                            else if (path == "\\\\Serverab\\Жилье\\MiNB\\1\\")
                                            {
                                                string[] row_1 = { "False", Path.GetFileName(files[i]), db_reader["bric"].ToString(), count_pay.ToString(), Math.Round(summa, 2).ToString(),
                                                                db_reader["sys_name"].ToString(), fileName,  Path.GetFileName(files[i]).Substring(33, 1) + Path.GetFileName(files[i]).Substring(35, 2)};
                                                gv_payfiles.Rows.Add(row_1);
                                            }
                                            else if (path == "\\\\Serverab\\Жилье\\MI\\1\\")
                                            {
                                                string[] row_1 = { "False", Path.GetFileName(files[i]), db_reader["bric"].ToString(), count_pay.ToString(), Math.Round(summa, 2).ToString(),
                                                                db_reader["sys_name"].ToString(), fileName,  Path.GetFileName(files[i]).Substring(34, 1) + Path.GetFileName(files[i]).Substring(36, 2)};
                                                gv_payfiles.Rows.Add(row_1);
                                            }
                                            else if (path == "\\\\Serverab\\Жилье\\Sber\\4\\")
                                            {
                                                string[] row_1 = { "False", Path.GetFileName(files[i]), db_reader["bric"].ToString(), count_pay.ToString(), Math.Round(summa, 2).ToString(),
                                                                db_reader["sys_name"].ToString(), fileName, Path.GetFileName(files[i]).Substring(39, 3)};
                                                gv_payfiles.Rows.Add(row_1);
                                            }
                                            if (Errord)
                                            {
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Orange;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Orange;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Не все принимаемые платежи одной даты. ";
                                            }
                                            if (Errors0)
                                            {
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Orange;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Orange;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText += "Есть платежи с нулевыми суммами. ";
                                            }
                                            if (Errors)
                                            {
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Red;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Red;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText += "Есть платежи с отрицательными суммами. ";
                                            }
                                            if (Errorl)
                                            {
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Red;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Red;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Есть платежи с некорректными л/с. ";
                                            }
                                            if (gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells["count"].Value.ToString() == "0")
                                            {
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Yellow;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Yellow;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells["summ"].Style.BackColor = Color.Yellow;
                                                gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Нет платежей для приема в данном периоде.";
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            MessageBox.Show(String.Format("Ошибка импорта файла {0}! {1}", fileName, err.Message));
                                        }
                                        finally
                                        {
                                            file.Close();
                                            file.Dispose();
                                        }
// -------------------------------------------------------------------------------------------------
                                    i++;
                                }
                                else
                                {
                                    DBF dbf = new DBF();
                                    string n1 = files[i].ToString();
                                    string n2 = db_reader["file"].ToString();
                                    if (CompareName(n1, @"^" + n2 + "$"))
                                    {
                                        //string command = "Select count(" + db_reader["LIC"].ToString() + ") as CNT, sum(" + db_reader["summa"].ToString() + ") as SMA From " + files[i] + " WHERE \""+db_reader["DATA"].ToString()+"\" < "+ 
                                        //    " {d'"+CurYear+"-"+MM(Convert.ToString(Convert.ToInt16(CurMonth) + 1))+"-01'}";

                                        //MessageBox.Show(command);                                   
                                    
                                        dbf.FilePath = files[i];
                                        dbf.ReadDBF();

                                         double summa = 0;
                                         int count_pay = 0;
                                         Boolean Errord = false, Errors = false, Errors0 = false, Errorl = false;

                                         ///if (tbl.Rows.Count > 0)
                                         if (dbf.Table.Rows.Count > 0)
                                         {
                                             string paydate = "";
                                         
                                             DataRow[] foundRows;
                                             bool next_year = false;
                                             if (CurMonth == "12") next_year = true;
                                            /* ///foundRows = tbl.Select(db_reader["DATA"].ToString() + " < '01/"+MM(Convert.ToString(Convert.ToInt16(CurMonth) + 1))+"/"+CurYear+"' and " + db_reader["DATA"].ToString()+" >= '01/"+CurMonth+"/"+CurYear+"'");*/
                                             foundRows = dbf.Table.Select(db_reader["DATA"].ToString() + " < '01/" + MM(Convert.ToString(Convert.ToInt16(CurMonth=="12"?"00":CurMonth) + 1)) + "/" + (next_year?(Convert.ToInt32(CurYear)+1).ToString():CurYear) + "' and " + db_reader["DATA"].ToString() + " >= '01/" + CurMonth + "/" + CurYear + "'");
                                             count_pay = foundRows.Length;
                                             if (count_pay > 0)
                                             {
                                                 paydate = foundRows[0][db_reader["DATA"].ToString()].ToString();
                                             }
                                             foreach(DataRow row in foundRows)
                                             {
                                                 summa += Convert.ToDouble(row[db_reader["SUMMA"].ToString()].ToString());
                                                 if (row[db_reader["DATA"].ToString()].ToString() != paydate) Errord = true;
                                                 if (Convert.ToDouble(row[db_reader["SUMMA"].ToString()].ToString()) == 0) Errors0 = true;
                                                 if (Convert.ToDouble(row[db_reader["SUMMA"].ToString()].ToString()) < 0) Errors = true;
                                                 if (row[db_reader["LIC"].ToString()].ToString().Length < 10) Errorl = true;
                                             }
                                             int summa_ = Convert.ToInt32(summa * 100);
                                             summa = Convert.ToDouble(summa_) / 100;
                                             //cl = tbl.Rows[0]["CNT"].ToString();
                                             //cp = tbl.Rows[0]["SMA"].ToString();
                                             //double c1 = Convert.ToDouble(cp);
                                             //int c2 = Convert.ToInt32(c1 * 100);
                                             //c1 = Convert.ToDouble(c2) / 100;
                                             //cp = c1.ToString();
                                         }
                                    
                                        string[] row_ = { "False", Path.GetFileName(files[i]), db_reader["bric"].ToString(),count_pay.ToString(),
                                                            summa.ToString(), db_reader["sys_name"].ToString(),files[i],db_reader["pach"].ToString()};
                                        gv_payfiles.Rows.Add(row_);
                                        if (Errord)
                                        {
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count -1].Cells[0].Style.BackColor = Color.Orange;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Orange;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Не все принимаемые платежи одной даты. ";
                                        }
                                        if (Errors0)
                                        {
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Orange;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Orange;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText += "Есть платежи с нулевыми суммами. ";
                                        }
                                        if (Errors)
                                        {
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Red;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Red;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText += "Есть платежи с отрицательными суммами. ";
                                        }
                                        if (Errorl)
                                        {
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Red;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Red;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Есть платежи с некорректными л/с. ";
                                        }
                                        if (gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells["count"].Value.ToString() == "0")
                                        {
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].Style.BackColor = Color.Yellow;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[1].Style.BackColor = Color.Yellow;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells["summ"].Style.BackColor = Color.Yellow;
                                            gv_payfiles.Rows[gv_payfiles.Rows.Count - 1].Cells[0].ErrorText = "Нет платежей для приема в данном периоде.";
                                        }
                                    }
                                    else files[i] = "";
                                    i++;
                                    dbf.Close();
                                }
                            }
                            Application.DoEvents();
                            
                        }
                        gv_payfiles.Refresh();
                    }
                    db_reader.Close();
                }
                files.Clear();
                panel5.Visible = false;
            }
            catch (Exception ex)
            {
                panel5.Visible = false;
                //MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

//--------------------------------------------------------------------------------------------------

        private void btn_payget_Click(object sender, EventArgs e)
        {
            /////// инициализация счетчиков и переменных
            SqlCommand db_com = new SqlCommand();
            SqlDataReader db_read;
            db_com.Connection = frmMain.db_con;
            db_com.CommandType = CommandType.Text;
            db_com.CommandTimeout = 0;
            //// данные для списка принимаемых файлов
            List<string> files = new List<string>(); //// путь и имя файла
            List<int> count_p = new List<int>(); //// кол-во платежей
            List<int> brik = new List<int>(); //// брикет платежей
            List<int> pach = new List<int>();/// пачка принимаемых платежей
            List<double> summ_p = new List<double>(); //// сумма платежей
            //List<Boolean> nextmonth = new List<Boolean>(); //// явл-ся ли файл платежным для след месяца
            string LIC9 = "1000888800"; //// счет 9 в базе Abon для зачисления неопознаных lic
            string zIDFile = ""; /// код файла ошибок
            string zDateFile = ""; /// дата файла ошибок по первому платежу;
            //// данные из БД
            string DBASE = "Abon"; ////БД по умолчанию
            string DLIC = ""; //// lic для приема
            double DH1 = 0, DH2 = 0, DH3 = 0, DG1 = 0, DG2 = 0, DG3 = 0; //// показания счетчиков
            double DDH1 =0, DDH2 = 0, DDH3 = 0, DDG1 = 0, DDG2 = 0, DDG3 = 0; //// поверка счетчиков
            int DVODOMER = 0; //// имеется ли водомер
            int DGROUPP = 0;
            int COMVOD = 0; ///Общий счетчик
            int DERROR = 0; /// ошибка по лицевому
            int DVERROR = 0; /// ошибка по водомерам
            //// названия полей в файле
            string LIC = "";
            string SUMMA = "";
            string DATA = "";
            string FSKOD = "";
            string SHOL1 = "";
            string SHOL2 = "";
            string SHOL3 = "";
            string SGOR1 = "";
            string SGOR2 = "";
            string SGOR3 = "";
            //// данные из файла
            //TDBF DBF = new TDBF();
            //DataTable FFILE = new DataTable("tmp"); /// сам принимамый файл
            DBF dbf = new DBF();
            StreamReader file;
            string FLIC = ""; //// lic в файле
            string FDATE = ""; //// дата платежа
            string FFSKOD = ""; //// штрих код
            double FSUMMA = 0; //// сумма платежа
            double FH1 = 0, FH2 = 0, FH3 = 0, FG1 = 0, FG2 = 0, FG3 = 0; //// показания счетчиков
            ////сброс счетчиков
            progressBar1.Minimum = progressBar2.Minimum = 0;
            progressBar1.Maximum = progressBar2.Maximum = 0;
            progressBar1.Value = 0;  ////текущего файла
            progressBar2.Value = 0;  ////общего списка принимаемых файлов
            for (int i = 0; i < gv_payfiles.Rows.Count; i++)
            {
                if (gv_payfiles.Rows[i].Cells["get"].Value.ToString() == "True")
                {
                   if (Convert.ToInt32(gv_payfiles.Rows[i].Cells["count"].Value.ToString()) > 0)
                   {
                       files.Add(gv_payfiles.Rows[i].Cells["path"].Value.ToString());
                       count_p.Add(Convert.ToInt32(gv_payfiles.Rows[i].Cells["count"].Value.ToString()));
                       brik.Add(Convert.ToInt32(gv_payfiles.Rows[i].Cells["bric"].Value.ToString()));
                       pach.Add(Convert.ToInt32(gv_payfiles.Rows[i].Cells["pach"].Value.ToString()));
                       summ_p.Add(Convert.ToDouble(gv_payfiles.Rows[i].Cells["summ"].Value.ToString()));
                       progressBar2.Maximum += Convert.ToInt32(gv_payfiles.Rows[i].Cells["count"].Value.ToString());
                   }
                }
            }
            if (progressBar2.Maximum > 0) ///значит есть платежи на прием и начинаем прием
            {
                panel3.Visible = true;
                panel1.Enabled = false;
                panel2.Enabled = false;
                gv_payfiles.Enabled = false;
                ///chk_delete.CheckState = CheckState.Unchecked;
                label6.Text = progressBar2.Maximum.ToString();
                if (chk_delete.CheckState == CheckState.Checked)  //// если флаг на удаление
                {
                    //// удаляем все платежи с таким брикетом и датам приемочном файле
                    for (int i = 0; i < files.Count; i++)
                    {
                        ///FFILE = DBF.Execute(@"select * from " + files.ElementAt(i).ToString(), files.ElementAt(i).ToString());
                        dbf.FilePath = files.ElementAt(i).ToString();
                        dbf.ReadDBF();
                        ///загрузка имен полей для данного брикета(файла)
                        db_com.CommandText = "select * from Abonuk.dbo.priemopt where bric=" + brik.ElementAt(i).ToString();
                        db_read = db_com.ExecuteReader();
                        if (!db_read.HasRows)
                        {
                            db_read.Close();
                            MessageBox.Show("Не найдены описания для удаления файла:" + Path.GetFileName(files.ElementAt(i).ToString()) + "\nБрикет: " + brik.ElementAt(i).ToString() + "\nПуть: " + files.ElementAt(i).ToString() + "\nФайл будет пропущен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            db_read.Read();
                            LIC = db_read["lic"].ToString();
                            SUMMA = db_read["summa"].ToString();
                            DATA = db_read["data"].ToString();
                            FSKOD = db_read["fskod"].ToString();
                            SHOL1 = db_read["shol1"].ToString();
                            SHOL2 = db_read["shol2"].ToString();
                            SHOL3 = db_read["shol3"].ToString();
                            SGOR1 = db_read["sgor1"].ToString();
                            SGOR2 = db_read["sgor2"].ToString();
                            SGOR3 = db_read["sgor3"].ToString();
                            db_read.Close();
                            /// определяем даты в файле
                            DataRow[] foundRows;
                            bool next_year = false;
                            if (CurMonth == "12") next_year = true;
                            /*///foundRows = FFILE.Select(DATA + " < '01/" + MM(Convert.ToString(Convert.ToInt16(CurMonth) + 1)) + "/" + CurYear + "' and " + DATA + " >= '01/" + CurMonth + "/" + CurYear + "'");*/
                            string[] date_pay = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                            if (brik.ElementAt(i).ToString() != "921" && brik.ElementAt(i).ToString() != "922" && brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                            {
                                // -- Для DBF -- 
                                foundRows = dbf.Table.Select(DATA + " < '01/" + MM(Convert.ToString(Convert.ToInt16((CurMonth == "12" ? "00" : CurMonth)) + 1)) + "/" + (next_year ? Convert.ToInt32(CurYear + 1).ToString() : CurYear) + "' and " + DATA + " >= '01/" + CurMonth + "/" + CurYear + "'");
                                foreach (DataRow row in foundRows)
                                {
                                    string date = row[DATA].ToString();
                                    if (!date_pay.Contains(date))
                                    {
                                        for (int z = 0; z < 15; z++) if (date_pay[z] == "") { date_pay[z] = date; break; }
                                    }
                                }
                            }
                            else
                            {
                                // -- Для текстового файла от Сбера --
                                int j = 0;
                                string line;
                                file = new StreamReader(files[i]);
                                string DocDates = "";
                                try
                                {
                                    while ((line = file.ReadLine()) != null)
                                    {
                                        if (line[0] != '=')
                                        {
                                            // -- смотрим даты платежей --
                                            if (DocDates == "")
                                            {
                                                DocDates = line.Substring(0, 10);
                                                date_pay[j] = DocDates;
                                            }
                                            if (date_pay[j] != DocDates)
                                                date_pay[++j] = DocDates;
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    MessageBox.Show(String.Format("Ошибка при чтении сберовского файла {0}! {1}", files[i], err.Message));
                                }
                                finally
                                {
                                    file.Close();
                                    file.Dispose();
                                }
                            }
                            for (int z = 0; z < 15; z++)
                            {
                                if (date_pay[z] != "")
                                {
                                    string dd = date_pay[z].Replace(".", "-");
                                    db_com.CommandText = "delete from abon.dbo.pos" + frmMain.CurPer + " where (brik=" + brik.ElementAt(i) + ") and (pach="+pach.ElementAt(i)+") and (data_p=convert(DATETIME,'" + dd + "',104))";
                                    db_com.ExecuteNonQuery();
                                    db_com.CommandText = "delete from abonuk.dbo.pos" + frmMain.CurPer + " where (brik=" + brik.ElementAt(i) + ") and (pach=" + pach.ElementAt(i) + ") and (data_p=convert(DATETIME,'" + dd + "',104))";
                                    db_com.ExecuteNonQuery();
                                    db_com.CommandText = "delete from abon.dbo.posvod where brikpach=" + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString() + " and modirec = convert(datetime,'" + dd + "',104) and poscur='pos" + frmMain.CurPer + "' and prim_code=18";
                                    db_com.ExecuteNonQuery();
                                    db_com.CommandText = "delete from abonuk.dbo.posvod where brikpach=" + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString() + " and modirec = convert(datetime,'" + dd + "',104) and poscur='pos" + frmMain.CurPer + "' and prim_code=18";
                                    db_com.ExecuteNonQuery();
                                    ///db_com.CommandText = "delete from abonuk.dbo.payerror" + frmMain.CurPer + " where brik=" + brik.ElementAt(i) + " and date_pay = convert(datetime,'" + dd + "',104) and pay_file='" + files.ElementAt(i) + "'";
                                    ///db_com.ExecuteNonQuery();
                                    ///
                                    db_com.CommandText = "select * from abon.dbo.zErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zDate=convert(datetime,'" + date_pay[z] + "',104) and zBrik=" + brik.ElementAt(i).ToString() + "";
                                    db_read = db_com.ExecuteReader();
                                    if (db_read.HasRows)
                                    {
                                        db_read.Read();
                                        string zIDFile_DEl = db_read["zIDFile"].ToString();
                                        db_read.Close();
                                        db_com.CommandText = "delete from abon.dbo.zErrFiles where zIDFile=" + zIDFile_DEl;
                                        db_com.ExecuteNonQuery();
                                    }
                                    db_read.Close();
                                }
                            }
                            //// удаляем так же лог с ошибками
                            for (int z = 0; z < 15; z++)
                            {
                                if (date_pay[z] != "")
                                {
                                    db_com.CommandText = "select * from abon.dbo.zErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zDate=convert(datetime,'" + date_pay[z] + "',104)  and zBrik=" + brik.ElementAt(i).ToString() + "";
                                    db_read = db_com.ExecuteReader();
                                    if (db_read.HasRows)
                                    {
                                        db_read.Read();
                                        string zIDFile_DEl = db_read["zIDFile"].ToString();
                                        db_read.Close();
                                        db_com.CommandText = "delete from abon.dbo.zErrFiles where zIDFile=" + zIDFile_DEl;
                                        db_com.ExecuteNonQuery();
                                    }
                                    db_read.Close();
                                }
                            }
                        }
                    }
                    goto END_PRIEM;
                }
                //// начинаем прием всех файлов по очереди
                for (int i = 0; i < files.Count; i++)
                {
                    // -- Максимальный размер ползунка приравниваем к количеству платежей --
                    progressBar1.Maximum = count_p.ElementAt(i);
                    // -- Пишем около ползунка сколько всего лицевых --
                    label5.Text = progressBar1.Maximum.ToString();
                    Application.DoEvents();
                    // -- выдираем имя файла --
                    lbl_file.Text = Path.GetFileName(files.ElementAt(i));
                    // -- сохраняем количество записей в файле --
                    lbl_count.Text = progressBar1.Maximum.ToString();
                    // хз
                    Boolean ZFileIs = false;
                    zDateFile = "";
                    // -- начальное значение ползунка в 0 --
                    progressBar1.Value = 0;
                    // -- открываем ДБФ файл --
                    //dbf.FilePath = files.ElementAt(i).ToString();
                    //if (pach.ElementAt(i).ToString() != "921")
                    //    dbf.ReadDBF();
                    //else
                    //    file = new StreamReader(files[i]);
                    ///загрузка имен полей для данного брикета(файла)
                    db_com.CommandText = "select * from Abonuk.dbo.priemopt where bric=" + brik.ElementAt(i).ToString();
                    db_read = db_com.ExecuteReader();
                    if (!db_read.HasRows)
                    {
                        db_read.Close();
                        MessageBox.Show("Не найдены описания для принятия файла:" + Path.GetFileName(files.ElementAt(i).ToString()) + "\nБрикет: " + brik.ElementAt(i).ToString() + "\nПуть: " + files.ElementAt(i).ToString() + "\nФайл будет пропущен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        //// начинается прием конкретного файла
                    {
                        db_read.Close(); ///закрывем чтение, откроем его после удаления
                        /////удаляем старые записи о приеме файла

                        //// удаляем все платежи с таким брикетом и датам приемочном файле
                        {
                            dbf.FilePath = files.ElementAt(i).ToString();
                            if (brik.ElementAt(i).ToString() != "921" && brik.ElementAt(i).ToString() != "922" && brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                dbf.ReadDBF();
//                            else
//                                file = new StreamReader(files[i]);
                            ///загрузка имен полей для данного брикета(файла)
                            db_com.CommandText = "select * from Abonuk.dbo.priemopt where bric=" + brik.ElementAt(i).ToString();
                            db_read = db_com.ExecuteReader();
                            if (!db_read.HasRows)
                            {
                                db_read.Close();
                                MessageBox.Show("Не найдены описания для удаления файла:" + Path.GetFileName(files.ElementAt(i).ToString()) + "\nБрикет: " + brik.ElementAt(i).ToString() + "\nПуть: " + files.ElementAt(i).ToString() + "\nФайл будет пропущен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                            else
                            {
                                db_read.Read();
                                LIC = db_read["lic"].ToString();
                                SUMMA = db_read["summa"].ToString();
                                DATA = db_read["data"].ToString();
                                FSKOD = db_read["fskod"].ToString();
                                SHOL1 = db_read["shol1"].ToString();
                                SHOL2 = db_read["shol2"].ToString();
                                SHOL3 = db_read["shol3"].ToString();
                                SGOR1 = db_read["sgor1"].ToString();
                                SGOR2 = db_read["sgor2"].ToString();
                                SGOR3 = db_read["sgor3"].ToString();
                                db_read.Close();
                                /// определяем даты в файле
                                DataRow[] foundRows;
                                bool next_year = false;
                                if (CurMonth == "12") next_year = true;
                                /*///foundRows = FFILE.Select(DATA + " < '01/" + MM(Convert.ToString(Convert.ToInt16(CurMonth) + 1)) + "/" + CurYear + "' and " + DATA + " >= '01/" + CurMonth + "/" + CurYear + "'");*/
                                string[] date_pay = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
                                if (brik.ElementAt(i).ToString() != "921" && brik.ElementAt(i).ToString() != "922" && brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                {
                                    // -- Для DBF -- 
                                    //Int32 q1 = Convert.ToInt32(CurMonth == "12" ? "00" : CurMonth) + 1;
                                    //string q2 = Convert.ToString(q1);
                                    //string q3 = MM(q2);
                                    //string sstrrr = DATA + " < '01." + q3 + "." + (next_year ? (Convert.ToInt32(CurYear) + 1).ToString() : CurYear) + "' and " + DATA + " >= '01." + CurMonth + "." + CurYear + "'";
                                    string sstrrr = DATA + " < '01." + MM(Convert.ToString(Convert.ToInt32(CurMonth == "12" ? "00" : CurMonth) + 1)) + "." + (next_year ? (Convert.ToInt32(CurYear) + 1).ToString() : CurYear) + "' and " + DATA + " >= '01." + CurMonth + "." + CurYear + "'";
                                    foundRows = dbf.Table.Select(DATA + " < '01." + MM(Convert.ToString(Convert.ToInt32(CurMonth == "12" ? "00" : CurMonth) + 1)) + "." + (next_year ? (Convert.ToInt32(CurYear) + 1).ToString() : CurYear) + "' and " + DATA + " >= '01." + CurMonth + "." + CurYear + "'");
                                    foreach (DataRow row in foundRows)
                                    {
                                        string date = row[DATA].ToString();
                                        if (!date_pay.Contains(date))
                                        {
                                            for (int z = 0; z < 15; z++) if (date_pay[z] == "") { date_pay[z] = date; break; }
                                        }
                                    }
                                }
                                else
                                {

                                    // -- Для текстового файла от Сбера --
                                    int j = 0;

                                    string line;
                                    file = new StreamReader(files[i]);
                                    string DocDates = "";
                                    try
                                    {
                                        while ((line = file.ReadLine()) != null)
                                        {
                                            if (line[0] != '=')
                                            {
                                                // -- смотрим даты платежей --
                                                if (DocDates == "")
                                                {
                                                    DocDates = line.Substring(0, 10);
                                                    date_pay[j] = DocDates;
                                                }
                                                if (date_pay[j] != DocDates)
                                                    date_pay[++j] = DocDates;
                                            }
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        MessageBox.Show(String.Format("Ошибка при чтении сберовского файла {0}! {1}", files[i], err.Message));
                                    }
                                    finally
                                    {
                                        file.Close();
                                        file.Dispose();
                                    }
                                }
                                for (int z = 0; z < 15; z++)
                                {
                                    if (date_pay[z] != "")
                                    {
                                        string dd = date_pay[z].Replace(".", "-");
                                        db_com.CommandText = "delete from abon.dbo.pos" + frmMain.CurPer + " where (brik=" + brik.ElementAt(i) + ") and (pach=" + pach.ElementAt(i) + ") and (data_p=convert(DATETIME,'" + dd + "',104))";
                                        db_com.ExecuteNonQuery();
                                        db_com.CommandText = "delete from abonuk.dbo.pos" + frmMain.CurPer + " where (brik=" + brik.ElementAt(i) + ") and (pach=" + pach.ElementAt(i) + ") and (data_p=convert(DATETIME,'" + dd + "',104))";
                                        db_com.ExecuteNonQuery();
                                        db_com.CommandText = "delete from abon.dbo.posvod where brikpach=" + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString() + " and modirec = convert(datetime,'" + dd + "',104) and poscur='pos" + frmMain.CurPer + "' and prim_code=18";
                                        db_com.ExecuteNonQuery();
                                        db_com.CommandText = "delete from abonuk.dbo.posvod where brikpach=" + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString() + " and modirec = convert(datetime,'" + dd + "',104) and poscur='pos" + frmMain.CurPer + "' and prim_code=18";
                                        db_com.ExecuteNonQuery();
                                        ///db_com.CommandText = "delete from abonuk.dbo.payerror" + frmMain.CurPer + " where brik=" + brik.ElementAt(i) + " and date_pay = convert(datetime,'" + dd + "',104) and pay_file='" + files.ElementAt(i) + "'";
                                        ///db_com.ExecuteNonQuery();
                                        ///
                                        db_com.CommandText = "select * from abon.dbo.zErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zDate=convert(datetime,'" + date_pay[z] + "',104) and zBrik=" + brik.ElementAt(i).ToString() + "";
                                        db_read = db_com.ExecuteReader();
                                        if (db_read.HasRows)
                                        {
                                            db_read.Read();
                                            string zIDFile_DEl = db_read["zIDFile"].ToString();
                                            db_read.Close();
                                            db_com.CommandText = "delete from abon.dbo.zErrFiles where zIDFile=" + zIDFile_DEl;
                                            db_com.ExecuteNonQuery();
                                        }
                                        db_read.Close();
                                    }
                                }
                                //// удаляем так же лог с ошибками
                                for (int z = 0; z < 15; z++)
                                {
                                    if (date_pay[z] != "")
                                    {
                                        db_com.CommandText = "select * from abon.dbo.zErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zDate=convert(datetime,'" + date_pay[z] + "',104)  and zBrik=" + brik.ElementAt(i).ToString() + "";
                                        db_read = db_com.ExecuteReader();
                                        if (db_read.HasRows)
                                        {
                                            db_read.Read();
                                            string zIDFile_DEl = db_read["zIDFile"].ToString();
                                            db_read.Close();
                                            db_com.CommandText = "delete from abon.dbo.zErrFiles where zIDFile=" + zIDFile_DEl;
                                            db_com.ExecuteNonQuery();
                                        }
                                        db_read.Close();
                                    }
                                }
                            }
                        }

                        if (chk_delete.CheckState == CheckState.Unchecked)
                        {
                        /////продолжаем прием
                        db_com.CommandText = "select * from Abonuk.dbo.priemopt where bric=" + brik.ElementAt(i).ToString();
                        db_read = db_com.ExecuteReader();
                        db_read.Read();
                        LIC = db_read["lic"].ToString();
                        SUMMA = db_read["summa"].ToString();
                        DATA = db_read["data"].ToString();
                        FSKOD = db_read["fskod"].ToString();
                        SHOL1 = db_read["shol1"].ToString();
                        SHOL2 = db_read["shol2"].ToString();
                        SHOL3 = db_read["shol3"].ToString();
                        SGOR1 = db_read["sgor1"].ToString();
                        SGOR2 = db_read["sgor2"].ToString();
                        SGOR3 = db_read["sgor3"].ToString();
                        db_read.Close();

                        /// определяем даты в файле
                        //DataRow[] nextper;
                        //bool next_year = false;
                        //if (CurMonth == "12") next_year = true;
                        //    nextper = dbf.Table.Select(DATA + " > '01/" + MM(Convert.ToString(Convert.ToInt16(CurMonth=="12"?"00":CurMonth)+1)) + "/" + ((next_year)?(Convert.ToInt32(CurYear)+1).ToString():CurYear) + "'");
                        //foreach (DataRow nextdate in nextper)
                        //{
                        //    ///раз мы здесь, значит есть
                        //    nextmonth.Add(true);
                        //}
                        ////получаем данные из файла и начинаем обработку
                        DataRow[] foundRows = {};
                        if (brik.ElementAt(i).ToString() != "921" && brik.ElementAt(i).ToString() != "922" && brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                        {
                            foundRows = dbf.Table.Select(DATA + " < '01/" + MM(Convert.ToString(Convert.ToInt16(CurMonth == "12" ? "00" : CurMonth) + 1)) + "/" + (CurMonth == "12" ? (Convert.ToInt32(CurYear) + 1).ToString() : CurYear) + "' and " + DATA + " >= '01/" + CurMonth + "/" + CurYear + "'");
                        }
                        else
                        {
                            DataTable table;
                            table = MakeNamesTable();
                            int j = 0;
                            string line;
                            file = new StreamReader(files[i], System.Text.Encoding.Default);
                            try
                            {
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line[0] != '=')
                                    {
                                        Array.Resize(ref foundRows, foundRows.Length + 1);
                                        foundRows[foundRows.Length - 1] = table.NewRow();
                                        foundRows[foundRows.Length - 1][DATA] = line.Substring(0, 10);
                                        for (int l = 0; l < 5; l++)
                                            line = line.Substring(line.IndexOf(";") + 1, line.Length - line.IndexOf(";") - 1);
                                        string ll = line;
                                        // -- Если платеж от Энергосбыта --
//                                        if(line.Substring(0, 1) != "0" && line.Substring(0, 1) != "1")
                                        if (line.ToUpper().IndexOf(";ВОДОСНАБЖЕНИЕ #") >= 0)
                                            {
                                                foundRows[foundRows.Length - 1][LIC] = line.Substring(line.ToUpper().IndexOf(";ВОДОСНАБЖЕНИЕ #") + 16, 10);
                                            if (line.ToUpper().IndexOf(";ХВС-1;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ХВС-1;") + 1, line.Length - line.ToUpper().IndexOf(";ХВС-1;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL1] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else 
                                                foundRows[foundRows.Length - 1][SHOL1] = "0";
                                            if (line.ToUpper().IndexOf(";ХВС-2;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ХВС-2;") + 1, line.Length - line.ToUpper().IndexOf(";ХВС-2;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL2] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SHOL2] = "0";
                                            if (line.ToUpper().IndexOf(";ХВС-3;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ХВС-3;") + 1, line.Length - line.ToUpper().IndexOf(";ХВС-3;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL3] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SHOL3] = "0";
                                            if (line.ToUpper().IndexOf(";ГВС-1;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ГВС-1;") + 1, line.Length - line.ToUpper().IndexOf(";ГВС-1;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR1] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR1] = "0";
                                            if (line.ToUpper().IndexOf(";ГВС-2;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ГВС-2;") + 1, line.Length - line.ToUpper().IndexOf(";ГВС-2;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR2] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR2] = "0";
                                            if (line.ToUpper().IndexOf(";ГВС-3;") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";ГВС-3;") + 1, line.Length - line.ToUpper().IndexOf(";ГВС-3;") - 1);
                                                for (int l = 0; l < 2; l++)
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR3] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR3] = "0";
                                        }
                                        else
                                        // -- Иначе, если платеж наш --
                                        {
                                            foundRows[foundRows.Length - 1][LIC] = line.Substring(0, 10);
                                            if (line.ToUpper().IndexOf(";Х1(КУХ)") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Х1(КУХ)") + 1, line.Length - line.ToUpper().IndexOf(";Х1(КУХ)") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL1] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SHOL1] = "0";
                                            if (line.ToUpper().IndexOf(";Х2(C/У)") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Х2(C/У)") + 1, line.Length - line.ToUpper().IndexOf(";Х2(C/У)") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL2] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SHOL2] = "0";
                                            if (line.ToUpper().IndexOf(";Х3") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Х3") + 1, line.Length - line.ToUpper().IndexOf(";Х3") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SHOL3] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SHOL3] = "0";
                                            if (line.ToUpper().IndexOf(";Г1(КУХ)") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Г1(КУХ)") + 1, line.Length - line.ToUpper().IndexOf(";Г1(КУХ)") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR1] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR1] = "0";
                                            if (line.ToUpper().IndexOf(";Г2(C/У)") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Г2(C/У)") + 1, line.Length - line.ToUpper().IndexOf(";Г2(C/У)") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR2] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR2] = "0";
                                            if (line.ToUpper().IndexOf(";Г3") >= 0)
                                            {
                                                ll = line.Substring(line.ToUpper().IndexOf(";Г3") + 1, line.Length - line.ToUpper().IndexOf(";Г3") - 1);
                                                ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                if (brik.ElementAt(i).ToString() != "956" && brik.ElementAt(i).ToString() != "955")
                                                    ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                                foundRows[foundRows.Length - 1][SGOR3] = ll.Substring(0, ll.IndexOf(";")) == "" ? "0" : ll.Substring(0, ll.IndexOf(";"));
                                            }
                                            else
                                                foundRows[foundRows.Length - 1][SGOR3] = "0";
                                        }
                                        foundRows[foundRows.Length - 1][FSKOD] = "";
                                        for (int l = 0; l < 2; l++)
                                            ll = ll.Substring(ll.IndexOf("[!]") + 3, ll.Length - ll.IndexOf("[!]") - 3);
                                        for (int l = 0; l < 2; l++)
                                            ll = ll.Substring(ll.IndexOf(";") + 1, ll.Length - ll.IndexOf(";") - 1);
                                        foundRows[foundRows.Length - 1]["SUMMA"] = ll.Substring(0, ll.IndexOf(";")).Replace(',','.');
                                        table.Rows.Add(foundRows[foundRows.Length - 1]);
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                MessageBox.Show(String.Format("Ошибка при чтении текстового файла {0}! {1}", files[i], err.Message));
                            }
                            finally
                            {
                                file.Close();
                                file.Dispose();
                            }
                        }

                            foreach (DataRow row in foundRows)
                        {
                            DLIC = "";
                            DERROR = 0;
                            DVERROR = 0;
                            DVODOMER = 0;
                            DGROUPP = 0;
                            COMVOD = 0;

                            FLIC = row[LIC].ToString().Trim();
                            FFSKOD = row[FSKOD].ToString() + "";
                            FSUMMA = Convert.ToDouble(row[SUMMA].ToString());
                            FDATE = row[DATA].ToString().Replace(".", "-").Substring(0, 10);
                            FH1 = Convert.ToDouble(row[SHOL1].ToString());
                            FH2 = Convert.ToDouble(row[SHOL2].ToString());
                            FH3 = Convert.ToDouble(row[SHOL3].ToString());
                            FG1 = Convert.ToDouble(row[SGOR1].ToString());
                            FG2 = Convert.ToDouble(row[SGOR2].ToString());
                            FG3 = Convert.ToDouble(row[SGOR3].ToString());
                            //////определяем отношение лицевого счета к базе
                            try
                            {
                                db_com.CommandText = "select * from " +
                                                     "(select isnull(v.bUK,0) as buk, isnull(a.gruppa3,0) as gruppa3, '1' as base from abonuk.dbo.abonent" + frmMain.CurPer + " a " +
                                                     "inner join abonuk.dbo.SpVedomstvo v on a.KodVedom = v.ID " +
                                                     "where a.Lic = '2'+right('" + FLIC + "',9) " +
                                                     "union all " +
                                                     "select isnull(v.bUK,0) as buk, isnull(a.gruppa3,0) as gruppa3, '0' as base from abon.dbo.abonent" + frmMain.CurPer + " a " +
                                                     "inner join abon.dbo.SpVedomstvo v on a.KodVedom = v.ID " +
                                                     "where a.Lic = '1'+right('" + FLIC + "',9)) u";
                                db_read = db_com.ExecuteReader();
                            }
                            catch
                            {
                            }
                            int buk = -1, gruppa3 = -1, base_ = 0;

                            if (db_read.HasRows)
                            {

                                int c_op = 0;
                                while (db_read.Read())
                                {
                                    if (c_op == 0)
                                    {
                                        if (db_read["buk"].ToString() == "True") buk = 1; else buk = 0;
                                        if (db_read["gruppa3"].ToString() == "True") gruppa3 = 1; else gruppa3 = 0;
                                        if (db_read["base"].ToString() == "1") base_ = 1; else base_ = 0;
                                    }


                                    if (c_op == 1)
                                    {

                                        if ((db_read["buk"].ToString() == "True") & (buk == 1))
                                        {
                                            DBASE = "AbonUK";
                                            DLIC = "2" + row[LIC].ToString().Remove(0, 1);
                                            if (gruppa3 == 1)
                                            {
                                                DGROUPP = 1;
                                                ///DERROR = 2 ;//// ErrorCode[2];
                                            }
                                        }
                                        if ((db_read["buk"].ToString() == "True") & (buk == 0))
                                        {
                                            DBASE = "Abon";
                                            DLIC = LIC9;
                                            DERROR = 22;/// ErrorCode[0];
                                        }
                                        if ((db_read["buk"].ToString() == "False") & (buk == 0))
                                        {
                                            DBASE = "Abon";
                                            DLIC = "1" + row[LIC].ToString().Remove(0, 1);
                                            if (db_read["gruppa3"].ToString() == "True")
                                            {
                                                DGROUPP = 1;
                                            }
                                        }
                                        if ((db_read["buk"].ToString() == "False") & (buk == 1))
                                        {
                                            DBASE = "Abon";
                                            DLIC = LIC9;
                                            DERROR = 26;/// ErrorCode[0];
                                        }
                                    }
                                    c_op++;
                                }
                                if (c_op == 1)
                                {
                                    if ((buk == 1) & (base_ == 1)) { DLIC = "2" + row[LIC].ToString().Remove(0, 1); DBASE = "Abonuk"; }
                                    if ((buk == 0) & (base_ == 0)) { DLIC = "1" + row[LIC].ToString().Remove(0, 1); DBASE = "Abon"; }
                                    if ((buk == 1) & (base_ == 0)) { DLIC = "1" + row[LIC].ToString().Remove(0, 1); DERROR = 24; DBASE = "Abon"; }
                                    if ((buk == 0) & (base_ == 1)) { DLIC = "2" + row[LIC].ToString().Remove(0, 1); DERROR = 24; DBASE = "Abonuk"; }
                                    if (gruppa3 == 1) { DGROUPP = 1; }//// ErrorCode[2];
                                }
                            }
                            else
                            {
                                DBASE = "Abon";
                                DLIC = LIC9;
                                DERROR = 22; //// ErrorCode[1];
                            }

                            db_read.Close();

                            //// создаем новый файл ошибок
                            if (!ZFileIs)
                            {
                                if (zDateFile == "") zDateFile = FDATE.Replace(".", "-").Substring(0, 10);
                                db_com.CommandText = "insert into abon.dbo.zErrFiles(zFileName,zDate,zStatus,zCount,zGSum,zDateLoad,zBrik) values('" + Path.GetFileName(files.ElementAt(i)) + "',convert(datetime,'" + zDateFile + "',104),1," + count_p.ElementAt(i).ToString() + "," + summ_p.ElementAt(i).ToString().Replace(",",".") + ",convert(datetime,getdate(),104)," + brik.ElementAt(i).ToString() + ")";
                                db_com.ExecuteNonQuery();
                                db_com.CommandText = "select zIDFile from abon.dbo.ZErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zCount=" + count_p.ElementAt(i).ToString() + " and zBrik=" + brik.ElementAt(i).ToString() + " and zDate=convert(datetime,'" + zDateFile + "',104)";
                                db_read = db_com.ExecuteReader();
                                db_read.Read();
                                zIDFile = db_read["zIDFile"].ToString();
                                db_read.Close();
                                ZFileIs = true;
                            }
                            else
                                ///если файл существует но изменилась дата платежа
                            {
                                ////ищем такой же файл с новой датой
                                zDateFile = FDATE.Replace(".", "-").Substring(0, 10);
                                db_com.CommandText = "select zIDFile from abon.dbo.zErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zCount=" + count_p.ElementAt(i).ToString() + " and zBrik=" + brik.ElementAt(i).ToString() + " and zDate=convert(datetime,'" + zDateFile + "',104)";
                                db_read = db_com.ExecuteReader();
                                if (db_read.HasRows)
                                /// такой файл существует и пишем ошибки в него
                                {
                                    db_read.Read();
                                    zIDFile = db_read["zIDFile"].ToString();
                                    db_read.Close();
                                    ZFileIs = true;
                                }
                                else
                                    //// нет такой даты, создаем
                                {
                                    db_read.Close();
                                    zDateFile = FDATE.Replace(".", "-").Substring(0, 10);
                                    db_com.CommandText = "insert into abon.dbo.zErrFiles(zFileName,zDate,zStatus,zCount,zGSum,zDateLoad,zBrik) values('" + Path.GetFileName(files.ElementAt(i)) + "',convert(datetime,'" + zDateFile + "',104),1," + count_p.ElementAt(i).ToString() + "," + summ_p.ElementAt(i).ToString().Replace(",",".") + ",convert(datetime,getdate(),104)," + brik.ElementAt(i).ToString() + ")";
                                    db_com.ExecuteNonQuery();
                                    db_com.CommandText = "select zIDFile from abon.dbo.ZErrFiles where zFileName = '" + Path.GetFileName(files.ElementAt(i)) + "' and zCount=" + count_p.ElementAt(i).ToString() + " and zBrik=" + brik.ElementAt(i).ToString() + " and zDate=convert(datetime,'" + zDateFile + "',104)";
                                    db_read = db_com.ExecuteReader();
                                    db_read.Read();
                                    zIDFile = db_read["zIDFile"].ToString();
                                    db_read.Close();
                                    ZFileIs = true;
                                }
                                db_read.Close();
                            }
                            ////запуск транзакции
                            trans = frmMain.db_con.BeginTransaction();
                            db_com.Transaction = trans;

                            db_com.CommandText = "select isnull(MAX(NKvit),0) as mk from " + DBASE + ".dbo.pos" + frmMain.MaxCurPer;
                            db_read = db_com.ExecuteReader();
                            db_read.Read();
                            string MAX_KVIT = (Convert.ToInt32(db_read["mk"].ToString()) + 1).ToString();
                            db_read.Close();



                            try
                            {
                                db_com.CommandText = "insert into " + DBASE + ".dbo.pos" + frmMain.CurPer + "(brik,pach,data_p,opl,lic,poliv,prim,NKvit,PerOplReal,PerOpl) values(" + brik.ElementAt(i) + ","+pach.ElementAt(i)+",convert(datetime,'" + FDATE + "',104)," + FSUMMA.ToString() + ",'" + DLIC + "',0,''," + MAX_KVIT + ",'" + CurYear + "/" + CurMonth + "','" + CurYear + "/" + CurMonth + "')";
                                command_ = db_com.CommandText;
                                db_com.ExecuteNonQuery();



                                if (DERROR == 22) ////заносим оплату на 9ку и ошибку в отчет
                                {
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if (DERROR == 26) ////заносим оплату на 9ку и ошибку в отчет
                                {
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if (DGROUPP == 1)
                                {
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + ",1,convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                }
                                //// проверка показаний
                                db_com.CommandText = "select isnull(vvodomer,0) as vvodomer, isnull(vodkod,0) as vodkod from " + DBASE + ".dbo.abonent" + frmMain.CurPer + " where lic='" + DLIC + "'";
                                db_read = db_com.ExecuteReader();
                                db_read.Read();
                                if (db_read["vvodomer"].ToString() == "True") DVODOMER = 1;
                                else if ((db_read["vvodomer"].ToString() == "False") & (db_read["vodkod"].ToString() != "0")) DVODOMER = 1;
                                else DVODOMER = 0;
                                COMVOD = Convert.ToInt32(db_read["vodkod"].ToString());
                                db_read.Close();

                                db_com.CommandText = "select KubH1n, case when H1 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as H1, KubH2n,case when H2 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as H2, KubH3n,case when H3 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as H3, " +
                                                     "KubG1n, case when G1 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as G1, KubG2n,case when G2 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as G2, KubG3n,case when G3 < Convert(DATETIME,'" + FDATE + "',104) THEN  0 ELSE 1 END as G3 " +
                                                     "from " +
                                                     "(select KubH1n, isnull(H1,0) AS H1, KubH2n,isnull(H2,0) AS H2,KubH3n,isnull(H3,0) AS H3, KubG1n,isnull(G1,0) AS G1,KubG2n, isnull(G2,0) AS G2,KubG3n,isnull(G3,0) AS G3 " +
                                                     "from " + DBASE + ".dbo.posvod b " +
                                                     "inner join " +
                                                     "(SELECT     ISNULL(MAX(RowID), 0) AS ROWID " +
                                                      "FROM         " + DBASE + ".dbo.PosVod " +
                                                      "WHERE     (Lic = '" + DLIC + "')) a on a.ROWID = b.RowID " +
                                                      "inner join " +
                                                      DBASE + ".dbo.VodomerDate" + frmMain.CurPer + " c on c.Lic = '" + DLIC + "') d";
                                db_read = db_com.ExecuteReader();
                                db_read.Read();
                                if (db_read.HasRows)
                                {
                                    DH1 = Convert.ToDouble(db_read["kubh1n"].ToString());
                                    DDH1 = Convert.ToInt16(db_read["h1"].ToString());
                                    DH2 = Convert.ToDouble(db_read["kubh2n"].ToString());
                                    DDH2 = Convert.ToInt16(db_read["h2"].ToString());
                                    DH3 = Convert.ToDouble(db_read["kubh3n"].ToString());
                                    DDH3 = Convert.ToInt16(db_read["h3"].ToString());
                                    DG1 = Convert.ToDouble(db_read["kubG1n"].ToString());
                                    DDG1 = Convert.ToInt16(db_read["G1"].ToString());
                                    DG2 = Convert.ToDouble(db_read["kubG2n"].ToString());
                                    DDG2 = Convert.ToInt16(db_read["G2"].ToString());
                                    DG3 = Convert.ToDouble(db_read["kubG3n"].ToString());
                                    DDG3 = Convert.ToInt16(db_read["G3"].ToString());
                                    db_read.Close();
                                }
                                else
                                {
                                    db_read.Close();
                                    DH1 = DH2 = DH3 = DG1 = DG2 = DG3 = 0;
                                    DDH1 = DDH2 = DDH3 = DDG1 = DDG2 = DDG3 = 0;
                                }

                                if ((FH1 > (DH1 + 100)) || (FH2 > (DH2 + 100)) || (FH3 > (DH3 + 100)) || (FG1 > (DG1 + 100)) || (FG2 > (DG2 + 100)) || (FG3 > (DG3 + 100)))
                                {
                                    DVERROR = 19;
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if ((DVODOMER == 1 || COMVOD > 0) && ((FH1 == 0) && (FH2 == 0) && (FH3 == 0) && (FG1 == 0) && (FG2 == 0) && (FG3 == 0)))
                                {
                                    DVERROR = 20;///ErrorCode[5];
                                    //string zDB = "";
                                    //if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    //db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    //db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if ((DVODOMER == 0 && COMVOD == 0) && ((FH1 == 0) && (FH2 == 0) && (FH3 == 0) && (FG1 == 0) && (FG2 == 0) && (FG3 == 0)))
                                {
                                    DVERROR = 20;///ErrorCode[5];
                                    //string zDB = "";
                                    //if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    //db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    //db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if ((DVODOMER == 1 || COMVOD > 0) && ((FH1 < DH1) || (FH2 < DH2) || (FH3 < DH3) || (FG1 < DG1) || (FG2 < DG2) || (FG3 < DG3)) && ((FH1 != 0) || (FH2 != 0) || (FH3 != 0) || (FG1 != 0) ||(FG2 != 0) || (FG3 != 0)))
                                {
                                    DVERROR = 13;////ErrorCode[10];
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }


                                if (((FH1 != 0) && (DDH1 == 0)) || ((FH2 != 0) && (DDH2 == 0)) || ((FH3 != 0) && (DDH3 == 0)) || ((FG1 != 0) && (DDG1 == 0)) || ((FG2 != 0) && (DDG2 == 0)) || ((FG3 != 0) && (DDG3 == 0)))
                                {
                                    DVERROR = 14;
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }

                                if ((DVODOMER == 0 && COMVOD == 0) && ((FH1 != 0) || (FH2 != 0) || (FH3 != 0) || (FG1 != 0) || (FG2 != 0) || (FG3 != 0)))
                                {
                                    DVERROR = 10;///ErrorCode[5];
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }
                                /// есть водомер, нет показаний
                                if ((DVODOMER == 1 || COMVOD > 0) && ((FH1 == 0) && (FH2 == 0) && (FH3 == 0) && (FG1 == 0) && (FG2 == 0) && (FG3 == 0)))
                                {
                                    DVERROR = 20;///ErrorCode[5];
                                    //string zDB = "";
                                    //if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    //db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    //db_com.ExecuteNonQuery();
                                    goto END_ROW;
                                }
                                if ((DVERROR != 0))
                                {
                                    DVERROR = 17;///ErrorCode[5];
                                    string zDB = "";
                                    if (DBASE == "Abon") zDB = "1"; else zDB = "2";
                                    db_com.CommandText = "insert into abon.dbo.zErrDetail(zFile,zIDreason,zDTPack,zLic,zPeriod,zSum,zBarCode,ZDB,s_hol1,s_hol2,s_hol3,s_gor1,s_gor2,s_gor3,vvodomer,zIndex) values(" + zIDFile + "," + DVERROR + ",convert(datetime,'" + FDATE + "',104),'" + FLIC + "'," + CurYear + CurMonth + "," + FSUMMA.ToString() + ",'" + FFSKOD + "'," + zDB + "," + Convert.ToInt32(FH1).ToString() + "," + Convert.ToInt32(FH2).ToString() + "," + Convert.ToInt32(FH3).ToString() + "," + Convert.ToInt32(FG1).ToString() + "," + Convert.ToInt32(FG2).ToString() + "," + Convert.ToInt32(FG3).ToString() + "," + DVODOMER + ",'')";
                                    db_com.ExecuteNonQuery();
                                }

                                if (COMVOD > 0)
                                {
                                    db_com.CommandText = "select lic from " + DBASE + ".dbo.abonent" + frmMain.CurPer + " where vodkod=" + COMVOD.ToString();
                                    db_read = db_com.ExecuteReader();
                                    if (db_read.HasRows)
                                    {
                                        List<string> comlic = new List<string>();
                                        while (db_read.Read())
                                        {
                                            comlic.Add(db_read["lic"].ToString());
                                        }
                                        db_read.Close();
                                        for (int com = 0; com < comlic.Count; com++)
                                        {
                                            db_com.CommandText = "insert into " + DBASE + ".dbo.posvod(lic,ind,kubh1n,kubh1s,kubh2n,kubh2s,kubh3n,kubh3s,kubg1n,kubg1s,kubg2n,kubg2s,kubg3n,kubg3s,KubPn,KubPs,nach,peropl,poscur,lastper,modirec,vnv,vnk,vnv_l,vnk_l,meancube,prim_code,brikpach,vodkod) values('" + comlic.ElementAt(com).ToString() + "',0," + FH1.ToString() + "," + DH1.ToString() + "," + FH2.ToString() + "," + DH2.ToString() + "," + FH3.ToString() + "," + DH3.ToString() + "," + FG1.ToString() + "," + DG1.ToString() + "," + FG2.ToString() + "," + DG2.ToString() + "," + FG3.ToString() + "," + DG3.ToString() + ",0,0,0,'" + frmMain.CurPer.Substring(2, 4) + "','pos" + frmMain.CurPer + "'," + frmMain.CurPer + ",convert(datetime,'" + FDATE + "',104),0,0,0,0,0,18," + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString()+",0)";
                                            db_com.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        db_read.Close();
                                    }
                                }
                                else
                                {
                                    db_com.CommandText = "insert into " + DBASE + ".dbo.posvod(lic,ind,kubh1n,kubh1s,kubh2n,kubh2s,kubh3n,kubh3s,kubg1n,kubg1s,kubg2n,kubg2s,kubg3n,kubg3s,KubPn,KubPs,nach,peropl,poscur,lastper,modirec,vnv,vnk,vnv_l,vnk_l,meancube,prim_code,brikpach,vodkod) values('" + DLIC + "',0," + FH1.ToString() + "," + DH1.ToString() + "," + FH2.ToString() + "," + DH2.ToString() + "," + FH3.ToString() + "," + DH3.ToString() + "," + FG1.ToString() + "," + DG1.ToString() + "," + FG2.ToString() + "," + DG2.ToString() + "," + FG3.ToString() + "," + DG3.ToString() + ",0,0,0,'" + frmMain.CurPer.Substring(2, 4) + "','pos" + frmMain.CurPer + "'," + frmMain.CurPer + ",convert(datetime,'" + FDATE + "',104),0,0,0,0,0,18," + brik.ElementAt(i).ToString() + pach.ElementAt(i).ToString()+",0)";
                                    db_com.ExecuteNonQuery();
                                }

                            END_ROW:
                                trans.Commit();
                                try
                                {
                                    using (SqlConnection n_con = new SqlConnection())
                                    {
                                        n_con.ConnectionString = frmMain.db_con.ConnectionString;
                                        n_con.Open();
                                        CalculateWater.MainForm Calc = new CalculateWater.MainForm();
                                        if (DBASE == "Abon") Calc.Query(0, frmMain.MaxCurPer, DLIC);
                                        if (DBASE == "AbonUK") Calc.Query(1, frmMain.MaxCurPer, DLIC);
                                    }
                                }
                                catch {}
                            }
                            catch (Exception ex)
                            {
                                db_read.Close();
                                trans.Rollback();
                                MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            progressBar1.Increment(1);
                            label7.Text = progressBar1.Value.ToString();
                            progressBar2.Increment(1);
                            label8.Text = progressBar2.Value.ToString();
                            Application.DoEvents();
                        }
                        } /// обработка конкретной записи
            
                    } /// идет прием если все нормально
                    if (button2.Text != "Файлы на прием")
                    {
                        if (File.Exists(@"" + files.ElementAt(i).ToString().Replace("post", "arh")))
                        {
                            string path_ = Path.GetDirectoryName(files.ElementAt(i).ToString().Replace("post", "arh"));
                            string name_ = Path.GetFileName(files.ElementAt(i).ToString().Replace("post", "arh"));
                            File.Delete(@"" + files.ElementAt(i).ToString().Replace("post", "arh"));
                        }
                        string path = @"" + files.ElementAt(i).ToString();
                        string path2 = @"" + files.ElementAt(i).ToString().Replace("post", "arh");
                        File.Move(path, path2);
                    }
                } /// прием всех файлов по очеренди
            END_PRIEM:
                for (int i = 0; i < gv_payfiles.Rows.Count; i++)
                    gv_payfiles.Rows[i].Cells["get"].Value = "False";
                panel3.Visible = false;
                panel1.Enabled = true;
                panel2.Enabled = true;
                gv_payfiles.Enabled = true;
                chk_delete.CheckState = CheckState.Unchecked;
                frmPayGet_Shown(this, e);
            }
        }

//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
        private DataTable MakeNamesTable()
        {
            // Создаем новую таблицу
            DataTable namesTable = new DataTable("Names");
            // Добавляем типы полей в таблицу
            // ключевое поле
            DataColumn idColumn = new DataColumn();
            idColumn.DataType = System.Type.GetType("System.Int32");
            idColumn.ColumnName = "ID";
            idColumn.AutoIncrement = true;
            namesTable.Columns.Add(idColumn);
            // лицевой
            DataColumn idColumn1 = new DataColumn();
            idColumn1.DataType = System.Type.GetType("System.String");
            idColumn1.ColumnName = "LIC";
            idColumn1.DefaultValue = "LIC";
            namesTable.Columns.Add(idColumn1);
            // код
            DataColumn idColumn2 = new DataColumn();
            idColumn2.DataType = System.Type.GetType("System.String");
            idColumn2.ColumnName = "FSKOD";
            idColumn2.DefaultValue = "FSKOD";
            namesTable.Columns.Add(idColumn2);
            // сумма
            DataColumn idColumn3 = new DataColumn();
            idColumn3.DataType = System.Type.GetType("System.String");
            idColumn3.ColumnName = "SUMMA";
            idColumn3.DefaultValue = "SUMMA";
            namesTable.Columns.Add(idColumn3);
            // дата
            DataColumn idColumn4 = new DataColumn();
            idColumn4.DataType = System.Type.GetType("System.String");
            idColumn4.ColumnName = "DATA";
            idColumn4.DefaultValue = "DATA";
            namesTable.Columns.Add(idColumn4);
            // Х1
            DataColumn idColumn5 = new DataColumn();
            idColumn5.DataType = System.Type.GetType("System.String");
            idColumn5.ColumnName = "SHOL1";
            idColumn5.DefaultValue = "SHOL1";
            namesTable.Columns.Add(idColumn5);
            // Х2
            DataColumn idColumn6 = new DataColumn();
            idColumn6.DataType = System.Type.GetType("System.String");
            idColumn6.ColumnName = "SHOL2";
            idColumn6.DefaultValue = "SHOL2";
            namesTable.Columns.Add(idColumn6);
            // Х3
            DataColumn idColumn7 = new DataColumn();
            idColumn7.DataType = System.Type.GetType("System.String");
            idColumn7.ColumnName = "SHOL3";
            idColumn7.DefaultValue = "SHOL3";
            namesTable.Columns.Add(idColumn7);
            // Г1
            DataColumn idColumn8 = new DataColumn();
            idColumn8.DataType = System.Type.GetType("System.String");
            idColumn8.ColumnName = "SGOR1";
            idColumn8.DefaultValue = "SGOR1";
            namesTable.Columns.Add(idColumn8);
            // Г2
            DataColumn idColumn9 = new DataColumn();
            idColumn9.DataType = System.Type.GetType("System.String");
            idColumn9.ColumnName = "SGOR2";
            idColumn9.DefaultValue = "SGOR2";
            namesTable.Columns.Add(idColumn9);
            // Г3
            DataColumn idColumn10 = new DataColumn();
            idColumn10.DataType = System.Type.GetType("System.String");
            idColumn10.ColumnName = "SGOR3";
            idColumn10.DefaultValue = "SGOR3";
            namesTable.Columns.Add(idColumn10);
            // Create an array for DataColumn objects.
            DataColumn[] keys = new DataColumn[1];
            keys[0] = idColumn;
            namesTable.PrimaryKey = keys;
            // Return the new DataTable.
            return namesTable;
        }
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------

        private void gv_payfiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 0) | (e.ColumnIndex == 1))
            {
                if (gv_payfiles.Rows[e.RowIndex].Cells[0].Style.BackColor != Color.Red)
                {
                    if (gv_payfiles.Rows[e.RowIndex].Cells["count"].Value.ToString() != "0")
                    {
                        if (gv_payfiles.Rows[e.RowIndex].Cells["get"].Value.ToString() == "False") gv_payfiles.Rows[e.RowIndex].Cells["get"].Value = "True";
                        else gv_payfiles.Rows[e.RowIndex].Cells["get"].Value = "False";
                    }
                }
            }
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            frmPayGet_Shown(this, e);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            ////прерывание приема и откат текущего файла

            panel3.Visible = false;
            btn_Refresh.Enabled = true;
            btn_payget.Enabled = true;
            chk_delete.Enabled = true;
            chk_top.Enabled = true;
            frmPayGet_Shown(this, e);
        }

        private void chk_repeat_CheckStateChanged(object sender, EventArgs e)
        {
            if (chk_delete.CheckState == CheckState.Checked) chk_top.CheckState = CheckState.Unchecked;
        }

        private void chk_top_CheckStateChanged(object sender, EventArgs e)
        {
            if (chk_top.CheckState == CheckState.Checked) chk_delete.CheckState = CheckState.Unchecked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gv_payfiles.Rows.Count; i++)
                if (gv_payfiles.Rows[i].Cells[0].Style.BackColor != Color.Red)
                {
                    if (gv_payfiles.Rows[i].Cells["count"].Value.ToString() != "0")
                    {
                        gv_payfiles.Rows[i].Cells["get"].Value = "True";
                    }
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Принятые файлы")
            {
                label1.Text = "Принятые файлы";
                button2.Text = "Файлы на прием";
                chk_delete.CheckState = CheckState.Checked;
                chk_delete.Enabled = false;
                chk_top.CheckState = CheckState.Unchecked;
                chk_top.Enabled = false;
                SqlCommand com = new SqlCommand();
                com.Connection = con;

                List<string> path = new List<string>();
                List<string> brik = new List<string>();
                List<string> pach = new List<string>();
                List<string> vedom = new List<string>();
                com.CommandText = "select * from abonuk.dbo.priemopt where active=1";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        while (read.Read())
                        {
                            path.Add(read["path"].ToString());
                            brik.Add(read["bric"].ToString());
                            pach.Add(read["pach"].ToString());
                            vedom.Add(read["sys_name"].ToString());
                        }
                    }
                }
                gv_payfiles.Rows.Clear();
                com.CommandText = "select zIDFile, zfileName, zBrik,  zDate, zCount, zGSum, zDateLoad from abon.dbo.zerrFiles WHERE (zDate >= CONVERT(DATETIME, '" + frmMain.MaxCurPer.Substring(0, 4) + "-" + frmMain.CurPer.Substring(4, 2) + "-01 00:00:00', 102)) group by zDate,zfileName,zIDFile,  zBrik,   zCount, zGSum, zDateLoad";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        while (read.Read())
                        {
                            string[] row_ = { "False", read["zfilename"].ToString(), read["zbrik"].ToString(), read["zcount"].ToString(), read["zgsum"].ToString(), vedom.ElementAt(brik.FindIndex(item => item == read["zbrik"].ToString())), path.ElementAt(brik.FindIndex(item => item == read["zbrik"].ToString()))+"\\arch", pach.ElementAt(brik.FindIndex(item => item == read["zbrik"].ToString())) };
                            gv_payfiles.Rows.Add(row_);
                        }
                    }
                }
            }
            else
            {
                label1.Text = "Файлы на прием";
                button2.Text = "Принятые файлы";
                chk_delete.CheckState = CheckState.Unchecked;
                chk_delete.Enabled = true;
                chk_top.CheckState = CheckState.Unchecked;
                chk_top.Enabled = true;
                frmPayGet_Shown(this, e);
            }
        }


    }
}

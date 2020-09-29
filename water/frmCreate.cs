using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;
using Excel = Microsoft.Office.Interop.Excel;

namespace water
{
    public partial class frmCreate : Form
    {

        SqlConnection con = new SqlConnection();
        System.Data.SqlClient.SqlTransaction tran = null;
        Boolean can_close = false;
        Boolean IsInBase = false;
        Boolean IsInBaseA = false;

        Excel.Application excel;
        Excel.Workbook workbook;
        Excel.Worksheet sheet;
        Excel.Range cells;

        public frmCreate()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            
            try
            {
                con.Open();
            }
            catch
            {
                MessageBox.Show("Не возможно соединиться с базой данных", "Ошибка");
            }
        }

        private void chk_house_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_house.CheckState == CheckState.Checked)
            {
                this.countlic.Text = "001";
                groupBox2.Enabled = true;
                cmb_kot.SelectedIndex = 0;
            }
            else
            {
                this.countlic.Text = "000";
                groupBox2.Enabled = false;
            }
        }

        private void WorkbookBeforeClose(Excel.Workbook Wb, ref bool Cancel)
        {
            excel.Quit();
            sheet = null;
            System.GC.Collect();
            excel = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ///проверка на ошибки
            /// количество лицевых
            Boolean error = false;

            int count_lic = 0;
            double normv =0, normk=0;
            SqlCommand com = new SqlCommand();
            com.Connection = con;

            if (IsInBase && countlic.Text.Trim().Length == 0)
            {
                com.CommandText = @"select b.cnt as cnt, b.base_ as base_ from (
                                    select count(a.lic) as cnt, 0 as base_ from abon.dbo.abonent" + frmMain.MaxCurPer + @" a inner join abon.dbo.spvedomstvo v on a.kodvedom = v.id and v.buk=0 where a.str_code=@str and a.dom=@dom
                                    union all
                                    select count(a.lic), 1 as base_ from abonuk.dbo.abonent" + frmMain.MaxCurPer + @" a inner join abonuk.dbo.spvedomstvo v on a.kodvedom = v.id and v.buk=1 where a.str_code=@str and a.dom=@dom
                                    ) b where b.cnt > 0";
                com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                com.Parameters.AddWithValue("@dom", cmb_dom.Text);
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        read.Read();
                        count_lic = Convert.ToInt32(read["cnt"].ToString().Trim());
                        countlic.Text = count_lic.ToString();
                        IsInBaseA = (read["base_"].ToString().Trim() == "0" ? true : false);
                    }
                }
                com.Parameters.Clear();
            }
            else
            {
                error = !(countlic.Text.Replace(" ", "").Length > 0 && Convert.ToInt32(countlic.Text.Replace(" ", "")) >= 1 && !error);
                if (!error) count_lic = Convert.ToInt32(countlic.Text.Replace(" ", "")); else MessageBox.Show("Не указано количество лицевых счетов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            error = !(mnormv.Text.Replace(" ", "").Length > 1 && Convert.ToDouble(mnormv.Text.Replace(" ", "").Replace(",",".")) >= 1 && !error);
            if (!error) normv = Convert.ToDouble(mnormv.Text.Replace(" ", "").Replace(",",".")); else MessageBox.Show("Не указана норма по воде", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            error = !(mnormk.Text.Replace(" ", "").Length > 1 && Convert.ToDouble(mnormk.Text.Replace(" ", "").Replace(",", ".")) >= 1 && !error);
            if (!error) normk = Convert.ToDouble(mnormk.Text.Replace(" ", "").Replace(",", ".")); else MessageBox.Show("Не указана норма по канализации", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (!error)
            {
                excel = new Excel.Application(); //создаем COM-объект Excel
                excel.Visible = true; //делаем объект видимым
                excel.SheetsInNewWorkbook = 1;//количество листов в книге
                excel.Workbooks.Add(Type.Missing); //добавляем книгу
                workbook = excel.Workbooks[1]; //получам ссылку на первую открытую книгу
                sheet = workbook.Worksheets.get_Item(1);//получаем ссылку на первый лист
                sheet.Name = "Создание";

                excel.WorkbookBeforeClose += WorkbookBeforeClose;


                //выводим в столбик чисел от 1 до 10;
                

                if (IsInBase)
                {
                    string _base = (IsInBaseA?"Abon":"AbonUK");
                    com.CommandText = @"use "+_base+ @"
                                        select a.Lic,a.flat_int, a.flat_str, a.fam,isnull(aa.floors,0) as floors, a.area, a.liver, case when a.lsneirc=0 OR a.lsnEirc is null then 11111111 else a.lsnEirc end as lsneirc,
                                        case when vd.h1 is null then '' else convert(varchar,vd.h1,104)end as h1,case when vd.h2 is null then '' else convert(varchar,vd.h2,104) end as h2,case when vd.H3 is null then '' else convert(varchar,vd.h3,104) end as h3, 
                                        case when vd.g1 is null then '' else convert(varchar,vd.G1,104) end as g1,case when vd.g2 is null then '' else CONVERT(varchar,vd.g2,104) end as g2,case when vd.g3 is null then '' else CONVERT(varchar,vd.g3,104) end as g3,
                                        case when (vd.h1 is not null) then cast(isnull(p.kubh1n,0) as nvarchar) else '' end as kh1, case when vd.h2 is not null then cast(isnull(p.kubh2n,0) as nvarchar) else '' end as kh2, case when vd.h3 is Not null then CAST(isnull(p.kubh3n,0) as nvarchar) else '' end as kh3,
                                        case when (vd.g1 is not null) then cast(isnull(p.kubg1n,0) as nvarchar) else '' end as kg1, case when vd.g2 is not null then cast(isnull(p.kubg2n,0) as nvarchar) else '' end as kg2, case when vd.g3 is not null then cast(isnull(p.kubg3n,0) as nvarchar) else '' end as kg3,
                                        H1Marka, H1Number, H2Marka, H2Number, H3Marka, H3Number, G1Marka, G1Number, G2Marka, G2Number, G3Marka, G3Number
                                        from abonent" + frmMain.MaxCurPer+@" a
                                        left join abon aa on aa.lic=a.lic and aa.PerEnd=0
                                        left join vodomerdate"+frmMain.MaxCurPer+@" vd on vd.lic=a.lic
                                        left join maxposvod p on p.lic=a.lic
                                        where a.str_code=@str and a.dom=@dom";
                    com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                    com.Parameters.AddWithValue("@dom", cmb_dom.Text);
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            int i = 1;
                            while(r.Read())
                            {
                                sheet.Cells[i, 1].Value = r["flat_int"].ToString().Trim();
                                sheet.Cells[i, 2].Value = r["flat_str"].ToString().Trim();
                                sheet.Cells[i, 3].Value = r["fam"].ToString().Trim();
                                sheet.Cells[i, 4].Value = r["liver"].ToString().Trim();
                                sheet.Cells[i, 5].Value = r["area"].ToString().Trim();
                                sheet.Cells[i, 6].Value = normv;
                                sheet.Cells[i, 7].Value = normk;
                                sheet.Cells[i, 8].Value = r["kh1"].ToString();
                                sheet.Cells[i, 9].Value = r["h1"].ToString();
                                sheet.Cells[i, 10].Value = r["H1Marka"].ToString();
                                sheet.Cells[i, 11].Value = r["H1Number"].ToString();
                                sheet.Cells[i, 12].Value = r["kh2"].ToString();
                                sheet.Cells[i, 13].Value = r["h2"].ToString();
                                sheet.Cells[i, 14].Value = r["H2Marka"].ToString();
                                sheet.Cells[i, 15].Value = r["H2Number"].ToString();
                                sheet.Cells[i, 16].Value = r["kh3"].ToString();
                                sheet.Cells[i, 17].Value = r["h3"].ToString();
                                sheet.Cells[i, 18].Value = r["H3Marka"].ToString();
                                sheet.Cells[i, 19].Value = r["H3Number"].ToString();
                                sheet.Cells[i, 20].Value = r["kg1"].ToString();
                                sheet.Cells[i, 21].Value = r["g1"].ToString();
                                sheet.Cells[i, 22].Value = r["G1Marka"].ToString();
                                sheet.Cells[i, 23].Value = r["G1Number"].ToString();
                                sheet.Cells[i, 24].Value = r["kg2"].ToString();
                                sheet.Cells[i, 25].Value = r["g2"].ToString();
                                sheet.Cells[i, 26].Value = r["G2Marka"].ToString();
                                sheet.Cells[i, 27].Value = r["G2Number"].ToString();
                                sheet.Cells[i, 28].Value = r["kg3"].ToString();
                                sheet.Cells[i, 29].Value = r["g3"].ToString();
                                sheet.Cells[i, 30].Value = r["G3Marka"].ToString();
                                sheet.Cells[i, 31].Value = r["G3Number"].ToString();
                                sheet.Cells[i, 32].Value = r["lsneirc"].ToString().Length>0?r["lsneirc"].ToString():"11111111";
                                sheet.Cells[i, 33].Value = r["floors"].ToString().Length>0?r["floors"].ToString():(floors.Text.Replace(" ","").Length>0?floors.Text.Replace(" ",""):"0");
                                sheet.Cells[i, 34].Value = r["lic"].ToString();
                                i++;
                            }
                        }
                    }
                    com.Parameters.Clear();
                }
                else
                {
                    for (int i = 1; i <= count_lic; i++)
                    {
                    
                            sheet.Cells[i, 1].Value = i;
                            sheet.Cells[i, 2].Value = " ";
                            sheet.Cells[i, 6].Value = normv;
                            sheet.Cells[i, 7].Value = normk;
                            sheet.Cells[i, 32].Value = "11111111";
                            sheet.Cells[i, 33].Value = "0" + floors.Text.ToString().Replace(" ", "");
                            if (X1.Checked)
                            {
                                sheet.Cells[i, 8].Value = 0;
                                sheet.Cells[i, 9].Value = dateTimePicker2.Text;
                            }
                            if (X2.Checked)
                            {
                                sheet.Cells[i, 12].Value = 0;
                                sheet.Cells[i, 13].Value = dateTimePicker3.Text;
                            }
                            if (X3.Checked)
                            {
                                sheet.Cells[i, 16].Value = 0;
                                sheet.Cells[i, 17].Value = dateTimePicker5.Text;
                            }
                            if (G1.Checked)
                            {
                                sheet.Cells[i, 20].Value = 0;
                                sheet.Cells[i, 21].Value = dateTimePicker11.Text;
                            }
                            if (G2.Checked)
                            {
                                sheet.Cells[i, 24].Value = 0;
                                sheet.Cells[i, 25].Value = dateTimePicker9.Text;
                            }
                            if (G3.Checked)
                            {
                                sheet.Cells[i, 28].Value = 0;
                                sheet.Cells[i, 29].Value = dateTimePicker7.Text;
                            }
                    }
                }

                ///.NumberFormat = "Д ММММ, ГГГГ";

                cells = sheet.Rows["1", Type.Missing];
                cells.Rows.Insert(1);
                sheet.Cells[1, 1].Value = "Квартира №";
                sheet.Columns["A", Type.Missing].NumberFormat = "### ##0";

                sheet.Cells[1, 2].Value = "Квартира литера";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 3].Value = "ФИО собственника";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 4].Value = "Проживающих";
                sheet.Columns["D", Type.Missing].NumberFormat = "### ##0";

                sheet.Cells[1, 5].Value = "Площадь";
                sheet.Columns["E", Type.Missing].NumberFormat = "### ##0,00";

                sheet.Cells[1, 6].Value = "Норма В";
                sheet.Columns["F", Type.Missing].NumberFormat = "### ##0,000";
                sheet.Cells[1, 7].Value = "Норма K";
                sheet.Columns["G", Type.Missing].NumberFormat = "### ##0,000";

                sheet.Cells[1, 8].Value = "X1 Нач. показания";
                sheet.Columns["H", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 9].Value = "X1 Дата поверки";
                sheet.Columns["I", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 10].Value = "Марка счетчика Х1";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 11].Value = "Номер счетчика Х1";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                
                sheet.Cells[1, 12].Value = "X2 Нач. показания";
                sheet.Columns["H", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 13].Value = "X2 Дата поверки";
                sheet.Columns["I", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 14].Value = "Марка счетчика Х2";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 15].Value = "Номер счетчика Х2";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 16].Value = "X3 Нач. показания";
                sheet.Columns["J", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 17].Value = "X3 Дата поверки";
                sheet.Columns["K", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 18].Value = "Марка счетчика Х3";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 19].Value = "Номер счетчика Х3";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 20].Value = "Г1 Нач. показания";
                sheet.Columns["L", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 21].Value = "Г1 Дата поверки";
                sheet.Columns["M", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 22].Value = "Марка счетчика Г1";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 23].Value = "Номер счетчика Г1";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 24].Value = "Г2 Нач. показания";
                sheet.Columns["N", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 25].Value = "Г2 Дата поверки";
                sheet.Columns["O", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 26].Value = "Марка счетчика Г2";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 27].Value = "Номер счетчика Г2";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 28].Value = "Г3 Нач. показания";
                sheet.Columns["P", Type.Missing].NumberFormat = "####0";
                sheet.Cells[1, 29].Value = "Г3 Дата поверки";
                sheet.Columns["Q", Type.Missing].NumberFormat = "ДД.ММ.ДДДД";
                sheet.Cells[1, 30].Value = "Марка счетчика Г3";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";
                sheet.Cells[1, 31].Value = "Номер счетчика Г3";
                sheet.Columns["D", Type.Missing].NumberFormat = "@";

                sheet.Cells[1, 32].Value = "Лицевой счет УК";
                sheet.Columns["R", Type.Missing].NumberFormat = "#########0";

                sheet.Cells[1, 33].Value = "Этажность";
                sheet.Columns["T", Type.Missing].NumberFormat = "##0";

                sheet.Cells[1, 34].Value = "Л/счет в БД";
                sheet.Columns["V", Type.Missing].NumberFormat = "##0";

                //делаем их жирными
                sheet.Rows["1", Type.Missing].Font.Bold = true;
                sheet.Columns["A:V", Type.Missing].EntireColumn.AutoFit();
                sheet.Columns["H:V", Type.Missing].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter; ;
            }
        }

        private void X3_CheckedChanged(object sender, EventArgs e)
        {
            if (X3.CheckState == CheckState.Checked && poliv.CheckState == CheckState.Checked)
            {
                poliv.CheckState = CheckState.Unchecked;
                dateTimePicker6.Enabled = false;
                xkn3.CheckState = CheckState.Unchecked;
            }
        }

        private void poliv_CheckedChanged(object sender, EventArgs e)
        {
            if (poliv.CheckState == CheckState.Checked && X3.CheckState == CheckState.Checked)
            {
                X3.CheckState = CheckState.Unchecked;
                dateTimePicker6.Enabled = true;
                xkn3.CheckState = CheckState.Checked;
            }
            else if (poliv.CheckState == CheckState.Checked && X3.CheckState == CheckState.Unchecked)
            {
                dateTimePicker6.Enabled = true;
                xkn3.CheckState = CheckState.Checked;
            }
            else if (poliv.CheckState == CheckState.Unchecked)
            {
                dateTimePicker6.Enabled = false;
                xkn3.CheckState = CheckState.Unchecked;
            }
        }

        private void frmCreate_Shown(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            if (con.State == ConnectionState.Open)
            {
                com.CommandText = @"select id,nameved,case when buk=0 then 0 else 1 end as uk from abonuk.dbo.spvedomstvo order by nameved";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        cmb_ved.Items.Clear();
                        while (read.Read())
                        {
                            cmb_ved.Items.Add(new SelectData(read["id"].ToString().Trim(),read["nameved"].ToString().Trim(),Convert.ToInt32(read["uk"].ToString())));
                        }
                        cmb_ved.SelectedIndex = 1;
                    }
                }

                com.CommandText = @"select kod,fam from abonuk.dbo.controler where kod>0 order by fam";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        cmb_kontr.Items.Clear();
                        while (read.Read())
                        {
                            cmb_kontr.Items.Add(new SelectData(read["kod"].ToString(),read["fam"].ToString()));
                        }
                        cmb_kontr.SelectedIndex = 1;
                    }
                }
                com.CommandText = @"select cod_yl, yl_name from Abonuk.dbo.Street where cod_yl <> 'тмп' order by yl_name";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        cmb_street.Items.Clear();
                        while (read.Read())
                        {
                            cmb_street.Items.Add(new SelectData(read["cod_yl"].ToString(),read["yl_name"].ToString()));
                        }
                        
                    }
                }
                cmb_street.SelectedIndex = 1;

                com.CommandText = "select id_Hworg, nm_hworg from common.dbo.sphworg where bkatel >= 0 order by nm_hworg";
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        cmb_kot.Items.Clear();
                        while (read.Read())
                        {
                            cmb_kot.Items.Add(new SelectData(read["id_hworg"].ToString(), read["nm_hworg"].ToString()));
                        }
                        cmb_kot.SelectedIndex = 0;
                    }
                }
            }
        }

        private void cmb_street_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmb_dom.Items.Clear();
            cmb_dom.Text = "";
            string street = ((SelectData)this.cmb_street.SelectedItem).Value;
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            com.CommandText = "select * from (" +
                        "select cod_yl, S, isnull(lit,'') as lit ,name_ul from Abonuk.dbo.streetuch where cod_yl = '" + street + "' group by cod_yl, S, lit ,name_ul " +
                        "union all " +
                        "select cod_yl, S, isnull(lit,'') as lit ,name_ul from Abon.dbo.streetuch where cod_yl = '" + street + "' group by cod_yl, S, lit ,name_ul" +
                        ") d group by cod_yl, S, lit ,name_ul";
            using (SqlDataReader read = com.ExecuteReader())
            {
                if (read.HasRows)
                {
                    while (read.Read())
                    {
                        cmb_dom.Items.Add(new SelectData("", read["S"].ToString() + read["lit"].ToString()));
                    }
                }
            }
        }

        private void odn_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void cmb_kot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_kot.Text != "")
            {
                if (G1.Checked) gvn1.CheckState = CheckState.Checked;
                if (G2.Checked) gvn2.CheckState = CheckState.Checked;
                if (G3.Checked) gvn3.CheckState = CheckState.Checked;
            }
            else
            {
                gvn1.CheckState = CheckState.Unchecked;
                gvn2.CheckState = CheckState.Unchecked;
                gvn3.CheckState = CheckState.Unchecked;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string LLL = "";
            can_close = true;
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            try
            {
                if (con.State != ConnectionState.Open) con.Open();
            }
            catch
            {
                goto END;
            }
            if (excel != null && (excel.WindowState == Excel.XlWindowState.xlNormal || excel.WindowState == Excel.XlWindowState.xlMinimized || excel.WindowState == Excel.XlWindowState.xlMaximized))
            {
                    ///
                    ///проверка exel
                    ///
                    /// проверка существующих лицевых счетов
                    /// 
                    com.CommandText = @"select sum(cnt) as cnt from (select count(lic) as cnt from abon.dbo.abonent" + frmMain.MaxCurPer + " where str_code=@str and dom=@dom" +
                                        " union all " +
                                        " select count(lic) as cnt from abonuk.dbo.abonent" + frmMain.MaxCurPer + " where str_code=@str and dom=@dom" +
                                        ") a";
                    com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                    com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                    using (SqlDataReader read = com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            read.Read();
                            if (Convert.ToInt32(read["cnt"].ToString()) > 0 && !IsInBase)
                            {
                                MessageBox.Show("В данном доме существуют лицевые счета", "Ошибка");
                                goto END;
                            }
                        }
                    }
                    com.Parameters.Clear();
                    /// 
                    /// проверка пустых квартир
                    /// 
                    int count_kv = Convert.ToInt32("0" + countlic.Text.Replace(" ", ""));
                    if (count_kv <= 0) { MessageBox.Show("Количество лицевых счетов не может быть отрицательным", "Ошибка"); goto END; }
                    else
                    {
                        for (int i = 1; i <= count_kv; i++)
                        {
                            string kv = Convert.ToString(sheet.Cells[i + 1, 1].Value);
                            string liver = Convert.ToString(sheet.Cells[i + 1, 4].Value);
                            string s = Convert.ToString(sheet.Cells[i + 1, 5].Value);
                            if (kv.Trim() == "") { MessageBox.Show("Номер квартиры не может быть пустым полем", "Ошибка"); goto END; }
                            if (liver.Trim() == "") { MessageBox.Show("Количество проживающих не может быть пустым полем", "Ошибка"); goto END; }
                            if (s == null || s.Trim() == "") { MessageBox.Show("Площадь не может быть пустым полем", "Ошибка"); goto END; }
                            if (Convert.ToString(sheet.Cells[i + 1, 32].Value) == "") sheet.Cells[i + 1, 32].Value = "11111111";
                        }
                    }
                    if (con.State == ConnectionState.Open)
                    {
     //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////                   
                        tran = con.BeginTransaction("create");
                        com.Transaction = tran;
      ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////                  
                        string base_ = "";
                        try
                        {
                            if (!IsInBase)
                            {
                                com.CommandText = "select case when buk=1 then 1 else 0 end as uk from abonuk.dbo.spvedomstvo where id=@id";
                                com.Parameters.AddWithValue("@id", ((SelectData)this.cmb_ved.SelectedItem).Value);
                                using (SqlDataReader read = com.ExecuteReader())
                                {
                                    if (read.HasRows)
                                    {
                                        while (read.Read())
                                        {
                                            if (read["uk"].ToString() == "0") base_ = "Abon.dbo."; else base_ = "Abonuk.dbo.";
                                        }
                                    }
                                }
                                com.Parameters.Clear();


                                string id_house = "";
                                com.CommandText = "select sh.id_house from common.dbo.sphouses sh inner join common.dbo.spstreets ss on ss.id_street = sh.street_code where ss.code_yl = @str and (cast(sh.numhouse as nvarchar)+isnull(lithouse,'')) = @dom";
                                com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                                com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                                using (SqlDataReader read = com.ExecuteReader())
                                {
                                    if (read.HasRows)
                                    {
                                        read.Read();
                                        id_house = read["id_house"].ToString();
                                    }
                                }
                                com.Parameters.Clear();

                                string tar_code = "";
                                com.CommandText = "select tarifKod from " + base_ + "street where cod_yl=@str";
                                com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                                using (SqlDataReader read = com.ExecuteReader())
                                {
                                    if (read.HasRows)
                                    {
                                        read.Read();
                                        tar_code = read["tarifkod"].ToString();
                                    }
                                }
                                com.Parameters.Clear();

                                if (id_house != "" && Convert.ToDouble("0" + s.Text.ToString().Replace(" ", "").Trim()) > 0)
                                {
                                    com.CommandText = "insert into common.dbo.housesdata (house_code,perbeg,perend,floors,tar_code,allarea,areahabitation,areanothabitation,iscalcodn) values(@house_code,0,0,@floors,@tar_code,@allarea,@areahabitation,@areanothabitation,@odn)";
                                    com.Parameters.AddWithValue("@house_code", id_house);
                                    com.Parameters.AddWithValue("@floors", (floors.Text.ToString().Replace(" ", "").Trim() != "" ? floors.Text.ToString().Replace(" ", "").Trim() : "NULL"));
                                    com.Parameters.AddWithValue("@tar_code", tar_code == "" ? "NULL" : tar_code);
                                    com.Parameters.AddWithValue("@allarea", Convert.ToDouble("0" + s.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@areahabitation", Convert.ToDouble("0" + sgl.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@areanothabitation", Convert.ToDouble("0" + sngl.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@odn", (isODN.Checked ? "1" : "0"));
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                }

                                if (id_house != "" && cmb_kot.Text.Trim().Length > 0)
                                {
                                    com.CommandText = "update common.dbo.sphouses set hworg_code = @kot where id_house=@id_house";
                                    com.Parameters.AddWithValue("@id_house", id_house);
                                    com.Parameters.AddWithValue("@kot,", ((SelectData)this.cmb_kot.SelectedItem).Value);
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                }

                                com.CommandText = "select MAX(lic) as cnt from abon.dbo.abonent" + frmMain.MaxCurPer + " union all select max(lic) as cnt from abonuk.dbo.abonent" + frmMain.MaxCurPer;
                                Int64 max = 0;
                                using (SqlDataReader read = com.ExecuteReader())
                                {
                                    if (read.HasRows)
                                    {
                                        while (read.Read())
                                        {
                                            max = (Convert.ToInt64(read["cnt"].ToString().Substring(1, 9)) > max) ? Convert.ToInt64(read["cnt"].ToString().Substring(1, 9)) : max;
                                        }
                                    }
                                }
                                string lic = "";
                                if (base_ == "Abon.dbo.") { lic = "1"; max = Convert.ToInt64("1" + Convert.ToString(max)); } else { lic = "2"; max = Convert.ToInt64("2" + Convert.ToString(max)); }
                                max = max + 500;
                                int cell = 2;
                                for (Int64 i = max; i < max + Convert.ToInt64(count_kv); i++)
                                {
                                    LLL = sheet.Cells[cell, 1].Value.ToString().Trim();
                                    com.Parameters.Clear();
                                    com.CommandText = "insert into " + base_ + "abonent" + frmMain.MaxCurPer + " (Lic,Kontr,Fam,Str_code,dom,flat,Liver,Vvodomer,N_vl,N_kl,SDolgBeg,SnDeb,SnKred,nv,nk,Nachisl,NvFull,NkFull,pv,pk,pos,skdeb,skkred,SDolgEnd,KodVedom,Vedom,EIRC,lsnEirc,VodKod,Area,AllN_vl,Alln_kl,flat_int,flat_str) " +
                                        " values(@lic,@kontr,@fam,@str,@dom,@flat,@liver,@vodomer";
                                    com.Parameters.AddWithValue("@lic", i + Convert.ToInt32(sheet.Cells[cell, 1].Value.ToString().Trim()) * 100);
                                    com.Parameters.AddWithValue("@kontr", ((SelectData)cmb_kontr.SelectedItem).Value);
                                    com.Parameters.AddWithValue("@fam", (Convert.ToString(sheet.Cells[cell, 3].Value) + ""));
                                    com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                                    com.Parameters.AddWithValue("@dom", cmb_dom.Text);
                                    com.Parameters.AddWithValue("@flat", sheet.Cells[cell, 1].Value.ToString().Trim() + "" + sheet.Cells[cell, 2].Value.ToString().Trim() + "");
                                    com.Parameters.AddWithValue("@liver", Convert.ToInt16("0" + sheet.Cells[cell, 4].Value.ToString().Trim()));
                                    com.Parameters.AddWithValue("@vodomer", (X1.Checked || X2.Checked || X3.Checked || G1.Checked || G2.Checked || G3.Checked || poliv.Checked) ? 1 : 0);
                                    if ((X1.Checked || X2.Checked || X3.Checked || G1.Checked || G2.Checked || G3.Checked || poliv.Checked)) com.CommandText = com.CommandText + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,";
                                    else
                                    {
                                        com.CommandText = com.CommandText + ",@nvl,@nkl,0,0,0,0,0,0,0,0,0,0,0,0,0,0,";
                                        com.Parameters.AddWithValue("@nvl", sheet.Cells[cell, 6].Value);
                                        com.Parameters.AddWithValue("@nkl", sheet.Cells[cell, 7].Value);
                                    }
                                    ///KodVedom,Vedom,EIRC,lsnEirc,VodKod,Area,AllN_vl,Alln_kl,flat_int,flat_str
                                    com.CommandText = com.CommandText + "@kodved,@ved,0,@lsneirc,0,@area,0,0,@flat_int,@flat_str)";
                                    com.Parameters.AddWithValue("@kodved", ((SelectData)cmb_ved.SelectedItem).Value);
                                    com.Parameters.AddWithValue("@ved", ((SelectData)cmb_ved.SelectedItem).Text);
                                    com.Parameters.AddWithValue("@lsneirc", Convert.ToInt64(sheet.Cells[cell, 32].Value + ""));
                                    com.Parameters.AddWithValue("@area", sheet.Cells[cell, 5].Value.ToString().Replace(",","."));
                                    com.Parameters.AddWithValue("@flat_int", Convert.ToInt32(sheet.Cells[cell, 1].Value));
                                    com.Parameters.AddWithValue("@flat_str", (Convert.ToString(sheet.Cells[cell, 2].Value) + ""));
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();

                                    if (floors.ToString().Replace(" ", "").Trim().Length > 0)
                                    {
                                        com.CommandText = "update " + base_ + "abon set floors=@floors, house_code=@house where lic=@lic and perend = 0";
                                        com.Parameters.AddWithValue("@lic", i + sheet.Cells[cell, 1].Value * 100);
                                        com.Parameters.AddWithValue("@floors", ("0" + floors.Text).Replace(" ", "").Trim());
                                        com.Parameters.AddWithValue("@house", ((id_house == "") ? "NULL" : id_house));
                                        com.ExecuteNonQuery();
                                        com.Parameters.Clear();
                                    }

                                    if ((X1.Checked || X2.Checked || X3.Checked || G1.Checked || G2.Checked || G3.Checked || poliv.Checked))
                                    {
                                        string pole = "lic,NormaV,NormaK,";
                                        string value = "@lic,@nvl,@nkl,";
                                        com.Parameters.AddWithValue("@lic", i + sheet.Cells[cell, 1].Value * 100);
                                        com.Parameters.AddWithValue("@nvl", sheet.Cells[cell, 6].Value);
                                        com.Parameters.AddWithValue("@nkl", sheet.Cells[cell, 7].Value);

                                        if (X1.Checked && Convert.ToString("" + sheet.Cells[cell, 9].Value).Trim() != "")
                                        {
                                            pole = pole + "H1,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 9].Value + "',104),";
                                            pole += @"H1Marka, H1Number, ";
                                            value += @"'" + sheet.Cells[cell, 10].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 11].Value.ToString().Trim() + @"', ";
                                            if (xvn1.Checked)
                                            {
                                                pole = pole + "H1Another,";
                                                value = value + "1,";
                                            }
                                            if (xkn1.Checked)
                                            {
                                                pole = pole + "Hk1Another,";
                                                value = value + "1,";
                                            }
                                        }
                                        if (X2.Checked && Convert.ToString("" + sheet.Cells[cell, 13].Value).Trim() != "")
                                        {
                                            pole = pole + "H2,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 13].Value + "',104),";
                                            pole += @"H2Marka, H2Number, ";
                                            value += @"'" + sheet.Cells[cell, 14].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 15].Value.ToString().Trim() + @"', ";
                                            if (xvn2.Checked)
                                            {
                                                pole = pole + "H2Another,";
                                                value = value + "1,";
                                            }
                                            if (xkn2.Checked)
                                            {
                                                pole = pole + "Hk2Another,";
                                                value = value + "1,";
                                            }
                                        }
                                        if (X3.Checked && Convert.ToString("" + sheet.Cells[cell, 17].Value).Trim() != "")
                                        {
                                            pole = pole + "H3,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 17].Value + "',104),";
                                            pole += @"H3Marka, H3Number, ";
                                            value += @"'" + sheet.Cells[cell, 18].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 19].Value.ToString().Trim() + @"', ";
                                            if (xvn2.Checked)
                                            {
                                                pole = pole + "H3Another,";
                                                value = value + "1,";
                                            }
                                            if (xkn2.Checked)
                                            {
                                                pole = pole + "Hk3Another,";
                                                value = value + value + "1,";
                                            }
                                        }
                                        if (G1.Checked && Convert.ToString("" + sheet.Cells[cell, 21].Value).Trim() != "")
                                        {
                                            pole = pole + "G1,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 21].Value + "',104),";
                                            pole += @"G1Marka, G1Number, ";
                                            value += @"'" + sheet.Cells[cell, 22].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 23].Value.ToString().Trim() + @"', ";
                                            if (gvn1.Checked)
                                            {
                                                pole = pole + "G1Another,";
                                                value = value + "1,";
                                            }
                                            if (gkn1.Checked)
                                            {
                                                pole = pole + "Gk1Another,";
                                                value = value + "1,";
                                            }
                                        }
                                        if (G2.Checked && Convert.ToString("" + sheet.Cells[cell, 25].Value).Trim() != "")
                                        {
                                            pole = pole + "G2,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 25].Value + "',104),";
                                            pole += @"G2Marka, G2Number, ";
                                            value += @"'" + sheet.Cells[cell, 26].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 27].Value.ToString().Trim() + @"', ";
                                            if (gvn2.Checked)
                                            {
                                                pole = pole + "G2Another,";
                                                value = value + "1,";
                                            }
                                            if (gkn2.Checked)
                                            {
                                                pole = pole + "Gk2Another,";
                                                value = value + "1,";
                                            }
                                        }
                                        if (G3.Checked && Convert.ToString("" + sheet.Cells[cell, 29].Value).Trim() != "")
                                        {
                                            pole = pole + "G3,";
                                            value = value + "convert(datetime,'" + sheet.Cells[cell, 29].Value + "',104),";
                                            pole += @"G3Marka, G3Number, ";
                                            value += @"'" + sheet.Cells[cell, 30].Value.ToString().Trim() + @"', '" + sheet.Cells[cell, 31].Value.ToString().Trim() + @"', ";
                                            if (gvn3.Checked)
                                            {
                                                pole = pole + "G3Another,";
                                                value = value + "1,";
                                            }
                                            if (gkn3.Checked)
                                            {
                                                pole = pole + "Gk3Another,";
                                                value = value + "1,";
                                            }
                                        }

                                        if (pole.Contains("H") || pole.Contains("G"))
                                        {
                                            pole = pole + "datvvod";
                                            value = value + "convert(datetime,'" + dateTimePicker1.Value.Day + "." + dateTimePicker1.Value.Month + "." + dateTimePicker1.Value.Date.Year + "',104)";
                                            com.CommandText = "insert into " + base_ + "vodomerdate" + frmMain.MaxCurPer + "(" + pole + ") values(" + value + ")";
                                            com.ExecuteNonQuery();
                                            com.Parameters.Clear();
                                            pole = "lic,";
                                            value = "@lic,";
                                            com.Parameters.AddWithValue("@lic", i + sheet.Cells[cell, 1].Value * 100);
                                            if (X1.Checked && Convert.ToString("" + sheet.Cells[cell, 8].Value).Trim() != "")
                                            {
                                                pole = pole + "kubh1n,kubh1s,";
                                                value = value + sheet.Cells[cell, 8].Value + ",0,";
                                            }
                                            if (X2.Checked && Convert.ToString("" + sheet.Cells[cell, 12].Value).Trim() != "")
                                            {
                                                pole = pole + "kubh2n,kubh2s,";
                                                value = value + sheet.Cells[cell, 12].Value + ",0,";
                                            }
                                            if (X3.Checked && Convert.ToString("" + sheet.Cells[cell, 16].Value).Trim() != "")
                                            {
                                                pole = pole + "kubh3n,kubh3s,";
                                                value = value + sheet.Cells[cell, 16].Value + ",0,";
                                            }
                                            if (G1.Checked && Convert.ToString("" + sheet.Cells[cell, 20].Value).Trim() != "")
                                            {
                                                pole = pole + "kubg1n,kubg1s,";
                                                value = value + sheet.Cells[cell, 20].Value + ",0,";
                                            }
                                            if (G2.Checked && Convert.ToString("" + sheet.Cells[cell, 24].Value).Trim() != "")
                                            {
                                                pole = pole + "kubg2n,kubg2s,";
                                                value = value + sheet.Cells[cell, 24].Value + ",0,";
                                            }
                                            if (G3.Checked && Convert.ToString("" + sheet.Cells[cell, 28].Value).Trim() != "")
                                            {
                                                pole = pole + "kubg3n,kubg3s,";
                                                value = value + sheet.Cells[cell, 28].Value + ",0,";
                                            }
                                            pole = pole + "prim_code,vodkod,modirec";
                                            value = value + "0,0,convert(datetime,'01." + frmMain.MaxCurPer.Substring(4, 2) + "." + frmMain.MaxCurPer.Substring(0, 4) + "',104)";
                                            com.CommandText = "insert into " + base_ + "posvod(" + pole + ") values(" + value + ")";
                                            com.ExecuteNonQuery();
                                            com.Parameters.Clear();
                                        }
                                    }
                                    cell++;
                                }
                                ; ;
                                //tran.Commit();
                                //can_close = false;
                                //MessageBox.Show("Дом успешно создан", "Внимание");
                                tran.Commit();
                                can_close = false;
                                MessageBox.Show("Дом успешно создан", "Внимание");
                            }
                            else
                            {
                               /////////////////////////////// получаем количество квартир в Excel
                                Boolean cont = false;
                                int ii = 2;
                                do {
                                    cont = false;
                                    if (sheet.Cells[ii++, 1].Value != null)
                                        cont = true;
                                } while(cont);
                                if ((ii-3) > 0)
                                    countlic.Text = (ii-3).ToString();
                                for (int i = 2; i <= Convert.ToInt32(countlic.Text)+1; i++)
                                {
                                    //////////обновляем данные по имеющимся таблицам
                                    string lic = "";
                                    com.Parameters.Clear();
                                    if ((sheet.Cells[i, 34].Value == null) || (sheet.Cells[i, 34].Value.ToString()==""))
                                    {
                                        ////cоздаем новый лицевой счет
                                        com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                select (max(lic)+10) as lic from abonent" + frmMain.MaxCurPer;
                                        using (SqlDataReader r = com.ExecuteReader())
                                        {
                                            if (r.HasRows) { r.Read(); lic = r["lic"].ToString(); }
                                            com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                    insert into abonent" + frmMain.MaxCurPer + @" (lic,fam,lsneirc,n_vl,n_kl,liver,flat_int,flat_str,flat,area,kontr,kodvedom,vedom,vvodomer,str_code,dom)
                                                    values (@lic,@fam,@lsneirc,@nv,@nk,@liver,@fi,@fs,@f,@s,@kontr,@vedom,'" + ((SelectData)cmb_ved.SelectedItem).Text + @"',@vodomer,@str,@dom)";
                                            com.Parameters.AddWithValue("@vedom", ((SelectData)cmb_ved.SelectedItem).Value);
                                            com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
                                            com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                                            com.Parameters.AddWithValue("@lic", lic);
                                            sheet.Cells[i, 34].Value = lic;
                                        }
                                    }
                                    else if ((Convert.ToInt64(sheet.Cells[i, 34].Value.ToString().Trim()) < 10000000000))
                                    {
                                        com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                            update a
                                            set a.fam = @fam, a.lsneirc = @lsneirc, a.n_vl = @nv, a.n_kl = @nk, a.liver = @liver, a.flat_int = @fi, a.flat_str = @fs, a.flat=@f, a.area = @s,
                                            a.kontr = @kontr, a.gruppa3=0, a.vvodomer=@vodomer
                                            from abonent" + frmMain.MaxCurPer + @" a
                                            where a.lic = @lic";
                                        com.Parameters.AddWithValue("@lic", sheet.Cells[i, 34].Value.ToString());
                                    }
                                    com.Parameters.AddWithValue("@fam", "" + sheet.Cells[i, 3].Value.ToString());
                                    com.Parameters.AddWithValue("@lsneirc", (sheet.Cells[i, 32].Value != null ? sheet.Cells[i, 32].Value.ToString().Trim() : "11111111"));
                                    com.Parameters.AddWithValue("@nv", sheet.Cells[i, 6].Value.ToString().Trim().Replace(",", "."));
                                    com.Parameters.AddWithValue("@nk", sheet.Cells[i, 7].Value.ToString().Trim().Replace(",", "."));
                                    com.Parameters.AddWithValue("@liver", (sheet.Cells[i, 4].Value==null?"1":sheet.Cells[i, 4].Value.ToString().Trim()));
                                    com.Parameters.AddWithValue("@fi", sheet.Cells[i, 1].Value.ToString().Trim());
                                    com.Parameters.AddWithValue("@fs", (sheet.Cells[i, 2].Value == null ? "" : sheet.Cells[i, 2].Value.ToString().Trim()));
                                    com.Parameters.AddWithValue("@f", sheet.Cells[i, 1].Value.ToString().Trim() + (sheet.Cells[i, 2].Value==null?"":sheet.Cells[i, 2].Value.ToString().Trim()));
                                    com.Parameters.AddWithValue("@s", sheet.Cells[i, 5].Value.ToString().Trim().Replace(",", "."));
                                    com.Parameters.AddWithValue("@kontr", ((SelectData)cmb_kontr.SelectedItem).Value);
                                    com.Parameters.AddWithValue("@vodomer",(sheet.Cells[i, 9].Value != null || sheet.Cells[i, 13].Value != null || sheet.Cells[i, 17].Value != null || sheet.Cells[i, 21].Value != null || sheet.Cells[i, 25].Value != null || sheet.Cells[i, 29].Value != null)?"1":"0");
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                    //// работа с водомерами

                                    /// если в excel нет ни одного водомера - удаляем из таблицы
                                    if ((sheet.Cells[i, 9].Value == null) && (sheet.Cells[i, 13].Value == null) && (sheet.Cells[i, 17].Value == null) && (sheet.Cells[i, 21].Value == null) && (sheet.Cells[i, 25].Value == null) && sheet.Cells[i, 29].Value == null)
                                    {
                                        com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                            delete from vodomerdate" + frmMain.MaxCurPer + @" where lic=@lic";
                                        com.Parameters.AddWithValue("@lic", (sheet.Cells[i, 34].Value == null ? "": sheet.Cells[i, 34].Value.ToString().Trim()));
                                        com.ExecuteNonQuery();
                                        com.Parameters.Clear();
                                    }
                                    else
                                    {
                                        com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                            select lic from vodomerdate" + frmMain.MaxCurPer + @" where lic = @lic";
                                        com.Parameters.AddWithValue("@lic", (sheet.Cells[i, 34].Value == null ? "" : sheet.Cells[i, 34].Value.ToString().Trim()));
                                        Boolean vodomer = false;
                                        using (SqlDataReader r = com.ExecuteReader())
                                        {
                                            if (r.HasRows) vodomer = true;  
                                        }

                                        if (vodomer)
                                        {
                                                ///////// Update
                                                com.Parameters.Clear();
                                                com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                            update v
                                                            set ";
                                                if (sheet.Cells[i, 9].Value != null)
                                                {       
                                                    com.CommandText += @" v.H1=convert(date,'" + sheet.Cells[i, 9].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.H1Marka = '" + sheet.Cells[i, 10].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.H1Number = '" + sheet.Cells[i, 11].Value.ToString().Trim() + @"', ";
                                                    if (xvn1.Checked) com.CommandText += @" H1Another = 'True',"; else com.CommandText += @" H1Another = 'False',";
                                                    if (xkn1.Checked) com.CommandText += @" HK1Another = 'True',"; else com.CommandText += @" HK1Another = 'False',";
                                                }
                                                else com.CommandText += @" v.H1 = NULL,";
                                                if (sheet.Cells[i, 13].Value != null)
                                                {
                                                    com.CommandText += @" v.H2=convert(date,'" + sheet.Cells[i, 13].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.H2Marka = '" + sheet.Cells[i, 14].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.H2Number = '" + sheet.Cells[i, 15].Value.ToString().Trim() + @"', ";
                                                    if (xvn2.Checked) com.CommandText += @" H2Another = 'True',"; else com.CommandText += @" H2Another = 'False',";
                                                    if (xkn2.Checked) com.CommandText += @" HK2Another = 'True',"; else com.CommandText += @" HK2Another = 'False',";
                                                }
                                                else com.CommandText += @" v.H2 = NULL,";
                                                if (sheet.Cells[i, 17].Value != null)
                                                {
                                                    com.CommandText += @" v.H3=convert(date,'" + sheet.Cells[i, 17].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.H3Marka = '" + sheet.Cells[i, 18].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.H3Number = '" + sheet.Cells[i, 19].Value.ToString().Trim() + @"', ";
                                                    if (xvn3.Checked) com.CommandText += @" H3Another = 'True',"; else com.CommandText += @" H3Another = 'False',";
                                                    if (xkn3.Checked) com.CommandText += @" HK3Another = 'True',"; else com.CommandText += @" HK3Another = 'False',";
                                                }
                                                else com.CommandText += @" v.H3 = NULL,";
                                                if (sheet.Cells[i, 21].Value != null)
                                                {
                                                    com.CommandText += @" v.G1=convert(date,'" + sheet.Cells[i, 21].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.G1Marka = '" + sheet.Cells[i, 22].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.G1Number = '" + sheet.Cells[i, 23].Value.ToString().Trim() + @"', ";
                                                    if (gvn1.Checked) com.CommandText += @" G1Another = 'True',"; else com.CommandText += @" G1Another = 'False',";
                                                    if (gkn1.Checked) com.CommandText += @" GK1Another = 'True',"; else com.CommandText += @" GK1Another = 'False',";
                                                }
                                                else com.CommandText += @" v.G1 = NULL,";
                                                if (sheet.Cells[i, 25].Value != null)
                                                {
                                                    com.CommandText += @" v.G2 = convert(date,'" + sheet.Cells[i, 25].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.G2Marka = '" + sheet.Cells[i, 26].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.G2Number = '" + sheet.Cells[i, 27].Value.ToString().Trim() + @"', ";
                                                    if (gvn2.Checked) com.CommandText += @" G2Another = 'True',"; else com.CommandText += @" G2Another = 'False',";
                                                    if (gkn2.Checked) com.CommandText += @" Gk2Another = 'True',"; else com.CommandText += @" Gk2Another = 'False',";
                                                }
                                                else com.CommandText += @" v.G2 = NULL,";
                                                if (sheet.Cells[i, 29].Value != null)
                                                {
                                                    com.CommandText += @" v.G3 = convert(date,'" + sheet.Cells[i, 29].Value.ToString().Trim() + @"',104), ";
                                                    com.CommandText += @" v.G3Marka = '" + sheet.Cells[i, 30].Value.ToString().Trim() + @"', ";
                                                    com.CommandText += @" v.G3Number = '" + sheet.Cells[i, 31].Value.ToString().Trim() + @"', ";
                                                    if (gvn3.Checked) com.CommandText += @" G3Another = 'True',"; else com.CommandText += @" G3Another = 'False',";
                                                    if (gkn3.Checked) com.CommandText += @" Gk3Another = 'True',"; else com.CommandText += @" Gk3Another = 'False',";
                                                }
                                                else com.CommandText += @" v.G3 = NULL,";
                                                if (com.CommandText[com.CommandText.Length - 1] == ',') com.CommandText = com.CommandText.Remove(com.CommandText.Length - 1, 1);
                                                com.CommandText += @" from vodomerdate" + frmMain.MaxCurPer + @" v where v.lic=@lic";
                                                com.Parameters.AddWithValue("@lic", (sheet.Cells[i, 34].Value == null ? "" : sheet.Cells[i, 34].Value.ToString().Trim()));
                                                com.ExecuteNonQuery();
                                                com.Parameters.Clear();
                                            }
                                        else
                                        {
                                            //////// Insert
                                            string pole = "lic,";
                                            string value = "@lic,";
                                            if (sheet.Cells[i, 9].Value != null)
                                            {
                                                pole += @"H1,";
                                                value += @"convert(date,'" + sheet.Cells[i, 9].Value.ToString().Trim() + @"',104),";
                                                pole += @"H1Marka, H1Number, ";
                                                value += @"'" + sheet.Cells[i, 10].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 11].Value.ToString().Trim() + @"', ";
                                                pole += @" H1Another,"; if (xvn1.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" HK1Another,"; if (xkn1.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"H1,"; value += @"NULL,"; }
                                            if (sheet.Cells[i, 13].Value != null)
                                            {
                                                pole += @"H2,";
                                                value += @"convert(date,'" + sheet.Cells[i, 13].Value.ToString().Trim() + @"',104),";
                                                pole += @"H2Marka, H2Number, ";
                                                value += @"'" + sheet.Cells[i, 14].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 15].Value.ToString().Trim() + @"', ";
                                                pole += @" H2Another,"; if (xvn2.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" HK2Another,"; if (xkn2.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"H2,"; value += @"NULL,"; }
                                            if (sheet.Cells[i, 17].Value != null)
                                            {
                                                pole += @"H3,";
                                                value += @"convert(date,'" + sheet.Cells[i, 17].Value.ToString().Trim() + @"',104),";
                                                pole += @"H3Marka, H3Number, ";
                                                value += @"'" + sheet.Cells[i, 18].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 19].Value.ToString().Trim() + @"', ";
                                                pole += @" H3Another,"; if (xvn3.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" HK3Another,"; if (xkn3.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"H3,"; value += @"NULL,"; }
                                            if (sheet.Cells[i, 21].Value != null)
                                            {
                                                pole += @"G1,";
                                                value += @"convert(date,'" + sheet.Cells[i, 21].Value.ToString().Trim() + @"',104),";
                                                pole += @"G1Marka, G1Number, ";
                                                value += @"'" + sheet.Cells[i, 22].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 23].Value.ToString().Trim() + @"', ";
                                                pole += @" G1Another,"; if (gvn1.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" GK1Another,"; if (gkn1.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"G1,"; value += @"NULL,"; }
                                            if (sheet.Cells[i, 25].Value != null)
                                            {
                                                pole += @"G2,";
                                                value += @"convert(date,'" + sheet.Cells[i, 25].Value.ToString().Trim() + @"',104),";
                                                pole += @"G2Marka, G2Number, ";
                                                value += @"'" + sheet.Cells[i, 26].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 27].Value.ToString().Trim() + @"', ";
                                                pole += @" G2Another,"; if (gvn2.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" GK2Another,"; if (gkn2.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"G2,"; value += @"NULL,"; }
                                            if (sheet.Cells[i, 29].Value != null)
                                            {
                                                pole += @"G3,";
                                                value += @"convert(date,'" + sheet.Cells[i, 29].Value.ToString().Trim() + @"',104),";
                                                pole += @"G3Marka, G3Number, ";
                                                value += @"'" + sheet.Cells[i, 30].Value.ToString().Trim() + @"', '" + sheet.Cells[i, 31].Value.ToString().Trim() + @"', ";
                                                pole += @" G3Another,"; if (gvn3.Checked) value += @"'True',"; else value += @"'False',";
                                                pole += @" GK3Another,"; if (gkn3.Checked) value += @"'True',"; else value += @"'False',";
                                            }
                                            else { pole += @"G3,"; value += @"NULL,"; }
                                            if (pole[pole.Length - 1] == ',') pole = pole.Remove(pole.Length - 1, 1);
                                            if (value[value.Length - 1] == ',') value = value.Remove(value.Length - 1, 1);
                                            com.Parameters.Clear();
                                            com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                            insert into vodomerdate" + frmMain.MaxCurPer + @"(" + pole + @") values(" + value + @")";
                                            com.Parameters.AddWithValue("@lic", (sheet.Cells[i, 34].Value == null ? "" : sheet.Cells[i, 34].Value.ToString().Trim()));
                                            com.ExecuteNonQuery();
                                            com.Parameters.Clear();
                                        }
                                        /////////// вставка показаний водомеров
                                        if (sheet.Cells[i, 9].Value != null || sheet.Cells[i, 13].Value != null || sheet.Cells[i, 17].Value != null || sheet.Cells[i, 21].Value != null || sheet.Cells[i, 25].Value != null || sheet.Cells[i, 29].Value != null)
                                        {
                                            com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                            ";
                                            string pole = "lic,";
                                            string value = "@lic,";
                                            com.Parameters.AddWithValue("@lic", sheet.Cells[i, 34].Value.ToString().Trim());
                                            if (sheet.Cells[i, 9].Value != null)
                                            {
                                                pole = pole + "kubh1n,kubh1s,";
                                                value = value + (sheet.Cells[i, 8].Value == null ? "0" : sheet.Cells[i, 8].Value.ToString().Trim()) + ",0,";
                                            }
                                            if (sheet.Cells[i, 13].Value != null)
                                            {
                                                pole = pole + "kubh2n,kubh2s,";
                                                value = value + sheet.Cells[i, 12].Value + ",0,";
                                            }
                                            if (sheet.Cells[i, 17].Value != null)
                                            {
                                                pole = pole + "kubh3n,kubh3s,";
                                                value = value + sheet.Cells[i, 16].Value + ",0,";
                                            }
                                            if (sheet.Cells[i, 21].Value != null)
                                            {
                                                pole = pole + "kubg1n,kubg1s,";
                                                value = value + sheet.Cells[i, 20].Value + ",0,";
                                            }
                                            if (sheet.Cells[i, 25].Value != null)
                                            {
                                                pole = pole + "kubg2n,kubg2s,";
                                                value = value + sheet.Cells[i, 24].Value + ",0,";
                                            }
                                            if (sheet.Cells[i, 29].Value != null)
                                            {
                                                pole = pole + "kubg3n,kubg3s,";
                                                value = value + sheet.Cells[i, 28].Value + ",0,";
                                            }
                                            pole = pole + "prim_code,vodkod,modirec";
                                            value = value + "0,0,convert(datetime,'01." + frmMain.MaxCurPer.Substring(4, 2) + "." + frmMain.MaxCurPer.Substring(0, 4) + "',104)";
                                            com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                                  insert into posvod(" + pole + ") values(" + value + ")";
                                            com.ExecuteNonQuery();
                                            com.Parameters.Clear();
                                        }

                                    }///проверка на водомеры
                                    
                                    /// снятие 3 группы
                                    com.Parameters.Clear();
                                    com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                                        update abonent" + frmMain.MaxCurPer + @" 
                                                         set gruppa3=0 where lic=@lic
                                                        delete from group3dog where lic=@lic";
                                    com.Parameters.AddWithValue("@lic", sheet.Cells[i, 34].Value.ToString().Trim());
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                }/// for
                                 /// обновление площадей в доме обновление ОДН
                                string id_house = "";
                                com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                    select sh.id_house from common.dbo.sphouses sh inner join common.dbo.spstreets ss on ss.id_street = sh.street_code where ss.code_yl = @str and (cast(sh.numhouse as nvarchar)+isnull(lithouse,'')) = @dom";
                                com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                                com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                                using (SqlDataReader read = com.ExecuteReader())
                                {
                                    if (read.HasRows)
                                    {
                                        read.Read();
                                        id_house = read["id_house"].ToString();
                                    }
                                }
                                com.Parameters.Clear();

                                if (id_house != "" && Convert.ToDouble("0" + s.Text.ToString().Replace(" ", "").Trim()) > 0)
                                {
                                    com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                        insert into common.dbo.housesdata (house_code,perbeg,perend,floors,allarea,areahabitation,areanothabitation,iscalcodn) values(@house_code,0,0,@floors,@allarea,@areahabitation,@areanothabitation,@odn)";
                                    com.Parameters.AddWithValue("@house_code", id_house);
                                    com.Parameters.AddWithValue("@floors", (floors.Text.ToString().Replace(" ", "").Trim() != "" ? floors.Text.ToString().Replace(" ", "").Trim() : "NULL"));
                                    com.Parameters.AddWithValue("@allarea", Convert.ToDouble("0" + s.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@areahabitation", Convert.ToDouble("0" + sgl.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@areanothabitation", Convert.ToDouble("0" + sngl.Text.ToString().Replace(" ", "").Trim()));
                                    com.Parameters.AddWithValue("@odn", (isODN.Checked ? "1" : "0"));
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                }

                                if (id_house != "" && cmb_kot.Text.Trim().Length > 0)
                                {
                                    com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
                                        update common.dbo.sphouses set hworg_code = @kot where id_house=@id_house";
                                    com.Parameters.AddWithValue("@id_house", id_house);
                                    com.Parameters.AddWithValue("@kot,", ((SelectData)this.cmb_kot.SelectedItem).Value);
                                    com.ExecuteNonQuery();
                                    com.Parameters.Clear();
                                }
                                 /// при необходимости перевод в другую базу и ведомство
//                                com.CommandText = @"use " + (IsInBaseA ? "Abon" : "AbonUK") + @"
//                                                    select max(cnt), kodvedom from (    
//                                                    select count(lic) as cnt, kodvedom from abonent" + frmMain.MaxCurPer + @"
//                                                    where str_code=@str and dom=@dom group by kodvedom) group by kodvedom";
//                                com.Parameters.AddWithValue("@str", ((SelectData)cmb_street.SelectedItem).Value);
//                                com.Parameters.AddWithValue("@dom", cmb_dom.Text);
//                                string old_vedom = "";
//                                using (SqlDataReader r = com.ExecuteReader())
//                                {
//                                    if (r.HasRows)
//                                    {
//                                        r.Read();
//                                        old_vedom = r["kovedom"].ToString();
//                                    }
//                                }
//                                com.Parameters.Clear();
//                                if (old_vedom != ((SelectData)cmb_ved.SelectedItem).Value)
//                                {
//                                    com.CommandText = "exec " +(IsInBaseA?"Abon":"Abonuk") + ".dbo.transinnewcompany " + ((SelectData)cmb_ved.SelectedItem).Value + ", '1=0 or (a.str_code=N''" + ((SelectData)cmb_street.SelectedItem).Value + "'' and a.dom=N''" +cmb_dom.Text + "'')'";
//                                    com.ExecuteNonQuery();
//                                    Application.DoEvents();
//                                }
                                tran.Commit();
                                can_close = false;
                                MessageBox.Show("Дом обновлен", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            can_close = false;
                            if (tran != null) tran.Rollback();
                            MessageBox.Show("Дом не создан." + ex.Message, "Ошибка");
                        }

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    else MessageBox.Show("Невозможно соединиться с БД", "Ошибка");
            }
            END: 
            tran = null;
        }

        private void frmCreate_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (can_close) {e.Cancel = true; MessageBox.Show("Дожди тесь завершения операции","Внимание");}
        }

        private void sliv_CheckedChanged(object sender, EventArgs e)
        {

            if (sliv.CheckState == CheckState.Checked) sliv1.CheckState = CheckState.Unchecked;
        }

        private void sliv1_CheckedChanged(object sender, EventArgs e)
        {
            if (sliv1.CheckState == CheckState.Checked) sliv.CheckState = CheckState.Unchecked;
        }

        private void cmb_dom_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            try
            {
                com.CommandText = @"select sum(cnt) as cnt from (select count(lic) as cnt from abon.dbo.abonent" + frmMain.MaxCurPer + " where str_code=@str and dom=@dom" +
                                        " union all " +
                                        " select count(lic) as cnt from abonuk.dbo.abonent" + frmMain.MaxCurPer + " where str_code=@str and dom=@dom" +
                                        ") a";
                com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        read.Read();
                        if (Convert.ToInt32(read["cnt"].ToString()) > 0)
                        {
                            if (MessageBox.Show("В данном доме существуют лицевые счета!\nВывести данные из базы данных?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                IsInBase = true;
                                button1.BackColor = Color.LightGreen;
                            }
                            else
                            {
                                button1.BackColor = SystemColors.Control;
                                IsInBase = false;
                            }
                        }
                        else
                        {
                            button1.BackColor = SystemColors.Control;
                            IsInBase = false;
                        }
                    }
                }
                com.Parameters.Clear();

                com.CommandText = @"select aa.cnt from (
                                SELECT     COUNT(Lic) AS cnt
                                FROM         abonuk.dbo.PosVod
                                WHERE     (ModiRec > CONVERT(date, '01."+frmMain.MaxCurPer.Substring(4,2)+@"."+frmMain.MaxCurPer.Substring(0,4)+@"', 104)) and peropl is not null
                                and lic in (select lic from abonuk.dbo.abonent"+frmMain.MaxCurPer+@" where str_code=@str and dom=@dom)
                                HAVING      (COUNT(Lic) > 1)
                                union all
                                SELECT     COUNT(Lic) AS cnt
                                FROM         abon.dbo.PosVod
                                WHERE     (ModiRec > CONVERT(date, '01." + frmMain.MaxCurPer.Substring(4, 2) + @"." + frmMain.MaxCurPer.Substring(0, 4) + @"', 104)) and peropl is not null
                                and lic in (select lic from abon.dbo.abonent" + frmMain.MaxCurPer + @" where str_code=@str and dom=@dom)
                                HAVING      (COUNT(Lic) > 1)) aa where aa.cnt > 0
                                ";
                com.Parameters.AddWithValue("@str", ((SelectData)this.cmb_street.SelectedItem).Value.Trim());
                com.Parameters.AddWithValue("@dom", cmb_dom.Text.Trim());
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        read.Read();
                        MessageBox.Show("В данном доме есть лицевые счета ( "+read["cnt"].ToString()+" ) с новыми показаниями в текущем периоде","Внимание!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    }
                }
                com.Parameters.Clear();
            }
            catch { }
        }
    }
}

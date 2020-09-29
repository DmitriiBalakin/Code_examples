using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;
using Excel = Microsoft.Office.Interop.Excel;

namespace water
{
    public partial class frmOtsrochka : Form
    {
        SqlConnection con = new SqlConnection();

        Excel.Application excel;
        Excel.Workbook workbook;
        Excel.Worksheet sheet;
        Excel.Range cells;

        public frmOtsrochka()
        {
            InitializeComponent();
            try
            {
                con.ConnectionString = frmMain.db_con.ConnectionString;
                con.Open();
            }
            catch
            {
                MessageBox.Show("Невозможно соединиться с базой данных","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void frmOtsrochka_Shown(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            if (con.State == ConnectionState.Open)
            {
                try
                {
                    com.CommandText = "Select count(lic) from abon.dbo.otsrochka";
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            r.Read();
                            label2.Text = r[0].ToString();
                        }
                    }
                }
                catch
                { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            if (con.State == ConnectionState.Open)
            {
                try
                {
                    com.CommandText = "select * from abon.dbo.otsrochka where right(lic,9)=@lic";
                    com.Parameters.AddWithValue("@lic", (textBox1.Text.Length == 10?textBox1.Text.Substring(1,9):"0"));
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            r.Read();
                        }
                    }
                }
                catch { }
            }
        }

        private void WorkbookBeforeClose(Excel.Workbook Wb, ref bool Cancel)
        {
            excel.Quit();
            sheet = null;
            System.GC.Collect();
            excel = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                excel = new Excel.Application(); //создаем COM-объект Excel
                excel.Visible = true; //делаем объект видимым
                excel.SheetsInNewWorkbook = 1;//количество листов в книге
                excel.Workbooks.Add(Type.Missing); //добавляем книгу
                workbook = excel.Workbooks[1]; //получам ссылку на первую открытую книгу
                sheet = workbook.Worksheets.get_Item(1);//получаем ссылку на первый лист
                sheet.Name = "Отчет по рассрочке оплаты";
                excel.WorkbookBeforeClose += WorkbookBeforeClose;

                for (int i = 1; i < 11; i++)
                {
                    sheet.Columns[i, Type.Missing].NumberFormat = "@";
                }

                sheet.Rows["1", Type.Missing].Font.Bold = true;
                
                sheet.Columns["A:V", Type.Missing].HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                sheet.Cells[1, 1].Value = "Лицевой счет";
                sheet.Cells[1, 2].Value = "Начислено";
                sheet.Cells[1, 3].Value = "Поступление за 02";
                sheet.Cells[1, 4].Value = "Сумма долга начальная";
                sheet.Cells[1, 5].Value = "Оплата по рассрочке";
                sheet.Cells[1, 6].Value = "Дата план. отключения";

                sheet.Columns["A:V", Type.Missing].EntireColumn.AutoFit();
                SqlCommand com = new SqlCommand();
                com.Connection = con;

                com.CommandText = @"select a.lic,a.nachisl,o.sdolgbeg,o.date_poff from abon.dbo.abonent201602 a 
inner join (select RIGHT(lic,9) as lic,sdolgbeg,date_poff from abon.dbo.otsrochka where date1>=convert(date,'01-02-2016',104) and date1<=convert(date,'29-02-2016',104) and datep1 is not null and datep2 is not null) o on a.Lic='1'+o.lic
inner join abon.dbo.SpVedomstvo v on v.id=a.kodvedom
where v.bUK=0
union all
select a.lic,a.nachisl,o.sdolgbeg,o.date_poff from abonuk.dbo.abonent201602 a 
inner join (select RIGHT(lic,9) as lic,sdolgbeg,date_poff from abon.dbo.otsrochka where date1>=convert(date,'01-02-2016',104) and date1<=convert(date,'29-02-2016',104) and datep1 is not null and datep2 is not null) o on a.Lic='2'+o.lic
inner join abonuk.dbo.SpVedomstvo v on v.id=a.kodvedom
where v.bUK=1";

                List<string> lic = new List<string>();
               
                using (SqlDataReader r = com.ExecuteReader())
                {
                    if (r.HasRows)
                    {
                        int i = 2;
                        while (r.Read())
                        {
                            lic.Add(r["lic"].ToString());
                            sheet.Cells[i, 1].Value = r["lic"].ToString();
                            sheet.Cells[i, 2].Value = r["nachisl"].ToString();
                            sheet.Cells[i, 4].Value = r["sdolgbeg"].ToString();
                            sheet.Cells[i, 6].Value = r["date_poff"].ToString().Length > 0 ? r["date_poff"].ToString().Substring(0, 10) : r["date_poff"].ToString();
                            i++;
                        }
                    }
                }

                com.CommandText = @"select sum(a.pay) as pay from 
(
select isnull(sum(pa.opl),0) as pay from abon.dbo.pos201602 pa where lic='1'+right(@lic,9) and brik<>1000
union all
select isnull(sum(pa.opl),0) as pay from abonuk.dbo.pos201602 pa where lic='2'+right(@lic,9) and brik<>1000
) a
";
                for(int i=0;i<lic.Count;i++)
                {
                    com.Parameters.AddWithValue("@lic",lic.ElementAt(i));
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            r.Read();
                            sheet.Cells[i + 2, 3].Value = r["pay"].ToString();
                        }
                    }
                    com.Parameters.Clear();
                }

                for (int i = 0; i < lic.Count; i++)
                {
                    double pay = Math.Round(Convert.ToDouble(sheet.Cells[i+2,2].Value)-Convert.ToDouble(sheet.Cells[i+2,3].Value),2);
                    sheet.Cells[i + 2, 5].Value = pay < 0 ? (-1 * pay) : 0;
                    if (pay >= 0) sheet.Rows[i + 2].Font.Color = Color.Red;
                }

                //Область сортировки             
                //Microsoft.Office.Interop.Excel.Range range = sheet.get_Range("A2", "A"+lic.Count);
                ////По какому столбцу сортировать
                //Microsoft.Office.Interop.Excel.Range rangeKey = sheet.get_Range("5");
                ////Добавляем параметры сортировки
                //sheet.Sort.SortFields.Add(rangeKey);
                //sheet.Sort.SetRange(range);
                //sheet.Sort.Orientation = Microsoft.Office.Interop.Excel.XlSortOrientation.xlSortColumns;
                //sheet.Sort.SortMethod = Microsoft.Office.Interop.Excel.XlSortMethod.xlPinYin;
                //sheet.Sort.Apply();
                //sheet.Sort.SortFields.Clear();
            }
            catch { }
        }
    }
}

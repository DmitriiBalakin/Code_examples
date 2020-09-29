using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace water
{
    public partial class frmClosePer : Form
    {

        SqlConnection con = new SqlConnection();

        public frmClosePer()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch { }
        }

        public static string PERMIN1(string per)
        {
            string retper = "", year = "", mon = "";
            year = per.Substring(0, 4);
            mon = per.Substring(4, 2);
            if (Convert.ToInt32(mon) == 1)
            {
                year = (Convert.ToInt32(year) - 1).ToString();
                retper = year + "12";
            }
            else
            {
                mon = (Convert.ToInt32(mon) - 1).ToString();
                if (mon.Length == 1) mon = "0" + mon;
                retper = year + mon;
            }
            return retper;
        }


        private void btn_perehod_Click(object sender, EventArgs e)
        {
            

        }

        private void frmClosePer_Shown(object sender, EventArgs e)
        {
            gv_close.Rows.Add("False","Начисление среднего по всем абонентам","Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            gv_close.Rows.Add("False", "Начисление по всем абонентам", "Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            gv_close.Rows.Add("False", "Перерасчет за некачественную услугу", "Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            gv_close.Rows.Add("False", "Начисление ОДН", "Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            gv_close.Rows.Add("False", "Проверка и выравнивание сальдо", "Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            gv_close.Rows.Add("False", "Переброска долгов в переходящих домах", "Не выполнено");
            gv_close.Rows[gv_close.Rows.Count-1].DefaultCellStyle.BackColor = Color.Orange;
            int height_ = 0;
            for (int i = 0; i<gv_close.Rows.Count; i++) height_ += gv_close.Rows[i].Height;
            height_ += gv_close.ColumnHeadersHeight+gv_close.RowTemplate.Height;
            gv_close.Height = height_;
            button1.Left = gv_close.Location.X;
            button1.Top = gv_close.Location.Y + gv_close.Height + 10;

            gv_create.Top = button1.Location.Y + button1.Height + 10;
            gv_create.Left = button1.Location.X;
            gv_create.Rows.Add("False", "Создание нового периода", "Не выполнено");
            gv_create.Rows[gv_create.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
            height_ = 0;
            for (int i = 0; i < gv_create.Rows.Count; i++) height_ += gv_create.Rows[i].Height;
            height_ += gv_create.ColumnHeadersHeight + gv_create.RowTemplate.Height;
            gv_create.Height = height_;
            button2.Left = gv_create.Location.X;
            button2.Top = gv_create.Location.Y + gv_create.Height + 10;
            button2.Enabled = false;

            gv_new.Left = button2.Location.X;
            gv_new.Top = button2.Location.Y + button2.Height + 10;
            gv_new.Rows.Add("False", "Переход домов в новые ведомства", "Не выполнено");
            gv_new.Rows[gv_new.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
            gv_new.Rows.Add("False", "Переход на нормативы", "Не выполнено");
            gv_new.Rows[gv_new.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
            gv_new.Rows.Add("False", "Начисление по всем абонентам", "Не выполнено");
            gv_new.Rows[gv_new.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Orange;
            height_ = 0;
            for (int i = 0; i < gv_new.Rows.Count; i++) height_ += gv_new.Rows[i].Height;
            height_ += gv_new.ColumnHeadersHeight + gv_new.RowTemplate.Height;
            gv_new.Height = height_;
            button3.Left = gv_new.Location.X;
            button3.Top = gv_new.Location.Y + gv_new.Height + 10;
            button3.Enabled = false;
        }

    }
}

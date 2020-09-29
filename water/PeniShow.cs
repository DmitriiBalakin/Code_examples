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
    public partial class PeniShow : Form
    {
        // -- переменные класса --
        string Lic;
        SqlConnection conn;

        private class InfoPeni
        {

            public string Per;
            public double DolgBeg;
            public double Peni;
            public double PeniPayed;
            public double DolgEnd;

            public InfoPeni(string per, double dolgBeg, double peni, double peniPayed, double dolgEnd)
            {
                Per = per;
                DolgBeg = dolgBeg;
                Peni = peni;
                PeniPayed = peniPayed;
                DolgEnd = dolgEnd;
            }
        }

        // -- Уменьшение строки с датой на месяц --
        private string GetPrevPeriod(string Period, int CountMonths = 1)
        {
            for (int i = CountMonths; i > 0; i--)
                Period = (Period.Substring(2, 2) == "01") ? (Convert.ToInt32(Period) - 89).ToString() : (Convert.ToInt32(Period) - 1).ToString();
            Period = "20" + Period;
            return Period;
        }

        private void RefreshGrid()
        {
            // -- Выбираем все показания по пене за последние 3 года --
            PeniGrid.Rows.Clear();
            string cmd_str = "";
            string BaseName = Lic.Substring(0, 1) == "1" ? "Abon.dbo." : "AbonUK.dbo.";
            int j = 0;
            for(int i = Convert.ToInt32(frmMain.MaxCurPer); i > Convert.ToInt32(frmMain.MaxCurPer) - 500; i--)
            {
                j++;
                if(i % 100 == 00) 
                    i -= 88;
                cmd_str = cmd_str + "SELECT '" + i.ToString() + "' as per, a.SDolgBeg, a.Peni, b.Pay, a.SdolgEnd FROM " + BaseName + "abonent" + i.ToString() +
                                    " a INNER JOIN " + BaseName + "abonservsaldo" + i.ToString() + " b ON a.lic = b.lic and b.abonserv_code = 6 " +
                                    "WHERE a.lic = @Lic " + (j < 60 ? " UNION ALL " : "");
            }
            cmd_str = cmd_str + " ORDER BY per";
            SqlCommand cmd = new SqlCommand(cmd_str, conn);
            cmd.Parameters.AddWithValue("@Lic", Lic);




            //frmMain.MaxCurPer
            List<InfoPeni> infoPeni = new List<InfoPeni>();
            using (SqlDataReader DRaeder = cmd.ExecuteReader())
            {
                if (DRaeder.HasRows)
                {
                    while (DRaeder.Read())
                    {
                        infoPeni.Add(new InfoPeni(DRaeder["per"].ToString(), Convert.ToDouble(DRaeder["SDolgBeg"]), Convert.ToDouble(DRaeder["Peni"]),
                                                     Convert.ToDouble(DRaeder["Pay"]), Convert.ToDouble(DRaeder["SDolgEnd"])));
                    }
                }
                else
                {
                    infoPeni = null;
                }
                DRaeder.Close();
            }
            if (infoPeni.Count > 0)
            {
                for(int i = 0; i < infoPeni.Count; i++)
                {
                    InfoPeni iPeni = infoPeni[i];
                    PeniGrid.Rows.Add(iPeni.Per, iPeni.DolgBeg, iPeni.Peni, iPeni.PeniPayed, iPeni.DolgEnd);
                }
            }

            // -- Выбираем из базы все записи по текущему лицевому счету --
        }
        
        public PeniShow(string lic, SqlConnection con)
        {
            InitializeComponent();
            Lic = lic;
            conn = new SqlConnection(con.ConnectionString);
            // -- инициируем заголовки грида --
            PeniGrid.Columns.Add("Per", "Период");
            PeniGrid.Columns.Add("DolgBeg", "Долг на начало");
            PeniGrid.Columns.Add("Peni", "Начислено пени");
            PeniGrid.Columns.Add("PeniPayed", "Оплачено пени");
            PeniGrid.Columns.Add("DolgEnd", "Конечный долг");
            // -- Открываем соединение с базой данных --
            try
            {
                conn.Open();
            }
            catch
            {
                MessageBox.Show("Ошибка при открытии соединения с базой данных!");
                Close();
            }
            // -- Заполняем таблицу данными --
            RefreshGrid();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DeletePeniInPeriod(Lic, PeniGrid.CurrentRow.Cells[0].Value.ToString(), frmMain.MaxCurPer);
            int Number = PeniGrid.CurrentRow.Index;
            RefreshGrid();
            PeniGrid.Rows[0].Selected = false;
            PeniGrid.Rows[Number].Selected = true;
           
        }

// -- Удаление пени за какой-то период --
        private void DeletePeniInPeriod(string lic, string period, string lastperiod)
        {
            SqlConnection con = new SqlConnection(conn.ConnectionString);
            con.Open();
            double Peni = 0;
            string BaseName = lic.Substring(0, 1) == "1" ? "Abon" : "AbonUK";
            SqlCommand cmd = new SqlCommand(BaseName + ".dbo.PeniCorrect", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            for (int i = Convert.ToInt32(period); i <= Convert.ToInt32(lastperiod); i++)
            {
                string Per = (i.ToString().Substring(i.ToString().Length - 2, 2) == "13" ? (i + 88).ToString() : i.ToString());
                string cmd_str;
                i = (i.ToString().Substring(i.ToString().Length - 2, 2) == "13" ? i + 88 : i);
                if (i.ToString() == period)
                {
                    cmd_str = "SELECT Peni FROM " + BaseName + ".dbo.Abonent" + Per + " WHERE lic = @lic";
                    SqlCommand cmd1 = new SqlCommand(cmd_str, conn);
                    cmd1.Parameters.AddWithValue("@Lic", lic);
                    using (SqlDataReader DRaeder = cmd1.ExecuteReader())
                    {
                        if (DRaeder.HasRows)
                        {
                            DRaeder.Read();
                            Peni = Convert.ToDouble(DRaeder["Peni"]);
                        }
                        DRaeder.Close();
                    }
                    if (Peni == 0)
                        return;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = lic;
                    cmd.Parameters.Add("@TableChange", SqlDbType.NVarChar).Value = BaseName + ".dbo.Abonent" + Per;
                    cmd.Parameters.Add("@FieldChange", SqlDbType.NVarChar).Value = " Peni = 0 ";
                    cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = lic;
                    cmd.Parameters.Add("@TableChange", SqlDbType.NVarChar).Value = BaseName + ".dbo.AbonServSaldo" + Per;
                    cmd.Parameters.Add("@FieldChange", SqlDbType.NVarChar).Value = " Charge = 0 ";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = lic;
                    cmd.Parameters.Add("@TableChange", SqlDbType.NVarChar).Value = BaseName + ".dbo.Abonent" + Per;
                    cmd.Parameters.Add("@FieldChange", SqlDbType.NVarChar).Value = " Sdolgbeg = SDolgBeg - " + Peni.ToString();
                    cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = lic;
                    cmd.Parameters.Add("@TableChange", SqlDbType.NVarChar).Value = BaseName + ".dbo.AbonServSaldo" + Per;
                    cmd.Parameters.Add("@FieldChange", SqlDbType.NVarChar).Value = " Saldo = Saldo - " + Peni.ToString();
                    cmd.ExecuteNonQuery();
                }
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = lic;
                cmd.Parameters.Add("@TableChange", SqlDbType.NVarChar).Value = BaseName + ".dbo.Abonent" + Per;
                cmd.Parameters.Add("@FieldChange", SqlDbType.NVarChar).Value = " Sdolgend = Sdolgend - " + Peni.ToString();
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }
        
    }
}

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


    public partial class frmSpis : Form
    {
        SqlConnection con = new SqlConnection();
        List<spis> lic = new List<spis>();

        private string PERMIN(string per)
        {
            int year = Convert.ToInt32(per.Substring(0,4));
            int mon = Convert.ToInt32(per.Substring(4, 2));
            mon = (mon - 1) == 0 ? 12 : mon - 1;
            year = mon == 12 ? year - 1 : year;
            per = year.ToString() + ((mon.ToString().Length == 1) ? "0" + mon.ToString() : mon.ToString());
            return per;
        }

        private string PERPLU(string per)
        {
            int year = Convert.ToInt32(per.Substring(0, 4));
            int mon = Convert.ToInt32(per.Substring(4, 2));
            mon = (mon + 1) == 13 ? 1 : mon + 1;
            year = mon == 1 ? year + 1 : year;
            per = year.ToString() + ((mon.ToString().Length == 1) ? "0" + mon.ToString() : mon.ToString());
            return per;
        }

        public frmSpis()
        {
            InitializeComponent();
            dateTimePicker1.Value = Convert.ToDateTime(PERMIN(frmMain.MaxCurPer).Substring(0,4) + "-" + PERMIN(frmMain.MaxCurPer).Substring(4,2) + "-01");
            dateTimePicker1.Value = dateTimePicker1.Value.AddYears(-3);
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lic.Clear();
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            lic.Clear();
            string per = dateTimePicker1.Value.Year.ToString()+(dateTimePicker1.Value.Month.ToString().Length == 1?"0"+dateTimePicker1.Value.Month.ToString():dateTimePicker1.Value.Month.ToString());
            per = PERPLU(per);
            try
            {
                if (con.State == ConnectionState.Open)
                {
                    label1.Text = "Идет поиск лицевых...";
                    Application.DoEvents();
                    com.CommandText = @"select a.lic,a.sndeb,a.sdolgbeg from abon.dbo.abonent"+per+@" a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=0 and a.sndeb>0 and a.sndeb >= a.sdolgbeg and a.sdolgbeg>0
                                    union all
                                    select a.lic,a.sndeb,a.sdolgbeg from abonuk.dbo.abonent" + per + @" a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=1 and a.sndeb>0 and a.sndeb >= a.sdolgbeg and a.sdolgbeg>0";
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            while (r.Read())
                            {
                                spis lic_ = new spis();
                                lic_.lic = r["lic"].ToString();
                                lic_.saldo = Convert.ToDouble(r["sndeb"].ToString());
                                lic_.dolg = Convert.ToDouble(r["sdolgbeg"].ToString());
                                lic_.pos = 0;
                                lic_.spisanie = 0;
                                lic.Add(lic_);
                            }
                        }
                        double sumd = 0, sumk=0;
                        for (int i = 0; i < lic.Count; i++)
                        {
                            sumd += lic[i].saldo;
                            sumk += lic[i].dolg;
                        }
                        label1.Text = "Найдено " + lic.Count.ToString() + " лиц. счетов. ДЕБ сальдо "+Math.Round(sumd,2).ToString()+". Квитанционный долг "+Math.Round(sumk,2).ToString();
                    }
                }
            }
            catch { label1.Text = "0"; }
        }


        private bool STOP = false;
        private void button2_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = lic.Count;
            progressBar1.Value = 0;
            STOP = false;
            string per = dateTimePicker1.Value.Year.ToString() + (dateTimePicker1.Value.Month.ToString().Length == 1 ? "0" + dateTimePicker1.Value.Month.ToString() : dateTimePicker1.Value.Month.ToString());
            per = PERPLU(per);
            try
            {
                int cnt = 0;
                double spisanie = 0;
                com.CommandText = "delete from abon.dbo.spisanie where per='"+per+"'" ;
                com.ExecuteNonQuery();
                for (int i = 0; i < lic.Count; i++)
                {
                    label4.Text = "Обработка л/сч " + lic[i].lic;
                    string curper = per;
                    //// собираем все платежи абонента до текущего периода
                    while (curper != frmMain.MaxCurPer)
                    {
                        com.CommandText = "select a.pos from abon.dbo.abonent" + curper + " a inner join abon.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=0 and a.lic='1"+lic[i].lic.Substring(1,9)+@"'
                                            union all
                                            select a.pos from abonuk.dbo.abonent" + curper + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=1 and a.lic='2"+lic[i].lic.Substring(1,9)+"'";
                        using (SqlDataReader r = com.ExecuteReader())
                        {
                            if (r.HasRows)
                            {
                                r.Read();
                                lic[i].pos += Convert.ToDouble(r["pos"].ToString());
                            }
                        }
                        curper = PERPLU(curper);
                        if (STOP) break;
                    }
                    //// определяем остаток долга для списания
                    lic[i].spisanie = (lic[i].dolg - lic[i].pos) > 0 ? lic[i].dolg - lic[i].pos : 0;
                    //// проверяем есть ли абоенет в текущем месяце и сальдо >= квитанции
                    if (lic[i].spisanie > 0)
                    {
                        com.CommandText = "select a.lic from abon.dbo.abonent" + frmMain.MaxCurPer + @" a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=0 and a.sndeb>0 and a.sdolgbeg>0 and a.sndeb>=a.sdolgbeg and right(a.lic,9)='"+lic[i].lic.Substring(1,9)+@"'
                                            union all
                                            select a.lic from abonuk.dbo.abonent" + frmMain.MaxCurPer + " a inner join abonuk.dbo.spvedomstvo v on v.id=a.kodvedom where v.buk=1 and a.sndeb>0 and a.sdolgbeg>0 and a.sndeb>=a.sdolgbeg and right(a.lic,9)='" + lic[i].lic.Substring(1, 9) + "'";
                        using (SqlDataReader r = com.ExecuteReader())
                        {
                            if (!r.HasRows) lic[i].spisanie = 0; else lic[i].spisanie = Math.Round(lic[i].spisanie, 2);
                        }
                    }
                    if (lic[i].spisanie > 0)
                    {
                        cnt++;
                        spisanie += lic[i].spisanie;
                    

                    com.CommandText = "insert into abon.dbo.spisanie(per,lic,spisanie) values(@per,@lic,@sp)";
                    com.Parameters.AddWithValue("@per", per);
                    com.Parameters.AddWithValue("@lic", lic[i].lic);
                    com.Parameters.AddWithValue("@sp", Math.Round(lic[i].spisanie, 2));
                    com.ExecuteNonQuery();
                    com.Parameters.Clear();
                    }
                    if (STOP)  break; 
                    progressBar1.Value++;
                    Application.DoEvents();
                }

                
                if (STOP)
                {
                    STOP = false;
                    MessageBox.Show("Расчет прерван.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Определено "+cnt.ToString()+" л/счетов. Сумма на списание "+Math.Round(spisanie,2).ToString(), "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                STOP = false;
                MessageBox.Show("Произошел сбой.\nОбратитесь в отдел АСУ.","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            STOP = true;
        }
    }
}

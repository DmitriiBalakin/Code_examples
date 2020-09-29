using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace water
{
    public partial class frmMoveCountLic : Form
    {
        private int Z;
        public frmMoveCountLic()
        {
            InitializeComponent();
            Z = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr;
            dr = folderBrowserDialog1.ShowDialog();
            if (dr  == System.Windows.Forms.DialogResult.OK)
            {
                loadFiles(folderBrowserDialog1.SelectedPath);
            }

            checkvedom();
        }

        private void checkvedom()
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            com.CommandType = CommandType.Text;
            try
            {
                con.Open();
                for (int i = 0; i < gv_move.Rows.Count; i++)
                {
                com.CommandText = "select 1 from abonuk.dbo.spvedomstvo where MoveLic = @vedom";
                com.Parameters.AddWithValue("@vedom", gv_move.Rows[i].Cells["vedom_"].Value);
                using (SqlDataReader read = com.ExecuteReader())
                {
                    if (read.HasRows)
                    {
                        gv_move.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        gv_move.Rows[i].DefaultCellStyle.BackColor = Color.Orange;
                    }
                }
                com.Parameters.Clear();
                }
            }
            catch { }
        }

        private void loadFiles(string path)
        {
            gv_move.Rows.Clear();
            var dir = new DirectoryInfo(path);
            List<string> files = new List<string>();
            DBF dbf = new DBF();
            foreach (FileInfo file in dir.GetFiles())
            {
                files.Add(file.FullName);
                dbf.FilePath = file.FullName;
                dbf.ReadDBF();
                var row = dbf.Table.Compute("min(UO)","UO>''");
                gv_move.Rows.Add(false, Path.GetFileName(files.ElementAt(files.Count - 1)), row.ToString(), files.ElementAt(files.Count-1));
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gv_move.Rows.Count; i++)
            {
                if (gv_move.Rows[i].DefaultCellStyle.BackColor == Color.LightGreen) gv_move.Rows[i].Cells[0].Value = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gv_move.Rows.Count; i++)
            {
                gv_move.Rows[i].Cells[0].Value = false;
            }
        }

        private void gv_move_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gv_move.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.LightGreen) 
             if (!(bool)gv_move.Rows[e.RowIndex].Cells["check"].Value) gv_move.Rows[e.RowIndex].Cells["check"].Value = true;
             else gv_move.Rows[e.RowIndex].Cells["check"].Value = false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!backgroundWorker1.CancellationPending) e.Result = get_move();
            else
            {
                e.Cancel = true;
            }
        }

        private bool get_move()
        {
            Boolean result = false;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.Text;
            com.Connection = con;
            
            DBF dbf = new DBF();
            Int32 cur = 0;
            
            try
            {
                con.Open();
                for (int i = 0; i < gv_move.Rows.Count; i++)
                {
                    if ((bool)gv_move.Rows[i].Cells["check"].Value)
                    {
                        dbf.FilePath = gv_move.Rows[i].Cells["path_"].Value.ToString();
                        dbf.ReadDBF();
                        DataRow[] data = dbf.Table.Select("ISNEIRC > ''");
                        foreach (DataRow row in data)
                        {
                            com.Parameters.Clear();
                            com.CommandText = @"select a.* from AbonUK.dbo.Abonent"+frmMain.MaxCurPer+" a inner join abonuk.dbo.spvedomstvo v on a.kodvedom=v.id where a.lsnEirc=@isneirc and v.MoveLic=@vedom";
                            com.Parameters.AddWithValue("@isneirc", row["isneirc"].ToString().Trim());
                            com.Parameters.AddWithValue("@vedom", row["UO"].ToString().Trim());

                            string com1 = "", com2="", com3="";
                            using (SqlDataReader nread = com.ExecuteReader())
                            {
                                SqlDataReader read = null;
                                if (nread.HasRows)
                                {
                                    DataTable dt = new DataTable();
                                    dt.Load(nread);
                                    if (dt.Rows.Count > 1)
                                    {
                                        com3 = "insert into abonuk.dbo.movelicerror(isneirc,vedom,adres,new,per,modirec,[user],[file],reason) values('" + row["isneirc"].ToString().Trim() + "','" + row["uo"].ToString().Trim() + "','" + row["address"].ToString().Trim() + "'," + Convert.ToInt32(row["perkon"].ToString().Trim()).ToString() + "," + frmMain.MaxCurPer + ",GETDATE(),'" + System.Net.Dns.GetHostName()+"','"+dbf.FileName+"','Найдено несколько счетов с одинаковым ISNEIRC')";
                                    }
                                    else
                                    {
                                        read = com.ExecuteReader();
                                        read.Read();
                                        ///если в управляющей стало 0, то у нас должна быть 1
                                        ///если у нас тоже 0, то оставляем 0.
                                        int NOW = Convert.ToInt16(row["pernach"].ToString().Trim());
                                        int CON = Convert.ToInt16(row["perkon"].ToString().Trim());
                                        int VIB = Convert.ToInt16(row["vibilo"].ToString().Trim());
                                        int PRIB = Convert.ToInt16(row["pribilo"].ToString().Trim());
                                        string ISNEIRC = row["isneirc"].ToString().Trim();
                                        string lic = read["lic"].ToString();

                                        if (NOW == 0 & CON == 0) com1 = "insert into abonuk.dbo.movelic(lic,per,modirec,old,new,FIO_uk,[user]) values('" + lic + "'," + frmMain.MaxCurPer + ",GETDATE()," + Convert.ToInt16(read["liver"].ToString()).ToString() + "," + Convert.ToInt16(read["liver"].ToString()).ToString() + ",'" + row["fam"].ToString().Trim() + "','" + System.Net.Dns.GetHostName() + "')";
                                        if (NOW > 0 & CON == 0 && Convert.ToInt32(read["Liver"]) != 0)
                                        {
                                            com2 = @"UPDATE AbonUK.dbo.Abonent"+frmMain.MaxCurPer+" SET Liver=1 where lic='"+lic+"'";
                                            com1 = "insert into abonuk.dbo.movelic(lic,per,modirec,old,new,FIO_uk,[user]) values('" + lic + "'," + frmMain.MaxCurPer + ",GETDATE()," + Convert.ToInt16(read["liver"].ToString()).ToString() + "," + CON.ToString() + ",'" + row["fam"].ToString().Trim() + "','" + System.Net.Dns.GetHostName() + "')";
                                        }
                                        else
                                            if (NOW > 0 && CON == 0 && Convert.ToInt32(read["Liver"]) == 0) { com1 = "insert into abonuk.dbo.movelic(lic,per,modirec,old,new,FIO_uk,[user]) values('" + lic + "'," + frmMain.MaxCurPer + ",GETDATE()," + Convert.ToInt16(read["liver"].ToString()).ToString() + "," + CON.ToString() + ",'" + row["fam"].ToString().Trim() + "','" + System.Net.Dns.GetHostName() + "')"; }
                                            else
                                                ///если в упраляющей изменения и конечное чило не 0, то заносим значение ЖЭУ
                                                if ((PRIB != 0 || VIB != 0) && CON != 0)
                                                {
                                                    com2 = @"UPDATE AbonUK.dbo.Abonent"+frmMain.MaxCurPer+" SET Liver=" + CON + " where lic='" + lic + "'";
                                                    com1 = "insert into abonuk.dbo.movelic(lic,per,modirec,old,new,FIO_uk,[user]) values('" + lic + "'," + frmMain.MaxCurPer + ",GETDATE()," + Convert.ToInt16(read["liver"].ToString()).ToString() + "," + CON.ToString() + ",'" + row["fam"].ToString().Trim() + "','" + System.Net.Dns.GetHostName() + "')";
                                                }
                                                else
                                                    if (NOW != 0 && CON != 0)
                                                    {
                                                        com2 = @"UPDATE AbonUK.dbo.Abonent"+frmMain.MaxCurPer+" SET Liver=" + CON + " where lic='" + lic + "'";
                                                        com1 = "insert into abonuk.dbo.movelic(lic,per,modirec,old,new,FIO_uk,[user]) values('" + lic + "'," + frmMain.MaxCurPer + ",GETDATE()," + Convert.ToInt16(read["liver"].ToString()).ToString() + "," + CON.ToString() + ",'" + row["fam"].ToString().Trim() + "','" + System.Net.Dns.GetHostName() + "')";
                                                    }
                                    }
                                }
                                else com3 = "insert into abonuk.dbo.movelicerror(isneirc,vedom,adres,new,per,modirec,[user],[file],reason) values('" + row["isneirc"].ToString().Trim() + "','" + row["uo"].ToString().Trim() + "','" + row["address"].ToString().Trim() + "'," + Convert.ToInt32(row["perkon"].ToString().Trim()).ToString() + "," + frmMain.MaxCurPer + ",GETDATE(),'" + System.Net.Dns.GetHostName() + "','" + dbf.FileName + "','Не найден счет с таким ISNEIRC')";
                            if (read != null) read.Close();
                            }///using
                            cur++;
                            Z = cur;
                            com.Parameters.Clear();
                            if (com1 != "")
                            {
                                com.CommandText = com1;
                                com.ExecuteNonQuery();
                            }
                            if (com2 != "")
                            {
                                com.CommandText = com2;
                                com.ExecuteNonQuery();
                            }
                            if (com3 != "")
                            {
                                com.CommandText = com3;
                                com.ExecuteNonQuery();
                            }
                        } ///foreach
                        gv_move.Rows[i].Cells["check"].Value = false;
                    }
                    dbf.Clear();
                }
                dbf.Close();
                con.Close();
                result = true;
            }
            catch
            {
                if (con.State == ConnectionState.Open) con.Close();
                result = false;
            }
            if (result) goto end;
        error:
            result = false;
            end:
            return result;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Z = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Enabled = false;
            Z = 0;
            progressBar1.Value = 0;
            label4.Text = progressBar1.Maximum + " из " + progressBar1.Maximum;
            button4.Text = "Начать прием";
            panel1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = Z;
            label4.Text = Z.ToString() + " из " + progressBar1.Maximum;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DBF dbf = new DBF();
            if (!backgroundWorker1.IsBusy)
            {
                panel1.Enabled = false;
                progressBar1.Maximum = 0;
                try
                {
                    for (int i = 0; i < gv_move.Rows.Count; i++)
                    {
                        if ((bool)gv_move.Rows[i].Cells["check"].Value)
                        {
                            dbf.FilePath = gv_move.Rows[i].Cells["path_"].Value.ToString();
                            dbf.ReadDBF();
                            progressBar1.Maximum += dbf.RecordsCount;
                            dbf.Clear();
                        }
                    }
                    label4.Text = "0 из " + progressBar1.Maximum.ToString();
                }
                catch { }
                dbf.Close();
                button4.Text = "Остановить прием";
                timer1.Enabled = true;
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                backgroundWorker1.CancelAsync();
                panel1.Enabled = true;
                button4.Text = "Начать прием";
                timer1.Enabled = false;
            }
        }

        private void frmMoveCountLic_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                e.Cancel = true;
                MessageBox.Show("Не возможно закрыть форму, идет прием данных","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
    }
}

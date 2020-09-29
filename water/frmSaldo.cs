using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace water
{
    public partial class frmSaldo : Form
    {
        public frmSaldo()
        {
            InitializeComponent();
            db_con.ConnectionString = frmMain.db_con.ConnectionString;
            db_com.Connection = db_con;
            db_com.CommandTimeout = 0;
            db_com.CommandType = CommandType.Text;
            try
            {
                db_con.Open();
            }
            catch
            {
                MessageBox.Show("Невозможно соединиться с базой данных","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
            }
        }

        SqlCommand db_com = new SqlCommand();
        SqlConnection db_con = new SqlConnection();

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            panel3.Visible = true;
            panel1.Enabled = false;
            gv_saldo.Enabled = false;
            panel3.Enabled = true;
            Application.DoEvents();
            Boolean Error = false;
            string Per = "";
            Per = dateTimePicker1.Value.Year.ToString();
            Per += (dateTimePicker1.Value.Month.ToString().Length > 1)?dateTimePicker1.Value.Month.ToString():"0"+dateTimePicker1.Value.Month.ToString();
           
            ///выборка ведомств
            List<string> Vedom = new List<string>();
            try
            {
                db_com.CommandText = "select id from abonuk.dbo.spvedomstvo where buk = 1 and bpaketC =1";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            Vedom.Add(db_read["id"].ToString());
                        }
                    }
                }
            }
            catch { Error = true; goto END_ERROR; }

            ///формирование таблиц по ведомствам
            gv_saldo.Rows.Clear();
            try
            {
                for (int i = 0; i < Vedom.Count; i++)
                {
                    db_com.CommandText = @"Select sv.NameVed as NameVed,sv.bPaketC, sp.SpravId, ass.KodVed,-1,
							SUM((case when ass.SaldoBeg>=0 then ass.SaldoBeg else 0 end))  SnDeb,
							SUM((case when ass.SaldoBeg<0 then -1*(ass.SaldoBeg) else 0 end))  as SnKred,
							SUM(ass.Nv) as NvFull,SUM(ass.Nk) as NkFull, SUM(ass.Np) as NpFull,
							SUM(ass.Spisan) as Spisan,
							SUM(ass.Sv) as StorV,SUM(ass.Sk) as StorK,SUM(ass.Sp) as StorP,
							SUM(CASE WHEN PerEND = 0 THEN a.CubeV ELSE 0 END) AS CUBEV,
							SUM(CASE WHEN PerEND = 0 THEN a.CubeK ELSE 0 END) AS CubeK,
							SUM(CASE WHEN PerEND = 0 THEN ISNULL(an.CubeP,0) ELSE 0 END) AS CubeP,
							SUM(CASE WHEN PerEND = 0 THEN ISNULL(o.kv, 0) ELSE 0 END) AS kv,
							SUM(CASE WHEN PerEND = 0 THEN ISNULL(o.kk, 0) ELSE 0 END) AS kk, 
							SUM(CASE WHEN PerEND = 0 THEN ISNULL(o.kp, 0) ELSE 0 END) AS kp,
							SUM(isnull(ass.Pv,0)) as Pv,
							SUM(isnull(ass.Pk,0)) as Pk,
							SUM(isnull(ass.Pp,0)) as Pp,
							SUM((case when ass.SaldoEnd>=0 then ass.SaldoEnd else 0 end))  as SkDeb,
							SUM((case when ass.SaldoEnd<0 then -1*(ass.SaldoEnd) else 0 end )) as SkKred,
							SUM(ass.SaldoEnd) as SaldoEnd
							FROM        abonuk.dbo.AbonSaldo" + Per + @" AS ass INNER JOIN
												  abonuk.dbo.abonent" + Per + @" AS a ON ass.Lic = a.Lic INNER JOIN
												  abonuk.dbo.SpVedomstvo AS sv ON ass.KodVed = sv.ID LEFT OUTER JOIN
													  (SELECT     lic, SUM(kv) AS kv, SUM(kk) AS kk, SUM(kp) AS kp
														FROM          oplata" + Per + @"
														GROUP BY lic) AS o ON ass.Lic = o.lic
										  LEFT OUTER JOIN (SELECT Lic,sum(Cube) as CubeP
																			FROM abonuk.dbo.AbonentNach" + Per + @"  INNER JOIN
																					abonuk.dbo.SpGrantServices sgs ON sgs.Id = GranServiceID
																			WHERE sgs.TypeServicesID = 3
																			GROUP BY Lic) an ON an.Lic = a.Lic
											 INNER JOIN
												  abonuk.dbo.street AS s ON a.Str_code = s.cod_yl INNER JOIN
												  abonuk.dbo.SpPosel AS sp ON s.PoselKod = sp.PoselId
							WHERE sv.ID = @Vedom
                            GROUP BY  Sv.NameVed , ass.kodved, sv.bPaketC, sp.SpravId";
                    db_com.Parameters.AddWithValue("@Vedom", Vedom[i].ToString());
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            while (db_read.Read())
                            {
                                ///
                                double nv, nk, np;
                                double kv, kk, kp;
                                double pv, pk, pp;
                                double sv, sk, sp;
                                nv = Convert.ToDouble(db_read["nvfull"].ToString());
                                nk = Convert.ToDouble(db_read["nkfull"].ToString());
                                np = Convert.ToDouble(db_read["npfull"].ToString());
                                kv = Convert.ToDouble(db_read["cubev"].ToString());
                                kk = Convert.ToDouble(db_read["cubek"].ToString());
                                kp = Convert.ToDouble(db_read["cubep"].ToString());
                                pv = Convert.ToDouble(db_read["pv"].ToString());
                                pk = Convert.ToDouble(db_read["pk"].ToString());
                                pp = Convert.ToDouble(db_read["pp"].ToString());
                                sv = Convert.ToDouble(db_read["storv"].ToString());
                                sk = Convert.ToDouble(db_read["stork"].ToString());
                                sp = Convert.ToDouble(db_read["storp"].ToString());
                                string[] row = {db_read["NameVed"].ToString(),db_read["SnDeb"].ToString(),db_read["SnKred"].ToString(),(Math.Round(nv+nk+np,2)).ToString(),Math.Round(nv,2).ToString(),Math.Round(kv,3).ToString(),Math.Round(nk,2).ToString(),Math.Round(kk,3).ToString(),Math.Round(np,2).ToString(),Math.Round(kp,3).ToString(),Math.Round((sv+sk+sp),2).ToString(),Math.Round(sv,2).ToString(),Math.Round(sk,2).ToString(),Math.Round(sp,2).ToString(),Math.Round((pv+pk+pp),2).ToString(),Math.Round(pv,2).ToString(),Math.Round(pk,2).ToString(),Math.Round(pp,2).ToString(),db_read["skdeb"].ToString(),db_read["skkred"].ToString()};
                                gv_saldo.Rows.Add(row);
                            }
                        }
                    }
                    db_com.Parameters.Clear();
                }
            }
            catch { Error = true; goto END_ERROR; }

            ///ИТОГО
            try
            {
                string[] row = {"Итого","0","0","0","0","0","0","","","","","","","","","","","","",""};
                for(int i = 1; i<gv_saldo.ColumnCount;i++)
                {
                    double sum = 0;
                    for(int j=1;j<gv_saldo.Rows.Count;j++)
                        sum += Convert.ToDouble(gv_saldo.Rows[j].Cells[i].Value);
                    row[i] = sum.ToString();
                }
                gv_saldo.Rows.Add(row);
            }
            catch { }

        END_ERROR: if (Error) MessageBox.Show("Ошибка формирования отчета", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            db_com.Parameters.Clear();
            gv_saldo.Enabled = true;
            //gv_saldo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //gv_saldo.AutoResizeColumns();
            gv_saldo.ScrollBars = ScrollBars.Horizontal;
            gv_saldo.ScrollBars = ScrollBars.Both;
            gv_saldo.Refresh();
            panel3.Visible = false;
            panel1.Enabled = true;
        }

        private void frmSaldo_Shown(object sender, EventArgs e)
        {
            dateTimePicker1.MinDate = new DateTime(2012,01,01);
            dateTimePicker1.MaxDate = new DateTime(Convert.ToInt16(frmMain.MaxCurPer.Substring(0,4)), Convert.ToInt16(frmMain.MaxCurPer.Substring(4,2)), 30);

        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(gv_saldo.Size.Width + 10, gv_saldo.Size.Height + 10);
            gv_saldo.DrawToBitmap(bmp, gv_saldo.Bounds);
            e.PageSettings.Landscape = true;
            e.Graphics.DrawImage(bmp, 0, 0);
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            ///printPreviewDialog1.Show();
            printDocument1.DefaultPageSettings.Landscape = true;
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
            /*
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            printPreviewDialog1.Document = pd;
            printPreviewDialog1.ShowDialog();*/
        }

        private void frmSaldo_Load(object sender, EventArgs e)
        {
        }

        private void ToCsV(DataGridView dGV, string filename)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++)
            {
                string stLine = "";
                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
                stOutput += stLine + "\r\n";
            }
            Encoding enc = Encoding.GetEncoding(1251);
            byte[] output = enc.GetBytes(stOutput);
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        } 

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "Saldo.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(gv_saldo, sfd.FileName); // Here dataGridview1 is your grid view name
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string html = "";
            html = "<html><head><title>Сальдо</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/><style>body { font-family: Verdana; font-size: 7px;} tbody td { font-size: 7pt;} </style></head><body><center><table border=1><tbody>";
            html += "<tr>";
            for (int i = 0; i < gv_saldo.ColumnCount; i++)
                html += "<td align=\"center\">"+gv_saldo.Columns[i].HeaderText+"</td>";
            html += "</tr>";
            for (int i = 0; i < gv_saldo.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < gv_saldo.ColumnCount; j++)
                    html += "<td"+((j>0)?" align=\"right\"":"")+">" + gv_saldo.Rows[i].Cells[j].Value + "</td>";
                html += "</tr>";
            }
            html += "</tbody></table></center></body></html>";
            frmPrn frmCPrn = new frmPrn();
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath() + "tmpSaldo.html", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmpSaldo.html";
            file.WriteLine(html);
            file.Close();
            frmCPrn.Text = "Печать Сальдо";
            frmCPrn.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Color cc;
            if (gv_saldo.Rows.Count>0) 
            {
                cc = gv_saldo.Rows[gv_saldo.Rows.Count-1].Cells["NameCompany"].Style.BackColor;
            string html = "";
            html = "<html><head><title>Сальдо</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/><style>body { font-family: Verdana; font-size: 7px;} tbody td { font-size: 7pt;} </style></head><body><center><table border=1><tbody>";
            html += "<tr>";
            for (int i = 0; i < gv_saldo.ColumnCount; i++)
                if (gv_saldo.Columns[i].DefaultCellStyle.BackColor == Color.LightGreen) html += "<td align=\"center\">" + gv_saldo.Columns[i].HeaderText + "</td>";
            html += "</tr>";
            for (int i = 0; i < gv_saldo.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < gv_saldo.ColumnCount; j++)
                    if (gv_saldo.Columns[j].DefaultCellStyle.BackColor == Color.LightGreen) html += "<td" + ((j > 0) ? " align=\"right\"" : "") + ">" + gv_saldo.Rows[i].Cells[j].Value + "</td>";
                html += "</tr>";
            }
            html += "</tbody></table></center></body></html>";
            frmPrn frmCPrn = new frmPrn();
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath() + "tmpSaldo.html", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmpSaldo.html";
            file.WriteLine(html);
            file.Close();
            frmCPrn.Text = "Печать Сальдо";
            frmCPrn.Show();
            }
        }

        private void gv_saldo_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void gv_saldo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

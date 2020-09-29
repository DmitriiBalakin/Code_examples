using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;

namespace water
{
    public partial class frmPayView : Form
    {
        SqlCommand db_cmd = new SqlCommand();
        bool buhok = false, operok = false;

        public frmPayView()
        {
            InitializeComponent();
            db_cmd.Connection = frmMain.db_con;
            db_cmd.CommandType = CommandType.Text;
            db_cmd.CommandTimeout = 0;
        }

        string payper = "";

        private void frmPayView_Load(object sender, EventArgs e)
        {

        }

        private void frmPayView_Shown(object sender, EventArgs e)
        {
            int year = Convert.ToInt16(frmMain.MaxCurPer.Substring(0, 4));
            int month = Convert.ToInt16(frmMain.MaxCurPer.Substring(4, 2));
            dateTimePicker1.MinDate = new DateTime(2012, 01, 01);
            dateTimePicker1.MaxDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            payper = dateTimePicker1.Value.Year.ToString() + convmonth(dateTimePicker1.Value.Month.ToString());

            try 
            {
                SqlCommand db_cmd = new SqlCommand();
                db_cmd.Connection = frmMain.db_con;
                db_cmd.CommandType = CommandType.Text;
                db_cmd.CommandText = "select sys_name,bric from abonuk.dbo.priemopt group by sys_name,bric";
                using (SqlDataReader db_reader = db_cmd.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        while (db_reader.Read())
                        {
                            cmb_Agent.Items.Add(new SelectData(db_reader["bric"].ToString(), db_reader["sys_name"].ToString() + " [" + db_reader["bric"].ToString() + "]"));
                        }
                        cmb_Agent.Items.Add(new SelectData("99","ВСЕ"));
                        cmb_Agent.SelectedIndex = 0;
                    }
                    db_reader.Close();
                }
                db_cmd.CommandText = "select isnull(buhok,0) as buhok, isnull(operok,0) as operok from abon.dbo.config where hostname = '"+frmMain.Host+"'";
                using (SqlDataReader db_reader = db_cmd.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        db_reader.Read();
                        if (db_reader["buhok"].ToString() == "True") buhok = true;
                        if (db_reader["operok"].ToString() == "True") operok = true;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message+"\nОбратитесь в отдел АСУ","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private string convmonth(string month)
        {
            if (month.Length == 1) month = "0" + month;
            return month;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            payper = dateTimePicker1.Value.Year.ToString()+ convmonth(dateTimePicker1.Value.Month.ToString());
        }

        private void btn_payview_Click(object sender, EventArgs e)
        {
            gv_payview.Rows.Clear();
            gv_payview.Refresh();
            double sumu = 0, sumg = 0, sum = 0;
            int cu = 0, cg = 0, c = 0;
            string brik = "";
                try
                {
                    if (cmb_Agent.Text == "ВСЕ") brik = "brik > "; else brik = "brik = ";
                    db_cmd.CommandText = "select isnull(b.day_p,c.day_p) as day_pp ,isnull(b.oplu,0) as oplu,isnull(cu,0) as cu,isnull(c.oplg,0) as oplg,isnull(cg,0) as cg,(isnull(b.oplu,0)+isnull(c.oplg,0)) as summ  from " +
                        "(select day_p,SUM(opl) as oplu, count(opl) as cu from " +
                        "(select opl, data_p, cast(cast(YEAR(data_p)as char(4))+left(right(cast(data_p as DATE),5),2)as int) as per,cast(CAST(DAY(data_p) as CHAR(2))as int) as day_p " +
                        "from abonuk.dbo.Pos" + payper + " " +
                        "where "+brik + ((SelectData)this.cmb_Agent.SelectedItem).Value + " " +
                        ") a " +
                        "where a.per = " + payper + " " +
                        "group by day_p) b " +
                        "full outer join ( " +
                        "select day_p,SUM(opl) as oplg, count(opl) as cg from " +
                        "(select opl, data_p, cast(cast(YEAR(data_p)as char(4))+left(right(cast(data_p as DATE),5),2)as int) as per,cast(CAST(DAY(data_p) as CHAR(2))as int) as day_p " +
                        "from abon.dbo.Pos" + payper + " " +
                        "where "+brik + ((SelectData)this.cmb_Agent.SelectedItem).Value + " " +
                        ") a " +
                        "where a.per = " + payper + " " +
                        "group by day_p " +
                        ") c on b.day_p = c.day_p order by day_pp";
                    ///"group by b.day_p,c.day_p,b.oplu,c.oplg"; 
                    using (SqlDataReader db_reader = db_cmd.ExecuteReader())
                    {
                        if (db_reader.HasRows)
                        {

                            while (db_reader.Read())
                            {
                                sumu += Convert.ToDouble(db_reader["oplu"].ToString());
                                cu += Convert.ToInt32(db_reader["cu"].ToString());
                                sumg += Convert.ToDouble(db_reader["oplg"].ToString());
                                cg += Convert.ToInt32(db_reader["cg"].ToString());
                                sum += Convert.ToDouble(db_reader["oplu"].ToString()) + Convert.ToDouble(db_reader["oplg"].ToString());
                                c += Convert.ToInt32(db_reader["cu"].ToString()) + Convert.ToInt32(db_reader["cg"].ToString());
                                string[] row = new string[] { db_reader["day_pp"].ToString(), db_reader["oplu"].ToString(), db_reader["cu"].ToString(), db_reader["oplg"].ToString(), db_reader["cg"].ToString(), db_reader["summ"].ToString(), (Convert.ToInt32(db_reader["cu"].ToString()) + Convert.ToInt32(db_reader["cg"].ToString())).ToString() };
                                gv_payview.Rows.Add(row);

                            }
                            string[] row1 = new string[] { "Итого", sumu.ToString(), cu.ToString(), sumg.ToString(), cg.ToString(), sum.ToString(), c.ToString() };
                            gv_payview.Rows.Add(row1);
                            gv_payview.Rows[gv_payview.Rows.Count - 1].Cells["buhok"].Value = 0;
                            gv_payview.Rows[gv_payview.Rows.Count - 1].Cells["operok"].Value = 0;
                            gv_payview.Refresh();
                        }
                        db_reader.Close();
                    }

                    for (int i = 0; i < gv_payview.Rows.Count; i++)
                    {
                        db_cmd.CommandText = "select isnull(buhok,0) as buhok,isnull(operok,0) as operok from abon.dbo.zErrFiles where zbrik='" + ((SelectData)this.cmb_Agent.SelectedItem).Value + "' and Convert(date,zdate,104)='"+payper.Substring(0,4)+"-"+payper.Substring(4,2)+"-"+convmonth(gv_payview.Rows[i].Cells["Date"].Value.ToString())+"'";
                        using (SqlDataReader db_read = db_cmd.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                db_read.Read();
                                if (db_read["buhok"].ToString() == "True")
                                {
                                    gv_payview.Rows[i].Cells["buhok"].Value = 1;
                                    gv_payview.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                }
                                else gv_payview.Rows[i].Cells["buhok"].Value = 0;
                                if (db_read["operok"].ToString() == "True")
                                {
                                    gv_payview.Rows[i].Cells["operok"].Value = 1;
                                    gv_payview.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                                }
                                else gv_payview.Rows[i].Cells["operok"].Value = 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\nОбратитесь в отдел АСУ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

            private static string GetDefaultPrinterName()
        {
            String[] printers = new string[PrinterSettings.InstalledPrinters.Count];
            PrinterSettings.InstalledPrinters.CopyTo(printers, 0);
            for (int i = 0; i < printers.Length; i++)
                if (new PrinterSettings() { PrinterName = printers[i] }.IsDefaultPrinter)
                    return printers[i];
            return null;
        }

            private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            printDocument1.DefaultPageSettings.Margins.Left = 30;
            printDocument1.PrinterSettings.PrinterName = GetDefaultPrinterName();
            Graphics g = e.Graphics;
            int x = 40;
            int y = 20;
            int cell_height = 0;
 
            int colCount = gv_payview.ColumnCount-2;
            int rowCount = gv_payview.RowCount;
 
            Font font = new Font("Verdana", 9, FontStyle.Bold, GraphicsUnit.Point);
            SolidBrush brush = new SolidBrush(Color.Black);
 
            int[] widthC = new int[colCount];
 
            int current_col = 0;
            int current_row = 0;

            g.DrawString(cmb_Agent.Text+ "  (период "+payper+")", font, brush, x, y);
            y += 20;
 
            while (current_col < colCount)
            {
                if (g.MeasureString(gv_payview.Columns[current_col].HeaderText.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(gv_payview.Columns[current_col].HeaderText.ToString(), font).Width;
                    }
                current_col++;
            }

 
            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    if (g.MeasureString(gv_payview[current_col, current_row].Value.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(gv_payview[current_col, current_row].Value.ToString(), font).Width;
                    }
                    current_col++;
                }
                current_col = 0;
                current_row++;
            }
 
            current_col = 0;
            current_row = 0;
 
            string value = "";
 
            int width = widthC[current_col];
            int height = gv_payview[current_col, current_row].Size.Height;
 
            Rectangle cell_border;
            
 
 
            while (current_col < colCount)
            {
                width = widthC[current_col];
                cell_height = gv_payview[current_col, current_row].Size.Height;
                cell_border = new Rectangle(x, y, width, height);
                value = gv_payview.Columns[current_col].HeaderText.ToString();
                g.DrawRectangle(new Pen(Color.Black), cell_border);
                g.DrawString(value, font, brush, x, y);
                x += widthC[current_col];
                current_col++;
            }
            current_row = -1;
            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    width = widthC[current_col];
                    cell_height = gv_payview[current_col, current_row].Size.Height;
                    cell_border = new Rectangle(x, y, width, height);
                    value = gv_payview[current_col, current_row].Value.ToString();
                    g.DrawRectangle(new Pen(Color.Black), cell_border);
                    g.DrawString(value, font, brush, x, y);
                    x += widthC[current_col];
                    current_col++;
                }
                current_col = 0;
                current_row++;
                x = 40;
                y += cell_height;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.ShowDialog();
            ///printDialog1.ShowDialog();
            ///printDocument1.Print();
        }

        private void gv_payview_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 7) & (e.RowIndex != gv_payview.Rows.Count -1) & (buhok))
            {
                if (gv_payview.Rows[e.RowIndex].Cells["buhok"].Value.ToString() == "0")
                {
                    db_cmd.CommandText = "update top(1) abon.dbo.zErrFiles set buhok=1 where zbrik='" + ((SelectData)this.cmb_Agent.SelectedItem).Value + "' and Convert(date,zdate,104)='"+payper.Substring(0,4)+"-"+payper.Substring(4,2)+"-"+convmonth(gv_payview.Rows[e.RowIndex].Cells["Date"].Value.ToString())+"'";
                    if (db_cmd.ExecuteNonQuery()>0)
                    {
                        gv_payview.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                        gv_payview.Rows[e.RowIndex].Cells["buhok"].Value = 1;
                        db_cmd.CommandText = "Update top(1) z set z.operok = 1 from zErrFiles as z left join zErrDetail d on d.zFile = z.zIDFile where z.buhOK = 1 and z.operok=0 and d.zFile is null";
                        db_cmd.ExecuteNonQuery();
                    }
                }
                else if (gv_payview.Rows[e.RowIndex].Cells["buhok"].Value.ToString() == "1")
                {
                    db_cmd.CommandText = "update top(1) abon.dbo.zErrFiles set buhok=0,operok=0 where zbrik='" + ((SelectData)this.cmb_Agent.SelectedItem).Value + "' and Convert(date,zdate,104)='" + payper.Substring(0, 4) + "-" + payper.Substring(4, 2) + "-" + convmonth(gv_payview.Rows[e.RowIndex].Cells["Date"].Value.ToString()) + "'";
                    if (db_cmd.ExecuteNonQuery()>0)
                    {
                        gv_payview.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                        gv_payview.Rows[e.RowIndex].Cells["buhok"].Value = 0;
                        gv_payview.Rows[e.RowIndex].Cells["operok"].Value = 0;
                    }
                }
            }

            if ((e.ColumnIndex == 8) & (e.RowIndex != gv_payview.Rows.Count -1) & (operok))
            {
                if ((gv_payview.Rows[e.RowIndex].Cells["operok"].Value.ToString() == "0") & (gv_payview.Rows[e.RowIndex].Cells["buhok"].Value.ToString() != "0"))
                {
                    db_cmd.CommandText = "update abon.dbo.zErrFiles set operok=1 where zbrik='" + ((SelectData)this.cmb_Agent.SelectedItem).Value + "' and Convert(date,zdate,104)='" + payper.Substring(0, 4) + "-" + payper.Substring(4, 2) + "-" + convmonth(gv_payview.Rows[e.RowIndex].Cells["Date"].Value.ToString()) + "'";
                    if (db_cmd.ExecuteNonQuery()>0)
                    {
                        gv_payview.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                        gv_payview.Rows[e.RowIndex].Cells["operok"].Value = 1;
                    }
                }
                else if (gv_payview.Rows[e.RowIndex].Cells["operok"].Value.ToString() != "0")
                {
                    db_cmd.CommandText = "update abon.dbo.zErrFiles set operok=0 where zbrik='" + ((SelectData)this.cmb_Agent.SelectedItem).Value + "' and Convert(date,zdate,104)='" + payper.Substring(0, 4) + "-" + payper.Substring(4, 2) + "-" + convmonth(gv_payview.Rows[e.RowIndex].Cells["Date"].Value.ToString()) + "'";
                    if (db_cmd.ExecuteNonQuery()>0)
                    {
                        gv_payview.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                        gv_payview.Rows[e.RowIndex].Cells["operok"].Value = 0;
                    }
                }
            }
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "Pay.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(gv_payview, sfd.FileName); // Here dataGridview1 is your grid view name
            }
        }

    }
}

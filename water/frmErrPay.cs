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
    public partial class frmErrPay : Form
    {
        List<Button> but = new List<Button>();
        SqlCommand db_com = new SqlCommand();
        private int _selectedMenuItem;
        private readonly ContextMenuStrip collectionRoundMenuStrip;
        private bool operok = false;

        public frmErrPay()
        {
            InitializeComponent();
            db_com.Connection = frmMain.db_con;
            db_com.CommandType = CommandType.Text;
            db_com.CommandTimeout = 0;

            var toolStripMenuItem1 = new ToolStripMenuItem { Text = "Вывести отчет" };
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            var toolStripMenuItem2 = new ToolStripMenuItem { Text = "Отпечатан" };
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            var toolStripMenuItem3 = new ToolStripMenuItem { Text = "Проверено" };
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            var toolStripMenuItem4 = new ToolStripMenuItem { Text = "-" };
            collectionRoundMenuStrip = new ContextMenuStrip();
            collectionRoundMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem4, toolStripMenuItem3 });
            checkedListBox1.MouseDown += checkedListBox1_MouseDown;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            checkedListBox1.SetItemChecked(_selectedMenuItem, true);
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                {
                    db_com.CommandText = "update zErrFiles set prnt=1 where zIDFile = '" + ((SelectData)checkedListBox1.Items[i]).Value + "'";
                    if (db_com.ExecuteNonQuery() > 0)
                    {
                        string value = ((SelectData)checkedListBox1.Items[i]).Value;
                        string Text = "";
                        if (((SelectData)checkedListBox1.Items[i]).Text.IndexOf("(") > 0)
                        Text = ((SelectData)checkedListBox1.Items[i]).Text.Substring(0, ((SelectData)checkedListBox1.Items[i]).Text.IndexOf("(")) + " (отпечатан)";
                        else Text = ((SelectData)checkedListBox1.Items[i]).Text + " (отпечатан)";
                        checkedListBox1.Items.Remove(checkedListBox1.Items[i]);
                        checkedListBox1.Items.Add(new SelectData(value, Text));
                        i = -1;
                    }
                }
            checkedListBox1.Refresh();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (operok)
            {
                if (DialogResult.Yes == MessageBox.Show("Вы подтвердждаете проверку файлов?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    checkedListBox1.SetItemChecked(_selectedMenuItem, true);
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                        if (checkedListBox1.GetItemChecked(i))
                        {
                            db_com.CommandText = "update zErrFiles set operok=1 where zIDFile = '" + ((SelectData)checkedListBox1.Items[i]).Value + "'";
                            if (db_com.ExecuteNonQuery() > 0) checkedListBox1.Items.Remove(checkedListBox1.Items[i]);
                        }
                    checkedListBox1.Refresh();
                }
            }
            else MessageBox.Show("Вы не имеете прав для данной операции", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
            checkedListBox1.SetItemChecked(_selectedMenuItem, true);
            StreamWriter file = new StreamWriter(new FileStream(Path.GetTempPath() + "tmpErrPay.html", FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            frmMain.UrlPrn = Path.GetTempPath() + "tmpErrPay.html";
            file.WriteLine("<html><head><title>Ошибки приема эл. платежей</title><meta http-equiv='Content-Type' content='text/html'; charset='UTF-8'/>" +
                "<style rel='stylesheet' type='text/css'>" +
                "body {font-family: verdana; font-size:10pt; background-color: #FFFFFF;}\n" +
                ".pagebreak {page-break-after: always;}\n"+
                "tbody td {font-size:10pt; }\n" +
                "thead td {font-size:9pt; }\n" +
                "tfoot td {font-size:11pt; }\n" +
                "</style></head><body>");
            int first = 0;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                if (checkedListBox1.GetItemChecked(i))
                {
                    if (first != 0) file.WriteLine("<hr /><div class=\"pagebreak\"></div>");
                    first = 1;
                    if (checkedListBox1.Items[i].ToString().IndexOf("(") > 0) file.WriteLine(checkedListBox1.Items[i].ToString().Substring(0, checkedListBox1.Items[i].ToString().IndexOf("(")) + " ");
                    else file.WriteLine(checkedListBox1.Items[i].ToString());
                    List<string> reason = new List<string>();
                    List<string> reason_desc = new List<string>();
                    string zDate = "", zDateLoad = "";
                    try
                    {
                        db_com.CommandText = "SELECT d.zIDreason as zIDreason, r.zDescr as zDescr, f.zDate as zDate, f.zDateLoad as zDateLoad FROM zErrDetail d inner join abon.dbo.zErrReason r on r.zReasonID = d.zIDreason inner join abon.dbo.zErrFiles f on f.zIDFile=zFile WHERE (zFile = '" + ((SelectData)checkedListBox1.Items[i]).Value + "') GROUP BY zIDreason, zdescr, zDate, zDateLoad";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                while(db_read.Read())
                                {
                                    reason.Add(db_read["zIDreason"].ToString().Trim());
                                    reason_desc.Add(db_read["zdescr"].ToString().Trim());
                                    zDate = db_read["zDate"].ToString();
                                    zDateLoad = db_read["zDateLoad"].ToString();
                                }
                            }
                        }
                        file.WriteLine(" от "+zDate.Substring(0,10)+" (Внесено: "+zDateLoad+")<br /><br />");
                        for(int ii=0;ii<reason.Count;ii++)
                        {
                            db_com.CommandText = "SELECT     zIDreason, LEFT(CAST(zDTPack AS nvarchar), 10) AS ZDTPack, zLic, zSum, zBarCode, zDB, S_HOL1, S_HOL2, S_HOL3, S_GOR1, S_GOR2, S_GOR3, r.zDescr as Zdescr "+
                                            "FROM abon.dbo.zErrDetail e "+
                                            "inner join abon.dbo.zErrReason r on r.zReasonID = e.zIDreason "+
                                            "WHERE     (zFile='"+((SelectData)checkedListBox1.Items[i]).Value+"') AND (zIDreason='"+reason.ElementAt(ii)+"')"+
                                            "GROUP BY zIDreason, LEFT(CAST(zDTPack AS nvarchar), 10), zLic, zSum, zBarCode, zDB, S_HOL1, S_HOL2, S_HOL3, S_GOR1, S_GOR2, S_GOR3,r.zDescr";
                            using (SqlDataReader db_read = db_com.ExecuteReader())
                            {
                                if (db_read.HasRows)
                                {
                                    
                                    file.WriteLine(reason_desc.ElementAt(ii)+"<br/>");
                                    file.WriteLine("<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\">");
                                    file.WriteLine("<tr> <td width=\"100px\"  align=\"center\">Лиц.счет</td> <td width=\"100px\" align=\"center\">Сумма</td> <td width=\"250px\" align=\"center\">Штрих код</td> <td width=\"50px\" align=\"center\">База</td> <td width=\"60px\" align=\"center\">X1</td> <td width=\"60px\" align=\"center\">X2</td> <td width=\"60px\" align=\"center\">X3</td> <td width=\"60px\" align=\"center\">Г1</td> <td width=\"60px\" align=\"center\">Г2</td> <td width=\"60px\" align=\"center\">Г3</td></tr>");
                                    while(db_read.Read())
                                    {
                                        file.WriteLine("<tr> <td align=\"center\">" + db_read["zlic"].ToString() + "</td> <td align=\"right\">" + db_read["zsum"].ToString() + "</td> <td align=\"right\">" + db_read["zbarcode"].ToString() + "</td> <td align=\"center\">" + db_read["zdb"].ToString() + "</td> <td align=\"center\">" + db_read["s_hol1"].ToString() + "</td> <td align=\"center\">" + db_read["s_hol2"].ToString() + "</td> <td align=\"center\">" + db_read["s_hol3"].ToString() + "</td> <td align=\"center\">" + db_read["s_gor1"].ToString() + "</td> <td align=\"center\">" + db_read["s_gor2"].ToString() + "</td> <td align=\"center\">" + db_read["s_gor3"].ToString() + "</td></tr>");
                                    }
                                    file.WriteLine("</table><br />");
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    
                }
            

            file.WriteLine("</body>\n</html>");
            file.Close();
            frmPrn frmCPrn = new frmPrn();
            frmCPrn.Text = "Печать Ошибки приема эл. платежей";
            frmCPrn.Show();
        }

        public void update_date()
        {
            try
            {
                //db_com.CommandText = "Update zIDFile 

                for (int i = 0; i < but.Count; i++)
                {
                    db_com.CommandText = "SELECT zIDFile FROM abon.dbo.zErrFiles WHERE (RIGHT(LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 7), 2) = '" + frmMain.MaxCurPer.Substring(4, 2) + "') AND (RIGHT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 2) = '" + but.ElementAt(i).Text + "') AND (operok = 0) and (buhok = 0)";
                    List<string> zFile = new List<string>();
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            while (db_read.Read())
                            {
                                zFile.Add(db_read["zIDFile"].ToString());
                            }
                            but.ElementAt(i).BackColor = Color.Yellow;
                        }
                    }
                    db_com.CommandText = "SELECT zIDFile FROM abon.dbo.zErrFiles WHERE (RIGHT(LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 7), 2) = '" + frmMain.MaxCurPer.Substring(4, 2) + "') AND RIGHT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 2) = '" + but.ElementAt(i).Text + "' AND LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 4) = '" + frmMain.MaxCurPer.Substring(0, 4) + "'";
//                                         "SELECT zIDFile FROM abon.dbo.zErrFiles WHERE (RIGHT(LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 7), 2) = '" + frmMain.MaxCurPer.Substring(4, 2) + "') AND (RIGHT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 2) = '" + but.ElementAt(i).Text + "' AND LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 4) = '2017'"
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (!db_read.HasRows)
                            but.ElementAt(i).BackColor = Color.Yellow;
                    }
                    //if (zFile.Count > 0)
                    //    for (int ii = 0; ii < zFile.Count; ii++)
                    //    {
                    //        db_com.CommandText = "SELECT * FROM abon.dbo.zErrDetail wHERE (zFile = '" + zFile.ElementAt(ii) + "')";
                    //        using (SqlDataReader db_read = db_com.ExecuteReader())
                    //        {
                    //            if (db_read.HasRows)
                    //            {
                    //                but.ElementAt(i).BackColor = Color.Yellow;
                    //            }
                    //        }
                    //    }
                }
            }
            catch
            {
            }
            try
            {
                
                for (int i = 0; i < but.Count; i++)
                {
                    db_com.CommandText = "SELECT zIDFile FROM abon.dbo.zErrFiles WHERE (RIGHT(LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 7), 2) = '"+frmMain.MaxCurPer.Substring(4,2)+"') AND (RIGHT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 2) = '"+but.ElementAt(i).Text+"') AND (operok = 0) and (buhok = 1)";
                    List<string> zFile = new List<string>();
                    using (SqlDataReader db_read = db_com.ExecuteReader())
                    {
                        if (db_read.HasRows)
                        {
                            while (db_read.Read())
                            {
                                zFile.Add(db_read["zIDFile"].ToString());
                            }
                            but.ElementAt(i).BackColor = Color.Red;
                        }
                    }

                    /*if (zFile.Count > 0)
                        for(int ii=0;ii<zFile.Count;ii++)
                    {
                        db_com.CommandText = "SELECT * FROM abon.dbo.zErrDetail wHERE (zFile = '"+zFile.ElementAt(ii)+"')";
                        using (SqlDataReader db_read = db_com.ExecuteReader())
                        {
                            if (db_read.HasRows)
                            {
                                but.ElementAt(i).BackColor = Color.Red;
                            }
                        }
                    }*/
                }
            }
            catch
            {
            }
        }

        private void ProcessControls(Control ctrlContainer)
        {
            foreach (Control ctrl in ctrlContainer.Controls)
            {
                if (ctrl.GetType() == typeof(Button) && ctrl.Tag != null && ctrl.Tag.ToString() == "1")
                {
                    but.Add((Button)ctrl);
                }

                /*if (ctrl.HasChildren)
                    ProcessControls(ctrl);*/
            }
        }

        private void frmErrPay_Shown(object sender, EventArgs e)
        {
            label1.Text = "ЗАГРУЗКА...";
            panel1.Visible = true;
            Application.DoEvents();
            listBox1.Items.Clear();
            checkedListBox1.Items.Clear();
            ProcessControls(this);
            DateTime dt = new DateTime(Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2)), DateTime.DaysInMonth(Convert.ToInt32(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt32(frmMain.MaxCurPer.Substring(4, 2))));
            if (DateTime.DaysInMonth(Convert.ToInt16(frmMain.MaxCurPer.Substring(0, 4)), Convert.ToInt16(frmMain.MaxCurPer.Substring(4, 2))) == 30) button31.Visible = false;
            update_date();
            label1.Text = "Текущий период: " + dt.ToString("MMMM yyyy");
            button1.Focus();
            panel1.Visible = false;
            try
            {
                db_com.CommandText = "select isnull(operok,0) as operok from abon.dbo.config where hostname = '" + frmMain.Host + "'";
                using (SqlDataReader db_reader = db_com.ExecuteReader())
                {
                    if (db_reader.HasRows)
                    {
                        db_reader.Read();
                        if (db_reader["operok"].ToString() == "True") operok = true;
                    }
                }
            }
            catch
            {
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            checkedListBox1.Items.Clear();
            panel1.Visible = true;
            Application.DoEvents();
            try
            {
                db_com.CommandText = "SELECT zIDFile,zFileName,zBrik FROM abon.dbo.zErrFiles WHERE (RIGHT(LEFT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 7), 2) = '" + frmMain.MaxCurPer.Substring(4, 2) + "') AND (RIGHT(CAST(CONVERT(date, zDate, 104) AS nvarchar), 2) = '" + (sender as Button).Text + "') AND (operok = 0) and (buhok = 0)";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            listBox1.Items.Add("["+db_read["zBrik"].ToString()+"] "+db_read["zFileName"].ToString());
                        }
                    }
                }

                db_com.CommandText = "SELECT f.zIDFile,f.zFileName,f.zBrik,isnull(f.prnt,0) as prnt FROM abon.dbo.zErrFiles f inner join abon.dbo.zErrDetail d on d.zFile = f.ZIDFile WHERE (RIGHT(LEFT(CAST(CONVERT(date, f.zDate, 104) AS nvarchar), 7), 2) = '" + frmMain.MaxCurPer.Substring(4, 2) + "') AND (RIGHT(CAST(CONVERT(date, f.zDate, 104) AS nvarchar), 2) = '" + (sender as Button).Text + "') AND (f.operok = 0) and (f.buhok = 1) group by f.zIDFile,f.zFileName,f.zBrik,prnt";
                using (SqlDataReader db_read = db_com.ExecuteReader())
                {
                    if (db_read.HasRows)
                    {
                        while (db_read.Read())
                        {
                            string prnt = "";
                            if (db_read["prnt"].ToString() == "True") prnt = " (отпечатан)";
                            checkedListBox1.Items.Add(new SelectData(db_read["zIDFile"].ToString(),"[" + db_read["zBrik"].ToString() + "] " + db_read["zFileName"].ToString()+prnt));
                        }
                    }
                }

                panel1.Visible = false;
            }
            catch
            {
                panel1.Visible = false;
            }
        }

        private void checkedListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var index = checkedListBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                _selectedMenuItem = index;
                collectionRoundMenuStrip.Show(Cursor.Position);
                collectionRoundMenuStrip.Visible = true;
            }
            else
            {
                collectionRoundMenuStrip.Visible = false;
            }
        }



    }
}

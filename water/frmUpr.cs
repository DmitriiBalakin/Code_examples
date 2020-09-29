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
    public partial class frmUpr : Form
    {

        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();

        public frmUpr()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            com.Connection = con;
            try
            {
                con.Open();
            }
            catch
            { }
        }

        private void frmUpr_Shown(object sender, EventArgs e)
        {
            if (con.State == ConnectionState.Open)
            {
                try
                {
                    com.CommandText = @"select a.id, a.nameved, a.buk, case when (a.bpaketc=1 or b.bpaketc=1) then 1 else 0 end as bpaketc, a.contractor, SUM(isnull(s.SaldoBeg,0)+isnull(ss.saldobeg,0)) as saldo
                                        from abonuk.dbo.spvedomstvo a
                                        inner join abon.dbo.spvedomstvo b on b.id=a.id
left join AbonUK.dbo.AbonSaldo"+frmMain.MaxCurPer+@" s on a.ID = s.KodVed
left join (select kodvedom, sum(sndeb-snkred) as saldobeg from abon.dbo.abonent" + frmMain.MaxCurPer + @" group by kodvedom) ss on ss.KodVedom = a.ID
group by a.id, a.nameved, a.buk, a.bPaketC, b.bPaketC,a.Contractor
                                        order by a.nameved";
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            gv_upr.Rows.Clear();
                            while (r.Read())
                            {
                                string[] row = {"","","","","",""};
                                row[0] = r["id"].ToString();
                                row[1] = r["nameved"].ToString();
                                row[2] = (r["buk"].ToString() == "1" || r["buk"].ToString() == "True")?"Да":"Нет";
                                row[3] = (r["bpaketc"].ToString() == "1" || r["bpaketc"].ToString() == "True") ? "Да" : "Нет";
                                row[4] = r["contractor"].ToString();
                                row[5] = r["saldo"].ToString();
                                gv_upr.Rows.Add(row);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            SqlTransaction tran;
            tran = con.BeginTransaction("new_ved");
            com.Transaction = tran;
            try
            {
                if (textBox1.Text.Trim().Length > 0)
                {
                    if (MessageBox.Show("Добавить новую управляющую компанию?", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        int result = 0;
                        com.CommandText = @"insert into abon.dbo.spvedomstvo (nameved,buk,bpaketc,kodorg,numdog,stat) values(@ved," + (checkBox1.Checked ? "1" : "0") + @"," + ((checkBox1.Checked && checkBox2.Checked) ? "0" : (!checkBox1.Checked && checkBox2.Checked)?"1":"0") + @",0,0,1)";
                        com.Parameters.AddWithValue("@ved", textBox1.Text.Trim());
                        result = com.ExecuteNonQuery();
                        com.Parameters.Clear();
                        if (result > 0)
                        {
                            com.CommandText = @"insert into abonuk.dbo.spvedomstvo(ID, nameved, buk, bpaketc, kodorg,numdog,stat,contractor,DLSPackNm) select id, nameved, buk, " + ((checkBox1.Checked && checkBox2.Checked) ? "1" : "0") + @", kodorg, numdog, stat, @about,0 from abon.dbo.spvedomstvo where nameved=@ved";
                            com.Parameters.AddWithValue("@about", richTextBox1.Text.Trim());
                            com.Parameters.AddWithValue("@ved", textBox1.Text.Trim());
                            com.ExecuteNonQuery();
                            com.Parameters.Clear();
                        }
                        tran.Commit();
                        MessageBox.Show("Новое ведомство добавлено!","Внимание",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        textBox1.Text = "";
                        richTextBox1.Clear();
                        checkBox1.Checked = false;
                        checkBox2.Checked = false;
                        frmUpr_Shown(this, e);
                    }
                }
                else
                {
                    MessageBox.Show("Не введено наименование компании", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                if(tran != null) tran.Rollback();
                MessageBox.Show("Произошел сбой. Ведомство не добавлено", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["id"].Value.ToString();
            textBox1.Text = gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["name_"].Value.ToString();
            richTextBox1.Clear();
            richTextBox1.AppendText(gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["about_"].Value.ToString());
            if (gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["Upr_"].Value.ToString() == "Да") checkBox1.Checked = true; else checkBox1.Checked = false;
            if (gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["calc_"].Value.ToString() == "Да") checkBox2.Checked = true; else checkBox2.Checked = false;
            button3.Enabled = true;
            if(Convert.ToDouble(gv_upr.Rows[gv_upr.CurrentRow.Index].Cells["saldo"].Value.ToString()) != 0) checkBox1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SqlTransaction tran;
            if (MessageBox.Show("Сохранить изменения?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                tran = con.BeginTransaction("update");
                com.Transaction = tran;
                try
                {
                    if (textBox1.Text.Trim().Length > 0)
                    {
                        com.Parameters.Clear();
                        com.CommandText = @"update abon.dbo.spvedomstvo set nameved=@name, bpaketc=@bp, buk=@buk where id=@id";
                        com.Parameters.AddWithValue("@name", textBox1.Text.Trim());
                        com.Parameters.AddWithValue("@bp", ((checkBox1.Checked && checkBox2.Checked) ? "0" : (!checkBox1.Checked && checkBox2.Checked) ? "1" : "0"));
                        com.Parameters.AddWithValue("@buk", checkBox1.Checked?"0":"1");
                        com.Parameters.AddWithValue("@id", label3.Text);
                        com.ExecuteNonQuery();
                        com.Parameters.Clear();

                        com.CommandText = @"update abonuk.dbo.spvedomstvo set nameved=@name, bpaketc=@bp, buk=@buk, contractor=@about where id=@id";
                        com.Parameters.AddWithValue("@name", textBox1.Text.Trim());
                        com.Parameters.AddWithValue("@bp", ((checkBox1.Checked && checkBox2.Checked) ? "1":"0"));
                        com.Parameters.AddWithValue("@buk", checkBox1.Checked ? "1" : "0");
                        com.Parameters.AddWithValue("@about", richTextBox1.Text.Trim());
                        com.Parameters.AddWithValue("@id", label3.Text);
                        com.ExecuteNonQuery();
                        com.Parameters.Clear();

                        tran.Commit();
                        MessageBox.Show("Ведомство обновлено!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textBox1.Text = "";
                        richTextBox1.Clear();
                        checkBox1.Checked = false;
                        checkBox2.Checked = false;
                        button3.Enabled = false;
                        checkBox1.Enabled = true;
                        frmUpr_Shown(this, e);
                    }
                    else
                    {
                        MessageBox.Show("Не введено наименование компании", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    if (tran != null) tran.Rollback();
                    MessageBox.Show("Произошел сбой. Ведомство не обновлено", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                button3.Enabled = false;
                checkBox1.Enabled = true;
            }
        }
    }
}

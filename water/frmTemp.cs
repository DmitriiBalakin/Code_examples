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
    public partial class frmTemp : Form
    {
        SqlConnection con = new SqlConnection();

        public frmTemp()
        {
            InitializeComponent();
            con.ConnectionString = frmMain.db_con.ConnectionString;
            try
            {
                con.Open();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            SqlTransaction tran;
            tran = con.BeginTransaction("perebros");
            com.Transaction = tran;
            if (con.State == ConnectionState.Open)
            {
                label5.Text = "0";
                gv.Rows.Clear();
                try
                {
                    com.CommandText = @"select p.id,p.str_code,s.name_ul,p.dom from abonuk.dbo.perehod p left join abonuk.dbo.streetuch s on s.cod_yl=p.str_code where p.old_ved=@old and p.new_ved=@new and p.perbeg=@per group by p.id,p.str_code,p.dom,s.name_ul";
                    com.Parameters.AddWithValue("@old", textBox1.Text.Trim());
                    com.Parameters.AddWithValue("@new", textBox2.Text.Trim());
                    com.Parameters.AddWithValue("@per", textBox3.Text.Trim());
                    using (SqlDataReader r = com.ExecuteReader())
                    {

                        while (r.Read())
                        {
                            string[] row = {"","false","","","" };
                            label5.Text = (Convert.ToInt32(label5.Text) + 1).ToString();
                            row[0] = r["id"].ToString();
                            row[2] = r["str_code"].ToString();
                            row[3] = r["name_ul"].ToString();
                            row[4] = r["dom"].ToString();
                            gv.Rows.Add(row);
                        }
                    }
                    com.Parameters.Clear();
                    tran.Commit();
                }
                catch
                {
                    com.Parameters.Clear();
                    if (tran != null) tran.Rollback();
                    MessageBox.Show("Сбой при выполнении оперции","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        List<string> lic = new List<string>();

        private void button2_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;

            lic.Clear();
            label7.Text = "0";

            if (con.State == ConnectionState.Open)
            {
                try
                {
                    for (int i = 0; i < gv.Rows.Count; i++)
                    {
                        if (gv.Rows[i].Cells["select"].Value.ToString() == "True")
                        {
                            com.CommandText = @"select a.lic from abon.dbo.abonent"+textBox5.Text.Trim()+@" a inner join abon.dbo.spvedomstvo v on a.kodvedom=v.id where v.buk=0 and a.str_code=@str and a.dom=@dom
    union all
    select a.lic from abonuk.dbo.abonent" + textBox5.Text.Trim() + @" a inner join abonuk.dbo.spvedomstvo v on a.kodvedom=v.id where v.buk=1 and a.str_code=@str and a.dom=@dom
"; 
                            com.Parameters.AddWithValue("@str",gv.Rows[i].Cells["str_code"].Value.ToString());
                            com.Parameters.AddWithValue("@dom", gv.Rows[i].Cells["dom"].Value.ToString());
                            using (SqlDataReader r = com.ExecuteReader())
                            {
                                if (r.HasRows)
                                {
                                    while (r.Read())
                                    {
                                        lic.Add(r["lic"].ToString());
                                    }
                                }
                            }
                            com.Parameters.Clear();
                            label7.Text = lic.Count.ToString();
                        }
                    }
                }
                catch
                {
                    com.Parameters.Clear();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            SqlTransaction tran;
            tran = con.BeginTransaction("perehod");
            com.Transaction = tran;
            progressBar1.Maximum = lic.Count;
            progressBar1.Value = 0;
            try
            {
                for (int i = 0; i < lic.Count; i++)
                {
                    label10.Text = lic[i].ToString();
                    Application.DoEvents();
                    com.CommandText = @"delete from abon.dbo.pos201601 where right(lic,9)=right(@lic,9) and brik=27; delete from abonuk.dbo.pos201601 where right(lic,9)=right(@lic,9) and brik=27;";
                    com.Parameters.AddWithValue("@lic", lic[i].ToString());
                    com.ExecuteNonQuery();
                    com.Parameters.Clear();
                    com.CommandText = @"update a set a.kodvedom = @ved, a.vedom = v.nameved from abon.dbo.abonent201601 a inner join abon.dbo.spvedomstvo v on v.id=@ved where right(a.lic,9)=right(@lic,9);
                                        update a set a.kodvedom = @ved, a.vedom = v.nameved from abonuk.dbo.abonent201601 a inner join abonuk.dbo.spvedomstvo v on v.id=@ved where right(a.lic,9)=right(@lic,9);";
                    com.Parameters.AddWithValue("@ved", textBox4.Text.Trim());
                    com.Parameters.AddWithValue("@lic", lic[i].ToString());
                    com.ExecuteNonQuery();
                    com.Parameters.Clear();

                    com.CommandText = "select lic from abonuk.dbo.abonsaldo201601 where right(lic,9)=right(@lic,9) and perend='201512' and kodved=@ved";
                    com.Parameters.AddWithValue("@lic", lic[i].ToString());
                    com.Parameters.AddWithValue("@ved", textBox1.Text.Trim());
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            com.CommandText = "";
                        }
                        else
                        {
                            com.CommandText = @"update s set s.perend='201512', s.nv=0, s.nk=0, s.np=0, s.pv=0,s.pk=0,s.pp=0, s.sv=0, s.sk=0, s.sp=0, s.spisan =0 from abonuk.dbo.abonsaldo201601 s where right(s.lic,9)=right(@lic,9) and s.perend=0 and s.kodved=@ved;
                                                insert into abonuk.dbo.abonsaldo201601(lic,kodved,perbeg,perend,saldobeg,nv,nk,np,pv,pk,pp,sv,sk,sp,spisan) values(@lic,@nved,201601,0,0,0,0,0,0,0,0,0,0,0,0); update a set a.sndeb=0, a.snkred=0 from abonuk.dbo.abonent201601 a where a.lic=@lic;";
                        }
                    }
                    com.Parameters.Clear();

                    if (com.CommandText != "")
                    {
                        com.Parameters.AddWithValue("@lic", lic[i].ToString());
                        com.Parameters.AddWithValue("@ved", textBox1.Text.Trim());
                        com.Parameters.AddWithValue("@nved", textBox4.Text.Trim());
                        com.ExecuteNonQuery();
                        com.Parameters.Clear();
                    }

                    Int32 NKUK = 0, NKA = 0;
                    com.CommandText = @"select (isnull(MAX(NKvit),0)+1) as kvit FROM AbonUK.dbo.Pos201601";
                    using (SqlDataReader read = com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            read.Read();
                            NKUK = Convert.ToInt32(read["kvit"].ToString());
                        }
                    }
                    com.CommandText = @"select (isnull(MAX(NKvit),0)+1) as kvit FROM Abon.dbo.Pos201601";
                    using (SqlDataReader read = com.ExecuteReader())
                    {
                        if (read.HasRows)
                        {
                            read.Read();
                            NKA = Convert.ToInt32(read["kvit"].ToString());
                        }
                    }

                    double opl = 0;
                    com.CommandText = "select saldoend from abonuk.dbo.abonsaldo201601 where lic=@lic and perend=201512";
                    com.Parameters.AddWithValue("@lic", lic[i].ToString());
                    using (SqlDataReader r = com.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            r.Read();
                            opl = Convert.ToDouble(r["saldoend"].ToString());
                        }
                        else opl = 0;
                    }
                    com.Parameters.Clear();

                    if (opl != 0)
                    {
                        com.CommandText = @"INSERT INTO [AbonUK].[dbo].[Pos201601]
                                                   ([brik],[pach],[data_p],[opl],[lic],[Prim],[PerOpl],[NKvit],[PerOplReal],[poliv])
                                             VALUES
                                                   (27,10,convert(datetime,'31.01.2016',104),@opl,@lic,'переброска долго по переуступке','2015/12',@kv,'2016/01',0)";
                        com.Parameters.AddWithValue("@lic", lic[i].ToString());
                        com.Parameters.AddWithValue("@kv", NKUK);
                        com.Parameters.AddWithValue("@opl", opl);
                        com.Parameters.Clear();

                        com.CommandText = @"INSERT INTO [Abon].[dbo].[Pos201601]
                                                   ([brik],[pach],[data_p],[opl],[lic],[Prim],[PerOpl],[NKvit],[PerOplReal],[poliv])
                                             VALUES
                                                   (27,10,convert(datetime,'31.01.2016',104),@opl,@lic,'переброска долго по переуступке','2015/12',@kv,'2016/01',0)";
                        com.Parameters.AddWithValue("@lic", "1" + lic[i].ToString().Substring(1, 9));
                        com.Parameters.AddWithValue("@kv", NKUK);
                        com.Parameters.AddWithValue("@opl", -1 * opl);
                        com.Parameters.Clear();
                    }
                    progressBar1.Value++;
                }
                progressBar1.Value = 0;
                progressBar1.Maximum = 0;
                ///tran.Rollback();
                tran.Commit();
                MessageBox.Show("УсЁ хорошо.", "");
            }
            catch
            {
                if (tran != null) tran.Rollback();
                MessageBox.Show("Сбой при выполнении оперции", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

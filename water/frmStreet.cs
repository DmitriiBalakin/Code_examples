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
    public partial class frmStreet : Form
    {
        public frmStreet()
        {
            InitializeComponent();
        }

        int error = 0;
        SqlConnection con = new SqlConnection();
        static string UpperCaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        SqlTransaction tran;

        private void frmStreet_Shown(object sender, EventArgs e)
        {
            dtGrid.Columns[0].DefaultCellStyle.Format = "0000000000";
            dtGrid.Columns[4].DefaultCellStyle.Format = "000";
            dtGrid.Columns[13].DefaultCellStyle.Format = "000";
            dtGrid.Refresh();
            try
            {
                con.ConnectionString = "Data Source=SERVERAB; Initial Catalog=master; Integrated Security=True;Persist Security Info=False; User ID=SqlAbon; Connect Timeout=0; TrustServerCertificate=False";
                con.Open();
            }
            catch
            {
                MessageBox.Show("Ошибка соединения с сервером, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            try
            {
                cmd.CommandText = @"select Id_PrefStreet,Nm_PrefStreet from Common.dbo.SpPrefStreet";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbPref.Items.Add(new SelectData(sql_reader["id_PrefStreet"].ToString(), sql_reader["Nm_PrefStreet"].ToString()));

                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Common.dbo.SpPrefStreet, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select dKodOur,dName from Abon.dbo.PLdistr where kodZ<6";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbDistrict.Items.Add(new SelectData(sql_reader["dKodOur"].ToString(), sql_reader["dName"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Abon.dbo.PLdistr, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select distinct Id_Street,Prim,Nm_Street,Code_Yl from Common.dbo.SpStreets where Nm_Street!='temp' order by Nm_Street";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbStreet.Items.Add(new SelectData(sql_reader["Id_Street"].ToString(), sql_reader["Code_Yl"].ToString(), sql_reader["Nm_Street"].ToString() + " " + sql_reader["Prim"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Common.dbo.SpStreets, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select Id,NameVendor from AbonUK.dbo.SpVendor";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbVendor.Items.Add(new SelectData(sql_reader["Id"].ToString(), sql_reader["NameVendor"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице AbonUK.dbo.SpVendor, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select Id_PostIndex,Nm_PostIndex from Common.dbo.SpPostIndexes";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbPostIndex.Items.Add(new SelectData(sql_reader["Id_PostIndex"].ToString(), sql_reader["Nm_PostIndex"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Common.dbo.SpPostIndexes, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select PoselId,Poselenie from Abon.dbo.SpPosel where PoselId<8";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbPosel.Items.Add(new SelectData(sql_reader["PoselId"].ToString(), sql_reader["Poselenie"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Abon.dbo.SpPosel, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select Id_Settlement,Nm_Settlement from Common.dbo.SpSettlements where Id_Settlement<>12 and Id_Settlement<>25 and Id_Settlement<>26 and Id_Settlement<>27";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbSettlement.Items.Add(new SelectData(sql_reader["Id_Settlement"].ToString(), sql_reader["Nm_Settlement"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Common.dbo.SpSettlements, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }

            try
            {
                cmd.CommandText = @"select distinct punkt from Abon.dbo.StreetUch order by punkt";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbPunkt.Items.Add(sql_reader["punkt"]);
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Abon.dbo.StreetUch, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select distinct pref_ul from AbonUK.dbo.StreetUch where PREF_UL!='ul' order by pref_ul";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbPrefDom.Items.Add(sql_reader["pref_ul"]);
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице AbonUK.dbo.StreetUch, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }
            try
            {
                cmd.CommandText = @"select id_HWorg,Nm_HWorg from Common.dbo.SpHWorg where id_HWorg!=153 order by Nm_HWorg";
                SqlDataReader sql_reader = cmd.ExecuteReader();
                while (sql_reader.Read())
                {
                    cmbKotel.Items.Add(new SelectData(sql_reader["id_HWorg"].ToString(), sql_reader["Nm_HWorg"].ToString()));
                }
                sql_reader.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка при обращении к таблице Common.dbo.SpHWorg, обратитесь в службу АСУ", "ВНИМАНИЕ!!!", MessageBoxButtons.OK);
            }

        }

        private void cmbPref_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPref.SelectedIndex != -1)
            {
                cmbPosel.Enabled = true;
            }
        }
    }
}

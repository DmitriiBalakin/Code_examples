using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Windows.Forms;
using System.IO;

namespace water
{
    class TDBF
    {
        //private OdbcConnection Conn = null;
        private OleDbConnection Conn = null;
        public DataTable Execute(string Command,string base_file)
        {
            DataTable dt = null;

            string base_dir = base_file.Replace(Path.GetFileName(base_file), "");
            //base_dir = base_dir.Replace("\\\\", "\\");
            base_dir = base_dir.Replace("\\", @"\");
            //Conn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;DBQ="+base_dir+";SourceDB=" + base_dir+";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
            Conn.ConnectionString = @"Provider=vfpoledb;Data Source=" + base_dir + ";";
            ///Conn.Open();
            if (Conn != null)
            {
                try
                {
                    dt = new DataTable("tmp");
                    ///System.Data.Odbc.OdbcCommand oCmd = new OdbcCommand();
                    System.Data.OleDb.OleDbCommand oCmd = new OleDbCommand();
                    //Command = Command.Replace("\\", "\\\\");
                    //Command = Command.Replace("\\\\", "\\");
                    Command = Command.Replace("\\", @"\");
                    oCmd.CommandText = @Command; 
                    Conn.Open();
                    oCmd.Connection = Conn;
                    dt.Load(oCmd.ExecuteReader());
                    Conn.Close();
                }
                catch
                {
                    ///MessageBox.Show(e.Message, "Ошибка");
                    Conn.Close();
                }
            }
            return dt;
        }

        public DataTable GetAll(string DB_path)
        {
            return Execute("SELECT * FROM " + DB_path, DB_path);
        }

        public TDBF()
        {
            //this.Conn = new System.Data.Odbc.OdbcConnection();
            this.Conn = new System.Data.OleDb.OleDbConnection();
            /*Conn.ConnectionString = @"Drive={Microsoft dBase Driver (*.dbf)};" +
                "SourceType=DBF;Exclusive=No;" +
                "Collate=Machine;NULL=NO;DELETED=NO;" +
                "BACKGROUNDFETCH=NO;";*/
            Conn.ConnectionString = "";
        }
    }
}

/// TDBF dbf_ = new TDBF();
/// gv_payview.DataSource = dbf_.GetAll("C:\\DBF.dbf");
/// gv_payview.DataSource = dbf_.Execute("Select id, name From C:\\DBF.dbf");
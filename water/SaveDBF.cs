using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
//using Npgsql;
//using NpgsqlTypes;

namespace water
{
    public static class SaveDBF
    {
        /// <summary>
        /// Хранит информацию о поле дбф-таблицы: имя, тип данных.
        /// </summary>
        public class dbf_field
        {
            public string Name;
            public string DataType;
            public dbf_field(string name, string data_type)
            {
                this.Name = name;
                this.DataType = data_type;
            }
            public OleDbType ParamType
            {
                get
                {
                    OleDbType t=OleDbType.VarChar;
                    switch (DataType.ToUpper().Substring(0,1))
                    { 
                        case "C":
                            t = OleDbType.VarChar;
                            break;
                        case "L":
                            t = OleDbType.Boolean;
                            break;
                        case "I":
                            t = OleDbType.Integer;
                            break;
                        case "N":
                            t = OleDbType.Numeric;
                            break;
                        case "D":
                            t = OleDbType.Date;
                            break;
                    }
                    return t;
                }
            }
        }
        /// <summary>
        /// Выгружает данные в дбф-таблицу.Создаёт файл по заданому пути и грузит туда данные из дататейбл по заданным столбцам
        /// </summary>
        /// <param name="tbl">DataTable с данными для выгрузки</param>
        /// <param name="full_file_name">полный путь к создаваемому дбфнику - путь + имя файла</param>
        /// <param name="fields">список полей дбфника с их типами данных. задаёт создаваемые в дбфнике столбцы и вгружаемые из дататейбл данные.</param>

        public static void save_dbf(DataTable tbl, string full_file_name, dbf_field[] fields)
        {
            OleDbConnection con_dbf = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" +
                System.IO.Path.GetDirectoryName(full_file_name) +
                ";Password=;Collating Sequence=MACHINE;Extended Properties=dBASE III");
            con_dbf.Open();
            string file_name = System.IO.Path.GetFileNameWithoutExtension(full_file_name).Replace(' ', '_');
            using (OleDbCommand cmd_create_tbl = new OleDbCommand("create table " +
                file_name + "(", con_dbf))
            {
                foreach (dbf_field f in fields)
                { cmd_create_tbl.CommandText += f.Name + " " + f.DataType + ","; }
                cmd_create_tbl.CommandText = cmd_create_tbl.CommandText.Remove(cmd_create_tbl.CommandText.Length - 1);
                cmd_create_tbl.CommandText += ");";
                try
                {
                    cmd_create_tbl.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); return; }
            }
            using (OleDbCommand cmd_ins = new OleDbCommand("insert into " +
                file_name + " (", con_dbf))
            {
                string vals = string.Empty;
                foreach (dbf_field f in fields)
                { 
                    cmd_ins.CommandText += f.Name + ","; 
                    vals += "?,";
                    cmd_ins.Parameters.Add(f.Name, f.ParamType);
                }
                cmd_ins.CommandText = cmd_ins.CommandText.Remove(cmd_ins.CommandText.Length - 1);
                cmd_ins.CommandText += ") values (" + vals.Remove(vals.Length-1) + ");";
                
                foreach (DataRow r in tbl.Rows)
                {
                    foreach (dbf_field f in fields)
                    {
                        cmd_ins.Parameters[f.Name].Value = r[f.Name];
                    }
                    
                    try
                    {
                        cmd_ins.ExecuteNonQuery();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); return; }
                }
            }
            con_dbf.Close();
            con_dbf.Dispose();
            GC.Collect();
        }
        
        public static void DataTableSaveToDBF(DataTable DT, string Folder,string tblname)
  {
   // Создаю таблицу
   if(tblname.ToLower().EndsWith(".dbf")){tblname=tblname.Remove(tblname.Length-3);}
   System.IO.File.Delete(Folder+"\\"+tblname+".DBF");
   System.IO.FileStream FS = new System.IO.FileStream(Folder+"\\"+tblname+".DBF", System.IO.FileMode.Create);
   // Формат dBASE III 2.0
   byte[] buffer = new byte[]{0x03, 0x63, 0x04, 0x04}; // Заголовок  4 байта
   FS.Write(buffer, 0, buffer.Length);
   buffer = new byte[]{
                       (byte)(((DT.Rows.Count % 0x1000000) % 0x10000) % 0x100),
                       (byte)(((DT.Rows.Count % 0x1000000) % 0x10000) / 0x100),
                       (byte)(( DT.Rows.Count % 0x1000000) / 0x10000),
                       (byte)(  DT.Rows.Count / 0x1000000)
                      }; // Word32 -> кол-во строк 5-8 байты
   FS.Write(buffer, 0, buffer.Length);
   int i = (DT.Columns.Count+1)*32+1; // Изврат
   buffer = new byte[]{
                       (byte)( i % 0x100),
                       (byte)( i / 0x100)
                      }; // Word16 -> кол-во колонок с извратом 9-10 байты
   FS.Write(buffer, 0, buffer.Length);
   string[] FieldName = new string[DT.Columns.Count]; // Массив названий полей
   string[] FieldType = new string[DT.Columns.Count]; // Массив типов полей
   byte[] FieldSize = new   byte[DT.Columns.Count]; // Массив размеров полей
   byte[] FieldDigs = new   byte[DT.Columns.Count]; // Массив размеров дробной части
   int s = 1; // Считаю длину заголовка
   foreach (DataColumn C in DT.Columns)
   {
    string l = C.ColumnName.ToUpper(); // Имя колонки
    while (l.Length < 10) {l = l + (char)0;} // Подгоняю по размеру (10 байт)
    FieldName[C.Ordinal] = l.Substring(0, 10) + (char)0; // Результат
    FieldType[C.Ordinal] = "C"; 
    FieldSize[C.Ordinal] = 50;
    FieldDigs[C.Ordinal] = 0;
    switch (C.DataType.ToString())
    {
     case "System.String"   : 
     {
      DataTable tmpDT = DT.Copy();
      tmpDT.Columns.Add("StringLengthMathColumn", Type.GetType("System.Int32"), "LEN("+C.ColumnName+")");
      DataRow[] DR = tmpDT.Select("", "StringLengthMathColumn DESC");
      if (DR.Length > 0) 
      {
       if (DR[0]["StringLengthMathColumn"].ToString() != "")
       {
        int n = (int)DR[0]["StringLengthMathColumn"];
        if (n > 255)
         FieldSize[C.Ordinal] = 255;
        else
         FieldSize[C.Ordinal] = (byte)n;
       }
       if (FieldSize[C.Ordinal] == 0)
        FieldSize[C.Ordinal] = 1;
      }
      break;
     }
     case "System.Boolean"  : {FieldType[C.Ordinal] = "L"; FieldSize[C.Ordinal] =  1; break;}
     case "System.Byte"     : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] =  1; break;}
     case "System.DateTime" : {FieldType[C.Ordinal] = "D"; FieldSize[C.Ordinal] =  8; break;}
     case "System.Decimal"  : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] = 38; FieldDigs[C.Ordinal] = 5; break;}
     case "System.Double"   : {FieldType[C.Ordinal] = "F"; FieldSize[C.Ordinal] = 38; FieldDigs[C.Ordinal] = 5; break;}
     case "System.Int16"    : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] =  6; break;}
     case "System.Int32"    : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] = 11; break;}
     case "System.Int64"    : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] = 21; break;}
     case "System.SByte"    : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] =  6; break;}
     case "System.Single"   : {FieldType[C.Ordinal] = "F"; FieldSize[C.Ordinal] = 38; FieldDigs[C.Ordinal] = 5; break;}
     case "System.UInt16"   : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] =  6; break;}
     case "System.UInt32"   : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] = 11; break;}
     case "System.UInt64"   : {FieldType[C.Ordinal] = "N"; FieldSize[C.Ordinal] = 21; break;}
    }
    s = s + FieldSize[C.Ordinal];
   }
   buffer = new byte[]{
                       (byte)(s % 0x100), 
                       (byte)(s / 0x100)
                      }; // Пишу длину заголовка 11-12 байты
   FS.Write(buffer, 0, buffer.Length);
   for (int j = 0; j < 20; j++) {FS.WriteByte(0x00);} // Пишу всякий хлам - 20 байт, 
   //  итого: 32 байта - базовый заголовок DBF
   // Заполняю заголовок
   foreach (DataColumn C in DT.Columns)
   {
    buffer = System.Text.Encoding.Default.GetBytes(FieldName[C.Ordinal]); // Название поля
    FS.Write(buffer, 0, buffer.Length);
    buffer = new byte[]{
                        System.Text.Encoding.ASCII.GetBytes(FieldType[C.Ordinal])[0],
                        0x00, 
                        0x00,
                        0x00, 
                        0x00
                       }; // Размер
    FS.Write(buffer, 0, buffer.Length);
    buffer = new byte[]{
                        FieldSize[C.Ordinal],
                        FieldDigs[C.Ordinal]
                       }; // Размерность
    FS.Write(buffer, 0, buffer.Length);
    buffer = new byte[]{0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00}; // 14 нулей
    FS.Write(buffer, 0, buffer.Length);
   }
   FS.WriteByte(0x0D); // Конец описания таблицы
   System.Globalization.DateTimeFormatInfo dfi = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
   System.Globalization.NumberFormatInfo   nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
   string Spaces = ""; 
   while (Spaces.Length < 255) Spaces = Spaces + " ";
   foreach (DataRow R in DT.Rows)
   {
    FS.WriteByte(0x20); // Пишу данные
    foreach (DataColumn C in DT.Columns)
    {
     string l = R[C].ToString();
     if (l != "") // Проверка на NULL
     {
      switch (FieldType[C.Ordinal])
      {
       case "L" : 
       {
        l = bool.Parse(l).ToString();
        break;
       }
       case "N" :
       {
        l = decimal.Parse(l).ToString(nfi);
        break; 
       }
       case "F" :
       {
        l = float.Parse(l).ToString(nfi);
        break; 
       }
       case "D" :
       {
        l = DateTime.Parse(l).ToString("yyyyMMdd", dfi);
        break;
       }
       default : l = l.Trim() + Spaces; break;
      }
     }
     else 
     {
      if (FieldType[C.Ordinal] == "C"
       || FieldType[C.Ordinal] == "D")
       l = Spaces;
     }
     while (l.Length < FieldSize[C.Ordinal]){l = l + (char)0x00;} 
     l = l.Substring(0, FieldSize[C.Ordinal]); // Корректирую размер
     buffer = System.Text.Encoding.GetEncoding(866).GetBytes(l); // Записываю в кодировке (MS-DOS Russian)
     FS.Write(buffer, 0, buffer.Length);
    }
   }
   FS.WriteByte(0x1A); // Конец данных
   FS.Close();
  }
    }
}







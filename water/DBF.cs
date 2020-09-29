using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Globalization;

namespace water
{
    public partial class DBF : IDisposable
    {
        public string FileName;
        public string Format
        {
            get { return Format; }
            set { Format = value; }
        }
        public string FilePath;

        private Stream FileStream = null;
        private byte[] FileBytes = null;
        private List<string> FieldsName;
        private List<Type> FieldsType;
        private List<byte> FieldLenght;

        public Int32 FieldsCount;

        public Int32 RecordsCount;

        public DataTable Table;

        public string Code;

        public DBF()
        {
            this.FileName = null;
            this.FileStream = null;
            this.FileBytes = null;
            this.FieldsName = new List<string>();
            this.FieldsType = new List<Type>();
            this.FieldLenght = new List<byte>();
            this.Table = new DataTable();
            this.Code = null;
        }

        public bool ReadDBF()
        {
            bool result = false;
            if (FilePath != null)
            {
                try
                {
                    FieldsName.Clear();
                    FieldsType.Clear();
                    FieldLenght.Clear();
                    Table.Clear();
                    Table.Columns.Clear();
                    Table.Rows.Clear();
                    FileName = Path.GetFileName(FilePath);
                    FileStream = File.OpenRead(FilePath);
                    FileBytes = ToByteArray(FileStream);
                    FileStream.Close();
                    Int32[] FCount = new Int32[1];
                    Int32[] RCount = new Int32[1];
                    Int32[] code = new Int32[1];
                    Buffer.BlockCopy(FileBytes, 8, FCount, 0, 2);
                    FieldsCount = ((FCount.ElementAt(0)) - 33) / 32;
                    Buffer.BlockCopy(FileBytes, 4, RCount, 0, 4);
                    RecordsCount = RCount[0];
                    Buffer.BlockCopy(FileBytes, 29, code, 0, 1);
                    switch (code[0])
                    {
                        case 0: Code = "Ignore"; break;
                        case 1: Code = "437 DOS USA"; break;
                        case 2: Code = "850 DOS Multilang"; break;
                        case 38: Code = "866 DOS Russian"; break;
                        case 87: Code = "1251 Windows ANSI"; break;
                        case 200: Code = "1250 Windows EE"; break;
                        default: Code = "Unknown"; break;
                    }
                    ///чтение заголовков и типов
                    for (int i = 0; i < FieldsCount; i++)
                    {
                        byte[] name = new byte[11];
                        byte[] type = new byte[1];
                        byte[] len = new byte[1];
                        Buffer.BlockCopy(FileBytes, 32 + 32 * i, name, 0, 11);
                        FieldsName.Add(System.Text.Encoding.ASCII.GetString(name).TrimEnd('\0'));
                        Buffer.BlockCopy(FileBytes, 32 + 32 * i + 11, type, 0, 1);
                        string T = System.Text.Encoding.ASCII.GetString(type).ToString();
                        switch(T)
                        {
                            case "C": FieldsType.Add(typeof(string)); break;
                            case "D": FieldsType.Add(typeof(DateTime)); break;
                            case "F": FieldsType.Add(typeof(double)); break;
                            case "N": FieldsType.Add(typeof(double)); break;
                            case "L": FieldsType.Add(typeof(Boolean)); break;
                            case "2": FieldsType.Add(typeof(Int16)); break;
                            case "4": FieldsType.Add(typeof(Int32)); break;
                            case "8": FieldsType.Add(typeof(double)); break;
                            default: FieldsType.Add(typeof(string)); break;
                        }
                        Buffer.BlockCopy(FileBytes, 32 + 32 * i + 16, len, 0, 1);
                        FieldLenght.Add(len[0]);
                    }
                    int rowlen = 1;
                    for (int i = 0; i < FieldsCount; i++)
                        rowlen += FieldLenght.ElementAt(i);

                    for (int i = 0; i < FieldsCount; i++)
                    {
                        Table.Columns.Add(FieldsName.ElementAt(i).ToString(),FieldsType.ElementAt(i));
                    }
                    for (int i = 0; i < RecordsCount; i++)
                    {
                        DataRow row = Table.NewRow();
                        int offset = 1;
                        for (int j = 0; j < FieldsCount; j++)
                        {
                            ///(32+32*FieldsCount+1)+rowlen*i - ряд
                            byte[] data = new byte[FieldLenght.ElementAt(j)];
                            Buffer.BlockCopy(FileBytes, (32+32*FieldsCount+1)+rowlen*i+offset, data, 0, FieldLenght.ElementAt(j));

                            if (FieldsType.ElementAt(j) == typeof(string))
                            {
                                //byte[] bytes = Encoding.UTF8.GetBytes(
                                Encoding first = Encoding.GetEncoding(866);
                                Encoding dest = Encoding.Unicode;
                                byte[] ndata = Encoding.Convert(first, dest, data);
                                row[FieldsName.ElementAt(j).ToString()] = System.Text.Encoding.Unicode.GetString(ndata).Trim();
                            }
                            if (FieldsType.ElementAt(j) == typeof(DateTime)) { DateTime dt = new DateTime(Convert.ToInt16(System.Text.Encoding.ASCII.GetString(data).ToString().Substring(0, 4)), Convert.ToInt16(System.Text.Encoding.ASCII.GetString(data).ToString().Substring(4, 2)), Convert.ToInt16(System.Text.Encoding.ASCII.GetString(data).ToString().Substring(6, 2))); row[FieldsName.ElementAt(j).ToString()] = dt; }
                            if (FieldsType.ElementAt(j) == typeof(float) || FieldsType.ElementAt(j) == typeof(double)) { 
                                // decimal[] d = new decimal[1]; Buffer.BlockCopy(FileBytes, (32+32*FieldsCount+1)+rowlen*i+offset, d, 0, FieldLenght.ElementAt(j)); 
                                string data_ = System.Text.Encoding.ASCII.GetString(data).Trim();
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalSeparator = ",";
                                provider.NumberGroupSeparator = ".";
                                data_ = data_.Replace(".", ",");
                                if (data_ == "")
                                {
                                    data_ = "0";
                                }
                                row[FieldsName.ElementAt(j).ToString()] = Convert.ToDouble(data_, provider);
                            }
                            if (FieldsType.ElementAt(j) == typeof(Int16)) { Int16[] d = new Int16[1]; Buffer.BlockCopy(FileBytes, (32 + 32 * FieldsCount + 1) + rowlen * i + offset, d, 0, FieldLenght.ElementAt(j)); row[FieldsName.ElementAt(j).ToString()] = d[0]; }
                            if (FieldsType.ElementAt(j) == typeof(Int32)) { Int32[] d = new Int32[1]; Buffer.BlockCopy(FileBytes, (32 + 32 * FieldsCount + 1) + rowlen * i + offset, d, 0, FieldLenght.ElementAt(j)); row[FieldsName.ElementAt(j).ToString()] = d[0]; }
                            offset += FieldLenght.ElementAt(j);
                        }
                        Table.Rows.Add(row);
                    }
                    result = true;
                }
                catch { return false; }
            }
            return true;
        }

        private byte[] ToByteArray(Stream stream)
        {
            using (stream)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }

        ~DBF()
        {
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool Clear()
        {
            Table.Rows.Clear();
            Table.Columns.Clear();
            Table.Clear();
            this.FileStream = null;
            this.FileBytes = null;
            this.FieldsName = new List<string>();
            this.FieldsType = new List<Type>();
            this.FieldLenght = new List<byte>();
            this.Code = null;
            return true;
        }

    }
}

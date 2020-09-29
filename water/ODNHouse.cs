using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace CalculateWater
{
    class ODNHouse
    {
        int HouseCode;
        string Street;
        string HomeNumber;
        double Area;
        double CubeSumma;
        double ColdCounter;
        double HotCounter;
        double CirculateCounter;
        List<ODNFlat> Flats;
        Tarifs Tar;
        SqlConnection conn;
        string PerCur;

        public ODNHouse(SqlConnection conn, int House_Id)
        {
            HouseCode = House_Id;
            CubeSumma = 0;
            this.conn = conn;
        }

        public bool FillHouse(string PerCur, string LastPer)
        {
            this.PerCur = PerCur;
            Flats = new List<ODNFlat>();
            this.ColdCounter = 0;
            this.HotCounter = 0;
            this.CirculateCounter = 0;
            string sql = @"SELECT DISTINCT e.Circulation, e.ColdHot, c.Code_Yl, cast(a.numhouse as nvarchar(10)) + a.LitHouse as dom, 
                            b.AreaHabitation + b.AreaNotHabitation as FullArea, f.cube 
                            FROM [Common].[dbo].[spHouses] a, [Common].[dbo].[HousesData] b, [Common].[dbo].[SpStreets] c, 
                            [Common].[dbo].[HouseCounterAdres] d, [Common].[dbo].[HouseCounters] e, 
                            [Common].[dbo].[CountersIndication] f 
                            WHERE a.id_house = @HouseCode and b.PerBeg <= @PerCur and 
                            (case when b.PerEnd = 0 then @PerCur else b.PerEnd end) >= @PerCur and b.House_code = @HouseCode and 
                            a.Street_Code = c.Id_Street and a.Id_House = d.House_Code and d.Id_Adres = e.Adres_Code and 
                            f.HouseCounter_Code = e.Id_Counter and f.Per = @PerCur and f.Cube <> 0 ";
            SqlCommand cmd = new SqlCommand(sql, this.conn);
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = this.PerCur;
            cmd.Parameters.Add("@HouseCode", SqlDbType.Int).Value = this.HouseCode;
            using (SqlDataReader readHouses = cmd.ExecuteReader())
            {
                if (readHouses.HasRows)
                {
                    while (readHouses.Read())
                    {
                        this.Street = readHouses["Code_Yl"].ToString();
                        this.HomeNumber = readHouses["Dom"].ToString();
                        this.Area = Convert.ToDouble(readHouses["FullArea"]);
                        this.ColdCounter = (Convert.ToByte(readHouses["Circulation"]) + Convert.ToByte(readHouses["ColdHot"]) == 0) ? Convert.ToDouble(readHouses["Cube"]) : this.ColdCounter;
                        this.HotCounter = (Convert.ToByte(readHouses["Circulation"]) == 0 && Convert.ToByte(readHouses["ColdHot"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : this.HotCounter;
                        this.CirculateCounter = (Convert.ToByte(readHouses["Circulation"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : this.CirculateCounter;
                    }
                }
                else
                {
                    readHouses.Close();
                    return false;
                }
                readHouses.Close();
            }
// ------ Выбираем все квартиры и пересчитываем их --------------------------------------------------------------
            sql = "SELECT a.lic FROM [Abon].[dbo].[Abonent" + PerCur + "] a, [Abon].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and b.bUK = 0 " +
                "UNION  " +
                "SELECT a.lic FROM [AbonUK].[dbo].[Abonent" + PerCur + "] a, [AbonUK].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and bUK = 1";
            cmd = new SqlCommand(sql, this.conn);
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar).Value = this.Street;
            cmd.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = this.HomeNumber;
            using (SqlDataReader readFlats = cmd.ExecuteReader())
            {
                if (readFlats.HasRows)
                {
                    List<string> Lics = new List<string>();
                    while (readFlats.Read())
                    {
                        Lics.Add(readFlats["lic"].ToString());
                    }
                    readFlats.Close();
/*
                    MainForm MF = new MainForm();
                    System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Пересчитываем начисления\n");
                    for (int i = 0; i < Lics.Count; i++)
                    {
                        string Lic = Lics[i];
                        MF.Query((byte)(Lic.Substring(0, 1) == "1" ? 0 : 1), PerCur, conn, Lic);
                    }
                    System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Начисления пересчитаны\n");
                    
                    MF = null;
*/
                }
                else
                {
                    readFlats.Close();
                    return false;
                }
            }
            sql = "SELECT a.lic, a.flat, a.CubeV, a.Nachisl, a.OverNV_full, a.Area, b.bUK FROM [Abon].[dbo].[Abonent" + PerCur + "] a, [Abon].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and b.bUK = 0 " +
                "UNION  " +
                "SELECT a.lic, a.flat, a.CubeV, a.Nachisl, a.OverNV_full, a.Area, b.bUK FROM [AbonUK].[dbo].[Abonent" + PerCur + "] a, [AbonUK].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and bUK = 1";
            cmd = new SqlCommand(sql, this.conn);
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar).Value = this.Street;
            cmd.Parameters.Add("@Dom", SqlDbType.NVarChar).Value = this.HomeNumber;
            using (SqlDataReader readFlats = cmd.ExecuteReader())
            {
                if (readFlats.HasRows)
                {
                    while (readFlats.Read())
                    {
                        Flats.Add(new ODNFlat(readFlats["lic"].ToString(), readFlats["flat"].ToString(), Convert.ToDouble(readFlats["CubeV"]),
                                        Convert.ToDouble(readFlats["Nachisl"]), Convert.ToDouble(readFlats["Area"]), Convert.ToByte(readFlats["bUK"])));
                        CubeSumma += Convert.ToDouble(readFlats["CubeV"]);
                    }
                }
                else
                {
                    readFlats.Close();
                    return false;
                }
                readFlats.Close();
            }
            if (Flats.Count > 0)
            {
                byte Base = Convert.ToByte((this.Flats[0].Lic.Substring(0, 1) == "1") ? 0 : 1);
                this.Tar = new Tarifs(PerCur, LastPer, this.Street, this.conn, Base);
            }
            return true;
        }

        public void CalculateODN()
        {
            if (Flats.Count > 0)
            {
                ODNFlat Flat;
                for (int i = 0; i < Flats.Count; i++)
                {
                    Flat = Flats[i];
                    Flat.ODNCube = ((ColdCounter + HotCounter - CirculateCounter) - this.CubeSumma) * (Flat.Area / this.Area);
                    Flat.ODNMoney = Math.Round(((ColdCounter + HotCounter - CirculateCounter) - this.CubeSumma) * (Flat.Area / this.Area) * Tar.TarV, 2);
                    // -- Если возврат ОДН по счету превышает сумму начислений, то возврат становится равен начислению --
                    if (- Flat.ODNMoney > Flat.Moneys)
                    {
                        Flat.ODNMoney =  - Flat.Moneys;
                    }
                }
            }
        }

        public void Save()
        {
            SqlCommand cmd;
            string Base;
            string sql;
            ODNFlat Flat;
            if (Flats.Count > 0)
            {
                for (int i = 0; i < Flats.Count; i++)
                {
                    Flat = Flats[i];
                    Base = (Flat.Lic.Substring(0, 1) == "1") ? "Abon.dbo." : "AbonUK.dbo.";
                    sql = "UPDATE " + Base + "Abonent" + PerCur + " SET Nachisl = Nachisl + @OverNV_full, NvFull = NvFull + @OverNV_full, " +
                        "nv = nv + @OverNV_full, OverCubeV = @OverCubeV, OverNV_full = @OverNV_full WHERE lic = @lic";
                    cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add("@OverNV_full", SqlDbType.Decimal).Value = Flat.ODNMoney;
                    cmd.Parameters.Add("@OverCubeV", SqlDbType.Decimal).Value = Flat.ODNCube;
                    cmd.Parameters.Add("@Lic", SqlDbType.NVarChar).Value = Flat.Lic;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

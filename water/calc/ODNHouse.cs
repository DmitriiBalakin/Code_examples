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
        public ODNHouse(SqlConnection conn, int House_Id)
        {
            HouseCode = House_Id;
            CubeSumma = 0;

        }

        public bool FillHouse(SqlConnection conn, string PerCur, string LastPer)
        {
            Flats = new List<ODNFlat>();
            this.ColdCounter = 0;
            this.HotCounter = 0;
            this.CirculateCounter = 0;
            string sql = "SELECT DISTINCT e.Circulation, e.ColdHot, c.Code_Yl, a.numhouse + a.LitHouse as dom, " +
                            "b.AreaHabitation + b.AreaNotHabitation as FullArea, f.cube " +
                            "FROM [Common].[dbo].[spHouses] a, [Common].[dbo].[HousesData] b, [Common].[dbo].[SpStreets] c, " +
                            "[Common].[dbo].[HouseCounterAdres] d, [Common].[dbo].[HouseCounters] e, " +
                            "[Common].[dbo].[CountersIndication] f " +
                            "WHERE a.id_house = @HouseCode and (((b.PerBeg = 0) and ((b.PerEnd = 0) or (b.PerEnd >= @PerCur))) or " +
                            "(b.PerBeg <= @PerCur and ((b.PerEnd = 0) or (b.PerEnd >= @PerCur)))) and b.House_code = @HouseCode and " +
                            "a.Street_Code = c.Id_Street and a.Id_House = d.House_Code and d.Id_Adres = e.Adres_Code and " +
                            "f.HouseCounter_Code = e.Id_Counter and f.Per = @PerCur and f.Cube <> 0 ";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = PerCur;
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
                        this.ColdCounter = (Convert.ToByte(readHouses["Circulation"]) + Convert.ToByte(readHouses["ColdHot"]) == 0) ? Convert.ToDouble(readHouses["Cube"]) : 0;
                        this.HotCounter = (Convert.ToByte(readHouses["Circulation"]) == 0 && Convert.ToByte(readHouses["ColdHot"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : 0;
                        this.CirculateCounter = (Convert.ToByte(readHouses["Circulation"]) == 1) ? Convert.ToDouble(readHouses["Cube"]) : 0;
                    }
                }
                else
                {
                    readHouses.Close();
                    return false;
                }
                readHouses.Close();
            }
            string MagicSQLServer = "localhost";
            string connStr = @"Data Source=" + MagicSQLServer + "; Initial Catalog=Abon; Integrated Security=True";
            this.Tar = new Tarifs(PerCur, LastPer, this.Street, conn);
            sql = "SELECT a.lic, a.flat, a.CubeV, a.Nachisl, a.Area, b.bUK FROM [Abon].[dbo].[Abonent" + PerCur + "] a, [Abon].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and b.bUK = 0 " +
                "UNION  " +
                "SELECT a.lic, a.flat, a.CubeV, a.Nachisl, a.Area, b.bUK FROM [AbonUK].[dbo].[Abonent" + PerCur + "] a, [AbonUK].[dbo].[SpVedomstvo] b " +
                "WHERE a.str_code = @Street and a.dom = @Dom and a.KodVedom = b.ID and bUK = 1";
            cmd = new SqlCommand(sql, conn);
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
            return true;
        }

        public void CalculateODN()
        {
            if (Flats.Count > 0)
            {
                for (int i = 0; i < Flats.Count; i++)
                {
                    Flats[i].ODNCube = ((ColdCounter + HotCounter - CirculateCounter) - this.CubeSumma) * (Flats[i].Area / this.Area);
                    Flats[i].ODNMoney = Math.Round(((ColdCounter + HotCounter - CirculateCounter) - this.CubeSumma) * (Flats[i].Area / this.Area) * Tar.TarV, 2);
                }
            }
        }

        public void Save()
        {
            string lines = "Ул. (" + this.Street + ") Дом (" + this.HomeNumber + ") Дом.сч. (" +
                                (this.ColdCounter + this.HotCounter - this.CirculateCounter).ToString() +
                                ") Тер.дома (" + this.Area + ") Сум.куб. (" + this.CubeSumma + ")\r\n";
            if (Flats.Count > 0)
            {
                for (int i = 0; i < Flats.Count; i++)
                {
                    lines = lines + " л/с (" + Flats[i].Lic + ") Квар.(" + Flats[i].FlatNumber + ") S кв. (" + Flats[i].Area + 
                            ") Кубы кв. (" + Flats[i].WaterCube + ") Кубы ОДН (" + Flats[i].ODNCube + ")\r\n";

                }
                System.IO.File.AppendAllText(@"D:\Work\WriteLines.txt", lines);
            }
        }
    }
}
using AppThuyetMinh.Models;

namespace AppThuyetMinh.Data;

public static class PoiData
{
    public static List<Poi> GetPois()
    {
        return new List<Poi>
        {
            new Poi
            {
                Name = "Oc Oanh",
                Latitude = 10.759617,
                Longitude = 106.707120,
                Script = "Day la quan oc Oanh noi tieng o duong Vinh Khanh quan 4"
            },

            new Poi
            {
                Name = "Oc Thao",
                Latitude = 10.759300,
                Longitude = 106.706900,
                Script = "Quan oc Thao chuyen hai san tuoi song"
            }
        };
    }
}

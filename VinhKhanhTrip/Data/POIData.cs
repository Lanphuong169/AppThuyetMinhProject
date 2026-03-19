using VinhKhanhTrip.Models;

namespace VinhKhanhTrip.Data;

public static class POIData
{
    public static List<QuanAn> dsQuan = new()
    {
        new QuanAn
        {
            Ten = "Bún bò Vĩnh Khánh",
            Lat = 10.7621,
            Lng = 106.7022,
            MoTa = "Bạn đang tới quán bún bò nổi tiếng"
        },

        new QuanAn
        {
            Ten = "Ốc Vĩnh Khánh",
            Lat = 10.7615,
            Lng = 106.7012,
            MoTa = "Bạn đang tới khu ốc nổi tiếng"
        }
    };
}

using System.Collections.Generic;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip.Data
{
    public static class DanhSachQuanAn
    {
        public static List<QuanAn> dsQuan = new List<QuanAn>
        {
            new QuanAn { Ten = "Ốc Oanh", Lat = 10.762850, Lng = 106.701950, MoTa = "Quán ốc huyền thoại, đông nhất Quận 4 với menu đa dạng.", MonDacSan = "Ốc hương trứng muối", MucUuTien = 1, Radius = 35, DanhSachHinhAnh = new List<string>{"https://images.unsplash.com/photo-1569058242253-92a9c755a0ec?w=800"} },
            new QuanAn { Ten = "Lẩu Bò Nhà Cháy", Lat = 10.762100, Lng = 106.702300, MoTa = "Quán lẩu bò lâu đời, hương vị đậm đà không đổi theo thời gian.", MonDacSan = "Lẩu đuôi bò", MucUuTien = 1, Radius = 30, DanhSachHinhAnh = new List<string>{"https://images.unsplash.com/photo-1555126634-323283e090f1?w=800"} },
            new QuanAn { Ten = "Ốc Thảo", Lat = 10.761200, Lng = 106.702800, MoTa = "Hải sản tươi sống, giá cả bình dân cho giới trẻ.", MonDacSan = "Sò điệp nướng phô mai", MucUuTien = 2, Radius = 30, DanhSachHinhAnh = new List<string>{"https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=800"} },
            new QuanAn { Ten = "Tiệm Mì Chú Tắc", Lat = 10.760100, Lng = 106.703500, MoTa = "Mì vịt tiềm gia truyền thơm ngon nức tiếng khu Quận 4.", MonDacSan = "Mì vịt tiềm", MucUuTien = 1, Radius = 30, DanhSachHinhAnh = new List<string>{"https://images.unsplash.com/photo-1606850780554-b55ea40fa1e3?w=800"} },
            new QuanAn { Ten = "Chè Mâm 16 Món", Lat = 10.758800, Lng = 106.704200, MoTa = "Thiên đường đồ ngọt với 16 loại chè khác nhau, tha hồ lựa chọn.", MonDacSan = "Mâm chè thập cẩm", MucUuTien = 2, Radius = 30, DanhSachHinhAnh = new List<string>{"https://images.unsplash.com/photo-1551024601-bec78aea704b?w=800"} }
        };
    }
}
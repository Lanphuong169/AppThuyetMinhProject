using Microsoft.Maui.Controls.Maps;
using System.Collections.Generic;

namespace VinhKhanhTrip.Models
{
    public class QuanAn
    {
        public string Ten { get; set; } = "";
        public string MoTa { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }

        public string ImageUrl { get; set; } = "";

        // THÊM 2 BIẾN NÀY ĐỂ HIỂN THỊ 2 ẢNH NHỎ BÊN PHẢI
        public string ImageUrl2 { get; set; } = "";
        public string ImageUrl3 { get; set; } = "";

        public double Rating { get; set; }
        public string OpeningHours { get; set; } = "";
        public string PriceRange { get; set; } = "";

        public string DiaChi { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string Email { get; set; } = "";

        // THÊM BIẾN NÀY ĐỂ PHÂN LOẠI (Oc, Lau, Khac)
        public string Loai { get; set; } = "";

        public List<string> MenuItems { get; set; } = new List<string>();

        public Circle? GeofenceCircle { get; set; }
        public Circle? HighlightCircle { get; set; }

        // --- CÁC THUỘC TÍNH DÀNH CHO ĐA NGÔN NGỮ (BINDING UI) ---
        public string LblMonAn { get; set; } = "Món ăn:";
        public string LblPhucVu { get; set; } = "Phục vụ:";
        public string LblKhongGian { get; set; } = "Không gian:";
        public string BtnXemChiTiet { get; set; } = "Xem chi tiết";
        public string BtnThuyetMinh { get; set; } = "Thuyết minh";

        private string _moTaHienThi = "";
        public string MoTaHienThi
        {
            get => string.IsNullOrEmpty(_moTaHienThi) ? MoTa : _moTaHienThi;
            set => _moTaHienThi = value;
        }
    }
}
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
    }
}
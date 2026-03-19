using System;
using System.Collections.Generic;

namespace VinhKhanhTrip.Models
{
    public class QuanAn
    {
        public string Ten { get; set; } = "";
        public string MoTa { get; set; } = "";
        public string MonDacSan { get; set; } = "";
        public string DanhGia { get; set; } = "";
        public string MenuChiTiet { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string GioMoCua { get; set; } = "";
        public string TrangWeb { get; set; } = "";
        public List<string> DanhSachHinhAnh { get; set; } = new List<string>();

        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Radius { get; set; } = 30;
        public int MucUuTien { get; set; } = 1;

        // Các thuộc tính hỗ trợ hiển thị trên bản đồ
        public Microsoft.Maui.Controls.Maps.Circle? MarkerCircle { get; set; }
        public Microsoft.Maui.Controls.Maps.Circle? HighlightCircle { get; set; }
        public Microsoft.Maui.Controls.Maps.Circle? GeofenceCircle { get; set; }
    }
}
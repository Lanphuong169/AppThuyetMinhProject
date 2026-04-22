using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip.Data
{
    public static class DanhSachQuanAn
    {
        // ==========================================
        // ĐƯỜNG DẪN DATABASE FIREBASE
        // ==========================================
        private static readonly string FirebaseUrl = "https://vinhkhanhtrip-default-rtdb.asia-southeast1.firebasedatabase.app/quanan.json";

        public static List<QuanAn> dsQuan = new List<QuanAn>();

        // Thiết lập biến trạng thái để tránh tải đi tải lại nhiều lần
        public static bool IsLoaded = false;

        // Sự kiện để thông báo cho các trang khi dữ liệu đã sẵn sàng
        public static event EventHandler? DataLoaded;

        /// <summary>
        /// Tải dữ liệu từ Firebase Realtime Database
        /// </summary>
        public static async Task<bool> LoadDataAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(FirebaseUrl);
                    
                    if (string.IsNullOrEmpty(response) || response == "null")
                        return false;

                    // Sử dụng JsonConverter hoặc xử lý thủ công để ép kiểu Lat/Lng an toàn
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var data = JsonSerializer.Deserialize<Dictionary<string, QuanAn>>(response, options);

                    if (data != null)
                    {
                        var newList = new List<QuanAn>();
                        foreach (var kvP in data)
                        {
                            newList.Add(kvP.Value);
                        }

                        bool isFirstLoad = !IsLoaded;

                        // TRAO ĐỔI THAM CHIếU (Reference Swap) - Đảm bảo an toàn đa luồng
                        dsQuan = newList;
                        IsLoaded = true;

                        // Luôn kích hoạt sự kiện để các trang cập nhật UI (ghim bản đồ, danh sách)
                        DataLoaded?.Invoke(null, EventArgs.Empty);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tải dữ liệu Firebase: {ex.Message}");
                LoadFallbackData();
            }
            return false;
        }

        private static void LoadFallbackData()
        {
            if (dsQuan.Count == 0)
            {
                dsQuan.Add(new QuanAn { Ten = "Ốc Oanh", Loai = "Oc", MoTa = "Đang tải dữ liệu từ máy chủ...", Lat = 10.7628, Lng = 106.7019 });
            }
        }

        // Tự động kích hoạt việc tải dữ liệu và bắt đầu vòng lặp cập nhật
        static DanhSachQuanAn()
        {
            _ = LoadDataAsync();
            _ = StartAutoRefreshLoop();
        }

        private static async Task StartAutoRefreshLoop()
        {
            while (true)
            {
                await Task.Delay(15000); // Đợi 15 giây
                await LoadDataAsync();
            }
        }
    }
}
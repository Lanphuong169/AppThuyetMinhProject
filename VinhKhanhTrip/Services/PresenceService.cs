using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace VinhKhanhTrip.Services
{
    public static class PresenceService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string FirebaseBaseUrl = "https://vinhkhanhtrip-default-rtdb.asia-southeast1.firebasedatabase.app/";
        private static string _sessionId = "";
        private static string _deviceId = "";
        private static System.Timers.Timer? _heartbeatTimer;

        public static void Initialize()
        {
            if (!string.IsNullOrEmpty(_sessionId)) return;

            // 1. Lấy hoặc tạo Device ID bền vững (lưu trong Preferences)
            _deviceId = Preferences.Get("vkt_stable_device_id", "");
            if (string.IsNullOrEmpty(_deviceId))
            {
                _deviceId = "app_" + Guid.NewGuid().ToString("N").Substring(0, 12);
                Preferences.Set("vkt_stable_device_id", _deviceId);
            }

            // 2. Tạo Session ID duy nhất cho phiên làm việc này
            _sessionId = "sid_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            // 3. Ghi nhận lịch sử truy cập (Persistent History) - Chạy 1 lần khi mở app
            _ = LogVisitAsync();

            // 4. Bắt đầu gửi Heartbeat (Real-time Presence)
            _ = SendHeartbeatAsync();

            // Thiết lập Timer gửi mỗi 30 giây
            _heartbeatTimer = new System.Timers.Timer(30000);
            _heartbeatTimer.Elapsed += async (s, e) => await SendHeartbeatAsync();
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Enabled = true;
        }

        private static async Task LogVisitAsync()
        {
            try
            {
                var entry = new
                {
                    deviceId = _deviceId,
                    device = DeviceInfo.Current.Model,
                    platform = DeviceInfo.Current.Platform.ToString(),
                    source = "app",
                    entry_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    status = "entered"
                };

                string json = JsonSerializer.Serialize(entry);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Ghi vào nhánh device_history với key là sessionId để không bị ghi đè
                await _httpClient.PutAsync($"{FirebaseBaseUrl}device_history/{_sessionId}.json", content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogVisit Error: {ex.Message}");
            }
        }

        private static async Task SendHeartbeatAsync()
        {
            try
            {
                var data = new
                {
                    platform = DeviceInfo.Current.Platform.ToString(),
                    source = "app",
                    device = DeviceInfo.Current.Model,
                    deviceId = _deviceId,
                    last_seen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Sử dụng PATCH để chỉ cập nhật session hiện tại ở nhánh presence
                await _httpClient.PatchAsync($"{FirebaseBaseUrl}presence/{_sessionId}.json", content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Presence Error: {ex.Message}");
            }
        }
    }
}

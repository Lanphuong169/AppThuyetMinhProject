using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VinhKhanhTrip.Data;

namespace VinhKhanhTrip.Helpers
{
    public static class TranslationHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public static async Task PreLoadAllAsync(string langCode)
        {
            _cache.Clear();
            if (langCode.ToLower().StartsWith("vi")) return;

            var tasks = new List<Task>();

            // Dịch ngầm trước toàn bộ mô tả quán ăn khi người dùng vừa đổi ngôn ngữ
            foreach (var q in DanhSachQuanAn.dsQuan)
            {
                tasks.Add(TranslateAsync(q.MoTa, langCode));
            }
            await Task.WhenAll(tasks);
        }

        public static async Task<string> TranslateAsync(string text, string targetLangCode)
        {
            if (string.IsNullOrWhiteSpace(text) || targetLangCode.ToLower().StartsWith("vi"))
                return text;

            // Kiểm tra xem câu này đã được dịch và lưu vào RAM (Cache) chưa
            string cacheKey = $"{targetLangCode}_{text}";
            if (_cache.ContainsKey(cacheKey))
                return _cache[cacheKey];

            try
            {
                string shortLang = targetLangCode.Split('-')[0].ToLower(); // VD: lấy "fr", "de"

                // Gọi API Google Translate (Miễn phí)
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={shortLang}&dt=t&q={Uri.EscapeDataString(text)}";

                // Ngụy trang thành trình duyệt web để không bị chặn (403 Forbidden)
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    // Bóc tách JSON để lấy chữ đã được dịch
                    using JsonDocument doc = JsonDocument.Parse(json);
                    string fullTranslation = "";

                    // Do Google chia câu dài thành nhiều mảng nhỏ, cần ghép chúng lại
                    foreach (var element in doc.RootElement[0].EnumerateArray())
                    {
                        fullTranslation += element[0].GetString();
                    }

                    // Lưu vào bộ nhớ tạm để lần sau không cần tải lại mạng
                    _cache[cacheKey] = fullTranslation;
                    return fullTranslation;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Translation Error: {ex.Message}");
            }

            // Trả về tiếng Việt gốc nếu rớt mạng hoặc có lỗi
            return text;
        }
    }
}
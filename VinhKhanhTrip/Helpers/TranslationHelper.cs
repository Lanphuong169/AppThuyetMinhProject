using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VinhKhanhTrip.Data;

namespace VinhKhanhTrip.Helpers;

public static class TranslationHelper
{
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public static async Task<string> TranslateAsync(string text, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        if (targetLang.ToLower().StartsWith("vi")) return text;

        string code = targetLang.Split('-')[0].ToLower();
        string cacheKey = $"{code}_{text}";

        if (_cache.ContainsKey(cacheKey)) return _cache[cacheKey];

        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={code}&dt=t&q={Uri.EscapeDataString(text)}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var sb = new StringBuilder();
                foreach (var segment in doc.RootElement[0].EnumerateArray())
                {
                    sb.Append(segment[0].GetString());
                }
                string result = sb.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    _cache[cacheKey] = result;
                    return result;
                }
            }
        }
        catch { }
        return text;
    }

    public static async Task PreLoadAllAsync(string langCode)
    {
        _cache.Clear();
        if (langCode.ToLower().StartsWith("vi")) return;
        foreach (var q in DanhSachQuanAn.dsQuan)
        {
            string cauThuyetMinh = $"Bạn đang tiến vào khu vực {q.Ten}. {q.MoTa}";
            await TranslateAsync(cauThuyetMinh, langCode);
        }
    }
}
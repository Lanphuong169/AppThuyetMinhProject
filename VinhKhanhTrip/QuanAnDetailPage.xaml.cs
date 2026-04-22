using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace VinhKhanhTrip;

public partial class QuanAnDetailPage : ContentPage
{
    private QuanAn _poi;
    private CancellationTokenSource? _ttsCts;
    private IEnumerable<Locale>? _cachedLocales;

    public QuanAnDetailPage(QuanAn poi)
    {
        InitializeComponent();
        _poi = poi;
        BindingContext = _poi;

        // Đăng ký nhận thông báo thay đổi ngôn ngữ
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await UpdateUIStringsAsync();
            });
        });

        // Gọi hàm cập nhật giao diện ngay khi mở trang
        _ = UpdateUIStringsAsync();
    }

    // Đổi thành hàm async Task để chờ dịch nội dung Mô Tả
    private async Task UpdateUIStringsAsync()
    {
        var currentLang = LanguageManager.CurrentLang ?? "vi-VN";

        // 1. Cập nhật các Label tĩnh
        if (currentLang.Contains("vi"))
        {
            lblTitleDescription.Text = "Giới thiệu";
            lblTitleSpecialties.Text = "Menu Đặc sản";
            btnViewMap.Text = "XEM BẢN ĐỒ";
        }
        else if (currentLang.Contains("en"))
        {
            lblTitleDescription.Text = "Description";
            lblTitleSpecialties.Text = "Specialty Menu";
            btnViewMap.Text = "VIEW MAP";
        }
        else if (currentLang.Contains("de"))
        {
            lblTitleDescription.Text = "Beschreibung";
            lblTitleSpecialties.Text = "Spezialitäten-Menü";
            btnViewMap.Text = "KARTE ANZEIGEN";
        }
        else if (currentLang.Contains("fr"))
        {
            lblTitleDescription.Text = "Description";
            lblTitleSpecialties.Text = "Menu de spécialités";
            btnViewMap.Text = "VOIR LA CARTE";
        }
        else if (currentLang.Contains("es"))
        {
            lblTitleDescription.Text = "Descripción";
            lblTitleSpecialties.Text = "Menú de especialidades";
            btnViewMap.Text = "VER MAPA";
        }
        else if (currentLang.Contains("ru"))
        {
            lblTitleDescription.Text = "Описание";
            lblTitleSpecialties.Text = "Специальное меню";
            btnViewMap.Text = "ПОСМОТРЕТЬ КАРТУ";
        }
        else if (currentLang.Contains("zh"))
        {
            lblTitleDescription.Text = "介绍";
            lblTitleSpecialties.Text = "特色菜单";
            btnViewMap.Text = "查看地图";
        }
        else if (currentLang.Contains("ja"))
        {
            lblTitleDescription.Text = "説明";
            lblTitleSpecialties.Text = "特別メニュー";
            btnViewMap.Text = "地図を見る";
        }
        else if (currentLang.Contains("ko"))
        {
            lblTitleDescription.Text = "소개";
            lblTitleSpecialties.Text = "특선 메뉴";
            btnViewMap.Text = "지도 보기";
        }

        // 2. Sử dụng bản dịch từ phần Translations (nếu có)
        if (!string.IsNullOrEmpty(_poi.MoTa))
        {
            string shortLang = currentLang.Split('-')[0].ToLower(); // vi, en, de...
            
            if (shortLang == "vi")
            {
                lblMoTa.Text = _poi.MoTa;
            }
            else if (_poi.Translations != null && _poi.Translations.ContainsKey(shortLang) && !string.IsNullOrEmpty(_poi.Translations[shortLang]))
            {
                // Nếu đã có bản dịch chuẩn từ Admin Panel, dùng luôn
                lblMoTa.Text = _poi.Translations[shortLang];
            }
            else
            {
                // Fallback: Tự động dịch nếu chưa có bản dịch trong DB
                lblMoTa.Text = "...";
                try
                {
                    string translatedText = await TranslationHelper.TranslateAsync(_poi.MoTa, currentLang);
                    lblMoTa.Text = translatedText;
                }
                catch
                {
                    lblMoTa.Text = _poi.MoTa;
                }
            }
        }
    }

    private async void OnViewMapClicked(object sender, EventArgs e)
    {
        if (_poi != null)
        {
            var location = new Location(_poi.Lat, _poi.Lng);
            var options = new MapLaunchOptions { Name = _poi.Ten };

            try
            {
                await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không thể mở bản đồ: " + ex.Message, "OK");
            }
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (_ttsCts != null) _ttsCts.Cancel();
        await Navigation.PopAsync();
    }

    private async void OnSpeakerTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_ttsCts != null) { _ttsCts.Cancel(); _ttsCts.Dispose(); }
            _ttsCts = new CancellationTokenSource();

            string currentLang = LanguageManager.CurrentLang ?? "vi-VN";
            string shortLang = currentLang.Split('-')[0].ToLower();
            string textToSpeak = "";

            // Ưu tiên đọc từ bản dịch đã lưu
            if (shortLang == "vi") 
            {
                textToSpeak = $"{_poi.Ten}. {_poi.MoTa}";
            }
            else if (_poi.Translations != null && _poi.Translations.ContainsKey(shortLang) && !string.IsNullOrEmpty(_poi.Translations[shortLang]))
            {
                // Dịch nốt tên quán (thường ngắn) và gộp với bản dịch mô tả đã có
                string translatedName = await TranslationHelper.TranslateAsync(_poi.Ten, currentLang);
                textToSpeak = $"{translatedName}. {_poi.Translations[shortLang]}";
            }
            else 
            {
                // Fallback cũ: Dịch toàn bộ on-the-fly
                string cauGoc = $"{_poi.Ten}. {_poi.MoTa}";
                textToSpeak = await TranslationHelper.TranslateAsync(cauGoc, currentLang);
            }

            if (_cachedLocales == null) _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().StartsWith(shortLang));

            await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
        }
        catch { }
    }
}
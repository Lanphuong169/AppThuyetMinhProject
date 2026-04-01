using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Data;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using System.Net.Http;
using Microsoft.Maui.Storage;

namespace VinhKhanhTrip;

public partial class MainPage : ContentPage
{
    private readonly Color GoldLuxury = Color.FromArgb("#D4AF37");

    public Location? CurrentLocation { get; set; }

    Location vitriFake = new Location(10.762850, 106.701950);
    Circle fakeBlueDot = new Circle();

    private IAudioPlayer? _audioPlayer;
    private static readonly HttpClient _httpClient = new HttpClient();

    private Polyline[] _guideLines = new Polyline[6];
    private HashSet<string> _trangThaiTrongVung = new HashSet<string>();
    private bool _isSpeaking = false;

    public MainPage()
    {
        InitializeComponent();
        SetupMap();
        UpdateUIStrings();

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateUIStrings();
            });
        });
    }

    private void SetupMap()
    {
        if (map == null) return;
        fakeBlueDot.Center = vitriFake;
        fakeBlueDot.Radius = Distance.FromMeters(6);
        fakeBlueDot.FillColor = Color.FromArgb("#4285F4");
        fakeBlueDot.StrokeColor = Colors.White;
        fakeBlueDot.StrokeWidth = 3;
        map.MapElements.Add(fakeBlueDot);

        foreach (var quan in DanhSachQuanAn.dsQuan)
        {
            var pin = new Pin { Label = $"🍴 {quan.Ten}", Location = new Location(quan.Lat, quan.Lng) };

            // 1. Sự kiện khi ấn vào cái bảng tên của ghim
            pin.InfoWindowClicked += async (s, e) =>
            {
                await Navigation.PushAsync(new QuanAnDetailPage(quan));
            };

            // 2. Sự kiện khi ấn trực tiếp vào cái ghim
            pin.MarkerClicked += async (s, e) =>
            {
                // e.HideInfoWindow = true; 
                await Navigation.PushAsync(new QuanAnDetailPage(quan));
            };

            map.Pins.Add(pin);

            quan.GeofenceCircle = new Circle
            {
                Center = new Location(quan.Lat, quan.Lng),
                Radius = Distance.FromMeters(30),
                StrokeWidth = 4,
                StrokeColor = Colors.Red.WithAlpha(0.6f),
                FillColor = Colors.Red.WithAlpha(0.1f)
            };
            map.MapElements.Add(quan.GeofenceCircle);
        }
        for (int i = 0; i < 6; i++) { _guideLines[i] = new Polyline { StrokeWidth = 5, StrokeColor = Colors.Transparent }; map.MapElements.Add(_guideLines[i]); }
        map.MoveToRegion(MapSpan.FromCenterAndRadius(vitriFake, Distance.FromMeters(150)));
    }

    // --- HIỂN THỊ / ẨN OVERLAY NGÔN NGỮ ---
    private void OnLanguageBtnClicked(object sender, EventArgs e) => LanguageOverlay.IsVisible = true;
    private void CloseLanguageOverlay(object sender, EventArgs e) => LanguageOverlay.IsVisible = false;

    // --- XỬ LÝ SỰ KIỆN CHỌN NGÔN NGỮ ---
    private void OnEnglishSelected(object sender, EventArgs e) => SetLanguage("en-US", "EN");
    private void OnVietnameseSelected(object sender, EventArgs e) => SetLanguage("vi-VN", "VN");

    private async void OnGermanSelected(object sender, EventArgs e) => await SetLanguageAsync("de-DE", "DE");
    private async void OnFrenchSelected(object sender, EventArgs e) => await SetLanguageAsync("fr-FR", "FR");
    private async void OnRussianSelected(object sender, EventArgs e) => await SetLanguageAsync("ru-RU", "RU");
    private async void OnSpanishSelected(object sender, EventArgs e) => await SetLanguageAsync("es-ES", "ES");
    private async void OnChineseSelected(object sender, EventArgs e) => await SetLanguageAsync("zh-CN", "CN");
    private async void OnJapaneseSelected(object sender, EventArgs e) => await SetLanguageAsync("ja-JP", "JP");
    private async void OnKoreanSelected(object sender, EventArgs e) => await SetLanguageAsync("ko-KR", "KR");

    private void SetLanguage(string code, string shortLabel)
    {
        LanguageManager.CurrentLang = code;
        LanguageOverlay.IsVisible = false;
        UpdateUIStrings();
        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage());
    }

    private async Task SetLanguageAsync(string code, string shortLabel)
    {
        LanguageManager.CurrentLang = code;
        LanguageOverlay.IsVisible = false;
        await TranslationHelper.PreLoadAllAsync(code);
        UpdateUIStrings();
        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage());
    }

    private void UpdateUIStrings()
    {
        var currentLang = LanguageManager.CurrentLang ?? "vi-VN";

        if (currentLang.Contains("vi"))
        {
            searchBar.Placeholder = "Tìm quán ăn...";
            lblLanguage.Text = "VN";
        }
        else if (currentLang.Contains("de"))
        {
            searchBar.Placeholder = "Restaurant suchen...";
            lblLanguage.Text = "DE";
        }
        else if (currentLang.Contains("fr"))
        {
            searchBar.Placeholder = "Chercher un restaurant...";
            lblLanguage.Text = "FR";
        }
        else if (currentLang.Contains("ru"))
        {
            searchBar.Placeholder = "Поиск ресторана...";
            lblLanguage.Text = "RU";
        }
        else if (currentLang.Contains("es"))
        {
            searchBar.Placeholder = "Buscar restaurante...";
            lblLanguage.Text = "ES";
        }
        else if (currentLang.Contains("zh"))
        {
            searchBar.Placeholder = "搜索餐厅...";
            lblLanguage.Text = "CN";
        }
        else if (currentLang.Contains("ja"))
        {
            searchBar.Placeholder = "レストランを検索...";
            lblLanguage.Text = "JP";
        }
        else if (currentLang.Contains("ko"))
        {
            searchBar.Placeholder = "식당 검색...";
            lblLanguage.Text = "KR";
        }
        else
        {
            searchBar.Placeholder = "Search for restaurant...";
            lblLanguage.Text = "EN";
        }
    }

    // --- HÀM PHÁT ÂM THANH CLOUD TTS ---
    private async Task PhatAmThanh(string ten, string moTa)
    {
        try
        {
            _isSpeaking = true;

            if (_audioPlayer != null && _audioPlayer.IsPlaying)
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
            }

            string currentLang = LanguageManager.CurrentLang;
            string targetShort = currentLang.Split('-')[0].ToLower();

            string intro = LanguageManager.TranslatePoi(ten);
            string moTaDich = await TranslationHelper.TranslateAsync(moTa, currentLang);
            string textToSpeak = intro + moTaDich;

            if (textToSpeak.Length > 200)
            {
                textToSpeak = textToSpeak.Substring(0, 195) + "...";
            }

            int speedIndex = Preferences.Default.Get("TtsSpeedIndex", 2);
            string ttsSpeed = "1"; // Chuẩn
            if (speedIndex == 0) ttsSpeed = "0.24"; // Rất chậm
            if (speedIndex == 1) ttsSpeed = "0.5";  // Chậm

            string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(textToSpeak)}&tl={targetShort}&client=tw-ob&ttsspeed={ttsSpeed}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var audioStream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new System.IO.MemoryStream();
                await audioStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _audioPlayer = AudioManager.Current.CreatePlayer(memoryStream);
                _audioPlayer.Play();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Online TTS Error: {ex.Message}");
        }
        finally
        {
            _isSpeaking = false;
        }
    }

    // --- CÁC HÀM XỬ LÝ DI CHUYỂN ---
    void XuLyDiChuyen()
    {
        fakeBlueDot.Center = new Location(vitriFake.Latitude, vitriFake.Longitude);
        map.MoveToRegion(MapSpan.FromCenterAndRadius(vitriFake, Distance.FromMeters(150)));
        _ = KiemTraGeofenceVaTTS(vitriFake);
    }

    async Task KiemTraGeofenceVaTTS(Location currentLocation)
    {
        QuanAn? priorityPoi = null;
        double minDist = double.MaxValue;

        // BƯỚC 1: Tìm quán ăn gần nhất (trong phạm vi 30m) để làm priorityPoi
        foreach (var q in DanhSachQuanAn.dsQuan)
        {
            double dist = DistanceHelper.GetDistance(currentLocation, new Location(q.Lat, q.Lng)) * 1000;
            if (dist <= 30)
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    priorityPoi = q;
                }
            }
            else
            {
                _trangThaiTrongVung.Remove(q.Ten);
            }
        }

        // BƯỚC 2: Cập nhật màu sắc cho các vòng tròn (GeofenceCircle)
        foreach (var q in DanhSachQuanAn.dsQuan)
        {
            double dist = DistanceHelper.GetDistance(currentLocation, new Location(q.Lat, q.Lng)) * 1000;

            if (q.GeofenceCircle != null)
            {
                if (priorityPoi != null && q.Ten == priorityPoi.Ten)
                {
                    // Quán đang được chọn/thuyết minh -> VÀNG DẠ QUANG rực rỡ
                    q.GeofenceCircle.StrokeColor = Color.FromArgb("#FFFF00");
                    q.GeofenceCircle.StrokeWidth = 8; // Viền dày gấp đôi để nổi bật
                    q.GeofenceCircle.FillColor = Color.FromArgb("#FFFF00").WithAlpha(0.5f);
                }
                else if (dist <= 30)
                {
                    // Các quán lân cận trong bán kính 30m -> ĐỎ
                    q.GeofenceCircle.StrokeColor = Colors.Red;
                    q.GeofenceCircle.StrokeWidth = 4; // Viền bình thường
                    q.GeofenceCircle.FillColor = Colors.Red.WithAlpha(0.2f);
                }
                else
                {
                    // Ngoài vùng -> Trong suốt (ẩn đi)
                    q.GeofenceCircle.StrokeColor = Colors.Transparent;
                    q.GeofenceCircle.FillColor = Colors.Transparent;
                }
            }
        }

        // BƯỚC 3: Xử lý UI và phát âm thanh TTS cho priorityPoi
        if (priorityPoi != null)
        {
            VeVachDut(currentLocation, priorityPoi);
            DistIndicator.IsVisible = true;
            lblDistance.Text = $"{(int)minDist} m";
            lblTargetName.Text = LanguageManager.Get("Arrive") + ": " + priorityPoi.Ten;

            if (!_trangThaiTrongVung.Contains(priorityPoi.Ten) && !_isSpeaking)
            {
                _trangThaiTrongVung.Add(priorityPoi.Ten);
                await PhatAmThanh(priorityPoi.Ten, priorityPoi.MoTa);
            }
        }
        else
        {
            // Không có quán nào trong phạm vi
            foreach (var g in _guideLines) g.StrokeColor = Colors.Transparent;
            DistIndicator.IsVisible = false;
        }
    }

    void UpClicked(object s, EventArgs e) { vitriFake.Latitude += 0.0001; XuLyDiChuyen(); }
    void DownClicked(object s, EventArgs e) { vitriFake.Latitude -= 0.0001; XuLyDiChuyen(); }
    void LeftClicked(object s, EventArgs e) { vitriFake.Longitude -= 0.0001; XuLyDiChuyen(); }
    void RightClicked(object s, EventArgs e) { vitriFake.Longitude += 0.0001; XuLyDiChuyen(); }

    private void OnSearchButtonPressed(object s, EventArgs e) => SuggestionBox.IsVisible = false;
    private void OnSearchTextChanged(object s, TextChangedEventArgs e)
    {
        string keyword = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(keyword)) { SuggestionBox.IsVisible = false; return; }
        var matches = DanhSachQuanAn.dsQuan.Where(q => q.Ten.ToLower().Contains(keyword)).ToList();
        SuggestionList.ItemsSource = matches;
        SuggestionBox.IsVisible = matches.Any();
    }
    private void OnSuggestionSelected(object s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QuanAn selected)
        {
            SuggestionBox.IsVisible = false;
            searchBar.Text = selected.Ten;
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(selected.Lat, selected.Lng), Distance.FromMeters(150)));
            if (FakeModeSwitch.IsToggled) { vitriFake = new Location(selected.Lat, selected.Lng); XuLyDiChuyen(); }
        }
        if (s is CollectionView cv) cv.SelectedItem = null;
    }
    private void OnFakeModeToggled(object s, ToggledEventArgs e) => map.IsShowingUser = !e.Value;

    void VeVachDut(Location start, QuanAn target)
    {
        double dLat = target.Lat - start.Latitude; double dLng = target.Lng - start.Longitude;
        for (int i = 0; i < 6; i++)
        {
            _guideLines[i].Geopath.Clear();
            _guideLines[i].Geopath.Add(new Location(start.Latitude + dLat * (i / 6.0), start.Longitude + dLng * (i / 6.0)));
            _guideLines[i].Geopath.Add(new Location(start.Latitude + dLat * ((i + 0.6) / 6.0), start.Longitude + dLng * ((i + 0.6) / 6.0)));
            _guideLines[i].StrokeColor = Colors.White;
        }
    }
}
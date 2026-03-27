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

namespace VinhKhanhTrip;

public partial class MainPage : ContentPage
{
    private readonly Color GoldLuxury = Color.FromArgb("#D4AF37");

    // Đã thêm dấu "?" để fix lỗi Non-nullable property
    public Location? CurrentLocation { get; set; }

    Location vitriFake = new Location(10.762850, 106.701950);
    Circle fakeBlueDot = new Circle();

    // TRÌNH PHÁT AUDIO VÀ HTTPCLIENT CHO CLOUD TTS
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

    private void OnLanguageBtnClicked(object sender, EventArgs e) => LanguageMenu.IsVisible = !LanguageMenu.IsVisible;

    // --- XỬ LÝ CHỌN NGÔN NGỮ ---
    private void OnVietnameseSelected(object sender, EventArgs e) => SetLanguage("vi-VN", "VN");
    private async void OnGermanSelected(object sender, EventArgs e) => await SetLanguageAsync("de-DE", "DE");
    private async void OnFrenchSelected(object sender, EventArgs e) => await SetLanguageAsync("fr-FR", "FR");
    private async void OnRussianSelected(object sender, EventArgs e) => await SetLanguageAsync("ru-RU", "RU");

    private void SetLanguage(string code, string shortLabel)
    {
        LanguageManager.CurrentLang = code;
        lblLanguage.Text = shortLabel;
        LanguageMenu.IsVisible = false;
        UpdateUIStrings();
    }

    private async Task SetLanguageAsync(string code, string shortLabel)
    {
        LanguageManager.CurrentLang = code;
        lblLanguage.Text = shortLabel;
        LanguageMenu.IsVisible = false;
        await TranslationHelper.PreLoadAllAsync(code);
        UpdateUIStrings();
    }

    private void UpdateUIStrings()
    {
        searchBar.Placeholder = LanguageManager.Get("Search");
        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage());
    }

    // --- HÀM PHÁT ÂM THANH CLOUD TTS ---
    // --- HÀM PHÁT ÂM THANH CLOUD TTS ĐÃ SỬA LỖI ---
    private async Task PhatAmThanh(string ten, string moTa)
    {
        try
        {
            _isSpeaking = true;

            // Dừng âm thanh cũ nếu đang phát dở
            if (_audioPlayer != null && _audioPlayer.IsPlaying)
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
            }

            string currentLang = LanguageManager.CurrentLang;
            string targetShort = currentLang.Split('-')[0].ToLower();

            // Lấy nội dung cần đọc
            string intro = LanguageManager.TranslatePoi(ten);
            string moTaDich = await TranslationHelper.TranslateAsync(moTa, currentLang);
            string textToSpeak = intro + moTaDich;

            if (textToSpeak.Length > 200)
            {
                textToSpeak = textToSpeak.Substring(0, 195) + "...";
            }

            string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(textToSpeak)}&tl={targetShort}&client=tw-ob";

            // 1. NGỤY TRANG YÊU CẦU THÀNH TRÌNH DUYỆT ĐỂ GOOGLE KHÔNG CHẶN
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // 2. TẢI FILE MP3 VÀO RAM (MemoryStream) TRƯỚC KHI PHÁT
                var audioStream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new System.IO.MemoryStream();
                await audioStream.CopyToAsync(memoryStream);

                // Trả con trỏ về đầu file để bắt đầu phát
                memoryStream.Position = 0;

                // 3. Phát Audio từ RAM
                _audioPlayer = AudioManager.Current.CreatePlayer(memoryStream);
                _audioPlayer.Play();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Google TTS API Bị chặn: {response.StatusCode}");
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
        foreach (var q in DanhSachQuanAn.dsQuan)
        {
            double dist = DistanceHelper.GetDistance(currentLocation, new Location(q.Lat, q.Lng)) * 1000;
            if (q.GeofenceCircle != null)
            {
                q.GeofenceCircle.StrokeColor = dist <= 30 ? Colors.Red : Colors.Transparent;
                q.GeofenceCircle.FillColor = dist <= 30 ? Colors.Red.WithAlpha(0.2f) : Colors.Transparent;
            }
            if (dist <= 30) { if (dist < minDist) { minDist = dist; priorityPoi = q; } }
            else { _trangThaiTrongVung.Remove(q.Ten); }
        }

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
        else { foreach (var g in _guideLines) g.StrokeColor = Colors.Transparent; DistIndicator.IsVisible = false; }
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
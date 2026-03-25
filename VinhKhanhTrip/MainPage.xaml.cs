using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Media;
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Data;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace VinhKhanhTrip;

public partial class MainPage : ContentPage
{
    private readonly Color GoldLuxury = Color.FromArgb("#D4AF37");
    Location vitriFake = new Location(10.762850, 106.701950);
    Circle fakeBlueDot = new Circle();
    CancellationTokenSource? _ttsCts;
    private Polyline[] _guideLines = new Polyline[6];
    private HashSet<string> _trangThaiTrongVung = new HashSet<string>();
    private bool _isSpeaking = false;
    private IEnumerable<Locale>? _cachedLocales;

    public MainPage()
    {
        InitializeComponent();
        SetupMap();
        _ = Task.Run(async () => _cachedLocales = await TextToSpeech.Default.GetLocalesAsync());
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

    private async Task PhatAmThanh(string ten, string moTa)
    {
        try
        {
            if (_ttsCts != null) { _ttsCts.Cancel(); _ttsCts.Dispose(); }
            _ttsCts = new CancellationTokenSource();
            _isSpeaking = true;

            string currentLang = LanguageManager.CurrentLang;

            // BƯỚC ĐỘT PHÁ: KHÔNG GỌI GOOGLE API NỮA!
            // Sử dụng hàm dịch tĩnh nội bộ trong LanguageManager để lấy câu nói ngay lập tức (0.001s)
            string textToSpeak = LanguageManager.TranslatePoi(ten, moTa);

            // Nạp danh sách giọng đọc nếu chưa có
            if (_cachedLocales == null) _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();

            // Tìm đúng gói giọng đọc cho ngôn ngữ hiện tại
            string targetShort = currentLang.Split('-')[0].ToLower(); // Lấy "de", "fr", "ru"...

            var locale = _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().Replace("_", "-") == currentLang.ToLower())
                      ?? _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().StartsWith(targetShort));

            // Phát âm thanh lập tức
            await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions
            {
                Locale = locale,
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancelToken: _ttsCts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"TTS Error: {ex.Message}"); }
        finally { _isSpeaking = false; }
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
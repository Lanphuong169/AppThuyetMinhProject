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

    public MainPage()
    {
        InitializeComponent();
        SetupMap();
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

        for (int i = 0; i < 6; i++)
        {
            _guideLines[i] = new Polyline { StrokeWidth = 5, StrokeColor = Colors.Transparent };
            map.MapElements.Add(_guideLines[i]);
        }

        map.MoveToRegion(MapSpan.FromCenterAndRadius(vitriFake, Distance.FromMeters(150)));
    }

    void UpClicked(object s, EventArgs e) { vitriFake.Latitude += 0.0001; XuLyDiChuyen(); }
    void DownClicked(object s, EventArgs e) { vitriFake.Latitude -= 0.0001; XuLyDiChuyen(); }
    void LeftClicked(object s, EventArgs e) { vitriFake.Longitude -= 0.0001; XuLyDiChuyen(); }
    void RightClicked(object s, EventArgs e) { vitriFake.Longitude += 0.0001; XuLyDiChuyen(); }

    void XuLyDiChuyen()
    {
        fakeBlueDot.Center = new Location(vitriFake.Latitude, vitriFake.Longitude);
        map.MoveToRegion(MapSpan.FromCenterAndRadius(vitriFake, Distance.FromMeters(150)));
        KiemTraGeofenceVaTTS(vitriFake);
    }

    private void OnLanguageBtnClicked(object sender, EventArgs e) => LanguageMenu.IsVisible = !LanguageMenu.IsVisible;
    private void OnVietnameseSelected(object sender, EventArgs e) { lblLanguage.Text = "VN"; LanguageMenu.IsVisible = false; }
    private void OnEnglishSelected(object sender, EventArgs e) { lblLanguage.Text = "EN"; LanguageMenu.IsVisible = false; }

    async void KiemTraGeofenceVaTTS(Location currentLocation)
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

            if (dist <= 30)
            {
                if (dist < minDist) { minDist = dist; priorityPoi = q; }
            }
            else { _trangThaiTrongVung.Remove(q.Ten); }
        }

        if (priorityPoi != null)
        {
            VeVachDut(currentLocation, priorityPoi);
            DistIndicator.IsVisible = true;
            lblDistance.Text = $"{(int)minDist} m";
            lblTargetName.Text = $"Đang đến: {priorityPoi.Ten}";

            if (!_trangThaiTrongVung.Contains(priorityPoi.Ten) && !_isSpeaking)
            {
                _trangThaiTrongVung.Add(priorityPoi.Ten);
                string textToSpeak = LanguageManager.TranslatePoi(priorityPoi.Ten, priorityPoi.MoTa);
                _ = PhatAmThanh(textToSpeak);
            }
        }
        else
        {
            foreach (var g in _guideLines) g.StrokeColor = Colors.Transparent;
            DistIndicator.IsVisible = false;
        }
    }

    void VeVachDut(Location start, QuanAn target)
    {
        double dLat = target.Lat - start.Latitude;
        double dLng = target.Lng - start.Longitude;

        for (int i = 0; i < 6; i++)
        {
            _guideLines[i].Geopath.Clear();
            _guideLines[i].Geopath.Add(new Location(start.Latitude + dLat * (i / 6.0), start.Longitude + dLng * (i / 6.0)));
            _guideLines[i].Geopath.Add(new Location(start.Latitude + dLat * ((i + 0.6) / 6.0), start.Longitude + dLng * ((i + 0.6) / 6.0)));
            _guideLines[i].StrokeColor = Colors.White;
        }
    }

    private async Task PhatAmThanh(string t)
    {
        if (string.IsNullOrEmpty(t)) return;
        try
        {
            if (_ttsCts != null) { _ttsCts.Cancel(); _ttsCts.Dispose(); }
            _ttsCts = new CancellationTokenSource();
            _isSpeaking = true;

            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase));

            await TextToSpeech.Default.SpeakAsync(t, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
        }
        catch { }
        finally { _isSpeaking = false; }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string keyword = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(keyword)) { SuggestionBox.IsVisible = false; return; }
        var matches = DanhSachQuanAn.dsQuan.Where(q => q.Ten.ToLower().Contains(keyword)).ToList();
        SuggestionList.ItemsSource = matches;
        SuggestionBox.IsVisible = matches.Any();
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QuanAn selected)
        {
            SuggestionBox.IsVisible = false;
            searchBar.Text = selected.Ten;
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(selected.Lat, selected.Lng), Distance.FromMeters(150)));
            if (FakeModeSwitch.IsToggled)
            {
                vitriFake = new Location(selected.Lat, selected.Lng);
                XuLyDiChuyen();
            }
        }
        if (sender is CollectionView cv) cv.SelectedItem = null;
    }

    private void OnFakeModeToggled(object s, ToggledEventArgs e) => map.IsShowingUser = !e.Value;
    private void OnSearchButtonPressed(object s, EventArgs e) => SuggestionBox.IsVisible = false;
}
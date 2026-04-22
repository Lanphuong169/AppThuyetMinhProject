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
using VinhKhanhTrip.Services;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Audio;
using System.Net.Http;
using Microsoft.Maui.Storage;
using System.Threading;

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

    // Quản lý mapping giữa Dữ liệu và Ghim để Highlight
    private Dictionary<QuanAn, Pin> _pinMap = new Dictionary<QuanAn, Pin>();
    private string? _manuallySelectedPoiName = null;
    private CancellationTokenSource? _blinkCts;
    private readonly SemaphoreSlim _geoLock = new SemaphoreSlim(1, 1);

    public MainPage()
    {
        InitializeComponent();
        SetupMap();
        UpdateUIStrings();

        // Đợi dữ liệu tải xong thì vẽ lại map
        Task.Run(async () =>
        {
            while (!DanhSachQuanAn.IsLoaded) {
                await Task.Delay(1000);
            }
            MainThread.BeginInvokeOnMainThread(() => SetupMap());
        });

        // Lắng nghe khi Firebase có dữ liệu mới (quán mới được thêm từ Admin)
        DanhSachQuanAn.DataLoaded += OnDataReloaded;

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateUIStrings();
            });
        });
    }

    private void OnDataReloaded(object? sender, EventArgs e)
    {
        // Vẽ lại toàn bộ ghim trên bản đồ khi dữ liệu Firebase thay đổi (thêm/xóa quán)
        MainThread.BeginInvokeOnMainThread(() => SetupMap());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Nạp dữ liệu ngay khi hiện trang
        await DanhSachQuanAn.LoadDataAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    // Hủy đăng ký sự kiện khi trang bị hủy để tránh memory leak
    ~MainPage()
    {
        DanhSachQuanAn.DataLoaded -= OnDataReloaded;
    }

    private void SetupMap()
    {
        if (map == null) return;
        _pinMap.Clear();

        // Xóa các phần tử cũ trước khi vẽ lại để tránh trùng lặp
        map.Pins.Clear();
        map.MapElements.Clear();

        fakeBlueDot.Center = vitriFake;
        fakeBlueDot.Radius = Distance.FromMeters(6);
        fakeBlueDot.FillColor = Color.FromArgb("#4285F4");
        fakeBlueDot.StrokeColor = Colors.White;
        fakeBlueDot.StrokeWidth = 3;
        map.MapElements.Add(fakeBlueDot);

        foreach (var quan in DanhSachQuanAn.dsQuan.ToList())
        {
            // Kiểm tra tọa độ hợp lệ để tránh crash ứng dụng (Lat: -90 đến 90, Lng: -180 đến 180)
            if (quan.Lat < -90 || quan.Lat > 90 || quan.Lng < -180 || quan.Lng > 180)
                continue;

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
            _pinMap[quan] = pin;

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
        
        // QUAN TRỌNG: Gọi ngay lệnh kiểm tra để hiện màu Vàng quán gần nhất ngay khi vẽ map xong
        _ = KiemTraGeofenceVaTTS(vitriFake);
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
    private void ResetSearchHighlight()
    {
        string? oldName = _manuallySelectedPoiName;
        _manuallySelectedPoiName = null;
        _blinkCts?.Cancel();
        _blinkCts = null;

        // Xóa màu Xanh ngay lập tức trên Main Thread để không đè lên màu Vàng sắp vẽ
        if (!string.IsNullOrEmpty(oldName))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var poi = DanhSachQuanAn.dsQuan.ToList().FirstOrDefault(q => q.Ten == oldName);
                if (poi?.GeofenceCircle != null)
                {
                    poi.GeofenceCircle.StrokeColor = Colors.Transparent;
                    poi.GeofenceCircle.FillColor = Colors.Transparent;
                }
            });
        }
    }

    void XuLyDiChuyen()
    {
        fakeBlueDot.Center = new Location(vitriFake.Latitude, vitriFake.Longitude);
        map.MoveToRegion(MapSpan.FromCenterAndRadius(vitriFake, Distance.FromMeters(150)));
        _ = KiemTraGeofenceVaTTS(vitriFake);
    }

    async Task KiemTraGeofenceVaTTS(Location currentLocation)
    {
        // Sử dụng Semaphore để đảm bảo ổn định đa luồng
        if (!await _geoLock.WaitAsync(0)) return; 

        try
        {
            QuanAn? priorityPoi = null; 
            double minDist = double.MaxValue;

            var snapshot = DanhSachQuanAn.dsQuan.ToList();

            // BƯỚC 1: Tìm quán ăn gần nhất trong phạm vi 30m
            foreach (var q in snapshot)
            {
                if (q.Lat < -90 || q.Lat > 90 || q.Lng < -180 || q.Lng > 180) continue;

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

            // BƯỚC 2: Cập nhật màu sắc (Ưu tiên Xanh tìm kiếm > Vàng đang đứng > Đỏ lân cận)
            foreach (var q in snapshot)
            {
                if (q.GeofenceCircle == null) continue;

                double dist = DistanceHelper.GetDistance(currentLocation, new Location(q.Lat, q.Lng)) * 1000;

                // 1. Quán đang được tìm kiếm (Do luồng nháy quản lý)
                if (_manuallySelectedPoiName != null && string.Equals(q.Ten, _manuallySelectedPoiName, StringComparison.Ordinal))
                {
                    continue; 
                }

                // 2. Quán ĐANG ĐỨNG (Gần nhất trong 30m) -> HIỆN VÀNG RỰC
                if (priorityPoi != null && q.Ten == priorityPoi.Ten)
                {
                    q.GeofenceCircle.StrokeColor = Color.FromArgb("#FFFF00");
                    q.GeofenceCircle.StrokeWidth = 10;
                    q.GeofenceCircle.FillColor = Color.FromArgb("#FFFF00").WithAlpha(0.6f);
                }
                // 3. Quán lân cận trong 30m -> HIỆN ĐỎ
                else if (dist <= 30)
                {
                    q.GeofenceCircle.StrokeColor = Colors.Red;
                    q.GeofenceCircle.StrokeWidth = 4;
                    q.GeofenceCircle.FillColor = Colors.Red.WithAlpha(0.2f);
                }
                // 4. Còn lại -> TRONG SUỐT
                else
                {
                    q.GeofenceCircle.StrokeColor = Colors.Transparent;
                    q.GeofenceCircle.FillColor = Colors.Transparent;
                }
            }

            // BƯỚC 3: Xử lý TTS cho quán trong phạm vi 30m
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
                foreach (var g in _guideLines) g.StrokeColor = Colors.Transparent;
                DistIndicator.IsVisible = false;
            }
        }
        finally
        {
            _geoLock.Release();
        }
    }

    void UpClicked(object s, EventArgs e) { ResetSearchHighlight(); vitriFake.Latitude += 0.0001; XuLyDiChuyen(); }
    void DownClicked(object s, EventArgs e) { ResetSearchHighlight(); vitriFake.Latitude -= 0.0001; XuLyDiChuyen(); }
    void LeftClicked(object s, EventArgs e) { ResetSearchHighlight(); vitriFake.Longitude -= 0.0001; XuLyDiChuyen(); }
    void RightClicked(object s, EventArgs e) { ResetSearchHighlight(); vitriFake.Longitude += 0.0001; XuLyDiChuyen(); }

    private void OnSearchButtonPressed(object s, EventArgs e) => SuggestionBox.IsVisible = false;
    private void OnSearchTextChanged(object s, TextChangedEventArgs e)
    {
        string keyword = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(keyword)) { SuggestionBox.IsVisible = false; return; }
        var matches = DanhSachQuanAn.dsQuan.ToList().Where(q => q.Ten.ToLower().Contains(keyword)).ToList();
        SuggestionList.ItemsSource = matches;
        SuggestionBox.IsVisible = matches.Any();
    }
    private void OnSuggestionSelected(object s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QuanAn selected)
        {
            SuggestionBox.IsVisible = false;
            searchBar.Text = selected.Ten;

            // 1. Bay đến vị trí quán
            var targetLocation = new Location(selected.Lat, selected.Lng);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(targetLocation, Distance.FromMeters(100)));

            // 2. HighLight bằng cách thiết lập trạng thái nhấp nháy
            _manuallySelectedPoiName = selected.Ten;
            HighlightSelectedPoi(selected.Ten); // Dọn dẹp các highlight cũ ngay lập tức
            StartBlinkAnimation(selected.Ten);
            
            // Hiện bảng tên ghim nếu có thể tìm thấy Pin
            if (_pinMap.TryGetValue(selected, out var pin))
            {
                // Một số nền tảng hỗ trợ hiện InfoWindow tự động
                // pin.ShowInfoWindow(); 
            }

        }
        if (s is CollectionView cv) cv.SelectedItem = null;
    }

    private async void StartBlinkAnimation(string targetName)
    {
        // Hủy bỏ hiệu ứng nhấp nháy cũ nếu có
        _blinkCts?.Cancel();
        _blinkCts = new CancellationTokenSource();
        var token = _blinkCts.Token;

        try
        {
            float alpha = 0.8f;
            bool gettingBrighter = false;

            while (!token.IsCancellationRequested)
            {
                var target = DanhSachQuanAn.dsQuan.ToList().FirstOrDefault(q => string.Equals(q.Ten, targetName, StringComparison.Ordinal));

                if (target?.GeofenceCircle != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        target.GeofenceCircle.StrokeColor = Color.FromArgb("#00FF00").WithAlpha(alpha); 
                        target.GeofenceCircle.FillColor = Color.FromArgb("#00FF00").WithAlpha(alpha * 0.4f);
                        target.GeofenceCircle.StrokeWidth = 15;
                    });
                }
                else
                {
                    // Nếu chưa tìm thấy Circle (đang Sync), đợi một chút rồi thử lại
                    await Task.Delay(200);
                    continue;
                }

                if (gettingBrighter) alpha += 0.2f; else alpha -= 0.2f;
                if (alpha <= 0.2f) gettingBrighter = true;
                if (alpha >= 1.0f) gettingBrighter = false;

                await Task.Delay(150, token); 
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            // Tìm lại quán đó để dọn dẹp màu sắc
            var finalTarget = DanhSachQuanAn.dsQuan.FirstOrDefault(q => q.Ten == targetName);
            if (finalTarget?.GeofenceCircle != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_manuallySelectedPoiName != targetName)
                    {
                        finalTarget.GeofenceCircle.StrokeColor = Colors.Transparent;
                        finalTarget.GeofenceCircle.FillColor = Colors.Transparent;
                    }
                });
            }
        }
    }

    private void HighlightSelectedPoi(string targetName)
    {
        foreach (var q in DanhSachQuanAn.dsQuan.ToList())
        {
            if (q.Ten != targetName && q.GeofenceCircle != null)
            {
                q.GeofenceCircle.StrokeColor = Colors.Transparent;
                q.GeofenceCircle.FillColor = Colors.Transparent;
            }
        }
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
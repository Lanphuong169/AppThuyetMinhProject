using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Data;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace VinhKhanhTrip;

public partial class DanhSachPage : ContentPage
{
    private CancellationTokenSource? _ttsCts;
    private IEnumerable<Locale>? _cachedLocales;
    private string _currentCategory = "All";

    public DanhSachPage()
    {
        InitializeComponent();

        DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan;

        // Đợi dữ liệu tải xong thì nạp lại danh sách
        Task.Run(async () =>
        {
            while (!DanhSachQuanAn.IsLoaded) {
                await Task.Delay(1000);
            }
            MainThread.BeginInvokeOnMainThread(() => RefreshList());
        });

        // Lắng nghe khi Firebase có dữ liệu mới (quán mới được thêm từ Admin)
        DanhSachQuanAn.DataLoaded += OnDataReloaded;

        // Lắng nghe sự kiện đổi ngôn ngữ
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await UpdateUIStringsAsync();
            });
        });

        Task.Run(async () => _cachedLocales = await TextToSpeech.Default.GetLocalesAsync());

        _ = UpdateUIStringsAsync();
    }

    private void OnDataReloaded(object? sender, EventArgs e)
    {
        // Tự động làm mới danh sách khi có quán mới từ Firebase
        MainThread.BeginInvokeOnMainThread(() => RefreshList());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    // Hủy đăng ký khi trang bị hủy để tránh memory leak
    ~DanhSachPage()
    {
        DanhSachQuanAn.DataLoaded -= OnDataReloaded;
    }

    private async Task UpdateUIStringsAsync()
    {
        var currentLang = LanguageManager.CurrentLang ?? "vi-VN";

        string tMonAn = "Món ăn:"; string tPhucVu = "Phục vụ:"; string tKhongGian = "Không gian:";
        string tXemChiTiet = "Xem chi tiết"; string tThuyetMinh = "Thuyết minh";

        if (currentLang.Contains("vi"))
        {
            searchBar.Placeholder = "Tìm kiếm nhà hàng..."; LblAll.Text = "Tất cả"; LblOc.Text = "Ốc & Hải sản"; LblLau.Text = "Lẩu & Nướng";
        }
        else if (currentLang.Contains("en"))
        {
            searchBar.Placeholder = "Search..."; LblAll.Text = "All"; LblOc.Text = "Seafood"; LblLau.Text = "Hotpot & BBQ";
            tMonAn = "Food:"; tPhucVu = "Service:"; tKhongGian = "Ambiance:"; tXemChiTiet = "Details"; tThuyetMinh = "Listen";
        }
        else if (currentLang.Contains("de"))
        {
            searchBar.Placeholder = "Suchen..."; LblAll.Text = "Alle"; LblOc.Text = "Meeresfrüchte"; LblLau.Text = "Grill";
            tMonAn = "Essen:"; tPhucVu = "Service:"; tKhongGian = "Ambiente:"; tXemChiTiet = "Details"; tThuyetMinh = "Hören";
        }
        else if (currentLang.Contains("fr"))
        {
            searchBar.Placeholder = "Chercher..."; LblAll.Text = "Tout"; LblOc.Text = "Fruits de mer"; LblLau.Text = "Fondue";
            tMonAn = "Plats:"; tPhucVu = "Service:"; tKhongGian = "Ambiance:"; tXemChiTiet = "Détails"; tThuyetMinh = "Écouter";
        }
        else if (currentLang.Contains("es"))
        {
            searchBar.Placeholder = "Buscar..."; LblAll.Text = "Todo"; LblOc.Text = "Mariscos"; LblLau.Text = "Parrilla";
            tMonAn = "Comida:"; tPhucVu = "Servicio:"; tKhongGian = "Ambiente:"; tXemChiTiet = "Detalles"; tThuyetMinh = "Escuchar";
        }
        else if (currentLang.Contains("ru"))
        {
            searchBar.Placeholder = "Поиск..."; LblAll.Text = "Все"; LblOc.Text = "Морепродукты"; LblLau.Text = "Барбекю";
            tMonAn = "Еда:"; tPhucVu = "Сервис:"; tKhongGian = "Атмосфера:"; tXemChiTiet = "Детали"; tThuyetMinh = "Слушать";
        }
        else if (currentLang.Contains("zh"))
        {
            searchBar.Placeholder = "搜索..."; LblAll.Text = "全部"; LblOc.Text = "海鲜"; LblLau.Text = "火锅烧烤";
            tMonAn = "菜品:"; tPhucVu = "服务:"; tKhongGian = "环境:"; tXemChiTiet = "详情"; tThuyetMinh = "语音";
        }
        else if (currentLang.Contains("ja"))
        {
            searchBar.Placeholder = "検索..."; LblAll.Text = "すべて"; LblOc.Text = "シーフード"; LblLau.Text = "鍋＆BBQ";
            tMonAn = "料理:"; tPhucVu = "サービス:"; tKhongGian = "雰囲気:"; tXemChiTiet = "詳細"; tThuyetMinh = "音声";
        }
        else if (currentLang.Contains("ko"))
        {
            searchBar.Placeholder = "검색..."; LblAll.Text = "모두"; LblOc.Text = "해산물"; LblLau.Text = "전골&BBQ";
            tMonAn = "음식:"; tPhucVu = "서비스:"; tKhongGian = "분위기:"; tXemChiTiet = "상세 정보"; tThuyetMinh = "듣기";
        }

        // 1. Gán text cứng cho CollectionView và báo load Mô tả
        foreach (var q in DanhSachQuanAn.dsQuan)
        {
            q.LblMonAn = tMonAn;
            q.LblPhucVu = tPhucVu;
            q.LblKhongGian = tKhongGian;
            q.BtnXemChiTiet = tXemChiTiet;
            q.BtnThuyetMinh = tThuyetMinh;

            q.MoTaHienThi = currentLang.Contains("vi") ? q.MoTa : "...";
        }

        RefreshList(); // Ép UI vẽ lại các text vừa đổi

        // 2. Dịch Mô tả ngầm định
        if (!currentLang.Contains("vi"))
        {
            foreach (var q in DanhSachQuanAn.dsQuan)
            {
                try { q.MoTaHienThi = await TranslationHelper.TranslateAsync(q.MoTa, currentLang); }
                catch { q.MoTaHienThi = q.MoTa; } // Lỗi thì trả về gốc
            }
            RefreshList(); // Ép UI vẽ lại text sau khi đã dịch xong
        }
    }

    private void RefreshList()
    {
        // Gán Null rồi gán lại để CollectionView bắt buộc phải vẽ lại các Bindings
        DanhSachCV.ItemsSource = null;
        FilterData(searchBar.Text, _currentCategory);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterData(e.NewTextValue, _currentCategory);
    }

    private void OnCategoryTapped(object sender, EventArgs e)
    {
        var border = sender as Border;
        var category = (sender as Border)?.GestureRecognizers
                       .OfType<TapGestureRecognizer>()
                       .FirstOrDefault()?.CommandParameter?.ToString();

        if (category == null) return;
        _currentCategory = category;

        ResetCategoryColors();
        if (border != null)
        {
            border.BackgroundColor = Color.FromArgb("#C99446");
            var label = border.Content as Label;
            if (label != null) { label.TextColor = Colors.Black; label.FontAttributes = FontAttributes.Bold; }
        }

        FilterData(searchBar.Text, _currentCategory);
    }

    private void ResetCategoryColors()
    {
        var buttons = new[] { BtnAll, BtnOc, BtnLau };
        var labels = new[] { LblAll, LblOc, LblLau };

        foreach (var b in buttons) b.BackgroundColor = Colors.Transparent;
        foreach (var l in labels) { l.TextColor = Color.FromArgb("#C99446"); l.FontAttributes = FontAttributes.None; }
    }

    private void FilterData(string searchText, string category)
    {
        var filtered = DanhSachQuanAn.dsQuan.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
            filtered = filtered.Where(q => q.Ten.ToLower().Contains(searchText.ToLower()));

        if (category != "All")
            filtered = filtered.Where(q => q.Loai == category);

        DanhSachCV.ItemsSource = filtered.ToList();
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QuanAn selected)
        {
            await Navigation.PushAsync(new QuanAnDetailPage(selected));
        }
        DanhSachCV.SelectedItem = null;
    }

    private async void OnMapButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is QuanAn q)
        {
            await Navigation.PushAsync(new QuanAnDetailPage(q));
        }
    }

    private async void OnSpeakerButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is QuanAn q)
        {
            try
            {
                if (_ttsCts != null) { _ttsCts.Cancel(); _ttsCts.Dispose(); }
                _ttsCts = new CancellationTokenSource();

                string cauGoc = $"{q.Ten}. {q.MoTa}";
                string textToSpeak = await TranslationHelper.TranslateAsync(cauGoc, LanguageManager.CurrentLang);

                if (_cachedLocales == null) _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();
                string targetTag = LanguageManager.CurrentLang.Split('-')[0].ToLower();
                var locale = _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().StartsWith(targetTag));

                await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không thể phát âm thanh: " + ex.Message, "OK");
            }
        }
    }
}
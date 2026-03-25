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

        // Gán dữ liệu ban đầu
        DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan;

        // Đăng ký nhận tin đổi ngôn ngữ để cập nhật giao diện
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(UpdateUI);
        });

        // Tải trước danh sách giọng đọc
        Task.Run(async () => _cachedLocales = await TextToSpeech.Default.GetLocalesAsync());

        UpdateUI();
    }

    // Hàm cập nhật ngôn ngữ cho các Label tĩnh
    private void UpdateUI()
    {
        searchBar.Placeholder = LanguageManager.Get("Search");
        LblAll.Text = LanguageManager.Get("TabAll") ?? "Tất cả";
        // Bạn có thể thêm các Label khác vào LanguageManager để dịch ở đây
    }

    // Xử lý Tìm kiếm
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterData(e.NewTextValue, _currentCategory);
    }

    // Xử lý Lọc theo Category
    private void OnCategoryTapped(object sender, EventArgs e)
    {
        var border = sender as Border;
        var category = (sender as Border)?.GestureRecognizers
                       .OfType<TapGestureRecognizer>()
                       .FirstOrDefault()?.CommandParameter?.ToString();

        if (category == null) return;
        _currentCategory = category;

        // Đổi màu nút để người dùng biết đang chọn
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

    // Chuyển sang trang chi tiết
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

    // HÀM QUAN TRỌNG: Thuyết minh có tự động dịch
    private async void OnSpeakerButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is QuanAn q)
        {
            try
            {
                if (_ttsCts != null) { _ttsCts.Cancel(); _ttsCts.Dispose(); }
                _ttsCts = new CancellationTokenSource();

                // 1. Dùng hàm để Translate từ tiếng Việt gốc sang ngôn ngữ đang chọn
                string cauGoc = $"{q.Ten}. {q.MoTa}";
                string textToSpeak = await TranslationHelper.TranslateAsync(cauGoc, LanguageManager.CurrentLang);

                // 2. Tìm giọng đọc chuẩn
                if (_cachedLocales == null) _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();
                string targetTag = LanguageManager.CurrentLang.Split('-')[0].ToLower();
                var locale = _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().StartsWith(targetTag));

                // 3. Phát âm thanh ngay lập tức
                await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không thể phát âm thanh: " + ex.Message, "OK");
            }
        }
    }
}
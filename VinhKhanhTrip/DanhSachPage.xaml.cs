using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;
using VinhKhanhTrip.Data;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip;

public partial class DanhSachPage : ContentPage
{
    private CancellationTokenSource? _ttsCts;

    public DanhSachPage()
    {
        InitializeComponent();
        if (DanhSachCV != null)
            DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan;
    }

    public void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        string category = e.Parameter?.ToString() ?? "All";

        if (category == "All")
            DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan;
        else
            DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan.Where(q => q.Loai == category).ToList();

        UpdateCategoryUI(category);
    }

    private void UpdateCategoryUI(string selected)
    {
        if (BtnAll == null || BtnOc == null || BtnLau == null) return;
        if (LblAll == null || LblOc == null || LblLau == null) return;

        // 1. Reset tất cả về trạng thái chưa chọn (Nền trong suốt, chữ màu Đồng ánh kim chuẩn)
        BtnAll.BackgroundColor = Colors.Transparent;
        LblAll.TextColor = Color.FromArgb("#C99446");

        BtnOc.BackgroundColor = Colors.Transparent;
        LblOc.TextColor = Color.FromArgb("#C99446");

        BtnLau.BackgroundColor = Colors.Transparent;
        LblLau.TextColor = Color.FromArgb("#C99446");

        // 2. Nhuộm Đồng cho nút đang được chọn (Nền Đồng, chữ Đen)
        if (selected == "All")
        {
            BtnAll.BackgroundColor = Color.FromArgb("#C99446");
            LblAll.TextColor = Colors.Black;
        }
        else if (selected == "Oc")
        {
            BtnOc.BackgroundColor = Color.FromArgb("#C99446");
            LblOc.TextColor = Colors.Black;
        }
        else if (selected == "Lau")
        {
            BtnLau.BackgroundColor = Color.FromArgb("#C99446");
            LblLau.TextColor = Colors.Black;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = RemoveDiacritics(e.NewTextValue ?? "");
        if (string.IsNullOrWhiteSpace(keyword))
            DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan;
        else
            DanhSachCV.ItemsSource = DanhSachQuanAn.dsQuan
                .Where(q => RemoveDiacritics(q.Ten).Contains(keyword)).ToList();
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is QuanAn selected)
        {
            DanhSachCV.SelectedItem = null;
            await Navigation.PushModalAsync(new QuanAnDetailPage(selected));
        }
    }

    private void OnMapButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is QuanAn q)
        {
            Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(new Location(q.Lat, q.Lng),
                new MapLaunchOptions { Name = q.Ten });
        }
    }

    private async void OnSpeakerButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is QuanAn q)
        {
            if (_ttsCts != null) _ttsCts.Cancel();
            _ttsCts = new CancellationTokenSource();
            await TextToSpeech.Default.SpeakAsync(q.Ten + ". " + q.MoTa, cancelToken: _ttsCts.Token);
        }
    }

    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var normalizedString = text.ToLower().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace("đ", "d");
    }
}
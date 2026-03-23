using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Media;
using System;
using System.Linq;
using System.Threading;
using VinhKhanhTrip.Helpers;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip;

public partial class QuanAnDetailPage : ContentPage
{
    private QuanAn _poi;
    private CancellationTokenSource? _ttsCts;

    public QuanAnDetailPage(QuanAn poi)
    {
        InitializeComponent();
        _poi = poi;
        BindingContext = _poi;
        PopulateMenuItems();
    }

    private void PopulateMenuItems()
    {
        if (_poi.MenuItems == null || !_poi.MenuItems.Any()) return;
        MenuItemsLayout.Children.Clear();
        foreach (var item in _poi.MenuItems)
        {
            var chip = new Border
            {
                BackgroundColor = Color.FromArgb("#1A1A22"),
                Stroke = Color.FromArgb("#D4AF37"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 15 },
                Padding = new Thickness(15, 8)
            };
            chip.Content = new Label { Text = item, TextColor = Colors.White, FontSize = 13, FontAttributes = FontAttributes.Bold };
            MenuItemsLayout.Children.Add(chip);
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (_ttsCts != null) _ttsCts.Cancel();
        await Navigation.PopModalAsync();
    }

    // --- HÀM NÀY BỊ THIẾU DẪN ĐẾN LỖI XC0002 ---
    private async void OnSpeakerTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_ttsCts != null) _ttsCts.Cancel();
            _ttsCts = new CancellationTokenSource();
            string textToSpeak = LanguageManager.TranslatePoi(_poi.Ten, _poi.MoTa);
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(l => l.Language.StartsWith(LanguageManager.CurrentLang.Substring(0, 2), StringComparison.OrdinalIgnoreCase));
            await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
        }
        catch { }
    }

    private async void OnViewMapClicked(object sender, EventArgs e)
    {
        try
        {
            var location = new Location(_poi.Lat, _poi.Lng);
            var options = new MapLaunchOptions { Name = _poi.Ten, NavigationMode = NavigationMode.Driving };
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể mở bản đồ: " + ex.Message, "OK");
        }
    }
}
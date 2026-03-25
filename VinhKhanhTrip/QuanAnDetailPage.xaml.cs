using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel; // Cần thiết để mở bản đồ
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Helpers;

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
    }

    // FIX LỖI ĐỎ: Thêm hàm xử lý khi bấm nút "Xem bản đồ"
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

            string cauGoc = $"{_poi.Ten}. {_poi.MoTa}";
            string textToSpeak = await TranslationHelper.TranslateAsync(cauGoc, LanguageManager.CurrentLang);

            if (_cachedLocales == null) _cachedLocales = await TextToSpeech.Default.GetLocalesAsync();
            string targetTag = LanguageManager.CurrentLang.Split('-')[0].ToLower();
            var locale = _cachedLocales?.FirstOrDefault(l => l.Language.ToLower().StartsWith(targetTag));

            await TextToSpeech.Default.SpeakAsync(textToSpeak, new SpeechOptions { Locale = locale }, cancelToken: _ttsCts.Token);
        }
        catch { }
    }
}
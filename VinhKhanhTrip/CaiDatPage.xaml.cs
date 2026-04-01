using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;

namespace VinhKhanhTrip;

public partial class CaiDatPage : ContentPage
{
    public CaiDatPage()
    {
        InitializeComponent();

        // Load lại tốc độ đã lưu (mặc định là nấc 2 = Chuẩn)
        int savedSpeedIndex = Preferences.Default.Get("TtsSpeedIndex", 2);
        SpeedSlider.Value = savedSpeedIndex;
        UpdateSpeedLabel(savedSpeedIndex);

        UpdateUI();

        // Lắng nghe nếu ngôn ngữ bị đổi từ trang Maps thì cập nhật lại chữ Trang Cài Đặt
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateUI();
            });
        });
    }

    private void OnSpeedSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        // Ép thanh kéo nhảy số nguyên
        int stepValue = (int)Math.Round(e.NewValue);
        SpeedSlider.Value = stepValue;

        // Lưu vào máy
        Preferences.Default.Set("TtsSpeedIndex", stepValue);
        UpdateSpeedLabel(stepValue);
    }

    private void UpdateSpeedLabel(int stepValue)
    {
        // Nếu muốn chữ "Chuẩn" cũng đổi ngôn ngữ thì bạn cần thêm key vào Dictionary.
        // Tạm thời giữ nguyên logic hiện tại của bạn.
        lblCurrentSpeed.Text = stepValue switch
        {
            0 => "-2x",
            1 => "-1.5x",
            2 => "Chuẩn",
            3 => "1.5x",
            4 => "2x",
            _ => "Chuẩn"
        };
    }

    private async void OnEnglishClicked(object sender, EventArgs e) => await ChangeLanguage("en-US");
    private async void OnVietnameseClicked(object sender, EventArgs e) => await ChangeLanguage("vi-VN");
    private async void OnGermanClicked(object sender, EventArgs e) => await ChangeLanguage("de-DE");
    private async void OnFrenchClicked(object sender, EventArgs e) => await ChangeLanguage("fr-FR");
    private async void OnRussianClicked(object sender, EventArgs e) => await ChangeLanguage("ru-RU");
    private async void OnSpanishClicked(object sender, EventArgs e) => await ChangeLanguage("es-ES");
    private async void OnChineseClicked(object sender, EventArgs e) => await ChangeLanguage("zh-CN");
    private async void OnJapaneseClicked(object sender, EventArgs e) => await ChangeLanguage("ja-JP");
    private async void OnKoreanClicked(object sender, EventArgs e) => await ChangeLanguage("ko-KR");

    private async Task ChangeLanguage(string langCode)
    {
        if (LanguageManager.CurrentLang == langCode) return;

        string title = "Confirm";
        string msg = "Do you want to change language?";
        string btnChange = "Yes";
        string btnCancel = "No";

        if (LanguageManager.Translations.ContainsKey(langCode))
        {
            title = LanguageManager.Translations[langCode].ContainsKey("ConfirmTitle") ? LanguageManager.Translations[langCode]["ConfirmTitle"] : title;
            msg = LanguageManager.Translations[langCode].ContainsKey("ConfirmMsg") ? LanguageManager.Translations[langCode]["ConfirmMsg"] : msg;
            btnChange = LanguageManager.Translations[langCode].ContainsKey("Change") ? LanguageManager.Translations[langCode]["Change"] : btnChange;
            btnCancel = LanguageManager.Translations[langCode].ContainsKey("Cancel") ? LanguageManager.Translations[langCode]["Cancel"] : btnCancel;
        }

        bool answer = await DisplayAlert(title, msg, btnChange, btnCancel);

        if (answer)
        {
            LanguageManager.CurrentLang = langCode;
            UpdateUI();

            // Xóa dấu '_' nếu TranslationHelper.PreLoadAllAsync là lỗi không cần thiết
            // hoặc thêm try catch nếu đây là thao tác mạng
            _ = TranslationHelper.PreLoadAllAsync(langCode);
            WeakReferenceMessenger.Default.Send(new LanguageChangedMessage());
        }
    }

    private void UpdateUI()
    {
        lblPageTitle.Text = LanguageManager.Get("SettingTitle");
        lblFeature.Text = LanguageManager.Get("SettingFeature");
        lblLanguage.Text = LanguageManager.Get("SettingLanguage");

        // GỌI KEY TỪ ĐIỂN ĐỂ CẬP NHẬT CHỮ TỐC ĐỘ THUYẾT MINH
        lblSpeedTitle.Text = "⏱ " + LanguageManager.Get("SettingSpeedTitle");
    }
}
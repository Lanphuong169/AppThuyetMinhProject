using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VinhKhanhTrip.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace VinhKhanhTrip;

public partial class CaiDatPage : ContentPage
{
    public CaiDatPage()
    {
        InitializeComponent();
        UpdateUI();
    }

    // --- Sự kiện click cho từng ngôn ngữ ---
    private async void OnVietnameseClicked(object sender, EventArgs e) => await ChangeLanguage("vi-VN");

    // ĐÃ THÊM LẠI HÀM NÀY ĐỂ SỬA LỖI CHO NÚT TIẾNG ANH
    private async void OnEnglishClicked(object sender, EventArgs e) => await ChangeLanguage("en-US");

    private async void OnGermanClicked(object sender, EventArgs e) => await ChangeLanguage("de-DE");
    private async void OnFrenchClicked(object sender, EventArgs e) => await ChangeLanguage("fr-FR");
    private async void OnRussianClicked(object sender, EventArgs e) => await ChangeLanguage("ru-RU");
    private async void OnJapaneseClicked(object sender, EventArgs e) => await ChangeLanguage("ja-JP");
    private async void OnKoreanClicked(object sender, EventArgs e) => await ChangeLanguage("ko-KR");
    private async void OnChineseClicked(object sender, EventArgs e) => await ChangeLanguage("zh-CN");

    private async Task ChangeLanguage(string langCode)
    {
        // Nếu đang ở ngôn ngữ này rồi thì không làm gì cả
        if (LanguageManager.CurrentLang == langCode) return;

        // Lấy các chuỗi thông báo xác nhận từ từ điển
        string title = LanguageManager.Translations[langCode]["ConfirmTitle"];
        string msg = LanguageManager.Translations[langCode]["ConfirmMsg"];
        string btnChange = LanguageManager.Translations[langCode]["Change"];
        string btnCancel = LanguageManager.Translations[langCode]["Cancel"];

        bool answer = await DisplayAlert(title, msg, btnChange, btnCancel);

        if (answer)
        {
            LanguageManager.CurrentLang = langCode;
            UpdateUI();

            // Tải trước bản dịch
            _ = TranslationHelper.PreLoadAllAsync(langCode);

            // Cập nhật lại UI toàn hệ thống
            WeakReferenceMessenger.Default.Send(new LanguageChangedMessage());
        }
    }

    private void UpdateUI()
    {
        lblPageTitle.Text = LanguageManager.Get("SettingTitle");
        lblFeature.Text = LanguageManager.Get("SettingFeature");
        lblTts.Text = LanguageManager.Get("SettingTts");
        lblGeofence.Text = LanguageManager.Get("SettingGeofence");
        lblLanguage.Text = LanguageManager.Get("SettingLanguage");
    }
}
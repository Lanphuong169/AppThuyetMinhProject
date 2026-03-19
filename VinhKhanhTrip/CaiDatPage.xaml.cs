using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using VinhKhanhTrip.Helpers;

namespace VinhKhanhTrip;

public partial class CaiDatPage : ContentPage
{
    public CaiDatPage()
    {
        InitializeComponent();
    }

    private async void OnVietnameseClicked(object sender, EventArgs e) => await ChangeLanguage("vi-VN");
    private async void OnEnglishClicked(object sender, EventArgs e) => await ChangeLanguage("en-US");

    private async Task ChangeLanguage(string langCode)
    {
        if (LanguageManager.CurrentLang == langCode) return;

        string title, msg, btnChange, btnCancel;

        // Xác định ngôn ngữ của hộp thoại dựa trên nước cần đổi sang
        if (langCode == "en-US")
        {
            // Hỏi bằng tiếng Anh (vì đang muốn đổi sang Anh)
            title = LanguageManager.Translations["en-US"]["ConfirmToEnTitle"];
            msg = LanguageManager.Translations["en-US"]["ConfirmToEnMsg"];
            btnChange = LanguageManager.Translations["en-US"]["Change"];
            btnCancel = LanguageManager.Translations["en-US"]["Cancel"];
        }
        else
        {
            // Hỏi bằng tiếng Việt (vì đang muốn đổi sang Việt)
            title = LanguageManager.Translations["vi-VN"]["ConfirmToViTitle"];
            msg = LanguageManager.Translations["vi-VN"]["ConfirmToViMsg"];
            btnChange = LanguageManager.Translations["vi-VN"]["Change"];
            btnCancel = LanguageManager.Translations["vi-VN"]["Cancel"];
        }

        // Hiện hộp thoại
        bool answer = await DisplayAlert(title, msg, btnChange, btnCancel);

        if (answer)
        {
            LanguageManager.CurrentLang = langCode;
            // Báo loa cho toàn app đổi chữ
            MessagingCenter.Send<Page>(this, "LanguageChanged");
        }
    }
}
using Microsoft.Maui.Controls;
using VinhKhanhTrip.Models;
using VinhKhanhTrip.Helpers;

namespace VinhKhanhTrip;

public partial class QuanAnDetailPage : ContentPage
{
    private QuanAn _poi;

    public QuanAnDetailPage(QuanAn poi)
    {
        InitializeComponent();
        _poi = poi;
        BindingContext = _poi;
        UpdateUIStrings();
    }

    private void UpdateUIStrings()
    {
        // Đổi các nhãn cứng
        lblTitleDescription.Text = LanguageManager.Get("DetailDescription");
        lblTitleSpecialties.Text = LanguageManager.Get("DetailSpecialDishes");
        btnViewMap.Text = LanguageManager.Get("BtnMap");

        // Đổi nội dung nếu đang ở tiếng anh (Giả lập để không phải sửa file data)
        if (LanguageManager.CurrentLang == "en-US")
        {
            lblMoTa.Text = $"Detailed English description for {_poi.Ten} goes here. It is a very nice place to eat.";
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
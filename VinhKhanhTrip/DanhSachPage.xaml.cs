using VinhKhanhTrip.Data;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip
{
    public partial class DanhSachPage : ContentPage
    {
        public DanhSachPage()
        {
            InitializeComponent();
            danhSachQuanCollectionView.ItemsSource = DanhSachQuanAn.dsQuan;
        }

        private async void OnQuanAnSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is QuanAn selectedQuan)
            {
                // Bỏ chọn để có thể click lại lần sau
                danhSachQuanCollectionView.SelectedItem = null;

                // Mở trang chi tiết quán ăn
                await Navigation.PushModalAsync(new QuanAnDetailPage(selectedQuan));
            }
        }
    }
}
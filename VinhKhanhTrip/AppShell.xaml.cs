using Microsoft.Maui.Controls;
using System.Linq;
using VinhKhanhTrip.Helpers;

namespace VinhKhanhTrip;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        MessagingCenter.Subscribe<Page>(this, "LanguageChanged", (sender) => {
            UpdateTabBarStrings();
        });
        UpdateTabBarStrings(); // Gọi lần đầu
    }

    private void UpdateTabBarStrings()
    {
        // Tìm và đổi tên cho từng Tab trong TabBar
        var tabs = this.Items.FirstOrDefault()?.Items;
        if (tabs != null && tabs.Count >= 3)
        {
            tabs[0].Title = LanguageManager.Get("TabMap");
            tabs[1].Title = LanguageManager.Get("TabList");
            tabs[2].Title = LanguageManager.Get("TabSettings");
        }
    }
}
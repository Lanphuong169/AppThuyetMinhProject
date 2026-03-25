using CommunityToolkit.Mvvm.Messaging;
using VinhKhanhTrip.Helpers;

namespace VinhKhanhTrip;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateTabBarStrings();
            });
        });

        UpdateTabBarStrings();
    }

    private void UpdateTabBarStrings()
    {
        if (Items.Count > 0 && Items[0].Items.Count >= 3)
        {
            var tabs = Items[0].Items;
            tabs[0].Title = LanguageManager.Get("TabMap");
            tabs[1].Title = LanguageManager.Get("TabList");
            tabs[2].Title = LanguageManager.Get("TabSettings");
        }
    }
}
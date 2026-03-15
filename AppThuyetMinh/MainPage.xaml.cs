using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace AppThuyetMinh;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnMoveClicked(object sender, EventArgs e)
    {
        var pos = new Location(10.7639, 106.7047);

        map.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                pos,
                Distance.FromMeters(200)));
    }
}

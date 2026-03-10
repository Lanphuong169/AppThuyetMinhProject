using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;

namespace AppThuyetMinh;

public partial class MainPage : ContentPage
{
    bool running = true;

    public MainPage()
    {
        InitializeComponent();

        StartGPS();
    }

    async void StartGPS()
    {
        while (running)
        {
            try
            {
                var location =
                    await Geolocation.GetLocationAsync(
                        new GeolocationRequest(
                            GeolocationAccuracy.Medium));

                if (location != null)
                {
                    var pos = new Location(
                        location.Latitude,
                        location.Longitude);

                    map.MoveToRegion(
                        MapSpan.FromCenterAndRadius(
                            pos,
                            Distance.FromMeters(200)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(5000);
        }
    }
}

using Microsoft.Maui.Devices.Sensors;

namespace AppThuyetMinh.Services;

public class LocationService
{
    public async Task<Location?> GetLocation()
    {
        var request = new GeolocationRequest(
            GeolocationAccuracy.High,
            TimeSpan.FromSeconds(5));

        return await Geolocation.GetLocationAsync(request);
    }
}

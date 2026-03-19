using Microsoft.Maui.Devices.Sensors;

namespace VinhKhanhTrip.Services;

public class LocationService
{
    public async Task<Location> GetLocation()
    {
        return await Geolocation.GetLastKnownLocationAsync();
    }
}

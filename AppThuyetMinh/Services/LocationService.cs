using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Devices.Sensors;

namespace AutoNarrationApp.Services;

public class LocationService
{
    public async Task<Location?> GetLocation()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(5));

            var location = await Geolocation.GetLocationAsync(request);

            return location;
        }
        catch
        {
            return null;
        }
    }
}

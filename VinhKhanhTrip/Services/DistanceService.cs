using Microsoft.Maui.Devices.Sensors;

namespace VinhKhanhTrip.Services;

public class DistanceService
{
    public double GetDistance(
        Location a,
        Location b)
    {
        return Location.CalculateDistance(
            a,
            b,
            DistanceUnits.Kilometers);
    }
}

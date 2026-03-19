using Microsoft.Maui.Devices.Sensors;

namespace VinhKhanhTrip.Helpers;

public static class DistanceHelper
{
    public static double GetDistance(
        Location a,
        Location b)
    {
        return Location.CalculateDistance(
            a,
            b,
            DistanceUnits.Kilometers);
    }
}

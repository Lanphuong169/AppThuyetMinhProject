using Microsoft.Maui.Devices.Sensors;

namespace AppThuyetMinh.Services;

public class DistanceService
{
    public double GetDistance(
        Location a,
        Location b)
    {
        return Location.CalculateDistance(
            a,
            b,
            DistanceUnits.Kilometers
        ) * 1000;
    }
}

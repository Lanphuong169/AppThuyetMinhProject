using Microsoft.Maui.Devices.Sensors;

namespace AppThuyetMinh.Models;

public class QuanAn
{
    public string Ten { get; set; }

    public double Lat { get; set; }

    public double Lng { get; set; }

    public string MoTa { get; set; }

    public Location GetLocation()
    {
        return new Location(Lat, Lng);
    }
}

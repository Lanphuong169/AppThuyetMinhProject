using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoNarrationApp.Models;

namespace AutoNarrationApp.Data;

public static class POIData
{
    public static List<POI> List = new List<POI>()
    {
        new POI
        {
            Name = "Quan Bun Bo",
            Lat = 10.7765,
            Lng = 106.7009,
            Audio = "bunbo.mp3"
        },

        new POI
        {
            Name = "Quan Com Tam",
            Lat = 10.7767,
            Lng = 106.7012,
            Audio = "comtam.mp3"
        }
    };
}

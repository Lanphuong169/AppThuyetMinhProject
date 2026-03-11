using Microsoft.Maui.Controls.Maps;

namespace AppThuyetMinh;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps();

        return builder.Build();
    }
}

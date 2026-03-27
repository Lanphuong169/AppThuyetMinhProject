using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Plugin.Maui.Audio; // Khai báo thư viện Audio
using Microsoft.Extensions.DependencyInjection;

namespace VinhKhanhTrip;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Kích hoạt thư viện Plugin.Maui.Audio
        builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);

        // --- ÉP MÀU VÀNG GOLD CHO KÍNH LÚP (ANDROID) ---
#if ANDROID
        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("LuxuryGoldIcon", (handler, view) =>
        {
            var searchView = handler.PlatformView;
            int searchIconId = searchView.Context.Resources.GetIdentifier("android:id/search_mag_icon", null, null);
            if (searchIconId == 0) searchIconId = Microsoft.Maui.Resource.Id.search_mag_icon;

            if (searchIconId != 0)
            {
                var searchIcon = searchView.FindViewById<Android.Widget.ImageView>(searchIconId);
                if (searchIcon != null)
                {
                    searchIcon.SetColorFilter(Android.Graphics.Color.ParseColor("#D4AF37"), Android.Graphics.PorterDuff.Mode.SrcIn);
                }
            }
        });
#endif

#if DEBUG
        // Cần cài NuGet: Microsoft.Extensions.Logging.Debug
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
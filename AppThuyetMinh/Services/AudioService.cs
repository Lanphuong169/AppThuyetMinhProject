using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNarrationApp.Services;

public class AudioService
{
    private IAudioPlayer? player;

    public async Task Play(string fileName)
    {
        try
        {
            player = AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync(fileName)
            );

            player.Play();
        }
        catch
        {

        }
    }
}


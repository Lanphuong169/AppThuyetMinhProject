using Microsoft.Maui.Media;

namespace AppThuyetMinh.Services;

public class TtsService
{
    public async Task Speak(string text)
    {
        await TextToSpeech.Default.SpeakAsync(text);
    }
}

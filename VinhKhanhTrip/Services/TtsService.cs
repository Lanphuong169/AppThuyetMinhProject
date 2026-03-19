namespace VinhKhanhTrip.Services;

public class TtsService
{
    public async Task Speak(string text)
    {
        await TextToSpeech.SpeakAsync(text);
    }
}

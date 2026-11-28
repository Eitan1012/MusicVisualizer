using NAudio.Wave;


namespace MusicVisualizer.Audio;

// Captures system audio in real time
public sealed class LiveAudioCapture : IDisposable
{
// Captures system audio in real time from Windows deffult playback   
    private readonly WasapiLoopbackCapture _capture = new();
    
// Notifies when new audio data is available
    public event EventHandler<WaveInEventArgs>? DataAvailable;

    public LiveAudioCapture()
    {

        // Forward captured audio to the application
        _capture.DataAvailable += (s, e) => DataAvailable?.Invoke(this, e);
    }
    public WaveFormat WaveFormat => _capture.WaveFormat;
    public void Start() => _capture.StartRecording();
    public void Stop() => _capture.StopRecording();
    public void Dispose() => _capture.Dispose();
}


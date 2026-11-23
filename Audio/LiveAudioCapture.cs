using NAudio.Wave;


namespace MusicVisualizer.Audio;

public sealed class LiveAudioCapture : IDisposable
{
    private readonly WasapiLoopbackCapture _capture = new();
    

    public event EventHandler<WaveInEventArgs>? DataAvailable;

    public LiveAudioCapture()
    {
        _capture.DataAvailable += (s, e) => DataAvailable?.Invoke(this, e);
    }
    public WaveFormat WaveFormat => _capture.WaveFormat;
    public void Start() => _capture.StartRecording();
    public void Stop() => _capture.StopRecording();
    public void Dispose() => _capture.Dispose();
}


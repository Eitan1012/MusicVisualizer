using System;
using System.IO;
using NAudio.Wave;


namespace  MusicVisualizer.Audio
{
    public sealed class AudioPlayer : IDisposable
    {
        private WaveOutEvent? _output;
        private AudioFileReader? _reader;

        public bool IsLoaded => _reader != null;

        public TimeSpan Duration => _reader?.TotalTime ?? TimeSpan.Zero;

        public TimeSpan CurrentTime
        {
            get => _reader?.CurrentTime ?? TimeSpan.Zero;

            set
            {
                if (_reader != null)
                {
                    if (value < TimeSpan.Zero || value > _reader.TotalTime)
                        throw new ArgumentOutOfRangeException(nameof(value), "Current time must be within the duration of the audio file.");

                    _reader.CurrentTime = value;    
                }
            }

        }
        public PlaybackState playbackState => _output?.PlaybackState ?? NAudio.Wave.PlaybackState.Stopped;

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            Release();

            try
            {
                _reader = new AudioFileReader(filePath);
                _output = new WaveOutEvent();
                _output.Init(_reader);

            }
            catch
            {
                Release();
                throw;  
            }


    }

    public void Play()
        {
            if (_output == null)
            return;
            _output.Play();
        }

        public void Pause()
        {
            if (_output == null)
                return;
            _output.Pause();
        }

        public void Stop()
        {
            if (_output == null || _reader == null)
                return; 
            _output.Stop();
            _reader.Position = 0;
        }
    
    private void Release()
    {
        _output?.Stop();
        _output?.Dispose();
        _reader?.Dispose();
        _output = null;
        _reader = null;
    }
    public void Dispose()
    {
        Release();
    }
 }

}



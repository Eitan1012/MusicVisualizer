using NAudio.Wave;

namespace MusicVisualizer.Audio;

public class AudioAnalyzer : ISampleProvider
{
    private ISampleProvider _source;
    private float[] _sampels = new float[2048];
    private int _index = 0;
    private readonly object _lock = new object();

    public WaveFormat WaveFormat => _source.WaveFormat;

    public AudioAnalyzer(ISampleProvider source)
    {
        _source = source;
    }
    public int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        int channels = _source.WaveFormat.Channels;
        lock (_lock)
        {
            for (int i = 0; i < read; i += channels)
            {
                float sample = 0;

                for (int channel = 0; channel < channels; channel++)
                {
                    sample += buffer[offset + i + channel];
                }

                _sampels[_index] = sample / channels;
                _index = (_index + 1) % _sampels.Length;
            }
        }
        return read;
    }
    public float[] GetSamples()
    {
        float[] result = new float[_sampels.Length];
        lock (_lock)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _sampels[(_index + i) % _sampels.Length];
            }
        }
        return result;
    }
}
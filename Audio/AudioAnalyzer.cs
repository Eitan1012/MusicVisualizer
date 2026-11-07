using NAudio.Wave;
using NAudio.Dsp;

namespace MusicVisualizer.Audio;

public class AudioAnalyzer : ISampleProvider
{
    private ISampleProvider _source;
    private float[] _sampels = new float[2048];
    private float[] _smoothedBands = new float[64];
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

    public float[] GetSpectrum()
    {
        float[] samples = GetSamples();
        int size = samples.Length;
        Complex[] fftBuffer = new Complex[size];
        for (int i = 0; i < size; i++)
        {
            fftBuffer[i].X = (float)(samples[i] * FastFourierTransform.HammingWindow(i, size));
            fftBuffer[i].Y = 0;
        }
        FastFourierTransform.FFT(true, 11, fftBuffer);
        float[] spectrum = new float[size / 2];

        for (int i = 0; i < spectrum.Length; i++)
        {
            float real = fftBuffer[i].X;
            spectrum[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
        }
        return spectrum;
    }
    public float[] GetBands()
    {
        float[] spectrum = GetSpectrum();
        int bandsCount = 64;
        float[] bands = new float[bandsCount];
        int binsPerBand = spectrum.Length / bandsCount;
        for (int i = 0; i < bandsCount; i++)
        {
            float sum = 0;
            for (int j = 0; j < binsPerBand; j++)
            {
                sum += spectrum[i * binsPerBand + j];
            }
            float currentBandValue = sum / binsPerBand;
            _smoothedBands[i] = 0.7f * _smoothedBands[i] + 0.3f * currentBandValue;
            bands[i] = _smoothedBands[i];
        }
        return bands;
    }
}
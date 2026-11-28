using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;



namespace MusicVisualizer.Visualization;


// Renders a circular audio visualizer with a spinning disc
public class CircleVisualizer : Control
{
    private float[] _bands = new float[64];
    private float _bassPulse = 0f;
    private float _lastBass = 0f;
    private float _bassAttack = 0f;
    private float _bassAverage = 0.003f;
    private float _midFlow = 0f;
    private float _midPhase = 0f;
    private readonly float[] _smoothedLineResponse = new float[64];
    private readonly float[] _lineSpring = new float[64];
    private readonly float[] _lineSpringVelocity = new float[64];
    private float _glowLevel = 0f;
    private float _discAngle = 0f;
    private float _discSpinSpeed = 0f;
    private Image? _coverArt;
    private Color _neonColor = Color.Cyan;
    private Color _lineColor = Color.MediumPurple;
    private Color _ambientColor = Color.FromArgb(30, 36, 66);
        public CircleVisualizer()
    {
        Size = new Size(320, 320);
        BackColor = Color.FromArgb(12, 16, 30);
        DoubleBuffered = true;
    }

// Updates the visualizer with new audio 
    public void SetBands(float[] bands, float bass, float rms)
    {
        if (bands.Length != 64)
        {
            return;
        }
        _bands = bands;

        // Update glow level based on audio volume
        float glowTarget = Math.Clamp((rms - 0.03f) * 3f, 0f, 1f);
        _glowLevel += (glowTarget - _glowLevel) * 0.15f;
        // Smooth frequency response
        for (int i = 0; i < 64; i++)
        {
            float targetLength = Math.Min(35f, MathF.Sqrt(Math.Max(0f, bands[i])) * 300f);
            float speed =targetLength > _smoothedLineResponse[i] ? 0.55f : 0.14f;
            float previous = _smoothedLineResponse[i];
            float change = targetLength - previous;
            _lineSpringVelocity[i] = (_lineSpringVelocity[i] + change * 0.12f - _lineSpring[i] * 0.18f) * 0.65f;
            _lineSpring[i] = Math.Clamp(_lineSpring[i] + _lineSpringVelocity[i], -2f, 2f);
            _smoothedLineResponse[i] += (targetLength - _smoothedLineResponse[i]) * speed;
        }
        // Calculate mid-range energy
        float midEnergy = 0f;
        for (int j = 6; j < 20; j++)
        {
            midEnergy += bands[j];
        }
        midEnergy /= 14f;
        float midTarget = Math.Clamp(MathF.Sqrt(Math.Max(0f, midEnergy)) * 30f, 0f, 1f);
        _midFlow += (midTarget - _midFlow) * 0.15f;
        _midPhase += _midFlow * 0.035f + 0.025f;

        // Smooth bass response
        float target = Math.Clamp(MathF.Sqrt(Math.Max(0f, bass)) * 5f, 0f, 1f);
        _bassPulse += (target - _bassPulse) * (target > _bassPulse ? 0.35f : 0.10f);

        // Detect sudden bass peaks
        float rise = Math.Max(0f, bass - _lastBass);
        float relativeRise = bass > 0.002f ? rise / Math.Max(0.003f, _bassAverage) : 0f;
        _bassAttack = Math.Max(_bassAttack * 0.80f, Math.Clamp(relativeRise * 0.65f, 0f, 1f));
        _bassAverage += (bass - _bassAverage) * 0.025f;
        _lastBass = bass;

        // Accelerate and decelerate the spinning disc
        float spinTarget = rms > 0.01f ? 0.8f : 0f;
        float spinLerp = spinTarget > _discSpinSpeed ? 0.08f : 0.03f;
        _discSpinSpeed += (spinTarget - _discSpinSpeed) * spinLerp;
        _discAngle = (_discAngle + _discSpinSpeed) % 360f;
        Invalidate();
    }
    // Draws the visualizer using GDI+
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        Graphics g = e.Graphics;
        // Draw ambient background
        using var backgroundPath = new GraphicsPath();
        backgroundPath.AddEllipse(ClientRectangle);
        using var ambientBrush = new PathGradientBrush(backgroundPath);
        ambientBrush.CenterColor = _ambientColor;
        ambientBrush.SurroundColors = new Color[] {BackColor};
        g.FillEllipse(ambientBrush, ClientRectangle);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float centerX = Width / 2f;
        float centerY = Height / 2f;

        // Expand the ring based on the bass
        float radius = Math.Min(Width, Height) / 2f - 110f + _bassPulse * 16f + _bassAttack * 8f;
        using Pen circlePen = new Pen(_neonColor, 3);

        
        float highEnergy = 0f;
        for (int j = 20; j < 40; j++)
        {
            highEnergy += _bands[j];
        }
        highEnergy /= 20f;
        using Pen linePen = new Pen(_lineColor, 2f + Math.Clamp(MathF.Sqrt(Math.Max(0f, highEnergy)) * 80f, 0f, 1.5f));
        using Pen lineGlowPen = new Pen(Color.FromArgb(40, _lineColor), linePen.Width + 8f);
        using Pen glowPen = new Pen(Color.FromArgb((int)(_glowLevel * 95f), _neonColor), 10f);
        using Pen outerGlowPen = new Pen(Color.FromArgb((int)(_glowLevel * 18f)), 28f);
        using Pen middleGlowPen = new Pen(Color.FromArgb((int)(_glowLevel * 35f)), 16f);
        g.DrawEllipse(outerGlowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawEllipse(middleGlowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawEllipse(glowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);

        g.DrawEllipse(circlePen, centerX - radius, centerY - radius, radius * 2, radius * 2);


        
        // Draw frequency lines around the circle
        for (int i = 0; i < 64; i++)
        {
            double angle = 2 * Math.PI * i / 64;

            // Mirorr frequency bands
            int bandIndex = 4 + Math.Min(i, 64 - i) * 59 / 32;
            
        
        
        float lineLength = 20f + _bassPulse * 12f + _smoothedLineResponse[bandIndex] + _lineSpring[bandIndex];

        float wave = (1f + MathF.Sin(2f * (float)angle + _midPhase)) * 0.5f;
        lineLength += _midFlow * 6f * wave;
 

            float x1 = centerX + (float)((radius + 5) * Math.Cos(angle));
            float y1 = centerY + (float)((radius + 5) * Math.Sin(angle));
            float x2 = centerX + (float)((radius + 5 + lineLength) * Math.Cos(angle));
            float y2 = centerY + (float)((radius + 5 + lineLength) * Math.Sin(angle));
            g.DrawLine(lineGlowPen, x1, y1, x2, y2);
            g.DrawLine(linePen, x1, y1, x2, y2);
        }
        float discRadius =Math.Min(Width, Height) / 2f - 116f;
        using var discBrush = new LinearGradientBrush(new RectangleF(centerX - discRadius, centerY - discRadius, discRadius * 2, discRadius * 2), Color.FromArgb(35, 46, 68), Color.FromArgb(93, 98, 128), 45f + _discAngle);
        using var discRim = new Pen(Color.FromArgb(150, 180, 195, 230), 2f);
        g.FillEllipse(discBrush, centerX - discRadius, centerY - discRadius, discRadius * 2, discRadius * 2);

        // Show and rotate the album cover art
        if (_coverArt != null)
        {
            var state = g.Save();
            using var coverPath = new GraphicsPath();
            coverPath.AddEllipse(centerX - discRadius, centerX - discRadius, discRadius * 2, discRadius * 2);
            g.SetClip(coverPath, CombineMode.Intersect);
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(_discAngle);
            g.TranslateTransform(-centerX, -centerY);
            float scale = Math.Max(discRadius * 2 / _coverArt.Width, discRadius * 2 / _coverArt.Height);
            float imageWidth = _coverArt.Width * scale;
            float imageHeight = _coverArt.Height * scale;
            g.DrawImage(_coverArt, centerX - imageWidth / 2f, centerY - imageHeight / 2f, imageWidth, imageHeight);
            g.Restore(state);
        }
        // Draw reflections on the disc
        using var reflectionBrush = new SolidBrush(Color.FromArgb(20, 210, 225, 255)); 
        g.FillPie(reflectionBrush, centerX - discRadius, centerY - discRadius, discRadius * 2, discRadius * 2, 30f + _discAngle, 35f);
        g.FillPie(reflectionBrush, centerX - discRadius, centerY - discRadius, discRadius * 2, discRadius * 2, 210f + _discAngle, 35f);
        g.DrawEllipse(discRim, centerX - discRadius, centerY - discRadius, discRadius * 2, discRadius * 2);
        using var hubBrush = new SolidBrush(Color.FromArgb(12, 16, 30));
        g.FillEllipse(hubBrush, centerX - 18.75f, centerY - 18.75f, 37.5f, 37.5f);
        using var hubRim = new Pen(Color.FromArgb(150, 195, 210, 235), 2f);
        g.DrawEllipse(hubRim, centerX - 23f, centerY - 23f, 46f, 46f);


    }
    // Sets the album cover art and extracts colors for the visualizer
    public void setCoverArt(byte[]? coverData)
    {
        _coverArt?.Dispose();
        _coverArt = null;
        _neonColor = Color.Cyan;
        _lineColor = Color.MediumPurple;
        _ambientColor = Color.FromArgb(30, 36, 66);
        if (coverData != null && coverData.Length > 0)
        {
            using var stream = new System.IO.MemoryStream(coverData);
            using var image = Image.FromStream(stream);
            _coverArt = new Bitmap(image);
            //Gnerate the visualizer colors
            Color dominantColor = AlbumColorExtractor.GetDominantColor(_coverArt);
            _neonColor = AlbumColorExtractor.MakeNeonColor(dominantColor);
            _lineColor = AlbumColorExtractor.GetSeconderyColor(dominantColor);
            _ambientColor = AlbumColorExtractor.GetAmbientColor(dominantColor);
        }
        Invalidate();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _coverArt?.Dispose();
            _coverArt = null;
        }
        base.Dispose(disposing);
    }

}
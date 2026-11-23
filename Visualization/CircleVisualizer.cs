using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;



namespace MusicVisualizer.Visualization;



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
        public CircleVisualizer()
    {
        Size = new Size(320, 320);
        BackColor = Color.FromArgb(12, 16, 30);
        DoubleBuffered = true;
    }

    public void SetBands(float[] bands, float bass, float rms)
    {
        if (bands.Length != 64)
        {
            return;
        }
        _bands = bands;
        float glowTarget = Math.Clamp((rms - 0.03f) * 3f, 0f, 1f);
        _glowLevel += (glowTarget - _glowLevel) * 0.15f;
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
        float midEnergy = 0f;
        for (int j = 6; j < 20; j++)
        {
            midEnergy += bands[j];
        }
        midEnergy /= 14f;
        float midTarget = Math.Clamp(MathF.Sqrt(Math.Max(0f, midEnergy)) * 30f, 0f, 1f);
        _midFlow += (midTarget - _midFlow) * 0.15f;
        _midPhase += _midFlow * 0.035f + 0.025f;
        float target = Math.Clamp(MathF.Sqrt(Math.Max(0f, bass)) * 5f, 0f, 1f);
        _bassPulse += (target - _bassPulse) * (target > _bassPulse ? 0.35f : 0.10f);
        float rise = Math.Max(0f, bass - _lastBass);
        float relativeRise = bass > 0.002f ? rise / Math.Max(0.003f, _bassAverage) : 0f;
        _bassAttack = Math.Max(_bassAttack * 0.80f, Math.Clamp(relativeRise * 0.65f, 0f, 1f));
        _bassAverage += (bass - _bassAverage) * 0.025f;
        _lastBass = bass;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        Graphics g = e.Graphics;
        using var backgroundPath = new GraphicsPath();
        backgroundPath.AddEllipse(ClientRectangle);
        using var ambientBrush = new PathGradientBrush(backgroundPath);
        ambientBrush.CenterColor = Color.FromArgb(24, 30, 54);
        g.FillPath(ambientBrush, backgroundPath);
        ambientBrush.SurroundColors = new Color[] {BackColor};
        g.FillEllipse(ambientBrush, ClientRectangle);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float centerX = Width / 2f;
        float centerY = Height / 2f;
        float radius = Math.Min(Width, Height) / 2f - 110f + _bassPulse * 16f + _bassAttack * 8f;
        using Pen circlePen = new Pen(Color.Cyan, 3);

        
        float highEnergy = 0f;
        for (int j = 20; j < 40; j++)
        {
            highEnergy += _bands[j];
        }
        highEnergy /= 20f;
        using Pen linePen = new Pen(Color.MediumPurple, 2f + Math.Clamp(MathF.Sqrt(Math.Max(0f, highEnergy)) * 80f, 0f, 1.5f));
        using Pen lineGlowPen = new Pen(Color.FromArgb(40, Color.MediumPurple), linePen.Width + 8f);
        using Pen glowPen = new Pen(Color.FromArgb((int)(_glowLevel * 95f), Color.Cyan), 10f);
        using Pen outerGlowPen = new Pen(Color.FromArgb((int)(_glowLevel * 18f)), 28f);
        using Pen middleGlowPen = new Pen(Color.FromArgb((int)(_glowLevel * 35f)), 16f);
        g.DrawEllipse(outerGlowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawEllipse(middleGlowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawEllipse(glowPen, centerX - radius, centerY - radius, radius * 2, radius * 2);

        g.DrawEllipse(circlePen, centerX - radius, centerY - radius, radius * 2, radius * 2);


        

        for (int i = 0; i < 64; i++)
        {
            double angle = 2 * Math.PI * i / 64;

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
    }

}
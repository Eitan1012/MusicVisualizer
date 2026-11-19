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
        public CircleVisualizer()
    {
        Size = new Size(320, 320);
        BackColor = Color.FromArgb(12, 16, 30);
        DoubleBuffered = true;
    }

    public void SetBands(float[] bands, float bass)
    {
        if (bands.Length != 64)
        {
            return;
        }
        _bands = bands;
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
        g.DrawString($"Pulse: {_bassPulse:F2} | Attack: {_bassAttack:F2}", Font, Brushes.White, 8, 8);
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
        g.DrawString($"High: {highEnergy:F6} | Width: {linePen.Width:F2}", Font, Brushes.White, 8, 24);

        g.DrawEllipse(circlePen, centerX - radius, centerY - radius, radius * 2, radius * 2);

        

        for (int i = 0; i < 64; i++)
        {
            double angle = 2 * Math.PI * i / 64;

            int bandIndex = 4 + Math.Min(i, 64 - i) * 59 / 32;
            
        float strength = MathF.Sqrt(Math.Max(0f, _bands[bandIndex]));
        
        float lineLength = 20f + _bassPulse * 12f + Math.Min(35f, strength * 300f);

        float wave = (1f + MathF.Sin(2f * (float)angle + _midPhase)) * 0.5f;
        lineLength += _midFlow * 6f * wave;
 

            float x1 = centerX + (float)((radius + 5) * Math.Cos(angle));
            float y1 = centerY + (float)((radius + 5) * Math.Sin(angle));
            float x2 = centerX + (float)((radius + 5 + lineLength) * Math.Cos(angle));
            float y2 = centerY + (float)((radius + 5 + lineLength) * Math.Sin(angle));
            g.DrawLine(linePen, x1, y1, x2, y2);
        }
    }

}
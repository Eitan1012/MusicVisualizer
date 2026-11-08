using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;



namespace MusicVisualizer.Visualization;



public class CircleVisualizer : Control
{
    private float[] _bands = new float[64];
    public CircleVisualizer()
    {
        Size = new Size(320, 320);
        BackColor = Color.FromArgb(12, 16, 30);
        DoubleBuffered = true;
    }

    public void SetBands(float[] bands)
    {
        if (bands.Length != 64)
        {
            return;
        }
        _bands = bands;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float centerX = Width / 2f;
        float centerY = Height / 2f;
        float radius = Math.Min(Width, Height) / 2f - 100;

        using Pen circlePen = new Pen(Color.Cyan, 3);
        using Pen linePen = new Pen(Color.MediumPurple, 2);
        

        g.DrawEllipse(circlePen, centerX - radius, centerY - radius, radius * 2, radius * 2);

        for (int i = 0; i < 64; i++)
        {
            double angle = 2 * Math.PI * i / 64;
            
        float strength = MathF.Sqrt(Math.Max(0f, _bands[i]));
        float boost = 1f + i / 25f;
        float lineLength = 20f + Math.Min(70f, strength * 500f * boost);
 

            float x1 = centerX + (float)((radius + 5) * Math.Cos(angle));
            float y1 = centerY + (float)((radius + 5) * Math.Sin(angle));
            float x2 = centerX + (float)((radius + 5 + lineLength) * Math.Cos(angle));
            float y2 = centerY + (float)((radius + 5 + lineLength) * Math.Sin(angle));
            g.DrawLine(linePen, x1, y1, x2, y2);
        }
    }

}
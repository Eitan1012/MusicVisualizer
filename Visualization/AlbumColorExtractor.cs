using System.Drawing;

namespace MusicVisualizer.Visualization;

// Extracts dominant colors from album cover art
public static class AlbumColorExtractor
{
 public static Color GetDominantColor(Image image)
    {
        using var sample = new Bitmap(image, 32, 32);
        int[] hueCounts = new int[36];
        int[] redSums = new int[36];
        int[] greenSums = new int[36];
        int[] blueSums = new int[36];
        for (int y = 0; y < sample.Width; y++)
        {
            for (int x = 0; x < sample.Height; x++)
            {
                Color pixel = sample.GetPixel(x, y);
                // Ignore pixels that are too dark or too desaturated
                if (pixel.GetSaturation() < 0.25f || pixel.GetBrightness() < 0.15f)
                {
                    continue;
                }

                int hueIndex = (int)(pixel.GetHue() / 10) % 36;
                hueCounts[hueIndex]++;
                redSums[hueIndex] += pixel.R;
                greenSums[hueIndex] += pixel.G;
                blueSums[hueIndex] += pixel.B;
            }
        }
        // Find the most common hue
        int dominantIndex = 0;
        for (int i = 1; i < hueCounts.Length; i++)
        {
            if (hueCounts[i] > hueCounts[dominantIndex])
            {
                dominantIndex = i;
            }
        }
        int count = hueCounts[dominantIndex];
        if (count == 0)
        {
            return Color.Cyan;
        }
        return Color.FromArgb(redSums[dominantIndex] / count, greenSums[dominantIndex] / count, blueSums[dominantIndex] / count);
    }   
    public static Color MakeNeonColor(Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        if (max == 0)
        {
            return Color.Cyan;
        }
        return Color.FromArgb( color.R * 255 / max, color.G * 255 / max, color.B * 255 / max);
    }
    public static Color GetSeconderyColor(Color color)
    {
        return Color.FromArgb((int)(color.R * 0.75f + 40f), (int)(color.G * 0.75f + 40f), (int)(color.B * 0.75f + 40f));
    }
    public static Color GetAmbientColor(Color color)
    {
        return Color.FromArgb(12 + (int)(color.R * 0.18f), 16 + (int)(color.G * 0.18f), 30 + (int)(color.B * 0.18f));
    }
}
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RGS_Installer.Logic
{
    internal class GradientColor
    {
        public Color FirstColor { get; set; }
        public Color SecondColor { get; set; }

        public GradientColor() { }

        public GradientColor(Color firstColor, Color secondColor)
        {
            FirstColor = firstColor;
            SecondColor = secondColor;
        }

        public static GradientColor GetBestGradient(Color[] colors)
        {
            if (colors == null || colors.Length < 2)
                return new GradientColor();

            GradientColor bestPair = new GradientColor();
            double maxContrast = double.MinValue;

            for (int i = 0; i < colors.Length; i++)
            {
                for (int j = i + 1; j < colors.Length; j++)
                {
                    double contrast = CalculateColorContrast(colors[i], colors[j]);
                    if (contrast > maxContrast)
                    {
                        maxContrast = contrast;
                        bestPair.FirstColor = colors[i];
                        bestPair.SecondColor = colors[j];
                    }
                }
            }

            return bestPair;
        }

        private static double CalculateColorContrast(Color c1, Color c2)
        {
            (double h1, double s1, double b1) = RgbToHsb(c1);
            (double h2, double s2, double b2) = RgbToHsb(c2);

            double hueDifference = Math.Abs(h1 - h2);
            double brightnessDifference = Math.Abs(b1 - b2);
            double saturationDifference = Math.Abs(s1 - s2);

            double hueWeight = 0.6;
            double brightnessWeight = 0.3;
            double saturationWeight = 0.1;

            return hueWeight * hueDifference + brightnessWeight * brightnessDifference + saturationWeight * saturationDifference;
        }

        private static (double Hue, double Saturation, double Brightness) RgbToHsb(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float hue = 0f;
            if (delta > 0)
            {
                if (max == r) hue = (g - b) / delta;
                else if (max == g) hue = 2f + (b - r) / delta;
                else hue = 4f + (r - g) / delta;
            }
            hue = (hue * 60f + 360f) % 360f;

            float saturation = max == 0 ? 0 : delta / max;
            float brightness = max;

            return (hue, saturation, brightness);
        }

        public static Color[] GetDominantColors(BitmapImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // Convert BitmapImage to a writable pixel format
            FormatConvertedBitmap formattedImage = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
            WriteableBitmap writeableBitmap = new WriteableBitmap(formattedImage);

            int stride = writeableBitmap.PixelWidth * 4;
            int pixelCount = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;
            byte[] pixelData = new byte[stride * writeableBitmap.PixelHeight];

            writeableBitmap.CopyPixels(pixelData, stride, 0);

            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(pixelData[i + 3], pixelData[i + 2], pixelData[i + 1], pixelData[i]);

                if (pixelColor.A < 255)
                    pixelCount--;
                else
                {
                    if (colorCounts.ContainsKey(pixelColor))
                    {
                        colorCounts[pixelColor]++;
                    }
                    else
                    {
                        colorCounts[pixelColor] = 1;
                    }
                }
            }

            int threshold = (int)(0.05 * pixelCount);
            return colorCounts
                .Where(pair => pair.Value >= threshold)
                .Select(pair => pair.Key)
                .ToArray();
        }
    }
}

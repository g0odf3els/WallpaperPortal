using Accord.Imaging.ColorReduction;
using SixLabors.ImageSharp.Advanced;

namespace WallpaperPortal
{
    public class ColorUtils
    {
        public static List<System.Drawing.Color> GetColorPalette(string path, int paletteSize)
        {
            HashSet<Rgba32> colors = new HashSet<Rgba32>();

            using (var image = Image.Load<Rgba32>(path))
            {
                var pixels = image.GetPixelMemoryGroup();

                foreach (var pixel in pixels)
                {
                    for (int i = 0; i < pixel.Length; i++)
                    {
                        colors.Add(pixel.Span[i]);
                    }
                }
            }

            IColorQuantizer quantizer = new MedianCutQuantizer();

            foreach(var color in colors)
            {
                quantizer.AddColor(System.Drawing.Color.FromArgb(color.R, color.G, color.B));
            }

            return quantizer.GetPalette(5).ToList();
        }
    }
}

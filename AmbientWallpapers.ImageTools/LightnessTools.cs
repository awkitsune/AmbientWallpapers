using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientWallpapers.ImageTools
{
    public static class LightnessTools
    {
        static double lumaObjective(Color pixel) => (0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B);
        static double lumaPerceived(Color pixel) => (0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
        static double lumaPerceivedSlow(Color pixel) => Math.Sqrt((0.299 * Math.Pow(pixel.R, 2) + 0.587 * Math.Pow(pixel.G, 2) + 0.114 * Math.Pow(pixel.B, 2)));

        public enum FormulaType
        {
            lumaObjective,
            lumaPerceived,
            lumaPerceivedSlow
        }

        public static double CalculateAverageLightness(Bitmap bm, FormulaType formulaType, bool isPrecise)
        {
            double lum = 0;
            var tmpBmp = new Bitmap(bm);
            var width = bm.Width;
            var height = bm.Height;

            if (!isPrecise)
            {
                height = height / 10;
                width = width / 10;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pxl = tmpBmp.GetPixel(x * 10, y * 10);

                    switch (formulaType)
                    {
                        case FormulaType.lumaObjective:
                            lum += lumaObjective(pxl);
                            break;
                        case FormulaType.lumaPerceived:
                            lum += lumaPerceived(pxl);
                            break;
                        case FormulaType.lumaPerceivedSlow:
                            lum += lumaPerceivedSlow(pxl);
                            break;
                        default:
                            break;
                    }
                }
            }

            var avgLum = lum / (width * height);

            return avgLum / 255.0;
        }

    }
}

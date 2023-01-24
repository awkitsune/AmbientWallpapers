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
        static double lumaPerceived(Color pixel) => (0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

        public static double CalculateAverageLightness(Bitmap bm)
        {
            double lum = 0;
            var width = bm.Width;
            var height = bm.Height;

            height = height / 10;
            width = width / 10;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pxl = bm.GetPixel(x * 10, y * 10);

                    lum += lumaPerceived(pxl);
                }
            }

            bm.Dispose();

            return lum / (width * height) / 255.0;
        }

    }
}

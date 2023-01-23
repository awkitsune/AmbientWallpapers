using System.Drawing;
using System.IO;

namespace AmbientWallpapers.ImageTools
{
    public static class Tools
    {
        public static Bitmap ConvertToBitmap(string fileName)
        {
            try
            {
                Bitmap bitmap;
                using (Stream bmpStream = File.Open(fileName, FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);

                    bitmap = new Bitmap(image);

                }
                return bitmap;
            }
            catch (System.Exception)
            {
                return new Bitmap(1, 1);
            }
        }

    }
}

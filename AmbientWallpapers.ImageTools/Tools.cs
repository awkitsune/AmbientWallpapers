using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientWallpapers.ImageTools
{
    public static class Tools
    {
        public static async Task<Bitmap> ConvertToBitmap(string fileName, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
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
                },
                cancellationToken
            );
        }
    }
}

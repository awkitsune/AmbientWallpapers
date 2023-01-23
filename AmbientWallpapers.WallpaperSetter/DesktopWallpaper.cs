using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace AmbientWallpapers.WallpaperSetter
{
    public class DesktopWallpaper
    {
        static int currentWall = 0;

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Fill,
            Fit,
            Span,
            Stretch,
            Tile,
            Center
        }

        public static string Set(Uri uri, Style style = Style.Fill)
        {
            try
            {
                if (File.Exists(Path.Combine(AppContext.BaseDirectory, $"wallpaper{currentWall - 1}.bmp")))
                {
                    File.Delete(Path.Combine(AppContext.BaseDirectory, $"wallpaper{currentWall - 1}.bmp"));
                }

                Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

                System.Drawing.Image img = System.Drawing.Image.FromStream(s);
                string tempPath = Path.Combine(AppContext.BaseDirectory, $"wallpaper{currentWall}.bmp");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                currentWall++;

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                if (style == Style.Fit)
                {
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Span)
                {
                    if (double.Parse(
                        $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor >= 2}", 
                        CultureInfo.InvariantCulture) >= 6.2)
                    {
                        key.SetValue(@"WallpaperStyle", 22.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                    }
                    else
                    {
                        style = Style.Fill;
                    }
                }
                if (style == Style.Stretch)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Tile)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }
                if (style == Style.Center)
                {
                    key.SetValue(@"WallpaperStyle", 0.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Fill)
                {
                    key.SetValue(@"WallpaperStyle", 10.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                return "";
            }
            catch (Exception err)
            {
                return err.ToString();
            }
        }
    }
}

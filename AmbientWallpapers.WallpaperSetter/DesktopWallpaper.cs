using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace AmbientWallpapers.WallpaperSetter
{
    public class DesktopWallpaper
    {
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
            var path = uri.LocalPath;
            try
            {
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, $"temp")))
                {
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, $"temp"));
                }

                string tempPath = Path.Combine(AppContext.BaseDirectory, $"temp\\wallpaper{Path.GetExtension(path)}");

                File.Copy(path, tempPath, true);

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                if (style == Style.Fit)
                {
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }
                if (style == Style.Span)
                {
                    if (double.Parse(
                        $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}", 
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

                return tempPath;
            }
            catch (Exception err)
            {
                return err.ToString();
            }
        }
    }
}

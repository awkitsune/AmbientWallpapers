using Microsoft.Win32;
using System;
using System.IO;

namespace AmbientWallpapers.WallpaperSetter
{
    class LockscreenWallpaper
    {
        const string REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";

        private static string Set(Uri uri)
        {
            try
            {
                Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

                System.Drawing.Image img = System.Drawing.Image.FromStream(s);
                string tempPath = Path.Combine(System.IO.Path.GetDirectoryName(AppContext.BaseDirectory), "wallpaper_lock.bmp");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

                Registry.SetValue(REGISTRY_KEY, "LockScreenImageStatus", 1, RegistryValueKind.DWord);
                Registry.SetValue(REGISTRY_KEY, "LockScreenImagePath", tempPath, RegistryValueKind.String);
                Registry.SetValue(REGISTRY_KEY, "LockScreenImageUrl", tempPath, RegistryValueKind.String);
            }
            catch (Exception err)
            {
                return err.ToString();
            }

            return "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace AmbientWallpapers.App
{
    class Program
    {
        private static Timer timer;
        private static int eventId = 0;
        private static string imagesPath = Path.Combine(AppContext.BaseDirectory, "images\\");

        private static List<ImageTools.ImageFile> imagesList = new List<ImageTools.ImageFile>();

        static void Main(string[] args)
        {
            try
            {
                if (checkForExistance())
                {
                    updateFileList();
                    updateWallpaper();
                }
                else
                {
                    throw new FileNotFoundException("Check folder with files");
                }

                initTimer();
            }
            catch (Exception err)
            {
                
            }
        }

        private static bool checkForExistance()
        {
            try
            {
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        private static void initTimer()
        {
            timer = new Timer();
#if DEBUG
            timer.Interval = 10 * 1000;
#else
            timer.Interval = 60 * 60 * 1000;
#endif
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Start();
        }

        private static void updateWallpaper()
        {
            var luma = WallpaperSetter.TimeToLuminance.LuminanceNow();

            var delta = 0.0;

            var matchingWallpapers = new List<ImageTools.ImageFile>();

            while (matchingWallpapers.Count == 0)
            {
                delta += 0.05;
                matchingWallpapers = imagesList.Where(i => ApproxEquals(i.Luminance, luma, delta)).ToList();
            }

            var wallpaperId = new Random().Next(0, matchingWallpapers.Count - 1);
            var wallpaperUri = new Uri(matchingWallpapers[wallpaperId].Path);

            var response = WallpaperSetter.DesktopWallpaper.Set(wallpaperUri);
        }

        private static bool ApproxEquals(double value, double referenceValue, double delta) =>
            Math.Abs(value - referenceValue) < delta;

        private static void updateFileList()
        {
            DirectoryInfo d = new DirectoryInfo(imagesPath);
            List<FileInfo> files = d.GetFiles("*.png").ToList();
            files.AddRange(d.GetFiles("*.jpg").ToList());
            files.AddRange(d.GetFiles("*.jpeg").ToList());

            if (files.Count == 0)
            {
                throw new FileNotFoundException($"No files in images directory {d.FullName}");
            }
            else
            {
                foreach (var item in files)
                {
                    if (!imagesList.Exists(i => i.Name == item.Name))
                    {
                        imagesList.Add(new ImageTools.ImageFile()
                        {
                            Path = item.FullName,
                            Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                                ImageTools.Tools.ConvertToBitmap(item.FullName)
                                ),
                            Name = item.Name
                        });
                    }
                }

                foreach (var item in imagesList)
                {
                    if (!files.Exists(f => f.Name == item.Name))
                    {
                        imagesList.Remove(item);
                    }
                }
            }
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            updateFileList();
            updateWallpaper();
        }
    }
}

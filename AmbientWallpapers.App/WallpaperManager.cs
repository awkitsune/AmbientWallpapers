using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace AmbientWallpapers.App
{
    public class WallpaperManager
    {
        private string imagesPath;
        private static Timer timer;
        private static List<ImageTools.ImageFile> imagesList = new List<ImageTools.ImageFile>();

        public WallpaperManager(string imagesPath)
        {
            this.imagesPath = imagesPath;

            try
            {
                if (checkForExistance()) UpdateFileList();
                else throw new FileNotFoundException("Check folder with files");

                InitTimer(Properties.Settings.Default.WallpaperChangeTimeMin);

                System.Threading.Thread.Sleep(10 * 1000);
            }
            catch (Exception err)
            {

            }
        }

        private bool checkForExistance()
        {
            try
            {
                if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
                return true;
            }
            catch { return false; }
        }

        public void InitTimer(double periodMinutes)
        {
            timer = new Timer();
            timer.Interval = (int)(periodMinutes * 60 * 1000);
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Start();
        }

        public void UpdateWallpaper()
        {
            var luma = WallpaperSetter.TimeToLuminance.LuminanceNow();

            var matchingWallpapers = new List<ImageTools.ImageFile>();

            for (double delta = 0.0; delta <= 1.0; delta += 0.05)
            {
                matchingWallpapers = imagesList.Where(i => Math.Abs(i.Luminance - luma) < delta).ToList();
            }

            var wallpaperId = new Random().Next(0, matchingWallpapers.Count - 1);
            var wallpaperUri = new Uri(matchingWallpapers[wallpaperId].Path);

            var response = WallpaperSetter.DesktopWallpaper.Set(wallpaperUri);
        }

        public async void UpdateFileList()
        {
            DirectoryInfo d = new DirectoryInfo(imagesPath);
            List<FileInfo> files = d.GetFiles("*.png").ToList();
            files.AddRange(d.GetFiles("*.jpg").ToList());
            files.AddRange(d.GetFiles("*.jpeg").ToList());

            if (files.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show(
                    "No files found in ./images folder", "Warning", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error
                    );
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
                                await ImageTools.Tools.ConvertToBitmap(item.FullName, new System.Threading.CancellationToken())
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
            UpdateWallpaper();
            UpdateFileList();
        }

        public void Stop()
        {
            timer.Stop();
            timer.Dispose();
            imagesList.Clear();
        }
    }
}

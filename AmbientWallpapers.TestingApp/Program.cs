using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientWallpapers.TestingApp
{
    class Program
    {
        static List<ImageTools.ImageFile> fileList = new List<ImageTools.ImageFile>();

        static void Main(string[] args)
        {
            Console.WriteLine($"AmbientWallpapers libs testing tool\n" +
                $"List of available tests:\n" +
                $"1 - Luminance sort\n" +
                $"2 - Luminance calculation types test\n" +
                $"3 - Try to install wallpaper on desktop and lockscreen\n" +
                $"4 - Current luminance\n" +
                $"\n" +
                $"Enter option:");
            var option = Console.ReadLine();
            Console.WriteLine();

            switch (option)
            {
                case "1":
                    testLuminance();
                    break;
                case "2":
                    testSpeed();
                    break;
                case "3":
                    installBothWallpapers();
                    break;
                case "4":
                    checkLuma();
                    break;
                default:
                    Console.WriteLine("Wrong option... Press any key to exit");
                    Console.ReadKey();
                    break;
            }
        }

        private static void checkLuma()
        {
            Console.WriteLine("#  | Winter    Summer");
            for (int i = 0; i < 24; i++)
            {
                Console.WriteLine($"{i, 2} | {WallpaperSetter.TimeToLuminance.LuminanceInTime(new DateTime(2023, 12, 21, i, 0, 0)), -7}   {WallpaperSetter.TimeToLuminance.LuminanceInTime(new DateTime(2023, 6, 21, i, 0, 0)),-7}");
            }

            Console.WriteLine("\n#  | Fall      Spring");
            for (int i = 0; i < 24; i++)
            {
                Console.WriteLine($"{i,2} | {WallpaperSetter.TimeToLuminance.LuminanceInTime(new DateTime(2023, 9, 21, i, 0, 0)),-7}   {WallpaperSetter.TimeToLuminance.LuminanceInTime(new DateTime(2023, 3, 21, i, 0, 0)),-7}");
            }

            Console.ReadKey();
        }

        private static void updateFileList()
        {
            DirectoryInfo d = new DirectoryInfo("./images");
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
                    if (!fileList.Exists(i => i.Name == item.Name))
                    {
                        fileList.Add(new ImageTools.ImageFile()
                        {
                            Path = item.FullName,
                            Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                                ImageTools.Tools.ConvertToBitmap(item.FullName)
                                ),
                            Name = item.Name
                        });
                    }
                }

                foreach (var item in fileList)
                {
                    if (!files.Exists(f => f.Name == item.Name))
                    {
                        fileList.Remove(item);
                    }
                }
            }
        }

        private static void installBothWallpapers()
        {
            updateFileList();

            var luma = WallpaperSetter.TimeToLuminance.LuminanceNow();

            var delta = 0.0;

            var matchingWallpapers = new List<ImageTools.ImageFile>();

            while (matchingWallpapers.Count == 0)
            {
                delta += 0.05;
                matchingWallpapers = fileList.Where(i => ApproxEquals(i.Luminance, luma, delta)).ToList();
            }

            var wallpaperId = new Random().Next(0, matchingWallpapers.Count - 1);
            var wallpaperUri = new Uri(matchingWallpapers[wallpaperId].Path);

            var response = WallpaperSetter.DesktopWallpaper.Set(wallpaperUri);

            Console.WriteLine(response);
            Console.ReadKey();
        }

        private static bool ApproxEquals(double value, double referenceValue, double delta) =>
            Math.Abs(value - referenceValue) < delta;

        private static void testSpeed()
        {
            /*
            Console.WriteLine("Put some image files in ./images folder and press enter...");
            Console.ReadKey();

            DirectoryInfo d = new DirectoryInfo(@"./images");
            List<FileInfo> files = d.GetFiles("*.png").ToList();
            files.AddRange(d.GetFiles("*.jpg").ToList());
            files.AddRange(d.GetFiles("*.jpeg").ToList());

            if (files.Count == 0)
            {
                Console.WriteLine("No files in directory");
            }
            else
            {
                var watch = new System.Diagnostics.Stopwatch();
                var elapsedMs = new long();

                Console.WriteLine($"Running luminance calculation test...");

                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaObjective,
                            true
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with high precision using lumaObjective for {files.Count} images in {elapsedMs} ms");

                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaPerceived,
                            true
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with high precision using lumaPerceived for {files.Count} images in {elapsedMs} ms");

                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaPerceivedSlow,
                            true
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with high precision using lumaPerceivedSlow for {files.Count} images in {elapsedMs} ms");


                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaObjective,
                            false
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with low precision using lumaObjective for {files.Count} images in {elapsedMs} ms");

                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaPerceived,
                            false
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with low precision using lumaPerceived for {files.Count} images in {elapsedMs} ms");

                watch = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in files)
                {
                    fileList.Add(new ImageTools.ImageFile()
                    {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaPerceivedSlow,
                            false
                            ),
                        Name = item.Name
                    });
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Calculated luminance with low precision using lumaPerceivedSlow for {files.Count} images in {elapsedMs} ms");

                Console.WriteLine("Done!");

                Console.WriteLine("Lumas differences: ");
                for (int i = 0; i < files.Count * 3; i++)
                {
                    Console.WriteLine($"{i,5} --- {fileList[i].Luminance - fileList[i + 6 * 3].Luminance}");
                }
                Console.ReadKey();
            }
            */
        }

        private static void testLuminance()
        {
            Console.WriteLine("Put some image files in ./images folder and press enter...");
            Console.ReadKey();

            DirectoryInfo d = new DirectoryInfo(@"./images");
            List<FileInfo> files = d.GetFiles("*.png").ToList();
            files.AddRange(d.GetFiles("*.jpg").ToList());
            files.AddRange(d.GetFiles("*.jpeg").ToList());

            if (files.Count == 0)
            {
                Console.WriteLine("No files in directory");
            }
            else
            {
                foreach (var item in files)
                {
                    Console.WriteLine($"Calculating luminance for {item.FullName}...");
                    fileList.Add(new ImageTools.ImageFile() {
                        Path = item.FullName,
                        Luminance = ImageTools.LightnessTools.CalculateAverageLightness(
                            ImageTools.Tools.ConvertToBitmap(item.FullName)
                            ),
                        Name = item.Name
                    });
                }

                Console.WriteLine("Sorting...");
                fileList = fileList.OrderBy(f => f.Luminance).ToList();

                Console.WriteLine("Writing...");
                Directory.CreateDirectory("images_sorted");

                int i = 0;
                foreach (var item in fileList)
                {
                    File.Copy(item.Path, $"images_sorted/{i}_{item.Luminance}_{item.Name}");
                    i++;
                }

                Console.WriteLine("Done! Check ./images_sorted for results");
                Console.ReadKey();
            }
        }
    }
}

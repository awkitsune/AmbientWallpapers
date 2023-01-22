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
                $"\n" +
                $"Enter option:");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    testLuminance();
                    break;
                case "2":
                    testSpeed();
                    break;
                default:
                    Console.WriteLine("Wrong option... Press any key to exit");
                    Console.ReadKey();
                    break;
            }
        }

        private static void testSpeed()
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
                //Remake for cases
                foreach (var item in files)
                {
                    Console.WriteLine($"Calculating luminance for {item.FullName}...");
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

                Console.WriteLine("Sorting...");
                fileList = fileList.OrderBy(f => f.Luminance).ToList();

                Console.WriteLine("Done!");
                //Write results
            }
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
                            ImageTools.Tools.ConvertToBitmap(item.FullName),
                            ImageTools.LightnessTools.FormulaType.lumaPerceived,
                            false
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
            }
        }
    }
}

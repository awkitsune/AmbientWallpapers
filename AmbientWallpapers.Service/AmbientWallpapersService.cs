using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace AmbientWallpapers.Service
{
    public partial class AmbientWallpapersService : ServiceBase
    {
        private EventLog eventLog;
        private Timer timer;
        private int eventId = 0;
        private string imagesPath = Path.Combine(AppContext.BaseDirectory, "images\\");

        List<ImageTools.ImageFile> imagesList = new List<ImageTools.ImageFile>();

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public AmbientWallpapersService(string[] args)
        {
            InitializeComponent();

            eventLog = new EventLog();
            if (!EventLog.SourceExists("AmbientWallpapers Service"))
            {
                EventLog.CreateEventSource(
                    "AmbientWallpapers Service", "Main Log");
            }
            eventLog.Source = "AmbientWallpapers Service";
            eventLog.Log = "Main Log";
        }

        private bool checkForExistance()
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

        protected override void OnStart(string[] args)
        {
            try
            {
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                if (checkForExistance())
                {
                    updateFileList();
                    updateWallpaper();
                }
                else
                {
                    eventLog.WriteEntry($"AmbientWallpapers service failed to start in {AppContext.BaseDirectory} directory", EventLogEntryType.Information, eventId++);
                    throw new FileNotFoundException("Check folder with files");
                }

                eventLog.WriteEntry($"AmbientWallpapers service started in {AppContext.BaseDirectory} directory", EventLogEntryType.Information, eventId++);

                initTimer();

                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception err)
            {
                eventLog.WriteEntry(err.ToString(), EventLogEntryType.Error, eventId++);

                this.Stop();
            }
        }

        private void initTimer()
        {
            timer = new Timer();
#if DEBUG
            timer.Interval = 10 * 1000;
#else
            timer.Interval = 60 * 60 * 1000;
#endif
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        private void updateWallpaper()
        {
            var luma = WallpaperSetter.TimeToLuminance.LuminanceNow();
            eventLog.WriteEntry($"Current luma is {luma}", EventLogEntryType.Information, eventId++);

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
            eventLog.WriteEntry($"Setted up wallpaper with result: {response}", EventLogEntryType.Information, eventId++);
        }

        private bool ApproxEquals(double value, double referenceValue, double delta) =>
            Math.Abs(value - referenceValue) < delta;

        private void updateFileList()
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
                    if (!imagesList.Exists(i => i.Name == item.Name) )
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
            eventLog.WriteEntry("Setting wallpaper...", EventLogEntryType.Information, eventId++);
            updateFileList();
            updateWallpaper();
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("AmbientWallpapers service stopping", EventLogEntryType.Information, eventId++);
            timer.Stop();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnPause()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSE_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("AmbientWallpapers service pausing", EventLogEntryType.Information, eventId++);
            timer.Stop();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnContinue()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_CONTINUE_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("AmbientWallpapers service starting", EventLogEntryType.Information, eventId++);
            initTimer();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace AmbientWallpapers.App
{
    class TrayApplicationContext : ApplicationContext
    {
        private double lastLimePeriod = Properties.Settings.Default.WallpaperChangeTimeMin;
        private string imagesPath = Path.Combine(AppContext.BaseDirectory, "images\\");

        private NotifyIcon notifyIcon = new NotifyIcon();
        private ContextMenu contextMenu = new ContextMenu();

        private System.Threading.Timer iconUpdateTimer;
        private TimerCallback updateCallback;

        private WallpaperManager wallpaperManager;

        public TrayApplicationContext()
        {
            contextMenu.MenuItems.AddRange(
                new MenuItem[] {
                                new MenuItem($"Luminance: {WallpaperSetter.TimeToLuminance.LuminanceNow()}"),
                                new MenuItem("Update wallpaper", updateWallpaper),
                                new MenuItem("Exit", closeApp)
                });

            updateCallback = new TimerCallback(OnTimer);

            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetExecutingAssembly().Location
                );
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += updateWallpaper;

            notifyIcon.Text = "Ambient Wallpapers is starting";
            notifyIcon.Visible = true;

            notifyIcon.ShowBalloonTip(
                2000, 
                "Ambient Wallpapers",
                "Ambient Wallpapers is starting",
                ToolTipIcon.Info);
            
            wallpaperManager = new WallpaperManager(imagesPath);

            InitTimer();
        }

        private void InitTimer()
        {
            iconUpdateTimer = new System.Threading.Timer(
                updateCallback,
                notifyIcon,
                0,
                60 * 1000);
        }

        private void OnTimer(object obj)
        {
            notifyIcon.ContextMenu.MenuItems[0].Text = notifyIcon.Text =
                $"Luminance: {WallpaperSetter.TimeToLuminance.LuminanceNow()}";
            if (lastLimePeriod != Properties.Settings.Default.WallpaperChangeTimeMin)
            {
                wallpaperManager.InitTimer(Properties.Settings.Default.WallpaperChangeTimeMin);
            }
        }

        private void updateWallpaper(object sender, EventArgs e)
        {
            wallpaperManager.UpdateWallpaper();
            wallpaperManager.InitTimer(Properties.Settings.Default.WallpaperChangeTimeMin);
        }

        private void closeApp(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            wallpaperManager.Stop();
            iconUpdateTimer.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            base.Dispose(disposing);
        }
    }
}

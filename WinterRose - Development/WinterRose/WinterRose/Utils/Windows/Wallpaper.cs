using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace WinterRose;

public static partial class Windows
{
    /// <summary>
    /// A helper class for setting the wallpaper of the Windows desktop. This is an irreversible operation. Use <see cref="Get"/> to get the current wallpaper and make a backup.
    /// </summary>
    public static class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        /// <summary>
        /// Sets the wallpaper.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="style"></param>
        public static void Set(Uri uri, WallpaperStyle style)
        {
            Stream s = new WebClient().OpenRead(uri.ToString());

            Image img = Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == WallpaperStyle.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == WallpaperStyle.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == WallpaperStyle.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        
        /// <summary>
        /// Gets the current wallpaper.
        /// </summary>
        /// <returns></returns>
        public static Image Get()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
            string wallpaperPath = key.GetValue("Wallpaper").ToString();
            return Image.FromFile(wallpaperPath);
        }
    }
}

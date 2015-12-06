using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace WindPi.Helpers
{
    internal static class Debugging
    {
        public static string GetDisplayInfo()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            var result = "";
            result += $"Logical Resolution : {bounds.Width} x {bounds.Height}\n";
            result += $"Physical Resolution: {size.Width} x {size.Height}\n";
            result += $"Scale Factor:        {scaleFactor}";

            return result;
        }

        public static string GetAppVersion()
        {

            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}

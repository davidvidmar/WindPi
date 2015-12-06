using System.Diagnostics;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace WindyPi.Helpers
{
    internal class Debugging
    {
        public static void WriteDisplayInfo()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            Debug.WriteLine($"Logical Resolution : {bounds.Width} x {bounds.Height}");
            Debug.WriteLine($"Physical Resolution: {size.Width} x {size.Height}");
            Debug.WriteLine($"Scale Factor:        {scaleFactor}");
        }
    }
}

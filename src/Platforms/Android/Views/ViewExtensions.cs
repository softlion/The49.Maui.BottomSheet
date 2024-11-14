using AView = Android.Views.View;

namespace The49.Maui.BottomSheet;

internal static class ViewExtensions
{
    internal static Point GetLocationOnScreen(this AView view)
    {
        var location = new int[2];
        view.GetLocationOnScreen(location);
        var x = location[0];
        var y = location[1];

        return new Point(x, y);
    }
}

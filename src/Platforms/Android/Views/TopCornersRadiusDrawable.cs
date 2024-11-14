using Android.Graphics.Drawables;

namespace The49.Maui.BottomSheet;

/// <summary>
/// A drawable with non-square corners on top left and right only.
/// </summary>
internal class TopCornersRadiusDrawable: GradientDrawable
{
    internal void SetTopCornerRadius(int radius) => SetCornerRadii([radius, radius, radius, radius, 0, 0, 0, 0]);
}

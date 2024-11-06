using AView = Android.Views.View;
using Google.Android.Material.BottomSheet;

namespace The49.Maui.BottomSheet;

internal class BottomSheetCallback : BottomSheetBehavior.BottomSheetCallback
{
    public event EventHandler? StateChanged;

    public override void OnSlide(AView bottomSheet, float newState)
    {}

    public override void OnStateChanged(AView view, int newState)
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

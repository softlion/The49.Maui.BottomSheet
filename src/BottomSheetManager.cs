namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    internal static void Show(BottomSheet sheet, bool animated, bool aboveEverything)
    {
        PlatformShow(sheet, animated, aboveEverything);
    }
}

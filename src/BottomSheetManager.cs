namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    internal static void Show(IMauiContext mauiContext, BottomSheet sheet, bool animated, bool aboveEverything)
    {
        PlatformShow(mauiContext, sheet, animated, aboveEverything);
        //sheet.SizeChanged += OnSizeChanged;
    }

    // static void OnSizeChanged(object? sender, EventArgs e)
    // {
    //     if(sender != null)
    //         PlatformLayout((BottomSheet)sender);
    // }
}

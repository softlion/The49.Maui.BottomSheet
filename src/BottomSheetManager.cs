namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    internal static void Show(IMauiContext mauiContext, BottomSheet sheet, bool animated)
    {
        PlatformShow(mauiContext, sheet, animated);
        //sheet.SizeChanged += OnSizeChanged;
    }

    // static void OnSizeChanged(object? sender, EventArgs e)
    // {
    //     if(sender != null)
    //         PlatformLayout((BottomSheet)sender);
    // }
}

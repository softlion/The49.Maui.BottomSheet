namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    static void PlatformShow(IMauiContext mauiContext, BottomSheet sheet, bool animated)
    {
        var controller = new BottomSheetController(mauiContext, sheet);
        sheet.Controller = controller;
        controller.Show(animated);
    }
}
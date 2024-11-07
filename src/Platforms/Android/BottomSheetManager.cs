namespace The49.Maui.BottomSheet;

internal partial class BottomSheetManager
{
    static void PlatformShow(IMauiContext mauiContext, BottomSheet sheet, bool animated, bool aboveEverything)
    {
        var controller = new BottomSheetController(mauiContext, sheet, !aboveEverything);
        sheet.Controller = controller;
        controller.Show(animated);
    }
}
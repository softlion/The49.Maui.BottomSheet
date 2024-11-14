using Android.Content;
using Android.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace The49.Maui.BottomSheet;

/// <summary>
/// Init in 2 times:
/// 1) Create(): platform views are created
/// 2) the handler is initialized by BottomSheet, which automatically calls all mappers methods.
/// 3) PlatformShow(): the platform views are added to the visual tree
/// </summary>
internal partial class BottomSheetManager
{
    public static void Create(IMauiContext context, BottomSheet sheet, bool aboveEverything)
    {
        var controller = new BottomSheetController(context, !aboveEverything);
        sheet.Controller = controller;
        controller.CreateViews();
    }

    static void PlatformShow(BottomSheet sheet, bool animated, bool aboveEverything)
    {
        var controller = sheet.Controller;
        var platformView = (Android.Views.View)sheet.Handler!.PlatformView;

        controller.Show(Platform.CurrentActivity?.Window, platformView, animated, sheet);
    }
}
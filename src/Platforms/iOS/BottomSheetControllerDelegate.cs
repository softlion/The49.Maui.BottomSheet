using System.Runtime.Versioning;
using UIKit;

namespace The49.Maui.BottomSheet;

[SupportedOSPlatform("ios15.0")]
internal class BottomSheetControllerDelegate(BottomSheet sheet) : UISheetPresentationControllerDelegate
{
    public override void DidDismiss(UIPresentationController presentationController)
    {
        sheet.CachedDetents.Clear();
        sheet.NotifyDismissed();
    }

    public override void DidChangeSelectedDetentIdentifier(UISheetPresentationController sheetPresentationController)
    {
        ((BottomSheetHandler)sheet.Handler).UpdateSelectedDetent(sheet);
    }
}


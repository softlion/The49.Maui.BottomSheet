namespace The49.Maui.BottomSheet;

public partial class BottomSheetHandler
{
    partial void PlatformUpdateCornerRadius(BottomSheet view) => view.Controller.UpdateCornerRadius(view.CornerRadius);
    private void PlatformUpdateBackground(BottomSheetHandler _, BottomSheet view) => view.Controller.UpdateBackground();
    private void PlatformUpdateHasHandle(BottomSheet view) => view.Controller.UpdateHasHandle(view.HasHandle);

    partial void Dismiss(BottomSheet view, object request)
    {
        view?.CachedDetents.Clear();
        view?.Controller?.DismissViewController((bool)request, view.NotifyDismissed);
    }

    partial void PlatformMapSelectedDetent(BottomSheet view)
    {
        if (OperatingSystem.IsIOSVersionAtLeast(15))
            view.Controller.UpdateSelectedIdentifierFromDetent();
    }

    partial void PlatformUpdateSelectedDetent(BottomSheet view)
    {
        if (OperatingSystem.IsIOSVersionAtLeast(15))
            view.Controller.UpdateSelectedDetent();
    }
}
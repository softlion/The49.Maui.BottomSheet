namespace The49.Maui.BottomSheet;

public partial class BottomSheetHandler
{
    public static void PlatformUpdateBackground(BottomSheetHandler handler, BottomSheet sheet) => sheet.Controller.UpdateBackgroundAndCorners(sheet.CornerRadius, sheet.BackgroundBrush);

    partial void Dismiss(BottomSheet view, object request) => view?.Controller?.Dismiss((bool)request);

    void PlatformUpdateHandleColor(BottomSheet view) => view.Controller.UpdateHandleColor(view.HandleColor);
    //partial void PlatformUpdateSelectedDetent(BottomSheet view) => view.Controller.UpdateSelectedDetent(view);
    partial void PlatformMapSelectedDetent(BottomSheet view) => view.Controller.UpdateStateFromDetent(view.SelectedDetent);
    partial void PlatformUpdateHasBackdrop(BottomSheet view) => view.Controller.UpdateHasBackdrop(view.HasBackdrop);
    partial void PlatformUpdateCornerRadius(BottomSheet view) => view.Controller.UpdateBackgroundAndCorners(view.CornerRadius, view.BackgroundBrush);
    private void PlatformUpdateHasHandle(BottomSheet view) => view.Controller.UpdateHasHandle(view.HasHandle);
}
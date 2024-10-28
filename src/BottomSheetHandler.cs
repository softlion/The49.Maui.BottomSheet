using Microsoft.Maui.Handlers;

namespace The49.Maui.BottomSheet;

public partial class BottomSheetHandler : ContentViewHandler
{
    public static IPropertyMapper<BottomSheet, BottomSheetHandler> BottomSheetMapper =
        new PropertyMapper<BottomSheet, BottomSheetHandler>(Mapper)
        {
            [nameof(BottomSheet.HandleColor)] = MapHandleColor,
            [nameof(BottomSheet.HasBackdrop)] = MapHasBackdrop,
            [nameof(BottomSheet.SelectedDetent)] = MapSelectedDetent,
            [nameof(BottomSheet.CornerRadius)] = MapCornerRadius,
        };

    public static CommandMapper<BottomSheet, BottomSheetHandler> BottomSheetCommandMapper =
        new(CommandMapper)
        {
            [nameof(BottomSheet.DismissAsync)] = MapDismiss,
        };

    public BottomSheetHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? BottomSheetMapper, commandMapper ?? BottomSheetCommandMapper) {}
    public BottomSheetHandler(IPropertyMapper? mapper) : base(mapper ?? BottomSheetMapper, BottomSheetCommandMapper) {}
    public BottomSheetHandler() : base(BottomSheetMapper, BottomSheetCommandMapper) {}
    
    public override void UpdateValue(string property)
    {
        base.UpdateValue(property);
        if (property is nameof(IContentView.Background) or nameof(VisualElement.BackgroundColor))
            MapBackground(this, VirtualView);
    }

    static void MapCornerRadius(BottomSheetHandler handler, BottomSheet sheet)
    {
        handler.PlatformUpdateCornerRadius(sheet);
    }

    static void MapHasBackdrop(BottomSheetHandler handler, BottomSheet sheet)
    {
        handler.PlatformUpdateHasBackdrop(sheet);
    }

    static void MapHandleColor(BottomSheetHandler handler, BottomSheet sheet)
    {
        //Not supported on iOS
#if ANDROID
        handler.PlatformUpdateHandleColor(sheet);
#endif
    }


    static void MapDismiss(BottomSheetHandler handler, BottomSheet view, object? request) 
        => handler.Dismiss(view, request ?? false);

    public static void MapSelectedDetent(BottomSheetHandler handler, BottomSheet view) 
        => handler.PlatformMapSelectedDetent(view);

    internal void UpdateSelectedDetent(BottomSheet view) 
        => PlatformUpdateSelectedDetent(view);

    partial void PlatformMapSelectedDetent(BottomSheet view);
    partial void PlatformUpdateHasBackdrop(BottomSheet view);
    partial void PlatformUpdateSelectedDetent(BottomSheet view);
    partial void PlatformUpdateCornerRadius(BottomSheet view);
    partial void Dismiss(BottomSheet view, object request);

}
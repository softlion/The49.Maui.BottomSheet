using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using Android.Content;

namespace The49.Maui.BottomSheet;

internal class BottomSheetContainer : FrameLayout
{
    public AView ContentView { get; }
    public BottomSheetBackdrop Backdrop { get; }

    public static BottomSheetContainer Create(Context context, AView contentView)
    {
        var bsc = new BottomSheetContainer(context, contentView);
        bsc.Init();
        return bsc;
    }
    
    private BottomSheetContainer(Context context, AView contentView) : base(context)
    {
        ContentView = contentView;
        Backdrop = BottomSheetBackdrop.Create(context);
    }

    void Init()
    {
        AddView(Backdrop);
        AddView(ContentView);
    }

    internal void SetBackdropVisibility(bool hasBackdrop)
    {
        Backdrop.Visibility = hasBackdrop ? ViewStates.Visible : ViewStates.Gone;
    }
}

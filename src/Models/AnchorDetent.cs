namespace The49.Maui.BottomSheet;

#if ANDROID
using AView = Android.Views.View;
#elif IOS
using UIKit;
#endif

public class AnchorDetent: Detent
{
    public static readonly BindableProperty AnchorProperty = BindableProperty.Create(nameof(Anchor), typeof(VisualElement), typeof(AnchorDetent));

    public VisualElement Anchor
    {
        get => (VisualElement)GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }
    
    double _height;
    
    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        if (Anchor == null)
            throw new Exception("Could not update Detent height: Anchor is not set");

#if ANDROID
        var p = ((AView)Anchor.Handler.PlatformView).GetLocationOnScreen();
        var r = ((AView)page.Handler.PlatformView).GetLocationOnScreen();

        var offset = p - r;
        _height = offset.Height / DeviceDisplay.MainDisplayInfo.Density;

#elif IOS
        var pageView = (UIView)page.Handler.PlatformView;
        var targetView = (UIView)Anchor.Handler.PlatformView;
        var targetOrigin = targetView.Superview.ConvertPointToView(targetView.Frame.Location, pageView);

        _height = targetOrigin.Y;
#endif
        
        return _height;
    }
}

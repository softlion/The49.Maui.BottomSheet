using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;

namespace The49.Maui.BottomSheet;

public class BottomSheetViewController : UIViewController
{
    readonly UIWindow _window;
    readonly IMauiContext _windowMauiContext;
    readonly BottomSheet _sheet;
    NSObject? _keyboardDidHideObserver;

    public BottomSheetViewController(IMauiContext windowMauiContext, BottomSheet sheet, UIWindow window)
    {
        _windowMauiContext = windowMauiContext;
        _sheet = sheet;
        this._window = window;
        SheetPresentationController.Delegate = new BottomSheetControllerDelegate(_sheet);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        var container = _sheet.ToPlatform(_windowMauiContext);

        var cv = new BottomSheetContainer(_sheet, container, _window);

        View.AddSubview(cv);

        cv.TranslatesAutoresizingMaskIntoConstraints = false;

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            cv.TopAnchor.ConstraintEqualTo(View.TopAnchor),
            cv.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            cv.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            cv.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
        });

        UpdateBackground();
        _sheet.NotifyShowing();

        if (_keyboardDidHideObserver is null)
        {
            _keyboardDidHideObserver = UIKeyboard.Notifications.ObserveDidHide(KeyboardDidHide);
        }
    }

    void KeyboardDidHide(object? sender, UIKeyboardEventArgs e)
    {
        Layout();
    }

    public void Layout()
    {
        _sheet.CachedDetents.Clear();
        SheetPresentationController.InvalidateDetents();
    }
    internal void UpdateBackground()
    {
        if (_sheet?.BackgroundBrush != null)
        {
            Paint paint = _sheet.BackgroundBrush;
            View.BackgroundColor = paint.ToColor().ToPlatform();
        }
        else
            View.BackgroundColor = UIColor.SystemBackground;
    }
    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        Layout();
    }

    internal static UISheetPresentationControllerDetentIdentifier GetIdentifierForDetent(Detent d)
    {
        return d switch
        {
            FullscreenDetent => UISheetPresentationControllerDetentIdentifier.Large,
            RatioDetent { Ratio: .5f } => UISheetPresentationControllerDetentIdentifier.Medium,
            _ => UISheetPresentationControllerDetentIdentifier.Unknown
        };
    }

    internal void UpdateSelectedIdentifierFromDetent()
    {
        if (_sheet.SelectedDetent is null)
        {
            return;
        }
        SheetPresentationController.AnimateChanges(() =>
        {
            SheetPresentationController.SelectedDetentIdentifier = GetIdentifierForDetent(_sheet.SelectedDetent);
        });
    }

    internal Detent GetSelectedDetent()
    {
        var detents = _sheet.GetEnabledDetents();
        return SheetPresentationController.SelectedDetentIdentifier switch
        {
            UISheetPresentationControllerDetentIdentifier.Medium => detents.FirstOrDefault(d => d is RatioDetent { Ratio: .5f }),
            UISheetPresentationControllerDetentIdentifier.Large => detents.FirstOrDefault(d => d is FullscreenDetent),
            _ => null,
        };
    }

    internal void UpdateSelectedDetent()
    {
        _sheet.SelectedDetent = GetSelectedDetent();
    }

    internal void UpdateCornerRadius(double cornerRadius)
    {
        if (!OperatingSystem.IsIOSVersionAtLeast(15))
        {
            return;
        }
        SheetPresentationController.PreferredCornerRadius = (NFloat)cornerRadius;
    }

    public void UpdateHasHandle(bool hasHandle)
    {
        SheetPresentationController.PrefersGrabberVisible = hasHandle;
    }
}


#define USE_MATERIAL3

using Android.Views;
using Android.Content;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using Google.Android.Material.BottomSheet;

namespace The49.Maui.BottomSheet;

internal class BottomSheetContainerView : CoordinatorLayout
{
    /// <summary>
    /// Optional clickable backdrop
    /// </summary>
    public BottomSheetBackdropView Backdrop { get; private init; }

    /// <summary>
    /// Container for custom content
    /// </summary>
    public FrameLayout BottomSheetView { get; private init; }

    public static BottomSheetContainerView Create(Context context)
    {
#if USE_MATERIAL3
        var frameStyle = Resource.Style.Widget_Material3_BottomSheet_Modal;
#else
        var frameStyle = Resource.Style.Widget_MaterialComponents_BottomSheet_Modal;
#endif

        var bsc = new BottomSheetContainerView(context)
        {
            Focusable = false,
            ImportantForAccessibility = ImportantForAccessibility.No,
            SoundEffectsEnabled = false,
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
            Backdrop = BottomSheetBackdropView.Create(context),
            BottomSheetView = new FrameLayout(context, null, 0, frameStyle)
            {
                LayoutParameters =  new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)(GravityFlags.CenterHorizontal | GravityFlags.Bottom),
                    Behavior = new BottomSheetBehavior(),
                },
                ClipToOutline = true,
                OutlineProvider = ViewOutlineProvider.Background,
            }
        };

        bsc.AddView(bsc.Backdrop, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        bsc.AddView(bsc.BottomSheetView);
        ViewCompat.SetWindowInsetsAnimationCallback(bsc.BottomSheetView, new BottomSheetInsetsAnimationCallback(bsc));

        return bsc;
    }
    
    private BottomSheetContainerView(Context context) : base(context)
    {
    }

    internal void SetBackdropVisibility(bool hasBackdrop)
    {
        Backdrop.Visibility = hasBackdrop ? ViewStates.Visible : ViewStates.Gone;
    }

    private WindowInsetsCompat GetWindowInsets()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(23) && RootWindowInsets is not null)
            return WindowInsetsCompat.ToWindowInsetsCompat(RootWindowInsets);

        return WindowInsetsCompat.Consumed;
    }

    #region Listeners and callbacks
    class BottomSheetInsetsAnimationCallback(BottomSheetContainerView ownerView) : WindowInsetsAnimationCompat.Callback(DispatchModeStop)
    {
        int startHeight;
        int endHeight;

        public override void OnPrepare(WindowInsetsAnimationCompat animation)
        {
            startHeight = ownerView.GetWindowInsets().GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
            base.OnPrepare(animation);
        }

        public override WindowInsetsAnimationCompat.BoundsCompat OnStart(WindowInsetsAnimationCompat animation, WindowInsetsAnimationCompat.BoundsCompat bounds)
        {
            endHeight = ownerView.GetWindowInsets().GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
            ownerView.BottomSheetView.TranslationY = endHeight - startHeight;
            return bounds;
        }

        public override WindowInsetsCompat OnProgress(WindowInsetsCompat insets, IList<WindowInsetsAnimationCompat> runningAnimations)
        {
            var imeAnimation = runningAnimations.FirstOrDefault(animation => (animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0);
            if (imeAnimation != null)
                ownerView.BottomSheetView.TranslationY = (endHeight - startHeight) * (1 - imeAnimation.InterpolatedFraction);

            return insets;
        }
    }
    #endregion
}

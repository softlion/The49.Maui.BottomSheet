#define USE_MATERIAL3

using Android.Views;
using Microsoft.Maui.Platform;
using Google.Android.Material.BottomSheet;
using Android.Widget;
using AView = Android.Views.View;
using AWindow = Android.Views.Window;
using AndroidX.Core.View;
using AndroidX.AppCompat.App;
using Google.Android.Material.Internal;
using Google.Android.Material.Color;
using Android.Graphics.Drawables;
using Android.Content;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;
using Insets = AndroidX.Core.Graphics.Insets;
using Paint = Microsoft.Maui.Graphics.Paint;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace The49.Maui.BottomSheet;

public class BottomSheetController
{
    #region Listeners and callbacks
    class EdgeToEdgeCallback : BottomSheetBehavior.BottomSheetCallback
    {
        private BottomSheetController controller;
        WindowInsetsCompat insetsCompat;

        AWindow window;
        bool isStatusBarLight;

        public EdgeToEdgeCallback(BottomSheetController controller, WindowInsetsCompat insetsCompat)
        {
            this.controller = controller;
            this.insetsCompat = insetsCompat;
            SetPaddingForPosition(this.controller.bottomSheetFrameView);
        }

        public override void OnStateChanged(AView bottomSheet, int p1)
        {
            SetPaddingForPosition(bottomSheet);
        }

        public override void OnSlide(AView bottomSheet, float newState)
        {
            SetPaddingForPosition(bottomSheet);
        }

        public void SetWindow(AWindow window)
        {
            if (this.window == window)
            {
                return;
            }
            this.window = window;
            if (window != null)
            {
                WindowInsetsControllerCompat insetsController = WindowCompat.GetInsetsController(window, window.DecorView);
                isStatusBarLight = insetsController.AppearanceLightStatusBars;
            }
        }

        int TopInset
        {
            get
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    return insetsCompat.GetInsetsIgnoringVisibility(Android.Views.WindowInsets.Type.SystemBars()).Top;
                }
#pragma warning disable CS0618
                return insetsCompat.StableInsetTop;
#pragma warning restore CS0618
            }
        }

        void SetPaddingForPosition(AView bottomSheet)
        {
            var keyboardHeight = insetsCompat.GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
            if (bottomSheet.Top < TopInset)
            {
                // If the bottomsheet is light, we should set light status bar so the icons are visible
                // since the bottomsheet is now under the status bar.
                if (window != null)
                {
                    EdgeToEdgeUtils.SetLightStatusBar(
                        window, !controller.isBackgroundLight.HasValue ? isStatusBarLight : controller.isBackgroundLight.Value);
                }
                // Smooth transition into status bar when drawing edge to edge.
                bottomSheet.SetPadding(
                    bottomSheet.PaddingLeft,
                    TopInset - bottomSheet.Top,
                    bottomSheet.PaddingRight,
                    keyboardHeight);
            }
            else if (bottomSheet.Top != 0)
            {
                // Reset the status bar icons to the original color because the bottomsheet is not under the
                // status bar.
                if (window != null)
                {
                    EdgeToEdgeUtils.SetLightStatusBar(window, isStatusBarLight);
                }
                bottomSheet.SetPadding(
                    bottomSheet.PaddingLeft,
                    0,
                    bottomSheet.PaddingRight,
                    keyboardHeight);
            }
        }
    }
    class EdgeToEdgeListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        BottomSheetController _controller;
        EdgeToEdgeCallback _edgeToEdgeCallback;
        public EdgeToEdgeListener(BottomSheetController controller)
        {
            _controller = controller;
        }
        public WindowInsetsCompat OnApplyWindowInsets(AView v, WindowInsetsCompat insets)
        {
            if (_edgeToEdgeCallback is not null)
            {
                _controller.Behavior.RemoveBottomSheetCallback(_edgeToEdgeCallback);
            }

            if (insets != null)
            {
                _edgeToEdgeCallback = new EdgeToEdgeCallback(_controller, insets);
                _edgeToEdgeCallback.SetWindow(((AppCompatActivity)_controller.mauiContext.Context).Window);
                _controller.Behavior.AddBottomSheetCallback(_edgeToEdgeCallback);
                _controller.CalculateHeights(_controller.GetAvailableHeight());
                _controller.ResizeVirtualView();
                _controller.Layout();
            }


            return ViewCompat.OnApplyWindowInsets(v, insets);
        }
    }

    class BottomSheetInsetsAnimationCallback : WindowInsetsAnimationCompat.Callback
    {
        readonly BottomSheetController _controller;
        int _startHeight;
        int _endHeight;

        public BottomSheetInsetsAnimationCallback(BottomSheetController controller) : base(DispatchModeStop)
        {
            _controller = controller;
        }

        public override WindowInsetsAnimationCompat.BoundsCompat OnStart(WindowInsetsAnimationCompat animation, WindowInsetsAnimationCompat.BoundsCompat bounds)
        {
            _endHeight = _controller.WindowInsets.GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
            _controller.bottomSheetFrameView.TranslationY = _endHeight - _startHeight;
            return bounds;
        }

        public override void OnPrepare(WindowInsetsAnimationCompat animation)
        {
            _startHeight = _controller.WindowInsets.GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
            base.OnPrepare(animation);
        }

        public override WindowInsetsCompat OnProgress(WindowInsetsCompat insets, IList<WindowInsetsAnimationCompat> runningAnimations)
        {
            WindowInsetsAnimationCompat imeAnimation = null;
            foreach (var animation in runningAnimations)
            {
                if ((animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0)
                {
                    imeAnimation = animation;
                    break;
                }
            }
            if (imeAnimation != null)
            {
                _controller.bottomSheetFrameView.TranslationY = (_endHeight - _startHeight) * (1 - imeAnimation.InterpolatedFraction);
            }
            return insets;
        }
    }
    #endregion
    
    /// <summary>
    /// If the sheet is to be shown "inside" the page, i.e. under any flyout page,
    /// and vertically above the navigation bars, tab bars if any.
    /// In such a case, we don't use the "global" window's <see cref="StayOnFrontView"/>, but
    /// add one as a sibling of the <see cref="AppBarLayout"/>.
    /// </summary>
    private readonly bool showNextToAppBarLayout;
    private readonly IMauiContext mauiContext;
    private readonly BottomSheet sheet;

    bool isDuringShowingAnimation;
    bool? isBackgroundLight;
    int BottomInset => UseNavigationBarArea ? 0 : Insets.Bottom;

    StayOnFrontView stayOnFrontView;
    BottomSheetContainer bottomSheetContainerView;
    ViewGroup bottomSheetFrameView;
    BottomSheetDragHandleView handleView;
    BottomSheetBehavior behavior;

    readonly Dictionary<Detent, int> states = new();
    Dictionary<Detent, double> heights;


    public ViewGroup BottomSheetFrame => bottomSheetFrameView;
    public BottomSheetBehavior Behavior => behavior;
    public bool UseNavigationBarArea { get; set; } = false;

    public BottomSheetController(IMauiContext windowMauiContext, BottomSheet sheet, bool showNextToAppBarLayout)
    {
        mauiContext = windowMauiContext;
        this.sheet = sheet;
        this.showNextToAppBarLayout = showNextToAppBarLayout;
    }

    internal void CalculateHeights(double maxSheetHeight)
    {
        heights = sheet.GetEnabledDetents().ToDictionary(
            detent => detent,
            detent => detent.GetHeight(sheet, maxSheetHeight));
    }

    internal void CalculateStates()
    {
        var allHeights = heights.OrderByDescending(kv => kv.Value).ToList();

        states.Clear();

        if (allHeights.Count == 1)
        {
            states.Add(allHeights[0].Key, BottomSheetBehavior.StateCollapsed);
        }
        else if (allHeights.Count == 2)
        {
            states.Add(allHeights[0].Key, BottomSheetBehavior.StateExpanded);
            states.Add(allHeights[1].Key, BottomSheetBehavior.StateCollapsed);
        }
        else if (allHeights.Count == 3)
        {
            states.Add(allHeights[0].Key, BottomSheetBehavior.StateExpanded);
            states.Add(allHeights[1].Key, BottomSheetBehavior.StateHalfExpanded);
            states.Add(allHeights[2].Key, BottomSheetBehavior.StateCollapsed);
        }
    }

    internal int GetStateForDetent(Detent detent)
    {
        var state = -1;
        if (detent is not null)
            states.TryGetValue(detent, out state);
        
        if (state is -1)
            state = BottomSheetBehavior.StateCollapsed;
        if (state is BottomSheetBehavior.StateCollapsed && (behavior.SkipCollapsed || !sheet.IsCancelable))
            state = BottomSheetBehavior.StateExpanded;

        return state;
    }
    
    internal Detent GetDetentForState(int state) 
        => states.FirstOrDefault(kv => kv.Value == state).Key;

    public void Dismiss(bool animated)
    {
        behavior.Hideable = true;

        if (animated)
        {
            bottomSheetContainerView?.Backdrop.AnimateOut();
            behavior.State = BottomSheetBehavior.StateHidden;
        }
        else
        {
            WhenDismissed();
            sheet.NotifyDismissed();
        }
    }

    void WhenDismissed()
    {
        bottomSheetFrameView.LayoutChange -= OnLayoutChange;
        bottomSheetContainerView.RemoveFromParent();
    }

    void Dispose()
    {
        WhenDismissed();
    }

    public void Layout()
    {
        LayoutDetents(heights, GetAvailableHeight());
    }

    internal void UpdateBackground()
    {
        Paint paint = sheet.BackgroundBrush;
        if (bottomSheetFrameView is not null)
        {
            if (sheet.CornerRadius != -1)
            {
                SheetRadiusDrawable drawable;
                if (bottomSheetFrameView.Background is not SheetRadiusDrawable)
                {
                    drawable = new SheetRadiusDrawable();
                    bottomSheetFrameView.Background = drawable;
                }
                else
                {
                    drawable = (SheetRadiusDrawable)bottomSheetFrameView.Background;
                }
                drawable.SetCornerRadius(bottomSheetFrameView.Context.ToPixels(sheet.CornerRadius));
            }
            if (paint is not null)
            {
                var platformColor = paint.ToColor().ToPlatform();
                if (bottomSheetFrameView.Background is SheetRadiusDrawable sheetDrawable)
                {
                    sheetDrawable.SetColor(platformColor);
                }
                else
                {
                    bottomSheetFrameView.SetBackgroundColor(platformColor);
                }
            }
        }
        // Try to find the background color to automatically change the status bar icons so they will
        // still be visible when the bottomsheet slides underneath the status bar.
        var backgroundTint = ViewCompat.GetBackgroundTintList(bottomSheetFrameView);

        if (backgroundTint != null)
        {
            // First check for a tint
            isBackgroundLight = MaterialColors.IsColorLight(backgroundTint.DefaultColor);
        }
        else if (bottomSheetFrameView.Background is ColorDrawable)
        {
            // Then check for the background color
            isBackgroundLight = MaterialColors.IsColorLight(((ColorDrawable)bottomSheetFrameView.Background).Color);
        }
        else
        {
            // Otherwise don't change the status bar color
            isBackgroundLight = null;
        }
    }

    public void UpdateHandleColor()
    {
        if (handleView is null)
        {
            return;
        }
        if (sheet.HandleColor is not null)
        {
            handleView.SetColorFilter(sheet.HandleColor.ToPlatform());
        }
    }

    private void EnsureStayOnFrontView(Context context)
    {
        if (stayOnFrontView is null || !stayOnFrontView.IsAttachedToWindow)
        {
            stayOnFrontView = new StayOnFrontView(context);
            var window = ((AppCompatActivity)context).Window;
            var parentView = window?.DecorView as ViewGroup;
            
            if (showNextToAppBarLayout)
            {
                var appBarLayout = parentView.GetFirstChildOfType<AppBarLayout>();
                if(appBarLayout?.Parent is ViewGroup vg)
                    parentView = vg;
            }

            parentView.AddView(stayOnFrontView);
        }
    }

    void EnsureWindowContainer()
    {
        EnsureStayOnFrontView(mauiContext.Context);
        if (bottomSheetContainerView is null)
        {
            //var container = (FrameLayout)AView.Inflate(_mauiContext.Context, Resource.Layout.the49_maui_bottom_sheet_design, null);

            var matchParent = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            var touchOutsideView = new AView(mauiContext.Context)
            {
                LayoutParameters =  matchParent, 
                Focusable = false, 
                ImportantForAccessibility = ImportantForAccessibility.No,
                SoundEffectsEnabled = false
            };
            
#if USE_MATERIAL3
            var frameStyle = Resource.Style.Widget_Material3_BottomSheet_Modal;
#else
            var frameStyle = Resource.Style.Widget_MaterialComponents_BottomSheet_Modal;
#endif
            
            var bottomSheet = new FrameLayout(mauiContext.Context, null, 0, frameStyle)
            {
                LayoutParameters =  new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)(GravityFlags.CenterHorizontal | GravityFlags.Top),
                    Behavior = new BottomSheetBehavior(),
                },
            };
            

            var coordinatorLayout = new CoordinatorLayout(mauiContext.Context) { LayoutParameters =  matchParent };
            coordinatorLayout.AddView(touchOutsideView);
            coordinatorLayout.AddView(bottomSheet);
            var container = new FrameLayout(mauiContext.Context) { LayoutParameters =  matchParent };
            container.AddView(coordinatorLayout);
            behavior = BottomSheetBehavior.From(bottomSheet);
            
            
            bottomSheetContainerView = BottomSheetContainer.Create(mauiContext.Context, container);
            bottomSheetContainerView.Backdrop.Click += BackdropClicked;

            bottomSheetFrameView = bottomSheet;
            bottomSheetFrameView.OutlineProvider = ViewOutlineProvider.Background;
            bottomSheetFrameView.ClipToOutline = true;

            ViewCompat.SetOnApplyWindowInsetsListener(bottomSheetContainerView, new EdgeToEdgeListener(this));
            ViewCompat.SetWindowInsetsAnimationCallback(bottomSheetFrameView, new BottomSheetInsetsAnimationCallback(this));

            var callback = new BottomSheetCallback();
            callback.StateChanged += Callback_StateChanged;
            behavior.AddBottomSheetCallback(callback);
        }
    }

    void BackdropClicked(object? sender, EventArgs e)
    {
        if (sheet.IsCancelable)
            Dismiss(true);
    }

    WindowInsetsCompat WindowInsets
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(23) && bottomSheetContainerView.RootWindowInsets is not null)
            {
                return WindowInsetsCompat.ToWindowInsetsCompat(bottomSheetContainerView.RootWindowInsets);
            }

            return WindowInsetsCompat.Consumed;
        }
    }

    Insets Insets
    {
        get
        {
            var insets = WindowInsets;
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                return insets.GetInsetsIgnoringVisibility(Android.Views.WindowInsets.Type.SystemBars());
            }
#pragma warning disable CS0618
            return Insets.Of(insets.StableInsetLeft, insets.StableInsetTop, insets.StableInsetRight, insets.StableInsetBottom);
#pragma warning restore CS0618
        }
    }

    int KeyboardHeight => WindowInsets.GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
    int TopInset => Insets.Top;

    public double GetAvailableHeight()
    {
        var density = DeviceDisplay.MainDisplayInfo.Density;

        return (bottomSheetContainerView.Height - TopInset - BottomInset - KeyboardHeight) / density;
    }

    internal void LayoutDetents(IDictionary<Detent, double> heights, double maxSheetHeight)
    {
        // Android supports the following detents:
        // - expanded (top of screen - offset)
        // - half expanded (using ratio of expanded - peekHeight)
        // - collapsed (using peekHeight)

        var sortedHeights = heights
            .OrderByDescending(i => i.Value)
            .ToList();
        var density = DeviceDisplay.MainDisplayInfo.Density;

        var keyboardHeight = KeyboardHeight;

        var top = sortedHeights[0].Value;

        // Configure the sheet to handle up to 3 detents

        if (sortedHeights.Count == 1)
        { 
            // Only way to have one detent on Android is to use fitToContent. Use that
            behavior.FitToContents = true;
            behavior.SkipCollapsed = true;
        }
        else if (sortedHeights.Count == 2)
        { 
            // We can handle a second detent by adding a collapsed state. Use peek height
            behavior.FitToContents = true;
            behavior.SkipCollapsed = false;

            var bottom = sortedHeights[1].Value;

            behavior.PeekHeight = (int)(bottom * density) + BottomInset + keyboardHeight;
        }
        else if (sortedHeights.Count == 3)
        { // 3 detents can be done using the peek height AND disabling fitToContent
          // Doing so uses a property called halfExpandedRatio, giving us
          // Expanded: Use ExpandedOffset to offset from the top
          // HalfExpanded: Use HalfExpandedRatio
          // Collapsed: Use PeekHeight

            behavior.FitToContents = false;
            behavior.SkipCollapsed = false;

            var midway = sortedHeights[1].Value;
            var bottom = sortedHeights[2].Value;

            // Set the top detent by offsetting the requested height from the maxHeight
            var topOffset = (maxSheetHeight - top) * density;
            behavior.ExpandedOffset = Math.Max(0, (int)topOffset);

            // Set the midway detent by calculating the ratio using the top detent info
            var ratio = ((midway * density) + keyboardHeight + BottomInset) / bottomSheetFrameView.LayoutParameters.Height;
            behavior.HalfExpandedRatio = (float)ratio;

            // Set the bottom detent using the peekHeight
            behavior.PeekHeight = (int)(bottom * density) + BottomInset + keyboardHeight;
        }
    }

    double CalculateTallestDetent(double heightConstraint)
    {
        if (heights is null)
            CalculateHeights(heightConstraint);
        
        return heights.Values.Max();
    }

    void ResizeVirtualView()
    {
        var pv = (ContentViewGroup)sheet.Handler?.PlatformView;
        var maxHeight = GetAvailableHeight();
        var height = CalculateTallestDetent(maxHeight);

        var density = DeviceDisplay.MainDisplayInfo.Density;

        var platformHeight = (int)Math.Round(height * density);

        var newHeight = platformHeight + BottomInset + KeyboardHeight;
        if (height == maxHeight)
            newHeight += TopInset;

        var sheetChanged = false;

        if (pv.LayoutParameters is not FrameLayout.LayoutParams { Width: ViewGroup.LayoutParams.MatchParent } lp || lp.Height != platformHeight)
        {
            pv.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, platformHeight);
            sheetChanged = true;
        }

        var bottomSheetLayoutParams = bottomSheetFrameView.LayoutParameters;
        if (bottomSheetLayoutParams.Height != newHeight)
        {
            bottomSheetLayoutParams.Height = newHeight;
            sheetChanged = true;
        }

        var newBounds = new Rect(0, 0, bottomSheetFrameView.Width / density, height);
        if (sheet.Bounds != newBounds)
            sheetChanged = true;

        if(sheetChanged)
            sheet.Arrange(newBounds);
    }

    public void Show(bool animated)
    {
        isDuringShowingAnimation = true;

        EnsureWindowContainer();
        stayOnFrontView.AddView(bottomSheetContainerView);

        // The Android view for the page could already have a ContainerView as a parent if it was shown as a bottom sheet before
        //bottomSheetFrameView.RemoveAllViews();
        //(sheet.Handler?.PlatformView as ContentViewGroup)?.RemoveFromParent();
        
        var c = new FrameLayout(mauiContext.Context);
        var sheetView = sheet.ToPlatform(mauiContext); 
        c.AddView(sheetView);
        if (sheet.HasHandle)
        {
            handleView = new BottomSheetDragHandleView(mauiContext.Context);
            c.AddView(handleView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }
        bottomSheetFrameView.AddView(c);

        UpdateHasBackdrop();
        UpdateHandleColor();
        
        if (animated)
        {
            behavior.Hideable = true;
            behavior.State = BottomSheetBehavior.StateHidden;
            bottomSheetContainerView?.Backdrop.AnimateIn();
        }

        sheet.Dispatcher.Dispatch(() =>
        {
            ResizeVirtualView();

            CalculateHeights(GetAvailableHeight());
            CalculateStates();
            Layout();
            UpdateBackground();

            behavior.State = GetStateForDetent(sheet.SelectedDetent);

            sheetView.LayoutChange += OnLayoutChange;
            sheet.NotifyShowing();
        });
    }

    void OnLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
    {
        sheet.Dispatcher.Dispatch(() =>
        {
            CalculateHeights(GetAvailableHeight());
            CalculateStates();
            ResizeVirtualView();
            Layout();
        });
    }

    void Callback_StateChanged(object? sender, EventArgs e)
    {
        if (isDuringShowingAnimation 
            && behavior.State is BottomSheetBehavior.StateCollapsed or BottomSheetBehavior.StateHalfExpanded or BottomSheetBehavior.StateExpanded or BottomSheetBehavior.StateHidden)
        {
            isDuringShowingAnimation = false;

            if (behavior.State != BottomSheetBehavior.StateHidden)
            {
                behavior.Hideable = sheet.IsCancelable;
                sheet.NotifyShown();
            }
            else
            {
                behavior.State = GetStateForDetent(sheet.SelectedDetent);
                return;
            }
        }
        
        if (behavior.State is BottomSheetBehavior.StateHidden
            && bottomSheetContainerView.Parent is not null)
        {
            WhenDismissed();
            sheet.NotifyDismissed();
        }
        else
            UpdateSelectedDetent();
    }

    internal void UpdateSelectedDetent()
    {
        var detent = GetDetentForState(Behavior.State);
        if (detent is not null)
        {
            if (sheet.SelectedDetent == detent)
                sheet.SelectedDetent = null; //make sure to trigger a change
            sheet.SelectedDetent = detent;
        }
    }

    internal void UpdateStateFromDetent()
    {
        if (sheet.SelectedDetent is null || behavior is null)
            return;
        behavior.State = GetStateForDetent(sheet.SelectedDetent);
    }

    internal void UpdateHasBackdrop()
    {
        bottomSheetContainerView?.SetBackdropVisibility(sheet.HasBackdrop);
    }
}

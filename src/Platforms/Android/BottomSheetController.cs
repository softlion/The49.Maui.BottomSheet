using System.Reflection.Metadata;
using Android.App;
using Android.Views;
using Microsoft.Maui.Platform;
using Google.Android.Material.BottomSheet;
using Android.Widget;
using AView = Android.Views.View;
using AWindow = Android.Views.Window;
using AndroidX.Core.View;
using Google.Android.Material.Internal;
using Google.Android.Material.Color;
using Android.Graphics.Drawables;
using Google.Android.Material.AppBar;
using Color = Microsoft.Maui.Graphics.Color;
using Insets = AndroidX.Core.Graphics.Insets;
using Paint = Microsoft.Maui.Graphics.Paint;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace The49.Maui.BottomSheet;

public class BottomSheetController
{
    /// <summary>
    /// If the sheet is to be shown "inside" the page, i.e. under any flyout page,
    /// and vertically above the navigation bars, tab bars if any.
    /// In such a case, we add it as a sibling of the <see cref="AppBarLayout"/>.
    /// </summary>
    private readonly bool showNextToAppBarLayout;
    private readonly IMauiContext mauiContext;
    private double density;

    //TODO: remove the dependency on sheet. Do not store the value of sheet.
    private BottomSheet sheet;

    bool isShowInitializing;
    bool? isBackgroundLight;

    BottomSheetContainerView bottomSheetContainerView;
    BottomSheetDragHandleView handleView;
    BottomSheetBehavior behavior;

    readonly Dictionary<Detent, int> states = new();
    Dictionary<Detent, double> heights;
    private AWindow? window;

    public ViewGroup BottomSheetView => bottomSheetContainerView.BottomSheetView;
    public BottomSheetBehavior Behavior => behavior;

    /// <summary>
    /// TODO: move that in the maui view (BottomSheet)
    /// </summary>
    public bool UseNavigationBarArea { get; set; } = false;

    public BottomSheetController(IMauiContext mauiContext, bool showNextToAppBarLayout)
    {
        this.mauiContext = mauiContext;
        this.showNextToAppBarLayout = showNextToAppBarLayout;
    }

    /// <summary>
    /// The sheet's handler is null here
    /// </summary>
    public void CreateViews()
    {
        density = DeviceDisplay.MainDisplayInfo.Density;
     
        EnsureWindowContainer();
        
        handleView.Visibility = ViewStates.Gone;
    }

    private const int PlatformViewId = 245695;

    public void Show(AWindow? window, AView platformView, bool animated, BottomSheet sheet)
    {
        if(window?.DecorView is not ViewGroup parentView)
            return;

        this.sheet = sheet;
        this.window = window;

        platformView.Id = PlatformViewId;
        BottomSheetView.RemoveAllViews();
        BottomSheetView.AddView(platformView, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        BottomSheetView.AddView(handleView, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        
        if (showNextToAppBarLayout)
        {
            var firstAppBar = parentView.GetFirstChildOfType<AppBarLayout>();
            if (firstAppBar?.Parent is ViewGroup vg)
                parentView = vg;
        }

        //Start as hidden if animated. Otherwise, start in the default state.
        if (animated)
        {
            behavior.Hideable = true;
            behavior.State = BottomSheetBehavior.StateHidden;
        }


        //test
        //var windowManager = (IWindowManager?)Platform.CurrentActivity?.GetSystemService(Android.Content.Context.WindowService);
        //windowManager.AddView(bottomSheetContainerView, new WindowManagerLayoutParams());

        parentView.AddView(bottomSheetContainerView);
        BottomSheetView.Post(() => BottomSheetView.RequestLayout()); //Post required

        if (animated)
            bottomSheetContainerView.Backdrop.AnimateIn();
        
        isShowInitializing = true;
        sheet.NotifyShowing();

        bottomSheetContainerView.Post(() =>
        {
            Layout();
            //Post required before changing state
            behavior.State = GetStateForDetent(sheet.SelectedDetent); //Triggers OnStateChanged

            //TODO: reverse that dependency !
            platformView.LayoutChange += OnLayoutChange;
        });
    }

    void OnLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
    {
        Layout();
    }

    
    void Layout()
    {
        var maxHeight = GetAvailableHeight();
        if(maxHeight <= 0)
            return;
        
        CalculateHeights(maxHeight);
        CalculateStates();
        LayoutDetents();
        ResizeBottomSheetContentView();
    }
    
    public void Dismiss(bool animated)
    {
        isShowInitializing = false;

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

    private void WhenDismissed()
    {
        BottomSheetView.FindViewById(PlatformViewId)!.LayoutChange -= OnLayoutChange;

        //var windowManager = (IWindowManager?)Platform.CurrentActivity?.GetSystemService(Android.Content.Context.WindowService);
        //windowManager?.RemoveView(bottomSheetContainerView);
        bottomSheetContainerView.RemoveFromParent();
        BottomSheetView.RemoveAllViews();
    }

    private void CalculateHeights(double maxSheetHeight)
    {
        heights = sheet.GetEnabledDetents().ToDictionary(
            detent => detent,
            detent => detent.GetHeight(sheet, maxSheetHeight));
    }

    private void CalculateStates()
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

    private int GetStateForDetent(Detent? detent)
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
    
    private Detent GetDetentForState(int state) 
        => states.FirstOrDefault(kv => kv.Value == state).Key;


    void EnsureWindowContainer()
    {
        if (bottomSheetContainerView is not null) 
            return;
        
        handleView = new BottomSheetDragHandleView(mauiContext.Context);
            
        bottomSheetContainerView = BottomSheetContainerView.Create(mauiContext.Context);
        ViewCompat.SetOnApplyWindowInsetsListener(bottomSheetContainerView, new EdgeToEdgeListener(this));
        bottomSheetContainerView.Backdrop.Click += (_,_) =>
        {
            if (sheet.IsCancelable)
                Dismiss(true);
        };

        behavior = BottomSheetBehavior.From(bottomSheetContainerView.BottomSheetView);
        bottomSheetCallback = new BottomSheetCallback(OnStateChanged);
        behavior.AddBottomSheetCallback(bottomSheetCallback);
    }

    BottomSheetCallback bottomSheetCallback;

    void OnStateChanged(int newState)
    {
        if (isShowInitializing 
            && newState is BottomSheetBehavior.StateCollapsed or BottomSheetBehavior.StateHalfExpanded or BottomSheetBehavior.StateExpanded or BottomSheetBehavior.StateHidden)
        {
            isShowInitializing = false;

            if (newState != BottomSheetBehavior.StateHidden)
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
        
        if (newState is BottomSheetBehavior.StateHidden
            && bottomSheetContainerView.Parent is not null)
        {
            WhenDismissed();
            sheet.NotifyDismissed();
        }
        else
            UpdateSelectedDetent(sheet);
    }

    private WindowInsetsCompat GetWindowInsets()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(23) && bottomSheetContainerView.RootWindowInsets is not null)
            return WindowInsetsCompat.ToWindowInsetsCompat(bottomSheetContainerView.RootWindowInsets);

        return WindowInsetsCompat.Consumed;
    }

    int BottomInset() => UseNavigationBarArea ? 0 : GetInsets().Bottom;

    private Insets GetInsets()
    {
        var insets = GetWindowInsets();
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            return insets.GetInsetsIgnoringVisibility(Android.Views.WindowInsets.Type.SystemBars());
        
#pragma warning disable CS0618 // Type or member is obsolete
        return Insets.Of(insets.StableInsetLeft, insets.StableInsetTop, insets.StableInsetRight, insets.StableInsetBottom);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    int KeyboardHeight() => GetWindowInsets().GetInsets(WindowInsetsCompat.Type.Ime()).Bottom;
    int TopInset() => GetInsets().Top;

    /// <returns>Height in maui points</returns>
    public double GetAvailableHeight() 
        => (bottomSheetContainerView.Height - TopInset() - BottomInset() - KeyboardHeight()) / density;

    /// <summary>
    /// Sets behavior properties
    /// </summary>
    internal void LayoutDetents()
    {
        var maxSheetHeight = GetAvailableHeight();

        // Android supports the following detents:
        // - expanded (top of screen - offset)
        // - half expanded (using ratio of expanded - peekHeight)
        // - collapsed (using peekHeight)

        var sortedHeights = heights
            .OrderByDescending(i => i.Value)
            .ToList();

        var keyboardHeight = KeyboardHeight();

        var top = sortedHeights[0].Value;
        var bottomInset = BottomInset();

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
            behavior.PeekHeight = (int)(bottom * density) + bottomInset + keyboardHeight;
        }
        else if (sortedHeights.Count == 3)
        { 
            // 3 detents can be done using the peek height AND disabling fitToContent
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
            //var availableHeight = BottomSheetView.LayoutParameters.Height; //not computed yet
            var availableHeight = maxSheetHeight * density;
            var ratio = ((midway * density) + keyboardHeight + bottomInset) / availableHeight; 
            behavior.HalfExpandedRatio = MathF.Min(0.99f, MathF.Max(0.01f, (float)ratio));

            // Set the bottom detent using the peekHeight
            behavior.PeekHeight = (int)(bottom * density) + bottomInset + keyboardHeight;
        }
    }

    private double heightsComputedForHeight;

    /// <param name="maxHeight">in maui points</param>
    /// <returns>height in maui points</returns>
    double CalculateTallestDetent(double maxHeight)
    {
        if (heights is null || maxHeight != heightsComputedForHeight)
        {
            CalculateHeights(maxHeight);
            heightsComputedForHeight = maxHeight;
        }

        return heights.Values.Max();
    }

    void ResizeBottomSheetContentView()
    {
        var maxHeight = GetAvailableHeight();
        if(maxHeight <= 0)
            return;

        var height = CalculateTallestDetent(maxHeight);
        var platformHeight = (int)Math.Round(height * density) + BottomInset() + KeyboardHeight();

        //If extended to max, add top inset
        if (height >= maxHeight)
        {
            platformHeight += TopInset();
            height = maxHeight;
        }

        //Resize virtual view
        if (sheet.Handler?.PlatformView is ContentViewGroup bottomSheetContentView)
        {
            var newBounds = new Rect(0, 0, BottomSheetView.Width / density, height);
            bottomSheetContentView.CrossPlatformLayout?.CrossPlatformArrange(newBounds);

            //Required, otherwise the bottomSheet is full screen. Why ? No idea. WrapContent seems to be ignored.
            var lp = (FrameLayout.LayoutParams)bottomSheetContentView.LayoutParameters!;
            if (lp.Height != platformHeight)
                lp.Height = platformHeight;
        }
    }

    #region Updates from handlers's mappers
    internal void UpdateHandleColor(Color? color)
    {
        if (color != null)
            handleView.SetColorFilter(color.ToPlatform());
        else
            handleView.SetColorFilter(null);
    }

    internal void UpdateSelectedDetent(BottomSheet sheet)
    {
        var detent = GetDetentForState(Behavior.State);
        if (detent != null)
        {
            if (sheet.SelectedDetent == detent)
                sheet.SelectedDetent = null; //make sure to trigger a change
            sheet.SelectedDetent = detent;
        }
    }

    internal void UpdateStateFromDetent(Detent? detent)
    {
        if (detent != null && behavior != null)
            behavior.State = GetStateForDetent(detent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// When a corner is net, the background of the bottom sheet is automatically set to a TopCornersRadiusDrawable.
    /// When the background color is set, the original drawable is updated.
    /// By combining both updates, we can ensure that the background color is always set on the correct drawable.
    /// </remarks>
    internal void UpdateBackgroundAndCorners(double cornerRadius, Brush? backgroundBrush)
    {
        var bottomSheetView = BottomSheetView;
        if (bottomSheetView is null) 
            return;
        
        if (cornerRadius > 0)
        {
            TopCornersRadiusDrawable drawable;
            if (bottomSheetView.Background is not TopCornersRadiusDrawable sheetRadiusDrawable)
            {
                drawable = new TopCornersRadiusDrawable();
                bottomSheetView.Background = drawable;
            }
            else
                drawable = sheetRadiusDrawable;

            drawable.SetTopCornerRadius((int)bottomSheetView.Context.ToPixels(cornerRadius));
        }

        Paint paint = backgroundBrush;

        if (paint is not null)
        {
            var platformColor = paint.ToColor().ToPlatform();
            if (bottomSheetView.Background is TopCornersRadiusDrawable sheetDrawable)
                sheetDrawable.SetColor(platformColor);
            else
                bottomSheetView.SetBackgroundColor(platformColor);
        }
            
        // Try to find the background color to automatically change the status bar icons so they will
        // still be visible when the bottomsheet slides underneath the status bar.
        var backgroundTint = ViewCompat.GetBackgroundTintList(bottomSheetView);
            
        // First check for a tint
        if (backgroundTint != null)
            isBackgroundLight = MaterialColors.IsColorLight(backgroundTint.DefaultColor);
        // Then check for the background color
        else if (bottomSheetView.Background is ColorDrawable)
            isBackgroundLight = MaterialColors.IsColorLight(((ColorDrawable)bottomSheetView.Background).Color);
        // Otherwise don't change the status bar color
        else
            isBackgroundLight = null;
    }


    internal void UpdateHasBackdrop(bool hasBackdrop) => bottomSheetContainerView?.SetBackdropVisibility(hasBackdrop);
    public void UpdateHasHandle(bool hasHandle) => handleView.Visibility = hasHandle ? ViewStates.Visible : ViewStates.Gone;

    #endregion

    #region Listeners and callbacks
        
    class EdgeToEdgeListener(BottomSheetController controller) : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        EdgeToEdgeCallback? edgeToEdgeCallback;

        public WindowInsetsCompat OnApplyWindowInsets(AView view, WindowInsetsCompat insets)
        {
            if (edgeToEdgeCallback != null)
            {
                controller.Behavior.RemoveBottomSheetCallback(edgeToEdgeCallback);
                edgeToEdgeCallback = null;
            }

            if (insets != null)
            {
                edgeToEdgeCallback = new EdgeToEdgeCallback(controller, insets);
                controller.Behavior.AddBottomSheetCallback(edgeToEdgeCallback);
                controller.Layout();
            }

            return ViewCompat.OnApplyWindowInsets(view, insets);
        }


        class EdgeToEdgeCallback : BottomSheetBehavior.BottomSheetCallback
        {
            readonly BottomSheetController controller;
            readonly WindowInsetsCompat insetsCompat;
            AWindow? window => controller.window;
            readonly bool isStatusBarLight;

            public EdgeToEdgeCallback(BottomSheetController controller, WindowInsetsCompat insetsCompat)
            {
                this.controller = controller;
                this.insetsCompat = insetsCompat;

                SetPaddingForPosition(controller.BottomSheetView);
                
                if (window != null)
                {
                    var insetsController = WindowCompat.GetInsetsController(window, window.DecorView);
                    isStatusBarLight = insetsController.AppearanceLightStatusBars;
                }
            }

            public override void OnStateChanged(AView bottomSheet, int newState) => SetPaddingForPosition(bottomSheet);
            public override void OnSlide(AView bottomSheet, float percent) => SetPaddingForPosition(bottomSheet);

            int TopInset()
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                    return insetsCompat.GetInsetsIgnoringVisibility(Android.Views.WindowInsets.Type.SystemBars()).Top;

#pragma warning disable CS0618
                return insetsCompat.StableInsetTop;
#pragma warning restore CS0618
            }

            /// <summary>
            /// TODO: replace obsolete EdgeToEdgeUtils with androidx.compose.foundation
            /// </summary>
            /// <param name="bottomSheet"></param>
            void SetPaddingForPosition(AView bottomSheet)
            {
                var keyboardHeight = controller.KeyboardHeight();
                var topInset = TopInset();

                if (bottomSheet.Top < topInset)
                {
                    // If the bottomsheet is light, we should set light status bar so the icons are visible since the bottomsheet is now under the status bar.
                    if (window != null)
                        EdgeToEdgeUtils.SetLightStatusBar(window, !controller.isBackgroundLight.HasValue ? isStatusBarLight : controller.isBackgroundLight.Value);

                    // Smooth transition into status bar when drawing edge to edge.
                    bottomSheet.SetPadding(bottomSheet.PaddingLeft, topInset - bottomSheet.Top, bottomSheet.PaddingRight, keyboardHeight);
                }
                else if (bottomSheet.Top != 0)
                {
                    // Reset the status bar icons to the original color because the bottomsheet is not under the status bar.
                    if (window != null)
                        EdgeToEdgeUtils.SetLightStatusBar(window, isStatusBarLight);

                    bottomSheet.SetPadding(bottomSheet.PaddingLeft, 0, bottomSheet.PaddingRight, keyboardHeight);
                }
            }
        }
    }


    
    class BottomSheetCallback(Action<int> stateChanged) : BottomSheetBehavior.BottomSheetCallback
    {
        public override void OnStateChanged(AView view, int newState) => stateChanged(newState);

        public override void OnSlide(AView bottomSheet, float newState) {}
    }
    #endregion

}

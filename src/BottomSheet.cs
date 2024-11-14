namespace The49.Maui.BottomSheet;

public enum DismissOrigin
{
    Gesture,
    Programmatic,
}

public partial class BottomSheet : ContentPage
{
    public static readonly BindableProperty DetentsProperty = BindableProperty.Create(nameof(Detents), typeof(IList<Detent>), typeof(BottomSheet), defaultValueCreator: _ => new List<Detent>());
    public static readonly BindableProperty HasBackdropProperty = BindableProperty.Create(nameof(HasBackdrop), typeof(bool), typeof(BottomSheet), false);
    public static readonly BindableProperty HasHandleProperty = BindableProperty.Create(nameof(HasHandle), typeof(bool), typeof(BottomSheet), false);
    public static readonly BindableProperty HandleColorProperty = BindableProperty.Create(nameof(HandleColor), typeof(Color), typeof(BottomSheet));
    public static readonly BindableProperty IsCancelableProperty = BindableProperty.Create(nameof(IsCancelable), typeof(bool), typeof(BottomSheet), true);
    public static readonly BindableProperty SelectedDetentProperty = BindableProperty.Create(nameof(SelectedDetent), typeof(Detent), typeof(BottomSheet), defaultBindingMode: BindingMode.TwoWay);
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(BottomSheet), -1d);

    DismissOrigin _dismissOrigin = DismissOrigin.Gesture;

    //public event EventHandler<float> Sliding;
    public event EventHandler<DismissOrigin>? Dismissed;
    public event EventHandler? Showing;
    public event EventHandler? Shown;

    public IList<Detent> Detents
    {
        get => (IList<Detent>)GetValue(DetentsProperty);
        set => SetValue(DetentsProperty, value);
    }

    public bool HasBackdrop
    {
        get => (bool)GetValue(HasBackdropProperty);
        set => SetValue(HasBackdropProperty, value);
    }

    public bool HasHandle
    {
        get => (bool)GetValue(HasHandleProperty);
        set => SetValue(HasHandleProperty, value);
    }

    public Color? HandleColor
    {
        get => (Color?)GetValue(HandleColorProperty);
        set => SetValue(HandleColorProperty, value);
    }

    public bool IsCancelable
    {
        get => (bool)GetValue(IsCancelableProperty);
        set => SetValue(IsCancelableProperty, value);
    }

    public Detent? SelectedDetent
    {
        get => (Detent?)GetValue(SelectedDetentProperty);
        set => SetValue(SelectedDetentProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public BottomSheet()
    {
        Resources.Add(new Style(typeof(Label)));
        BackgroundColor = Colors.White;
    }

    /// <summary>
    /// Shows the bottom sheet above everything using the default window
    /// </summary>
    public Task ShowAsync(bool animated = true, bool aboveEverything = true)
    {
        var parent =  IPlatformApplication.Current!.Application.Windows[0];
        return ShowAsync(parent, animated, aboveEverything);
    }

    /// <summary>
    /// Shows the bottom sheet above everything using the specified Window
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="animated"></param>
    /// <param name="aboveEverything">Shows the bottom sheet "within" the page. The sheet will be obscured by flyout page and shows vertically above navigation bar/tab bar.</param>
    public Task ShowAsync(IWindow parent, bool animated = true, bool aboveEverything = true)
    {
        var completionSource = new TaskCompletionSource();
        void OnShown(object? sender, EventArgs e)
        {
            Shown -= OnShown;
            completionSource.SetResult();
        }
        Shown += OnShown;

        SelectedDetent ??= GetDefaultDetent();

#if ANDROID
        BottomSheetManager.Create(parent.Handler.MauiContext, this, aboveEverything);
#endif
        
        (parent.Content as Element)!.AddLogicalChild(this);

        Handler ??= BottomSheetHandler.CreateBottomSheetHandler(parent.Handler.MauiContext);
        
        BottomSheetManager.Show(this, animated, aboveEverything);

        return completionSource.Task;
    }

    public Task DismissAsync(bool animated = true)
    {
        _dismissOrigin = DismissOrigin.Programmatic;
        var completionSource = new TaskCompletionSource();
        void OnDismissed(object? sender, DismissOrigin origin)
        {
            Dismissed -= OnDismissed;
            completionSource.SetResult();
        }
        Dismissed += OnDismissed;
        Handler?.Invoke(nameof(DismissAsync), animated);
        return completionSource.Task;
    }
    
    internal void NotifyDismissed()
    {
        var parent = Parent;
        if (parent != null)
        {
            parent.RemoveLogicalChild(this);
            Dismissed?.Invoke(this, _dismissOrigin);
        }
    }

    internal List<Detent> GetEnabledDetents()
    {
        var enabledDetents = Detents.Where(d => d.IsEnabled).ToList();

        if (enabledDetents.Count > 0)
            return enabledDetents;

        return [new ContentDetent()];
    }

    private Detent? GetDefaultDetent()
    {
        var detent = SelectedDetent;
        if (detent != null)
            return detent;

        return GetEnabledDetents().FirstOrDefault(d => d.IsDefault);
    }

    internal Brush? BackgroundBrush
    {
        get
        {
            if (!Background.IsEmpty)
                return Background;
            if (BackgroundColor.IsNotDefault())
                return new SolidColorBrush(BackgroundColor);
            return null;
        }
    }

    internal void NotifyShowing() => Showing?.Invoke(this, EventArgs.Empty);
    internal void NotifyShown() => Shown?.Invoke(this, EventArgs.Empty);
}

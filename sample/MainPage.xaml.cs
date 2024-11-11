using The49.Maui.BottomSheet.Sample.DemoPages;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using Maui.BottomSheet.Sample.DemoPages;
using Mopups.Services;

namespace The49.Maui.BottomSheet.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public ObservableCollection<DemoEntry> Demos => new()
    {
        //TODO: can't close that modal. Why ?
        // new DemoEntry
        // {
        //     Title = "Open Modal Page",
        //     Description = "Open the demo modal page",
        //     Command = new Command(() => Shell.Current.GoToAsync("//ModalPage")),
        // },
        new DemoEntry
        {
            Title = "BindableLayout Demo",
            Description = "A sheet with a BindableLayout",
            Command = new Command(OpenBindableLayoutSheet),
        },
        new DemoEntry
        {
            Title = "Chat demo",
            Description = "Use a sheet for a chat editor",
            Command = new Command(OpenChat),
        },
        new DemoEntry
        {
            Title = "Entry",
            Description = "Display any page as a bottom sheet",
            Command = new Command(OpenEntrySheet),
        },
        new DemoEntry
        {
            Title = "Non-modal sheet",
            Description = "Display any page as a bottom sheet",
            Command = new Command(OpenSimpleSheet),
        },
        new DemoEntry
        {
            Title = "Modal sheet",
            Description = "Display the sheet as modal",
            Command = new Command(OpenModalSheet),
        },
        new DemoEntry
        {
            Title = "No sliding away",
            Description = "The sheet cannot be closed by sliding the sheet",
            Command = new Command(OpenNotCancelableSheet),
        },
        new DemoEntry
        {
            Title = "With handle",
            Description = "Display the drag handle",
            Command = new Command(OpenHandleSheet),
        },
        new DemoEntry
        {
            Title = "With custom handle color",
            Description = "Chose the color of the drag handle",
            Command = new Command(OpenHandleColorSheet),
        },
        new DemoEntry
        {
            Title = "Without animation",
            Description = "display the sheet immediately",
            Command = new Command(OpenNoAnimationSheet),
        },
        new DemoEntry
        {
            Title = "Specify a height",
            Description = "Use a dp value at which the sheet will open",
            Command = new Command(OpenHeightSheet),
        },
        new DemoEntry
        {
            Title = "Specify a ratio between 0 and 1",
            Description = "The sheet will open at the position specified",
            Command = new Command(OpenRatioSheet),
        },
        new DemoEntry
        {
            Title = "Anchor a detent to a specific view",
            Description = "The sheet will snap to a position hiding the anchor",
            Command = new Command(OpenPeekableSheet),
        },
        new DemoEntry
        {
            Title = "Fullscreen",
            Description = "Use the full height of the screen",
            Command = new Command(OpenFullscreenSheet),
        },
        new DemoEntry
        {
            Title = "Text sizing",
            Description = "Text should take as much space as it needs",
            Command = new Command(OpenTextSizing),
        },
        new DemoEntry
        {
            Title = "With ScrollView",
            Description = "Let the sheet expand, then scroll",
            Command = new Command(OpenScrollView),
        },
        new DemoEntry
        {
            Title = "Corner radius",
            Description = "specify a Corner radius for the sheet",
            Command = new Command(OpenCornerRadius),
        },
        new DemoEntry
        {
            Title = "Background color",
            Description = "specify a BackgroundColor for the sheet",
            Command = new Command(OpenBackgroundSheet),
        },
        new DemoEntry
        {
            Title = "Dismissed",
            Description = "listen to the dismissed event",
            Command = new Command(OpenDismissed),
        },
        new DemoEntry
        {
            Title = "Selected Detent",
            Description = "control the selected detent",
            Command = new Command(OpenSelectedDetent),
        },
        new DemoEntry
        {
            Title = "Default detent",
            Description = "define a detent to be opened by default",
            Command = new Command(OpenDefaultDetent),
        },
        new DemoEntry
        {
            Title = "Open modal page",
            Description = "A sheet should behave correctly around opening a modal page",
            Command = new Command(OpenModalPage),
        },
        new DemoEntry
        {
            Title = "Sizing test",
            Description = "Check that the content is sized to the sheet",
            Command = new Command(OpenSizingTest),
        },
        new DemoEntry
        {
            Title = "Keyboard layout",
            Description = "Layout should update with keyboard",
            Command = new Command(OpenKeyboard),
        },
        new DemoEntry
        {
            Title = "Content with scrolling collection",
            Description = "the sheet contains a CollectionView",
            Command = new Command(OpenWithCollectionView),
        },
        new DemoEntry
        {
            Title = "Content is ScrollView",
            Description = "the sheet contains a ScrollView",
            Command = new Command(OpenWithScrollView),
        },
        new DemoEntry
        {
            Title = "Inside a Mopup",
            Description = "Open a Mopup, then open a sheet inside the mopup",
            Command = new Command(OpenInMopup),
        },
#if ANDROID
        new DemoEntry
        {
            Title = "[Android] Customize behavior",
            Description = "access the Android BottomSheetBehavior",
            Command = new Command(OpenCustomizeBehavior),
        },
        new DemoEntry
        {
            Title = "[Android] Modal Sheet (Inside Page)",
            Description = "Shows behind the navigation bar, flyout, etc.",
            Command = new Command(OpenModalSheetWithinPage),
        },
        new DemoEntry
        {
            Title = "[Android] Non-Modal Sheet (Inside Page)",
            Description = "Shows behind the navigation bar, flyout, etc.",
            Command = new Command(OpenNonModalSheetWithinPage),
        },
#elif IOS
        new DemoEntry
        {
            Title = "[iOS] Customize behavior",
            Description = "access the iOS UISheetPresentationControllerDelegate",
            Command = new Command(OpenCustomizeBehavior),
        },
#endif
    };

    private void OpenSimpleSheet()
    {
        var page = new SimplePage();
        page.ShowAsync();
    }
    
    private void OpenModalSheet()
    {
        var page = new SimplePage
        {
            HasBackdrop = true
        };
        page.ShowAsync();
    }
    
    private void OpenNotCancelableSheet()
    {
        var page = new SimplePage
        {
            IsCancelable = false,
            HasBackdrop = true
        };
        page.ShowAsync();
    }
    
    private void OpenHandleSheet()
    {
        var page = new SimplePage
        {
            HasBackdrop = true,
            HasHandle = true
        };
        page.Detents = [
            new FullscreenDetent(),
            new ContentDetent(),
            new AnchorDetent { Anchor = page.Divider } ];
        page.ShowAsync();
    }
    
    private void OpenHandleColorSheet()
    {
        var page = new SimplePage
        {
            HasBackdrop = true,
            HasHandle = true,
            HandleColor = Colors.Salmon,
        };
        page.Detents = [
            new FullscreenDetent(),
            new ContentDetent(),
            new AnchorDetent { Anchor = page.Divider },
        ];
        page.ShowAsync();
    }
    
    private void OpenPeekableSheet()
    {
        var page = new SimplePage();
        page.Detents = [
            new FullscreenDetent(),
            new ContentDetent(),
            new AnchorDetent { Anchor = page.Divider },
        ];
        page.ShowAsync();
    }
    
    async void OpenEntrySheet()
    {
        var sheet = new EntrySheet();
        await sheet.ShowAsync();
    }
    
    private void OpenFullscreenSheet()
    {
        var page = new SimplePage
        {
            HasBackdrop = true,
            Detents = [new FullscreenDetent()]
        };
        page.ShowAsync();
    }
    
    private void OpenBackgroundSheet()
    {
        var page = new SimplePage
        {
            BackgroundColor = Colors.Salmon
        };
        page.Detents = [
            new FullscreenDetent(),
            new ContentDetent(),
            new AnchorDetent { Anchor = page.Divider },
        ];
        page.ExtraContent = new HorizontalStackLayout
        {
            new Button { Text = "Change background color", Command = new Command(() => page.BackgroundColor = RandomColors.RandomColor() ) },
        };
        page.ShowAsync();
    }

    void OpenCornerRadius()
    {
        var page = new SimplePage
        {
            Background = Colors.Salmon,
            CornerRadius = 10
        };
        page.Detents = [
            new FullscreenDetent(),
            new ContentDetent(),
            new AnchorDetent { Anchor = page.Divider },
        ];
        page.ShowAsync();
    }
    private void OpenRatioSheet()
    {
        var page = new SimplePage { Detents = [new RatioDetent { Ratio = .6f } ] };
        page.ShowAsync();
    }

    private void OpenHeightSheet()
    {
        var page = new SimplePage { Detents = [new HeightDetent { Height = 240 }] };
        page.ShowAsync();
    }

    void OpenTextSizing()
    {
        var p = new TextSheet();
        p.ShowAsync();
    }

    void OpenDismissed()
    {
        var page = new SimplePage
        {
            HasBackdrop = true
        };
        page.Dismissed += (s, e) =>
        {
            DisplayAlert("Sheet was dismissed", e == DismissOrigin.Gesture ? "Sheet was dismissed by a user gesture" : "Sheet was dismissed programmatically", "close");
        };
        page.ShowAsync();
    }

    void OpenSelectedDetent()
    {
        var page = new SimplePage
        {
            Detents =
            [
                new FullscreenDetent(),
                new MediumDetent(),
                new RatioDetent { Ratio = .2f },
            ],
            HasBackdrop = false
        };
        page.ExtraContent = new HorizontalStackLayout
        {
            new Button { Text = "small", Command = new Command(() => page.SelectedDetent = page.Detents[2]) },
            new Button { Text = "medium", Command = new Command(() => page.SelectedDetent = page.Detents[1]) },
            new Button { Text = "large", Command = new Command(() => page.SelectedDetent = page.Detents[0]) },
        };
        page.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(page.SelectedDetent))
                Toast.Make($"Selected Detent is now {(page.SelectedDetent is null ? "unknown" : page.SelectedDetent.ToString())}").Show();
        };
        page.ShowAsync();
    }

    void OpenDefaultDetent()
    {
        var page = new SimplePage
        {
            Detents =
            [
                new FullscreenDetent(),
                new MediumDetent { IsDefault = true },
                new RatioDetent { Ratio = .2f },
            ],
            HasBackdrop = false
        };
        page.ShowAsync();
    }

    void OpenNoAnimationSheet()
    {
        var page = new SimplePage
        {
            Detents =
            [
                new FullscreenDetent(),
                new ContentDetent(),
            ],
            HasBackdrop = true
        };
        page.ExtraContent = new Button { Text = "Dismiss without animation", Command = new Command(() => page.DismissAsync(false)) };
        page.ShowAsync(false);
    }

    void OpenScrollView()
    {
        var sheet = new ScrollSheet();
        sheet.ShowAsync();
    }

    void OpenBindableLayoutSheet()
    {
        var sheet = new BindableLayoutSheet();
        sheet.ShowAsync();
    }

    void OpenModalPage()
    {
        var page = new SimplePage
        {
            Detents =
            [
                new FullscreenDetent(),
                new ContentDetent(),
            ],
            HasBackdrop = true
        };

        var b = new Button { Text = "Go to page" };
        var g = new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                page.DismissAsync(false);
                Shell.Current.GoToAsync("//ModalPage");
            }),
        };
        b.GestureRecognizers.Add(g);
        page.ExtraContent =b;
        
        page.ShowAsync();
    }

    void OpenSizingTest()
    {
        var t = new SizingTest();
        t.ShowAsync();
    }

    void OpenKeyboard()
    {
        var t = new EntrySheet
        {
            Detents =
            [
                new FullscreenDetent(),
                new MediumDetent(),
                new ContentDetent(),
            ]
        };
        t.ShowAsync();
    }

    void OpenChat()
    {
        Shell.Current.Navigation.PushAsync(new ChatPage());
    }

    void OpenWithCollectionView()
    {
        var page = new SimplePage
        {
            Detents = [
                new FullscreenDetent(),
                new MediumDetent { IsDefault = true },
                new RatioDetent { Ratio = .2f },
            ],
            HasBackdrop = true,
            ExtraContent = new CollectionView
            {
                HeightRequest = 400,
                ItemsSource = Enumerable.Range(0, 1000).Select(i => $"item {i}").ToList(),
                ItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label { Margin = new(20, 10, 20, 10) };
                    label.SetBinding(Label.TextProperty, new Binding("."));
                    return label;
                })
            }
        };

        page.ShowAsync();
    }

    void OpenWithScrollView()
    {
        var page = new SimpleScrollViewPage
        {
            Detents = [
                new FullscreenDetent(),
                new MediumDetent { IsDefault = true },
                new RatioDetent { Ratio = .2f },
            ],
            HasBackdrop = true,
        };

        page.ShowAsync();
    }

    void OpenInMopup()
    {
        var container = new PopupTestPage();
        MopupService.Instance.PushAsync(container);
    }

#if ANDROID
    void OpenCustomizeBehavior()
    {
        var page = new SimplePage
        {
            HasBackdrop = true
        };
        page.Showing += (s, e) => page.Controller.Behavior.DisableShapeAnimations();
        page.ShowAsync();
    }
    
    void OpenModalSheetWithinPage()
    {
        var page = new SimplePage
        {
            Detents =
            [
                new FullscreenDetent(),
                new ContentDetent(),
            ],
            HasBackdrop = true
        };

        // var b = new Button { Text = "Go to page" };
        // var g = new TapGestureRecognizer
        // {
        //     Command = new Command(() =>
        //     {
        //         page.DismissAsync(false);
        //         Shell.Current.GoToAsync("//ModalPage");
        //     }),
        // };
        // b.GestureRecognizers.Add(g);
        // page.ExtraContent = b;

        page.ShowAsync(aboveEverything: false);
    }

    void OpenNonModalSheetWithinPage()
    {
        var sheet = new ScrollSheet();
        sheet.ShowAsync(aboveEverything: false);
    }
    
    
#elif IOS
    void OpenCustomizeBehavior()
    {
        var page = new SimplePage { HasBackdrop = true };
        page.Showing += (s, e) => page.Controller.SheetPresentationController.PreferredCornerRadius = 2;
        page.ShowAsync();
    }
#endif

    private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (list.SelectedItem is DemoEntry item)
        {
            list.SelectedItem = null;
            item.Command.Execute(null);
        }
    }

    void list_Scrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        Header.TranslationY = Math.Max(-e.VerticalOffset, -72);
    }
}
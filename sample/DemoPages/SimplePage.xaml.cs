using System.Collections.ObjectModel;
using System.Windows.Input;

namespace The49.Maui.BottomSheet.Sample.DemoPages;

public record ListAction(string Title, ICommand Command)
{
    public ListAction(string title, Action commandAction) : this(title, new Command(commandAction)){}
    public ListAction(string title) : this(title, new Command(() => {})){}
}

public partial class SimplePage : BottomSheet
{
    public VisualElement Divider => divider;

    public ObservableCollection<ListAction> Actions =>
    [
        new("Share"),
        new("Copy"),
        new("Open in browser"),
        new("Resize", () => divider.HeightRequest = divider.HeightRequest != 32 ? 32 : 1),
        new("Dismiss", () => DismissAsync())
    ];
    
    public SimplePage()
    {
        InitializeComponent();
    }

    public View ExtraContent
    {
        set => extra.Content = value;
    }
}
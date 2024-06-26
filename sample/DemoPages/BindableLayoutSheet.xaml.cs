namespace The49.Maui.BottomSheet.Sample.DemoPages;

public partial class BindableLayoutSheet
{
    public List<string> ItemList { get; set; }
    
    public BindableLayoutSheet()
    {
        InitializeComponent();
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            ItemList = Enumerable.Range(0,10).Select(i => i.ToString()).ToList();
            OnPropertyChanged(nameof(ItemList));
        });
    }
}
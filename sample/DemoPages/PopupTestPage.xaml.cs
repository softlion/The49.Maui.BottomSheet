using Mopups.Services;

namespace The49.Maui.BottomSheet.Sample.DemoPages;

public partial class PopupTestPage
{
    public PopupTestPage()
    {
        InitializeComponent();
    }

    private async void CloseButton_OnTapped(object? sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void OpenBottomSheet(object? sender, EventArgs e)
    {
        var page = new SimplePage
        {
            HasBackdrop = true
        };
        await page.ShowAsync();
    }
}
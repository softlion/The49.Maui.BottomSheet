using System.Windows.Input;

namespace The49.Maui.BottomSheet.Sample;

public record DemoEntry
{
    public string Title { get; set; }
    public string Description { get; set; }
    public ICommand Command { get; set; }
}
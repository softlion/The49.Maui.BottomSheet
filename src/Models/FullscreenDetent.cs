namespace The49.Maui.BottomSheet;

public class FullscreenDetent : Detent
{
    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        return maxSheetHeight;
    }
}

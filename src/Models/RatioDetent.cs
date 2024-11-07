namespace The49.Maui.BottomSheet;

[ContentProperty(nameof(Ratio))]
public class RatioDetent : Detent
{
    public static readonly BindableProperty RatioProperty = BindableProperty.Create(nameof(Ratio), typeof(float), typeof(RatioDetent), defaultValue: 0f);

    public float Ratio
    {
        get => (float)GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }
    
    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        return maxSheetHeight * Ratio;
    }
}

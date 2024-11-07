namespace The49.Maui.BottomSheet;

[ContentProperty(nameof(Height))]
public class HeightDetent : Detent
{
    public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(double), typeof(HeightDetent), defaultValue: 0.0);

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        return Height;
    }
}

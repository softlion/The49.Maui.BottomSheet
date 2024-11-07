namespace The49.Maui.BottomSheet.Sample;

public static class RandomColors
{
    private static readonly Random random = new();

    public static Color RandomColor()
    {
        return Color.FromRgb(random.Next(256), random.Next(256), random.Next(256));
    }
}
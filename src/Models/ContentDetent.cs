namespace The49.Maui.BottomSheet;

public class ContentDetent : Detent
{
    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        if (page?.Content is null)
        {
            return maxSheetHeight;
        }

        // I left this code as comment because it might be useful for future reference, but it's not used
        //if (page.Content is ScrollView sv)
        //{
        //    var s = sv.Content.Measure(page.Window.Width - page.Padding.HorizontalThickness, maxSheetHeight);
        //}

        // HACK: from 'page.Window.Width' to 'page.Window?.Width ?? page.Width' 
        var r = page.Content.Measure(page.Window?.Width ?? page.Width - page.Padding.HorizontalThickness, maxSheetHeight);

#if NET8_0
        var height = r.Request.Height;
#else
        var height = r.Height;
#endif
        
        return Math.Min(maxSheetHeight, height + page.Padding.VerticalThickness);
    }
}

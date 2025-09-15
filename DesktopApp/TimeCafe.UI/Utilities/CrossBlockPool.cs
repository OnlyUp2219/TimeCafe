using Microsoft.UI;

namespace TimeCafe.UI.Utilities;

public static class CrossBlockPool
{
    static readonly Stack<TextBlock> _pool = new();

    public static TextBlock Get()
    {
        if (_pool.Count > 0) return _pool.Pop();
        return new TextBlock
        {
            Text = "+",
            FontFamily = new FontFamily("VAG Round Cyrillic"),
            Foreground = new SolidColorBrush(Colors.White),
            Opacity = 0.2
        };
    }

    public static void ReleaseAll(Canvas canvas)
    {
        foreach (var tb in canvas.Children.OfType<TextBlock>())
            _pool.Push(tb);
        canvas.Children.Clear();
    }
}

namespace TimeCafe.UI.Views;

public sealed partial class TariffManagePage : Page
{
    public TariffManageViewModel ViewModel { get; }

    public TariffManagePage()
    {
        ViewModel = App.GetService<TariffManageViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.AdaptiveGrid = AdaptiveGrid;

        var sv = FindScrollViewer(AdaptiveGrid);
        if (sv != null)
            sv.ViewChanged += (s, ev) => DispatcherQueue.TryEnqueue(ApplyToVisible);
    }


    private async void OnPageChanged(object sender, int pageNumber)
    {
        if (ViewModel?.Source == null) return;
        await ViewModel.SetCurrentPage(pageNumber);
    }

    private void TariffBorder_Loaded(object sender, RoutedEventArgs e)
    {
        var border = (Border)sender;
        var canvas = (Canvas)border.FindName("CrossCanvas");
        if (canvas == null) return;

        CrossBlockPool.ReleaseAll(canvas);

        var tariff = (Tariff)border.DataContext;
        int idx = Math.Abs(tariff.TariffName.GetHashCode()) % CrossTemplates.Templates.Count;
        var template = CrossTemplates.Templates[idx];

        double w = border.ActualWidth;
        double h = border.ActualHeight;

        double scaleX = w / CrossTemplates.BaseWidth;
        double scaleY = h / CrossTemplates.BaseHeight;

        foreach (var item in template)
        {
            var cross = CrossBlockPool.Get();
            cross.FontSize = item.Size;

            cross.RenderTransform = new RotateTransform
            {
                Angle = item.Angle,
                CenterX = item.Size / 2,
                CenterY = item.Size / 2
            };

            Canvas.SetLeft(cross, item.Xpx * scaleX);
            Canvas.SetTop(cross, item.Ypx * scaleY);

            canvas.Children.Add(cross);
        }
    }

    private ScrollViewer FindScrollViewer(DependencyObject parent)
    {
        if (parent is ScrollViewer sv) return sv;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }
        return null;
    }

    private void ApplyToVisible()
    {
        var sv = FindScrollViewer(AdaptiveGrid);
        if (sv == null) return;

        double top = sv.VerticalOffset;
        double bottom = top + sv.ViewportHeight;

        foreach (var item in AdaptiveGrid.Items)
        {
            if (!(AdaptiveGrid.ContainerFromItem(item) is GridViewItem container)) continue;
            var transform = container.TransformToVisual(sv);
            var pos = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
            double itemTop = pos.Y;
            double itemBottom = pos.Y + container.ActualHeight;

            var border = container.ContentTemplateRoot as Border;
            if (border == null) continue;
            var canvas = border.FindName("CrossCanvas") as Canvas;
            if (canvas == null) continue;

            if (itemBottom >= 0 && itemTop <= sv.ViewportHeight)
            {
                // элемент видим — убедимся, что крестики нарисованы
                if (canvas.Children.Count == 0)
                    TariffBorder_Loaded(border, null);
            }
            else
            {
                // вне экрана — очистим и вернём в пул
                CrossBlockPool.ReleaseAll(canvas);
            }
        }
    }

    private async void DeleteTariff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Tariff tariff)
        {
            await ViewModel.DeleteTariffAsync(tariff);
        }
    }
}

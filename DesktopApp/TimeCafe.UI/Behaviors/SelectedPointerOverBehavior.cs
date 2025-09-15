using Microsoft.Xaml.Interactivity;

namespace TimeCafe.UI.Behaviors;

public class SelectedPointerOverBehavior : Behavior<GridViewItem>
{
    private bool _isPointerOver;
    private long _isSelectedCallbackToken; // Для хранения идентификатора обратного вызова

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PointerEntered += OnPointerEntered;
        AssociatedObject.PointerExited += OnPointerExited;
        // Регистрируем обратный вызов и сохраняем токен
        _isSelectedCallbackToken = AssociatedObject.RegisterPropertyChangedCallback(GridViewItem.IsSelectedProperty, OnIsSelectedChanged);
        UpdateVisualState();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PointerEntered -= OnPointerEntered;
        AssociatedObject.PointerExited -= OnPointerExited;
        // Отменяем регистрацию с использованием токена
        AssociatedObject.UnregisterPropertyChangedCallback(GridViewItem.IsSelectedProperty, _isSelectedCallbackToken);
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _isPointerOver = true;
        UpdateVisualState();
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _isPointerOver = false;
        UpdateVisualState();
    }

    private void OnIsSelectedChanged(DependencyObject sender, DependencyProperty dp)
    {
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        //System.Diagnostics.Debug.WriteLine($"IsSelected: {AssociatedObject.IsSelected}, IsPointerOver: {_isPointerOver}");
        if (AssociatedObject.IsSelected && _isPointerOver)
        {
            VisualStateManager.GoToState(AssociatedObject, "SelectedPointerOver", true);
        }
        else if (AssociatedObject.IsSelected)
        {
            VisualStateManager.GoToState(AssociatedObject, "Selected", true);
        }
        else if (_isPointerOver)
        {
            VisualStateManager.GoToState(AssociatedObject, "PointerOver", true);
        }
        else
        {
            VisualStateManager.GoToState(AssociatedObject, "Normal", true);
        }
    }
}

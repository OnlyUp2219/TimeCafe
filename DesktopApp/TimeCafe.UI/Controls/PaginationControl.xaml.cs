using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeCafe.UI.Controls;

public static class ControlExtensions
{
    public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent == null) return null;
        if (parent is T typedParent) return typedParent;
        return FindParent<T>(parent);
    }
}

public sealed partial class PaginationControl : UserControl, INotifyPropertyChanged
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(PaginationControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(20, OnPageSizeChanged));

    public static readonly DependencyProperty ShowFirstLastButtonsProperty =
        DependencyProperty.Register(
            nameof(ShowFirstLastButtons),
            typeof(bool),
            typeof(PaginationControl),
            new PropertyMetadata(true, OnShowFirstLastButtonsChanged));

    public static readonly DependencyProperty TargetControlProperty =
        DependencyProperty.Register(
            nameof(TargetControl),
            typeof(Control),
            typeof(PaginationControl),
            new PropertyMetadata(null, OnTargetControlChanged));

    public static readonly DependencyProperty HideOnSinglePageProperty =
        DependencyProperty.Register(
            nameof(HideOnSinglePage),
            typeof(bool),
            typeof(PaginationControl),
            new PropertyMetadata(true, OnHideOnSinglePageChanged));

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(
            nameof(TotalItems),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(0, OnTotalItemsChanged));

    #endregion

    #region Properties

    private IEnumerable _itemsSource;
    public IEnumerable ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (_itemsSource != value)
            {
                // Отписываемся от старой коллекции
                if (_itemsSource is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
                }

                _itemsSource = value;
                SetValue(ItemsSourceProperty, value);

                // Подписываемся на новую коллекцию
                if (_itemsSource is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
                }

                UpdatePagination();
            }
        }
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public bool ShowFirstLastButtons
    {
        get => (bool)GetValue(ShowFirstLastButtonsProperty);
        set => SetValue(ShowFirstLastButtonsProperty, value);
    }

    public Control TargetControl
    {
        get => (Control)GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    public bool HideOnSinglePage
    {
        get => (bool)GetValue(HideOnSinglePageProperty);
        set => SetValue(HideOnSinglePageProperty, value);
    }

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
                UpdatePageInfo();
            }
        }
    }

    private int _totalPages = 1;
    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (_totalPages != value)
            {
                _totalPages = value;
                OnPropertyChanged();
                UpdatePageInfo();
            }
        }
    }

    private string _pageInfo = "Страница 1 из 1";
    public string PageInfo
    {
        get => _pageInfo;
        private set
        {
            if (_pageInfo != value)
            {
                _pageInfo = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler<int> PageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Private Fields

    private static readonly Dictionary<string, int> _pagePositions = new Dictionary<string, int>();
    private string _currentListKey;

    #endregion

    public PaginationControl()
    {
        this.InitializeComponent();
        UpdateButtonsVisibility();
        UpdateControlVisibility();
    }

    #region Private Methods

    private void UpdatePageInfo()
    {
        PageInfo = $"Страница {CurrentPage} из {TotalPages}";
        UpdateControlVisibility();
    }

    private void UpdateButtonsVisibility()
    {
        FirstPageButton.Visibility = ShowFirstLastButtons ? Visibility.Visible : Visibility.Collapsed;
        LastPageButton.Visibility = ShowFirstLastButtons ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateControlVisibility()
    {
        if (HideOnSinglePage && TotalPages <= 1)
        {
            this.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.Visibility = Visibility.Visible;
        }
    }

    private void UpdatePagination()
    {
        if (TotalItems == 0)
        {
            TotalPages = 1;
            CurrentPage = 1;
            UpdateControlVisibility();
            return;
        }

        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

        // Получаем сохраненную позицию для текущего списка
        if (!string.IsNullOrEmpty(_currentListKey) && _pagePositions.TryGetValue(_currentListKey, out int savedPage))
        {
            var newPage = Math.Min(savedPage, TotalPages);
            if (CurrentPage != newPage)
            {
                CurrentPage = newPage;
                PageChanged?.Invoke(this, CurrentPage);
            }
        }
        else
        {
            if (CurrentPage != 1)
            {
                CurrentPage = 1;
                PageChanged?.Invoke(this, CurrentPage);
            }
        }

        UpdateControlVisibility();
    }

    public void ForceUpdatePage()
    {
        if (!string.IsNullOrEmpty(_currentListKey) && _pagePositions.TryGetValue(_currentListKey, out int savedPage))
        {
            CurrentPage = Math.Min(savedPage, TotalPages);
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void SaveCurrentPosition()
    {
        if (!string.IsNullOrEmpty(_currentListKey))
        {
            _pagePositions[_currentListKey] = CurrentPage;
        }
    }

    private void SetCurrentListKey()
    {
        if (TargetControl != null)
        {
            var page = TargetControl.FindParent<Page>();
            var pageType = page?.GetType().Name ?? "Unknown";
            var controlType = TargetControl.GetType().Name;
            _currentListKey = $"{pageType}_{controlType}";
        }
        else if (ItemsSource != null)
        {
            var page = this.FindParent<Page>();
            var pageType = page?.GetType().Name ?? "Unknown";
            _currentListKey = $"{pageType}_{ItemsSource.GetType().Name}";
        }
        else
        {
            _currentListKey = null!;
        }
    }

    private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdatePagination();
    }

    #endregion

    #region Event Handlers

    private void OnFirstPageClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage = 1;
            SaveCurrentPosition();
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void OnPreviousPageClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            SaveCurrentPosition();
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void OnNextPageClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            SaveCurrentPosition();
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private void OnLastPageClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage = TotalPages;
            SaveCurrentPosition();
            PageChanged?.Invoke(this, CurrentPage);
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.SetCurrentListKey();
            control.ItemsSource = e.NewValue as IEnumerable;
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdatePagination();
        }
    }

    private static void OnShowFirstLastButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateButtonsVisibility();
        }
    }

    private static void OnTargetControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.SetCurrentListKey();
            control.UpdatePagination();
        }
    }

    private static void OnHideOnSinglePageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateControlVisibility();
        }
    }

    private static void OnTotalItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdatePagination();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region TODO
    /*
     * TODO: Сохранение при перезапуске приложения
     * TODO: Отдельное сохранение для разных представлений одного списка
     * TODO: Анимация при переключении страниц
     * TODO: Обработка ошибок при загрузке данных из БД
     * TODO: Реализация pull-to-refresh
     * TODO: Программный переход на определенную страницу (например, по поиску)
     */
    #endregion
}
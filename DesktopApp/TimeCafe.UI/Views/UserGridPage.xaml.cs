using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace TimeCafe.UI.Views;

public sealed partial class UserGridPage : Page
{
    private Client _storeditem;
    private int _storedPage = 1;
    public UserGridViewModel ViewModel
    {
        get;
    }
    public UserGridPage()
    {
        ViewModel = App.GetService<UserGridViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        //this.NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_storeditem != null)
        {
            ViewModel.SetCurrentPage(_storedPage).Wait();
        }
    }

    private async void AdaptiveGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (_storeditem != null)
        {
            if (!ViewModel.Source.Contains(_storeditem))
            {
                await ViewModel.SetCurrentPage(_storedPage);
            }

            AdaptiveGrid.ScrollIntoView(_storeditem, ScrollIntoViewAlignment.Default);
            AdaptiveGrid.UpdateLayout();

            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BackConnectedAnimation");
            if (animation != null)
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                {
                    animation.Configuration = new DirectConnectedAnimationConfiguration();
                }

                await AdaptiveGrid.TryStartConnectedAnimationAsync(animation, _storeditem, "connectedElement");
            }

            AdaptiveGrid.Focus(FocusState.Programmatic);
        }
    }

    private void AdaptiveGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (AdaptiveGrid.ContainerFromItem(e.ClickedItem) is GridViewItem container)
        {
            _storeditem = container.Content as Client;
            _storedPage = ViewModel.CurrentPage;

            AdaptiveGrid.PrepareConnectedAnimation("ForwardConnectedAnimation", _storeditem, "connectedElement");
        }

        Frame.Navigate(typeof(UserGridDetailPage), _storeditem, new SuppressNavigationTransitionInfo());
    }

    private async void OnPageChanged(object sender, int pageNumber)
    {
        if (ViewModel?.Source == null) return;
        await ViewModel.SetCurrentPage(pageNumber);
    }
}

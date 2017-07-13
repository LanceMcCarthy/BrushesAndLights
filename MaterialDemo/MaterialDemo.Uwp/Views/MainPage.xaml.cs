using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MaterialDemo.Uwp.Views;

namespace MaterialDemo.Uwp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void BrushesDemoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BrushesPage));
        }

        private void LightsDemoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LightsPage));
        }
    }
}

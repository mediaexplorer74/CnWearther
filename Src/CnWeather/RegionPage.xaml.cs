using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CnWeather
{
    public sealed partial class RegionPage : Page
    {
        public RegionPage()
        {
            this.InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (RegionSelector.SelectedItem != null)
            {
                var selectedRegion = (RegionSelector.SelectedItem as ListViewItem).Content.ToString();
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["SavedCity"] = selectedRegion;
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Selection Required",
                    Content = "Please select a region before continuing.",
                    CloseButtonText = "OK"
                };
                dialog.ShowAsync();
            }
        }
    }
}

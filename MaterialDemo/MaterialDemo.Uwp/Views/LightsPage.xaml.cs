﻿using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MaterialDemo.Uwp.Views
{
    public sealed partial class LightsPage : Page
    {
        public LightsPage()
        {
            this.InitializeComponent();

            MapsComboBox.ItemsSource = new List<NormalMapItem>
            {
                new NormalMapItem {Name = "Texture", ImageUriString = "ms-appx:///Images/NormalMap.png"},
                new NormalMapItem {Name = "Broken Glass", ImageUriString = "ms-appx:///Images/BrokenGlass_NormalMap.png"}
            };

            MapsComboBox.SelectedIndex = 0;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                materialBrush.ImageUriString = (e.AddedItems[0] as NormalMapItem)?.ImageUriString;
            }
        }
    }

    internal class NormalMapItem
    {
        public string Name { get; set; }
        public string ImageUriString { get; set; }
    }
}

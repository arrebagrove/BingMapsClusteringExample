using BingMapsClusteringEngine;
using System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BingMapsClusteringExample
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private ItemLocationCollection _mockData;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Button Handlers

        private async void GenerateData_Clicked(object sender, RoutedEventArgs e)
        {
            MyMap.Children.Clear();

            int size;

            if (string.IsNullOrWhiteSpace(EntitySize.Text) ||
                !int.TryParse(EntitySize.Text, out size))
            {
                var dialog = new MessageDialog("Invalid size.");
                await dialog.ShowAsync();
                return;
            }

            GenerateMockData(size);
        }

        private void ViewAllData_Clicked(object sender, RoutedEventArgs e)
        {
            MyMap.Children.Clear();

            for (int i = 0; i < _mockData.Count; i++)
            {
                var pin = new Pushpin();
                pin.Tag = _mockData[i].Item;
                MapLayer.SetPosition(pin, _mockData[i].Location);
                MyMap.Children.Add(pin);
            }
        }

        private void PointClusterData_Clicked(object sender, RoutedEventArgs e)
        {
            MyMap.Children.Clear();

            //Create an instance of the Point Based clustering layer
            var layer = new PointBasedClusteredLayer();

            //Add event handlers to create the pushpins
            layer.CreateItemPushpin += CreateItemPushpin;
            layer.CreateClusteredItemPushpin += CreateClusteredItemPushpin;

            MyMap.Children.Add(layer);

            //Add mock data to layer
            layer.Items.AddRange(_mockData);
        }

        private void GridClusterData_Clicked(object sender, RoutedEventArgs e)
        {
            MyMap.Children.Clear();

            //Create an instance of the Grid Based clustering layer
            var layer = new GridBasedClusteredLayer();

            //Add event handlers to create the pushpins
            layer.CreateItemPushpin += CreateItemPushpin;
            layer.CreateClusteredItemPushpin += CreateClusteredItemPushpin;

            //Add mock data to layer
            layer.Items.AddRange(_mockData);
            MyMap.Children.Add(layer);     
        }

        #endregion

        #region Pushpin Callback Methods

        private UIElement CreateItemPushpin(object item)
        {
            var pin = new Pushpin()
            {
                Tag = item
            };

            pin.Tapped += pin_Tapped;

            return pin;
        }

        private UIElement CreateClusteredItemPushpin(ClusteredPoint clusterInfo)
        {
            var pin = new Pushpin()
            {
                Background = new SolidColorBrush(Colors.Red),
                Text = "+",
                Tag = clusterInfo                
            };

            pin.Tapped += pin_Tapped;

            return pin;
        }

        #endregion

        #region Private Helper Methods

        private async void pin_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            string msg = string.Empty;

            if (sender is FrameworkElement)
            {
                var tag = (sender as FrameworkElement).Tag;

                if (tag is ItemLocation)
                {
                    var item = (tag as ItemLocation).Item;

                    if (item is string)
                    {
                        msg = item as string;
                    }
                }
                else if (tag is ClusteredPoint)
                {
                    msg = (tag as ClusteredPoint).ItemIndices.Count + " items in cluster.";
                }
            }

            if (!string.IsNullOrEmpty(msg))
            {
                var dialog = new MessageDialog(msg);
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Method that generates mock Entity data
        /// </summary>
        private void GenerateMockData(int numEntities)
        {
            GenerateBtn.IsEnabled = false;
            ViewAllBtn.IsEnabled = false;
            PointBtn.IsEnabled = false;
            GridBtn.IsEnabled = false;

            _mockData = new ItemLocationCollection();

            Random rand = new Random();

            object item;
            Location loc;

            for (int i = 0; i < numEntities; i++)
            {
                item = "Location number: " + i;

                loc = new Location()
                {
                    Latitude = rand.NextDouble() * 180 - 90,
                    Longitude = rand.NextDouble() * 360 - 180
                };

                _mockData.Add(item, loc);
            }

            GenerateBtn.IsEnabled = true;
            ViewAllBtn.IsEnabled = true;
            PointBtn.IsEnabled = true;
            GridBtn.IsEnabled = true;
        }

        #endregion
    }
}

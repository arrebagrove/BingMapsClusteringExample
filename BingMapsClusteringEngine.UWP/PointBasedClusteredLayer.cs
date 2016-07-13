/*****************************************************************************************************
 * 
 * This class uses the Point Based clustering algorithm to sort data into clusters based on zoom level 
 * and is derived from this code is based on: 
 * http://bingmapsv7modules.codeplex.com/wikipage?title=Point%20Based%20Clustering
 * 
 * This algorithm sorts all data into clusters based on zoom level and the current map view. It takes 
 * the first item in the list of items and groups all nearby items into it's location. The next 
 * ungrouped item is then used to do the same until all the items have been alocated. This results in 
 * clusters always being the same at a specific zoom level which means that there is no data points that 
 * jump between clusters as you pan. 
 * 
 * Tests have found that this clustering algorithm works well for up to 30,000 data points or less.
 * 
 * Code Approach:
 * 
 * //Create an instance of the Point Based clustering layer
 * var layer = new PointBasedClusteredLayer();
 * 
 * //Add event handlers to create the pushpins
 * layer.CreateItemPushpin += CreateItemPushpin;
 * layer.CreateClusteredItemPushpin += CreateClusteredItemPushpin;
 * 
 * //Add mock data to layer
 * layer.Items.AddRange(_mockData);
 * 
 * //Add layer to map
 * MyMap.Children.Add(layer);
 *  
 * 
 * XAML Approach:
 * 
 * <Page ...
 *  xmlns:ce="using:BingMapsClusteringEngine"/>
 * 
 * <m:Map Name="MyMap" Credentials="YOUR_BING_MAPS_KEY">   
 *      <m:Map.Children>
 *          <ce:PointBasedClusteredLayer Name="PointLayer"
 *                  CreateItemPushpin="CreateItemPushpin" 
 *                  CreateClusteredItemPushpin="CreateClusteredItemPushpin"/>
 *      </m:Map.Children>
 * </m:Map>
 * 
 * Add data from code using PointLayer.Items.Add or PointLayer.Items.AddRange
 * 
 *****************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace BingMapsClusteringEngine
{
    /// <summary>
    /// This class uses the Point Based clustering algorithm to sort data into clusters based on zoom level.
    /// </summary>
    public class PointBasedClusteredLayer : BaseClusteredLayer
    {
        #region Private Properties

        private IList<ClusteredPoint> _clusteredData;

        #endregion

        #region Constructor

        public PointBasedClusteredLayer()
            : base()
        {
        }

        #endregion

        #region Private Methods

        internal override async void Cluster()
        {
            if (_map != null && _currentZoomLevel <= _maxZoomLevel)
            {
                var pixels = new List<Point>();
                _map.TryLocationsToPixels(_allLocations, pixels);

                int maxX = (int)Math.Ceiling(_map.ActualWidth + ClusterRadius);
                int maxY = (int)Math.Ceiling(_map.ActualHeight + ClusterRadius);

                await Task.Run(() =>
                {
                    var clusteredData = new List<ClusteredPoint>();

                    if (_items != null && _items.Count > 0)
                    {
                        double tileZoomRatio = 256 * Math.Pow(2, _currentZoomLevel);
                        Point pixel;
                        bool IsInCluster;

                        //Itirate through the data
                        for (int i = 0; i < _items.Count; i++)
                        {
                            var entity = _items[i];
                            pixel = pixels[i];
                            IsInCluster = false;

                            if (pixel.X < -ClusterRadius)
                            {
                                pixel.X += tileZoomRatio;
                            }
                            else if (pixel.X > maxX + ClusterRadius)
                            {
                                pixel.X -= tileZoomRatio;
                            }

                            //Check to see if the pin is within the bounds of the viewable map
                            if (pixel != null && pixel.X <= maxX && pixel.Y <= maxY && pixel.X >= -ClusterRadius && pixel.Y >= -ClusterRadius)
                            {
                                foreach (var cluster in clusteredData)
                                {
                                    //See if pixel fits into any existing clusters
                                    if (pixel.Y >= cluster.Top && pixel.Y <= cluster.Bottom &&
                                        ((cluster.Left <= cluster.Right && pixel.X >= cluster.Left && pixel.X <= cluster.Right) ||
                                        (cluster.Left >= cluster.Right && (pixel.X >= cluster.Left || pixel.X <= cluster.Right))))
                                    {
                                        cluster.ItemIndices.Add(i);
                                        IsInCluster = true;
                                        break;
                                    }
                                }

                                //If entity is not in a cluster then it does not fit an existing cluster
                                if (!IsInCluster)
                                {
                                    ClusteredPoint cluster = new ClusteredPoint()
                                    {
                                        Location = _allLocations[i],
                                        Left = pixel.X - ClusterRadius,
                                        Right = pixel.X + ClusterRadius,
                                        Top = pixel.Y - ClusterRadius,
                                        Bottom = pixel.Y + ClusterRadius,
                                        Zoom = _currentZoomLevel,
                                        ItemIndices = new List<int>() { i }
                                    };

                                    if (cluster.Left < 0)
                                    {
                                        cluster.Left += tileZoomRatio;
                                    }

                                    if (cluster.Right > tileZoomRatio)
                                    {
                                        cluster.Right -= tileZoomRatio;
                                    }

                                    clusteredData.Add(cluster);
                                }
                            }
                        }
                    }

                    _clusteredData = clusteredData;
                });
            }

            Render();
        }
 
        internal override void Render()
        {
            if (_map != null)
            {
                _baseLayer.Children.Clear();

                UIElement pin;

                foreach (var c in _clusteredData)
                {
                    if (c != null)
                    {
                        if (c.ItemIndices.Count == 1)
                        {
                            var item = _items[c.ItemIndices[0]];
                            pin = GetPin(item);
                        }
                        else
                        {
                            pin = GetClustedPin(c);
                        }

                        MapLayer.SetPosition(pin, c.Location);
                        _baseLayer.Children.Add(pin);
                    }
                }
            }
        }

        #endregion
    }
}
/*****************************************************************************************************
 * 
 * This class uses the Grid Based clustering algorithm to sort data into clusters based on the current map view 
 * and is derived from this code base: http://bingmapsv7modules.codeplex.com/wikipage?title=Client%20Side%20Clustering 
 * 
 * This algorithm is very fast and clusters the data that is only in the current map view. This algorithm 
 * recalculates the clusters every time the map moves (both when zooming and panning). As a result this causes 
 * the grid cells that are used for calculating clusters to move and thus results in some clusters moving on 
 * the screen when the map is moved the slighest bit. This makes for a less ideal user experience however this 
 * is a trade off for performance. This algorithm is recommended for data sets of 50,000 data points or less.
 * 
 * 
 * Code Approach:
 * 
 * //Create an instance of the Grid Based clustering layer
 * var layer = new GridBasedClusteredLayer();
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
 *          <ce:GridBasedClusteredLayer Name="GridLayer"
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
    /// This class uses the Grid Based clustering algorithm to sort data into clusters based on the current map view.
    /// </summary>
    public class GridBasedClusteredLayer : BaseClusteredLayer
    {       
        #region Private Properties

        private ClusteredPoint[] clusters;

        #endregion

        #region Constructor

        public GridBasedClusteredLayer()
            : base()
        {
        }

        #endregion

        #region Private Methods

        internal override async void Cluster()
        {
            if (_map != null && _items != null && _items.Count > 0)
            {
                var pixels = new List<Point>();
                _map.TryLocationsToPixels(_allLocations, pixels);

                int gridSize = ClusterRadius * 2;

                int numXCells = (int)Math.Ceiling(_map.ActualWidth / gridSize);
                int numYCells = (int)Math.Ceiling(_map.ActualHeight / gridSize);

                int maxX = (int)Math.Ceiling(_map.ActualWidth + ClusterRadius);
                int maxY = (int)Math.Ceiling(_map.ActualHeight + ClusterRadius);

                clusters = await Task.Run<ClusteredPoint[]>(() =>
                {
                    int numCells = numXCells * numYCells;

                    ClusteredPoint[] clusteredData = new ClusteredPoint[numCells];

                    Point pixel;
                    int k, j, key;

                    //Itirate through the data
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var entity = _items[i];
                        pixel = pixels[i];

                        //Check to see if the pin is within the bounds of the viewable map
                        if (pixel != null && pixel.X <= maxX && pixel.Y <= maxY && pixel.X >= -ClusterRadius && pixel.Y >= -ClusterRadius)
                        {
                            //calculate the grid position on the map of where the location is located
                            k = (int)Math.Floor(pixel.X / gridSize);
                            j = (int)Math.Floor(pixel.Y / gridSize);

                            //calculates the grid location in the array
                            key = k + j * numXCells;

                            if (key >= 0 && key < numCells)
                            {
                                if (clusteredData[key] == null)
                                {
                                    clusteredData[key] = new ClusteredPoint()
                                    {
                                        Location = _allLocations[i],
                                        ItemIndices = new List<int>(){ i },
                                        Zoom = _currentZoomLevel
                                    };
                                }
                                else
                                {
                                    clusteredData[key].ItemIndices.Add(i);
                                }
                            }
                        }
                    }

                    return clusteredData;
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

                foreach (var c in clusters)
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
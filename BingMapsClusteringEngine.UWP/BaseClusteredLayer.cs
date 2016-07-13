using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BingMapsClusteringEngine
{
    public abstract class BaseClusteredLayer : Panel
    {
        #region Internal Properties

        internal Map _map;
        internal MapLayer _parentLayer;
        internal MapLayer _baseLayer;
        internal int _currentZoomLevel;
        internal Geocoordinate _center;

        internal const int _maxZoomLevel = 21;

        internal ItemLocationCollection _items;
        internal LocationCollection _allLocations;

        #endregion

        #region Constructor 

        public BaseClusteredLayer()
        {
            _items = new ItemLocationCollection();
            _items.CollectionChanged += () =>
            {
                _allLocations.Clear();

                foreach (var i in _items)
                {
                    _allLocations.Add(i.Location);
                }

                Cluster();
            };

            _allLocations = new LocationCollection();

            this.Loaded += (s, e) =>
            {
                DependencyObject parent = this;
                while (parent != null && !(parent is Map))
                {
                    parent = VisualTreeHelper.GetParent(parent);

                    if (parent is MapLayer)
                    {
                        _parentLayer = parent as MapLayer;
                    }
                }

                if (parent != null && _parentLayer != null)
                {
                    _map = parent as Map;
                    Init();
                }
            };

            this.Unloaded += (s, e) =>
            {
                if (_map != null)
                {
                    _map.ViewChangeEnded -= _map_ViewChangeEnded;
                    _map.SizeChanged -= _map_SizeChanged;
                }
            };
        }

        #endregion

        #region Public Events

        public delegate UIElement ItemRenderCallback(object item);
        public event ItemRenderCallback CreateItemPushpin;

        public delegate UIElement ClusteredItemRenderCallback(ClusteredPoint clusterInfo);
        public event ClusteredItemRenderCallback CreateClusteredItemPushpin;

        #endregion

        #region Public Properties

        public ItemLocationCollection Items
        {
            get
            {
                return _items;
            }
        }

        private int clusterRadius = 45;
        public int ClusterRadius
        {
            get
            {
                return clusterRadius;
            }
            set
            {
                if (value > 0)
                {
                    clusterRadius = value;
                    Cluster();
                }
            }
        }

        #endregion

        #region Internal Methods

        internal UIElement GetPin(object item)
        {
            if (CreateItemPushpin != null)
            {
                return CreateItemPushpin(item);
            }

            return null;
        }

        internal UIElement GetClustedPin(ClusteredPoint clusterInfo)
        {
            if (CreateItemPushpin != null)
            {
                return CreateClusteredItemPushpin(clusterInfo);
            }

            return null;
        }

        #endregion

        #region Abstract Methods

        internal abstract void Cluster();

        internal abstract void Render();

        #endregion

        #region Private Methods

        private void Init()
        {
            _baseLayer = new MapLayer();
            _parentLayer.Children.Add(_baseLayer);

            _currentZoomLevel = (int)Math.Round(_map.ZoomLevel);
            _map.ViewChangeEnded += _map_ViewChangeEnded;
            _map.SizeChanged += _map_SizeChanged;

            Cluster();
        }

        private void _map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Cluster();
        }

        private void _map_ViewChangeEnded(object sender, ViewChangeEndedEventArgs e)
        {
            int zoom = (int)Math.Round(_map.ZoomLevel);

            //Only recluster if the map has moved
            if (_center != _map.Center || _currentZoomLevel != zoom)
            {
                _currentZoomLevel = zoom;
                _center = _map.Center;
                Cluster();
            }
        }

        #endregion
    }
}

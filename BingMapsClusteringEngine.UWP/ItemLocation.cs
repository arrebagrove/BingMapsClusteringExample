using Windows.Devices.Geolocation;

namespace BingMapsClusteringEngine
{
    public class ItemLocation
    {
        public ItemLocation(object item, Geocoordinate location)
        {
            Item = item;
            Location = location;
        }

        public object Item { get; set; }
        public Geocoordinate Location { get; set; }
    }
}

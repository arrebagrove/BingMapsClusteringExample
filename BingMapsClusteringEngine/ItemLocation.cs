using Bing.Maps;

namespace BingMapsClusteringEngine
{
    public class ItemLocation
    {
        public ItemLocation(object item, Location location)
        {
            Item = item;
            Location = location;
        }

        public object Item { get; set; }
        public Location Location { get; set; }
    }
}

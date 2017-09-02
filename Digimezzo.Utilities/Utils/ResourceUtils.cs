using System.Windows;
using System.Windows.Media;

namespace Digimezzo.Utilities.Utils
{
    public static class ResourceUtils
    {
        public static string GetStringResource(string resourceName)
        {
            object resource = Application.Current.TryFindResource(resourceName);
            return resource == null ? resourceName : resource.ToString();
        }

        public static Geometry GetGeometryResource(string resourceName)
        {
            return (Geometry)Application.Current.TryFindResource(resourceName);
        }
    }
}

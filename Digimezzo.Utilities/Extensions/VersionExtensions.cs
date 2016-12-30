using System;

namespace Digimezzo.Utilities.Extensions
{
    public static class VersionExtensions
    {
        public static string FormatVersion(this Version version)
        {
            //  {0}: Major Version,
            //  {1}: Minor Version,
            //  {2}: Build Number,
            //  {3}: Revision

            if (version.Build != 0)
            {
                return string.Format("{0}.{1}.{2} (Build {3})", version.Major, version.Minor, version.Build, version.Revision);
            }
            else
            {
                return string.Format("{0}.{1} (Build {2})", version.Major, version.Minor, version.Revision);
            }
        }
    }
}

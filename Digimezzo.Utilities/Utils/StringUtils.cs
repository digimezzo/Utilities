using System;
using System.Linq;

namespace Digimezzo.Utilities.Utils
{
    public static class StringUtils
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input)) return input;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}

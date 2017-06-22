using System.Text.RegularExpressions;

namespace Digimezzo.Utilities.Utils
{
    public static class ValidationUtils
    {
        public static bool IsUrl(string word)
        {

            bool retVal = false;

            // As found at http://www.dotnetfunda.com/codes/show/1519/regex-pattern-to-validate-url
            Match m = Regex.Match(word, @"^(file|http|https):/{2}[a-zA-Z./&\d_-]+", RegexOptions.IgnoreCase);

            if ((m.Success))
            {
                retVal = true;
            }

            return retVal;
        }

        public static bool IsEmail(string word)
        {

            bool retVal = false;

            // As found at http://www.regular-expressions.info/email.html
            Match m = Regex.Match(word, @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase);

            if ((m.Success))
            {
                retVal = true;
            }

            return retVal;
        }
    }
}

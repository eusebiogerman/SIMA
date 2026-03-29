
namespace SIMA.ExtensionsHelper
{
    public static class Extensions
    {
        #region String Extension
        public static string defaultCategory(this string str)
        {
            return "All Category";
        }
        public static string getDefaultEmptyCat(this string str)
        {
            return str.Equals(str.defaultCategory()) ? string.Empty : str;
        }
        public static string isNull(this string str, string exp)
        {
            return string.IsNullOrEmpty(str) ? exp : str;
        }
        #endregion

        #region Int Extension
        public static int isNone(this int num, int excepval)
        {
            return (num == -1 ? excepval : num);
        }
        #endregion
    }
}

namespace Jering.Utilities
{
    public static class StringExtensions
    {
        public static string firstCharToLowercase(this string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}

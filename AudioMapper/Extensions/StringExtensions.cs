namespace DatabaseHelper.Extensions
{
    public static class StringExtensions
    {
        public static string AddTrailingBackslash(this string source)
        {
            return source.AddTrailingString("\\");
        }

        public static string AddTrailingChar(this string source, char append)
        {
            return source?.EndsWith($"{append}") ?? false ? source : source + append;
        }

        public static string AddTrailingDirectorySeparator(this string source)
        {
            return source.AddTrailingChar(System.IO.Path.DirectorySeparatorChar);
        }

        public static string AddTrailingForwardslash(this string source)
        {
            return source.AddTrailingString("/");
        }

        public static string AddTrailingString(this string source, string append)
        {
            return source?.EndsWith(append) ?? false ? source : source + append;
        }

        public static bool HasNoValue(this string source)
        {
            return !HasValue(source);
        }

        public static bool HasValue(this string source)
        {
            return !string.IsNullOrWhiteSpace(source);
        }

        public static string SafeTrim(this string source)
        {
            return source.HasNoValue() ? string.Empty : source.Trim();
        }
    }
}
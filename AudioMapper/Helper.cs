using System;

namespace AudioMapper
{
    internal class Helper
    {
        public static void ConsumeExceptions(Action action)
        {
            try
            {
                action();
            }
            catch { }
        }

        public static T ConsumeExceptions<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        public static void NothingButMemes()
        {
        }
    }
}
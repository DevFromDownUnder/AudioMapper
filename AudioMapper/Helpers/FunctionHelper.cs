using System;

namespace AudioMapper.Helpers
{
    internal class FunctionHelper
    {
        public static void ConsumeExceptions(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
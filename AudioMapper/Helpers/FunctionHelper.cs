using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMapper.Helpers
{
    class FunctionHelper
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

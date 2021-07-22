using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioMapper.Extensions
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> Flatten(this Exception exception)
        {
            var ex = exception;

            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            };
        }

        public static IEnumerable<string> FlattenExceptionMessages(this Exception exception)
        {
            return exception?.Flatten()?.Select((ex) => ex.Message);
        }

        public static string FlattenExceptionMessagesToString(this Exception exception)
        {
            return string.Join(Environment.NewLine, exception?.FlattenExceptionMessages() ?? new List<string>());
        }
    }
}
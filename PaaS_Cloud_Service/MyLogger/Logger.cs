using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLogger
{
    public interface ILogger
    {
        void Information(string message);
        void Information(string fmt, params object[] vars);
        void Information(Exception exception, string fmt, params object[] vars);

        void Warning(string message);
        void Error(string message);
        void Error(string message, params object[] vars);

        void TraceApi(string componentName, string method, TimeSpan timespan, string properties);

    }

    public class Logger : ILogger
    {
        public void Information(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Information(string fmt, params object[] vars)
        {
            Trace.TraceInformation(fmt, vars);
        }

        public void Information(Exception exception, string fmt, params object[] vars)
        {
            var msg = String.Format(fmt, vars);
            Trace.TraceInformation(string.Format(fmt, vars) + ";Exception Details={0}", exception.ToString());
        }

        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Error(string message, params object[] vars)
        {
            string err = String.Concat("Error message: ", String.Format(message, vars));
            Trace.TraceError(message);
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string properties)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString(), ";properties:", properties);
            Trace.TraceInformation(message);
        }
    }
}

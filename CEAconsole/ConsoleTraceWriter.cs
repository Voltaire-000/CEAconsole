using Newtonsoft.Json.Serialization;
using System.Diagnostics;

namespace CEAconsole
{
    public class ConsoleTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception? ex)
        {
            Debug.WriteLine(message);
        }
    }
}
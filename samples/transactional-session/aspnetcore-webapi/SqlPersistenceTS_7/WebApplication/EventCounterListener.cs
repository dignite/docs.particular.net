using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using NServiceBus.Logging;

public class EventCounterListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.Equals("Microsoft.Data.SqlClient.EventSource"))
        {
            var options = new Dictionary<string, string>();
            options.Add("EventCounterIntervalSec", "10");
            EnableEvents(eventSource, EventLevel.Informational, EventKeywords.None, options);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.Payload.FirstOrDefault(p => p is IDictionary<string, object> x && x.ContainsKey("Name")) is IDictionary<string, object> counters)
        {
            if (counters.TryGetValue("DisplayName", out object name) && name is string cntName
                                                                     && counters.TryGetValue("Mean", out object value) && value is double cntValue)
            {
                // print event counter's name and mean value
                Console.WriteLine($"{cntName}\t\t{cntValue}");
            }
        }
    }
}
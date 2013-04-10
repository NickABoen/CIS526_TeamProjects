using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CIS526_QueueManager
{
    public delegate IList<object> NewMessageHandler(string action, object data);

    /// <summary>
    /// An interface for the consumer to continually recieve data from the queue.
    /// </summary>
    public interface IMessageQueueConsumer
        : IDisposable
    {
        /// <summary>
        /// Raised when data is received from the queue.
        /// </summary>
        event NewMessageHandler NewMessage;

        /// <summary>
        /// Starts recieving data from the queue.
        /// </summary>
        void BeginProcessing();

        /// <summary>
        /// Stops recieving data from the queue.
        /// </summary>
        void StopProcessing();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;

namespace CIS526_QueueManager
{
    public delegate IList<T> NewMessageHandler<T>(string action, IList<T> course);

    /// <summary>
    /// An interface for the consumer to continually recieve data from the queue.
    /// </summary>
    public interface IMessageQueueConsumer<T>
        : IDisposable
    {
        /// <summary>
        /// Raised when data is received from the queue.
        /// </summary>
        event NewMessageHandler<T> NewMessage;

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
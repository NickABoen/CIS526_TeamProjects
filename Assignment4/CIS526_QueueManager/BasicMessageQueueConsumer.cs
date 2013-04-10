using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;

namespace CIS526_QueueManager
{
    /// <summary>
    /// A basic implementation of the MessageQueueConsumer.
    /// </summary>
    /// <typeparam name="T">IModel type</typeparam>
    public class BasicMessageQueueConsumer
        : IMessageQueueConsumer
    {
        MessageQueue _queue;
        private bool _recieving;

        public BasicMessageQueueConsumer(string queueName, IMessageFormatter formatter)
        {
            if (MessageQueue.Exists(queueName))
                _queue = new MessageQueue(queueName);
            else
                _queue = MessageQueue.Create(queueName);
            _queue.Formatter = formatter;
            _queue.ReceiveCompleted += _queue_ReceiveCompleted;
        }

        #region IMessageQueueConsumer members

        public event NewMessageHandler NewMessage;

        public void BeginProcessing()
        {
            _recieving = true;
            _queue.BeginReceive();
        }

        public void StopProcessing()
        {
            _recieving = false;
        }

        public void Dispose()
        {
            StopProcessing();
            _queue.Close();
            _queue.Dispose();
        }

        #endregion

        private void _queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (!_recieving)
                return;

            Message recievedMessage = _queue.EndReceive(e.AsyncResult);
            //Let what ever owns this class process the data.
            recievedMessage.Body = NewMessage(recievedMessage.Label, recievedMessage.Body);

            //Send the processed data back into the queue.
            _queue.Send(recievedMessage);
            //Look for the next message.
            _queue.BeginReceive();
        }
    }
}
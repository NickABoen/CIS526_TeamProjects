using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;

namespace CIS526_QueueManager
{
    /// <summary>
    /// A basic implementation of the IMessageQueuePublisher
    /// </summary>
    /// <typeparam name="T">IModel type</typeparam>
    public class BasicMessageQueueProducer
        : IMessageQueueProducer
    {
        private MessageQueue _queue;

        public BasicMessageQueueProducer(string queueName, IMessageFormatter formatter)
        {
            if (MessageQueue.Exists(queueName))
                _queue = new MessageQueue(queueName);
            else
                _queue = MessageQueue.Create(queueName);
            _queue.Formatter = formatter;
        }

        #region IMessageQueueProducer members

        public IList<object> GetAll()
        {
            //Send a message to Get T.
            string messageId = sendMessage("GET", null);

            //Wait for the database to respond.
            Message messageToReceive;
            try
            {
                messageToReceive = _queue.ReceiveById(messageId);
            }
            catch
            {
                messageToReceive = null;
            }

            //If the message is not null then the database gave use something. Return that.
            if (messageToReceive != null)
                return (List<object>)messageToReceive.Body;
            //Otherwise the database did not respond.
            return null;
        }

        public void Update(IList<object> data)
        {
            sendMessage("UPDATE", data);
        }

        public void Create(IList<object> data)
        {
            sendMessage("CREATE", data);
        }

        public void Remove(IList<object> data)
        {
            sendMessage("REMOVE", data);
        }

        public void Dispose()
        {
            _queue.Close();
            _queue.Dispose();
        }

        #endregion IMessageQueueProducer members

        /// <summary>
        /// Puts a message on the queue with the given action as the label and the data as the body.
        /// </summary>
        /// <param name="action">Action for the database to perform.</param>
        /// <param name="data">Data for the database to use.</param>
        /// <returns>The id of the message. This can be used to get the response from the queue.</returns>
        private string sendMessage(string action, IList<object> data)
        {
            Message messageToSend = new Message();
            messageToSend.Label = action;
            messageToSend.Body = data;

            _queue.Send(messageToSend);

            return messageToSend.Id;
        }
    }
}
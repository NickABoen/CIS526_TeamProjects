using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Messaging;
using CIS726_Assignment2.Models;

namespace CIS726_Assignment2
{
    /// <summary>
    /// An interface for the Producer to push messages onto a queue.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    public interface IMessageQueuePublisher<T> 
        : IDisposable 
        where T : IModel
    {
        /// <summary>
        /// Retives all models from the data base.
        /// </summary>
        /// <returns></returns>
        IList<T> GetAll();

        /// <summary>
        /// Updates all models contained in the list.
        /// </summary>
        /// <param name="data"></param>
        void Update(IList<T> data);

        /// <summary>
        /// Creates all models in the database.
        /// </summary>
        /// <param name="data"></param>
        void Create(IList<T> data);

        /// <summary>
        /// Removes all models contains in the list from the database.
        /// </summary>
        /// <param name="data"></param>
        void Remove(IList<T> data);
    }

    /// <summary>
    /// A basic implementation of the IMessageQueuePublisher
    /// </summary>
    /// <typeparam name="T">IModel type</typeparam>
    public class BasicMessageQueuePublisher<T> 
        : IMessageQueuePublisher<T>
        where T : IModel
    {
        private MessageQueue _queue;

        public BasicMessageQueuePublisher(string queueName, IMessageFormatter formatter)
        {
            if (MessageQueue.Exists(queueName))
                _queue = new MessageQueue(queueName);
            else
                _queue = MessageQueue.Create(queueName);
            _queue.Formatter = formatter;
        }

        #region IMessageQueueProducer members

        public IList<T> GetAll()
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
                return (List<T>)messageToReceive.Body;
            //Otherwise the database did not respond.
            return null;
        }

        public void Update(IList<T> data)
        {
            sendMessage("UPDATE", data);
        }

        public void Create(IList<T> data)
        {
            sendMessage("CREATE", data);
        }

        public void Remove(IList<T> data)
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
        private string sendMessage(string action, IList<T> data)
        {
            Message messageToSend = new Message();
            messageToSend.Label = action;
            messageToSend.Body = data;

            _queue.Send(messageToSend);

            return messageToSend.Id;
        }
    }

    /// <summary>
    /// Handler for the NewMessage event.
    /// </summary>
    /// <param name="data">Data recived from the queue.</param>
    public delegate IList<T> NewMessageHandler<T>(string action, object data);

    /// <summary>
    /// An interface for the consumer to continually recieve data from the queue.
    /// </summary>
    public interface IMessageQueueConsumer<T>
        : IDisposable
        where T : IModel
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

    /// <summary>
    /// A basic implementation of the MessageQueueConsumer.
    /// </summary>
    /// <typeparam name="T">IModel type</typeparam>
    public class BasicMessageQueueConsumer<T> 
        : IMessageQueueConsumer<T>
        where T : IModel
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

        public event NewMessageHandler<T> NewMessage;

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
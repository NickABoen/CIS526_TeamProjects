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
    public class BasicMessageQueueProducer<T>
        : IMessageQueueProducer<T>
    {
        private MessageQueue _producerQueue = null; //This is the queue used by the Controller
        private MessageQueue _consumerQueue = null; //This is the queue used by the database

        private Guid _id;

        public BasicMessageQueueProducer(string queueName, IMessageFormatter formatter)
        {
            Util.CreateProducerAndConsumerQueues(queueName,
                out _producerQueue,
                out _consumerQueue);
            _producerQueue.Formatter = new RequestFormatter();
            _consumerQueue.Formatter = new ResponseFormatter();
            _id = Guid.NewGuid();
        }

        #region IMessageQueueProducer members

        public IList<T> GetAll()
        {
            sendMessage("GET", null);
            return reciveMessage();
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
            _producerQueue.Close();
            _producerQueue.Dispose();

            _consumerQueue.Close();
            _consumerQueue.Dispose();
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
            messageToSend.Label = _id.ToString();
            messageToSend.Body = new Request()
            {
                ID = _id,
                Action = action,
                Data = data
            };

            _consumerQueue.Send(messageToSend);

            return messageToSend.Id;
        }

        private IList<T> reciveMessage()
        {
            Response response = null;
            while (response == null)
            {
                foreach (Message m in _consumerQueue.GetAllMessages())
                {
                    if (m.Label == _id.ToString())
                        response = (Response)_consumerQueue.ReceiveById(m.Id).Body;
                }
            }

            if (response.Success)
            {
                return (IList<T>)response.Result;
            }
            return null;
        }
    }
}
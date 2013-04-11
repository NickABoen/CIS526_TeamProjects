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
    public class BasicMessageQueueConsumer<T>
        : IMessageQueueConsumer<T>
    {
        MessageQueue _producerQueue; //This is the queue used by the Controller
        MessageQueue _consumerQueue; //This is the queue used by the database

        private bool _recieving;

        public BasicMessageQueueConsumer(string queueName, IMessageFormatter formatter)
        {
            Util.CreateProducerAndConsumerQueues(queueName,
                out _producerQueue,
                out _consumerQueue);
            _consumerQueue.Formatter = new ResponseFormatter();
            _producerQueue.ReceiveCompleted += _queue_ReceiveCompleted;
            _producerQueue.Formatter = new RequestFormatter();
        }

        #region IMessageQueueConsumer members

        public event NewMessageHandler<T> NewMessage;

        public void BeginProcessing()
        {
            _recieving = true;
            _producerQueue.BeginReceive();
        }

        public void StopProcessing()
        {
            _recieving = false;
        }

        public void Dispose()
        {
            StopProcessing();
            _producerQueue.Close();
            _producerQueue.Dispose();

            _consumerQueue.Close();
            _consumerQueue.Dispose();
        }

        #endregion

        private void _queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (!_recieving)
                return;

            Message recievedMessage = _producerQueue.EndReceive(e.AsyncResult);
            //Let what ever owns this class process the data.
            Request request = (Request)recievedMessage.Body;
            Response response = new Response()
                {
                    ID = request.ID
                };
            try
            {
                response.Result = NewMessage(request.Action, (IList<T>)request.Data);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Result = ex;
                response.Success = false;
            }

            //Send the processed data back into the queue.
            _consumerQueue.Send(new Message()
                {
                    Label = request.ID.ToString(),
                    Body = response
                });
            //Look for the next message.
            _producerQueue.BeginReceive();
        }
    }
}
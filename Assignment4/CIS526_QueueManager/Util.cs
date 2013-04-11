using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;

namespace CIS526_QueueManager
{
    public class Util
    {
        public static void CreateProducerAndConsumerQueues(string queueName, 
            out MessageQueue producerQueue, 
            out MessageQueue consumerQueue)
        {
            string producerQueueName = queueName + "-p";
            if (MessageQueue.Exists(producerQueueName))
                producerQueue = new MessageQueue(producerQueueName);
            else
                producerQueue = MessageQueue.Create(producerQueueName);

            string consumerQueueName = queueName + "-c";
            if (MessageQueue.Exists(consumerQueueName))
                consumerQueue = new MessageQueue(consumerQueueName);
            else
                consumerQueue = MessageQueue.Create(consumerQueueName);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;

namespace CIS526_QueueManager
{
    public class Request
    {
        public Guid ID { get; set; }
        public string Action { get; set; }
        public object Data { get; set; }
    }

    public class Response
    {
        public Guid ID { get; set; }
        public bool Success { get; set; }
        public object Result { get; set; }
    }

    public class RequestFormatter : IMessageFormatter
    {
        public bool CanRead(Message message)
        {
            return message.Body is Request;
        }

        public object Read(Message message)
        {
            return message.Body;
        }

        public void Write(Message message, object obj)
        {
            message.Body = obj;
        }

        public object Clone()
        {
            return this;
        }
    }

    public class ResponseFormatter : IMessageFormatter
    {
        public bool CanRead(Message message)
        {
            return message.Body is Response;
        }

        public object Read(Message message)
        {
            return message.Body;
        }

        public void Write(Message message, object obj)
        {
            message.Body = obj;
        }

        public object Clone()
        {
            return this;
        }
    }
}
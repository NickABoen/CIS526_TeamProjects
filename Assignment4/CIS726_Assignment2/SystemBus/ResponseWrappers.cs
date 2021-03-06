﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Serialization;
using System.Web;

namespace CIS726_Assignment2.SystemBus
{
    /// <summary>
    /// Wrapper for the response from the database.
    /// </summary>
    /// <typeparam name="T">Type of the result.</typeparam>
    public class Response<T>
    {
        /// <summary>
        /// ID of the message.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// If there was an error, this will contain the message from the exception. Otherwise null.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Contains the results from the request.
        /// </summary>
        public T Result { get; set; }
    }

    /// <summary>
    /// Handles formatting Responses of type T.
    /// </summary>
    /// <typeparam name="T">Type of the response.</typeparam>
    public class ResponseFormatter<T> : IMessageFormatter
    {
        /// <summary>
        /// This is the actual type of the response.
        /// </summary>
        private Type responseType = typeof(Response<T>);

        public bool CanRead(Message message)
        {
            return true;
        }

        /// <summary>
        /// Based off the example here: http://www.novokshanov.com/2012/02/datacontractserializer-with-msmq/
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public object Read(Message message)
        {
            DataContractSerializer serializer = new DataContractSerializer(responseType);
            return serializer.ReadObject(message.BodyStream);
        }

        /// <summary>
        /// Based off th eexample here: http://www.novokshanov.com/2012/02/datacontractserializer-with-msmq/
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        public void Write(Message message, object obj)
        {
            //if (!(obj is Response))
            //    throw new ArgumentException("Obj must be a response type.");

            MemoryStream stream = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(responseType);
            serializer.WriteObject(stream, obj);

            stream.Position = 0;

            message.BodyStream = stream;
        }

        public object Clone()
        {
            return new ResponseFormatter<T>();
        }
    }
}
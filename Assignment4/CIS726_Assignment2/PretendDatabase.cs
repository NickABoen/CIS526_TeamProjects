using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;
using CIS726_Assignment2.Models;
using CIS726_Assignment2.Repositories;

namespace CIS726_Assignment2
{
    public class PretendDatabase
    {
        private IMessageQueueConsumer<Course> _courseQueue;
        private IStorageContext<Course> _courseContext;

        public PretendDatabase()
        {
            _courseContext = new StorageContext<Course>(new CourseDBContext());
            _courseQueue = new BasicMessageQueueConsumer<Course>(@".\Private$\CourseQueue", new XmlMessageFormatter());
            _courseQueue.NewMessage += _courseQueue_NewMessage;

            _courseQueue.BeginProcessing();
        }

        private IList<Course> _courseQueue_NewMessage(string action, object data)
        {
            switch (action)
            {
                case "GET":
                    return _courseContext.Set().ToList();
                case "CREATE":
                    foreach(Course course in (IList<Course>)data)
                        _courseContext.Add((Course)data);
                    _courseContext.SaveChanges();
                    break;
                case "UPDATE":
                    foreach (Course course in (IList<Course>)data)
                        _courseContext.Edit((Course)data);
                    _courseContext.SaveChanges();
                    break;
                case "REMOVE":
                    foreach (Course course in (IList<Course>)data)
                        _courseContext.Remove((Course)data);
                    _courseContext.SaveChanges();
                    break;
            }

            //If no other case returned then just return null;
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using CIS526_Database.Models;
using CIS526_QueueManager;

namespace CIS526_Database
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private IMessageQueueConsumer _coursesQueue;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitQueues();
        }

        //This will hopefully be called at the end of the application's lifecycle.
        protected void Application_End()
        {
            //Dispose all queues.
            _coursesQueue.Dispose();
        }

        private void InitQueues()
        {
            _coursesQueue = new BasicMessageQueueConsumer(
                @"/.Private$/Courses", 
                new XmlMessageFormatter() 
                { 
                    TargetTypes = new Type[] { typeof(Course) }
                });
            _coursesQueue.NewMessage += _coursesQueue_NewMessage;
            _coursesQueue.BeginProcessing();
        }

        private IList<object> _coursesQueue_NewMessage(string action, object data)
        {
            throw new NotImplementedException();
        }
    }
}
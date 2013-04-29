using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using MessageParser.Models;
using MessageParser;
using System.Data.SqlClient;
using AuthParser.Models;
using System.Windows.Forms;

using System.Windows.Forms;
using System.Threading;

namespace AuthParser
{
    class Program
    {
        static AccountDBContext context;
        static ObjectMessageQueue queue;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Application.StartupPath);

            context = new AccountDBContext();

            ObjectMessageQueue.InitializeQueue(ObjectMessageQueue.AUTH_REQUEST);
            queue = new ObjectMessageQueue();

            while (true)
            {
                try
                {
                    Object obj = queue.receiveObject(ObjectMessageQueue.AUTH_REQUEST);
                    new Thread(ProcessRequest).Start(obj);
                    //ProcessRequest(obj);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    continue;
                }
            }
        }

        static void ProcessRequest(object obj)
        {
            try
            {
                GenericRequest gen_req = obj as GenericRequest;
                if (gen_req == null)
                {
                    Console.WriteLine("Error processing request, it is not a request object!");
                    queue.sendObject(null, ObjectMessageQueue.AUTH_RESPONSE);
                }
                else
                {
                    Console.WriteLine("Fetching data...");
                    AuthProcessor parser = new AuthProcessor(context, gen_req);
                    try
                    {
                        Object result = typeof(AuthProcessor).GetMethod(gen_req.Method.ToString()).Invoke(parser, null);
                        Console.WriteLine("It worked!");
                        queue.sendResponse(result, ObjectMessageQueue.AUTH_RESPONSE);
                    }
                    catch (SqlException)
                    {
                        Console.WriteLine("An error occurred.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

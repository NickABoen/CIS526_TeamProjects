using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MessageParser;

namespace CIS726_Assignment2.Controllers
{
    public class ControllerCache<T>
    {
        private List<T> allEntities;
        private Dictionary<int, T> entityDetails = new Dictionary<int, T>();

        public List<T> GetAll()
        {
            if (allEntities == null)
                allEntities = Request<T>.GetAll("A", "B");
            return allEntities;
        }

        public T Get(int id)
        {
            if (!entityDetails.ContainsKey(id))
                entityDetails[id] = Request<T>.GetItemByID(id, "A", "B");
            return entityDetails[id];
        }

        public void Clear()
        {
            allEntities = null;
            entityDetails.Clear();
        }
    }
}
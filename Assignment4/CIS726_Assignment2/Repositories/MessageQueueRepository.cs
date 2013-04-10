using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CIS526_Database.Models;
using CIS526_QueueManager;
using System.Linq.Expressions;

namespace CIS726_Assignment2.Repositories
{
    /// <summary>
    /// Implements the GenericRepository interface to use a queue under the hood. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueueRepository<T> 
        : IGenericRepository<T> 
        where T : IModel
    {
        private IMessageQueueProducer _publisher;

        public MessageQueueRepository(IMessageQueueProducer publisher)
        {
            _publisher = publisher;
        }

        public IQueryable<T> GetAll()
        {
            return (IQueryable<T>)_publisher.GetAll().AsQueryable();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return ((IQueryable<T>)_publisher.GetAll()).AsQueryable().Where(predicate);
        }

        public T Find(int id)
        {
            //TODO make this better.
            //Currently querires the list for a specific id.
            return ((IQueryable<T>)_publisher.GetAll().AsQueryable()).Where((x) => x.ID == id).First();
        }

        public void Add(T entity)
        {
            _publisher.Create(new List<object>() { entity });
        }

        public void Remove(T entity)
        {
            _publisher.Remove(new List<object>() { entity });
        }

        public void Edit(T entity)
        {
            //I think this should be taken care of.
        }

        public void UpdateValues(T entity, T item)
        {
            _publisher.Update(new List<object>() { item });
        }

        public void SaveChanges()
        {
            //This should be done automatically.
        }

        public void Dispose()
        {
            _publisher.Dispose();
        }
    }
}
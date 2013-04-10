using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CIS726_Assignment2.Models;

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
        private IMessageQueuePublisher<T> _publisher;

        public MessageQueueRepository(IMessageQueuePublisher<T> publisher)
        {
            _publisher = publisher;
        }

        public IQueryable<T> GetAll()
        {
            return _publisher.GetAll().AsQueryable();
        }

        public IQueryable<T> Where(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return _publisher.GetAll().AsQueryable().Where(predicate);
        }

        public T Find(int id)
        {
            //TODO make this better.
            //Currently querires the list for a specific id.
            return _publisher.GetAll().AsQueryable().Where((x) => x.ID == id).First();
        }

        public void Add(T entity)
        {
            _publisher.Create(new List<T>() { entity });
        }

        public void Remove(T entity)
        {
            _publisher.Remove(new List<T>() { entity });
        }

        public void Edit(T entity)
        {
            //I think this should be taken care of.
        }

        public void UpdateValues(T entity, T item)
        {
            _publisher.Update(new List<T>() { item });
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
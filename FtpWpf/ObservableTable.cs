using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace FtpWpf
{
    public sealed class ObservableTable<T> : INotifyCollectionChanged, ITable<T> where T : class
    {
        private readonly Table<T> _table;

        public ObservableTable(ref DataContext dbContext)
        {
            _table = dbContext.GetTable<T>();

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Expression Expression => _table.AsQueryable().Expression;
        public Type ElementType => _table.AsQueryable().ElementType;
        public IQueryProvider Provider => _table.AsQueryable().Provider;

        public IEnumerator<T> GetEnumerator()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void InsertOnSubmit(T entity)
        {
            _table.InsertOnSubmit(entity);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity));
        }

        public void Attach(T entity)
        {
            _table.Attach(entity);
        }

        public void DeleteOnSubmit(T entity)
        {
            var index = IndexOf(entity);

            _table.DeleteOnSubmit(entity);

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity, index));
        }

        public int IndexOf(T entity)
        {
            return _table.ToList().IndexOf(entity);
        }
    }
}
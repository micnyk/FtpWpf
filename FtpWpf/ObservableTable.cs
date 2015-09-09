using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FtpWpf
{
    public sealed class ObservableTable<T> : INotifyCollectionChanged, ITable<T> where T : class
    {
        private Table<T> _table;

        public Expression Expression => _table.AsQueryable().Expression;
        public Type ElementType => _table.AsQueryable().ElementType;
        public IQueryProvider Provider => _table.AsQueryable().Provider;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ObservableTable(ref DataContext dbContext)
        {
            _table = dbContext.GetTable<T>();

            if(CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void InsertOnSubmit(T entity)
        {
            _table.InsertOnSubmit(entity);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity));
        }

        public void Attach(T entity)
        {
            _table.Attach(entity);
        }

        public void DeleteOnSubmit(T entity)
        {
            var index = IndexOf(entity);

            _table.DeleteOnSubmit(entity);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity, index));
        }

        public int IndexOf(T entity)
        {
            return _table.ToList().IndexOf(entity);
        }
    }
}

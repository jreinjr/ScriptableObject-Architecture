using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectArchitecture
{
    public class Collection<T> : BaseCollection, IEnumerable<T>, IStackTraceObject
    {
        public new T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                T oldValue = _list[index];
                _list[index] = value;
                RaiseItemRemoved(oldValue);
                RaiseItemAdded(value);
            }
        }

        [SerializeField]
        private List<T> _list = new List<T>();

        [SerializeField]
        private GameEventBase<T> _onItemAddedEvent;

        [SerializeField]
        private GameEventBase<T> _onItemRemovedEvent;

        public GameEventBase<T> OnItemAddedEvent { get { return _onItemAddedEvent; } }
        public GameEventBase<T> OnItemRemovedEvent { get { return _onItemRemovedEvent; } }

        public List<StackTraceEntry> StackTraces { get { return _stackTraces; } }
        private List<StackTraceEntry> _stackTraces = new List<StackTraceEntry>();

        public override IList List
        {
            get
            {
                return _list;
            }
        }
        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public void Add(T obj)
        {
            _list.Add(obj);
            RaiseItemAdded(obj);
        }
        public void Remove(T obj)
        {
            if (_list.Contains(obj))
            {
                _list.Remove(obj);
                RaiseItemRemoved(obj);
            }
        }
        public void Clear()
        {
            var removedItems = new List<T>(_list);
            _list.Clear();

            for (int i = 0; i < removedItems.Count; i++)
                RaiseItemRemoved(removedItems[i]);
        }
        public bool Contains(T value)
        {
            return _list.Contains(value);
        }
        public int IndexOf(T value)
        {
            return _list.IndexOf(value);
        }
        public void RemoveAt(int index)
        {
            T item = _list[index];
            _list.RemoveAt(index);
            RaiseItemRemoved(item);
        }
        public void Insert(int index, T value)
        {
            _list.Insert(index, value);
            RaiseItemAdded(value);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public override string ToString()
        {
            return "Collection<" + typeof(T) + ">(" + Count + ")";
        }
        public T[] ToArray() {
            return _list.ToArray();
        }

        public void AddStackTrace()
        {
#if UNITY_EDITOR
            if (SOArchitecturePreferences.IsDebugEnabled)
                _stackTraces.Insert(0, StackTraceEntry.Create());
#endif
        }
        public void AddStackTrace(object value)
        {
#if UNITY_EDITOR
            if (SOArchitecturePreferences.IsDebugEnabled)
                _stackTraces.Insert(0, StackTraceEntry.Create(value));
#endif
        }

        private void RaiseItemAdded(T item)
        {
            AddStackTrace(item);

            if (_onItemAddedEvent != null)
                _onItemAddedEvent.Raise(item);
        }
        private void RaiseItemRemoved(T item)
        {
            AddStackTrace(item);

            if (_onItemRemovedEvent != null)
                _onItemRemovedEvent.Raise(item);
        }
    }
}

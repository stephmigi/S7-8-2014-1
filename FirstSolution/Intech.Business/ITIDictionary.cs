﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intech.Business
{
    public class ITIDictionary <TKey, TValue>
    {
        private int _count;
        Bucket[] _buckets;
        static readonly int[] _primeNumbers = new int[]{ 11, 23, 47, 97, 199, 397, 809 };
        IDicStrat<TKey> _strategy;

        private class Bucket
        {
            public readonly TKey Key;
            public TValue Value;
            public Bucket Next;

            public Bucket(TKey key, TValue value, Bucket bucket)
            {
                Key = key;
                Value = value;
                Next = bucket;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public ITIDictionary()
        {
            _buckets = new Bucket[11];
            _strategy = new DefaultStrategy();
        }

        public ITIDictionary(IDicStrat<TKey> strategy)
        {
            if (strategy == null) throw new ArgumentException();
            _buckets = new Bucket[11];
            _strategy = strategy;
        }

        private void Grow()
        {
            int newCapacity = _primeNumbers[Array.IndexOf(_primeNumbers, _buckets.Length) + 1];
            Bucket[] newBuckets = new Bucket[newCapacity];

            for (int i = 0; i < _buckets.Length; i++)
            {
                Bucket b = _buckets[i];
                while (b != null)
                {
                    int newSlot = Math.Abs(_strategy.ComputeHashCode(b.Key) % newBuckets.Length);
                    var oldNext = b.Next;
                    b.Next = newBuckets[newSlot];
                    newBuckets[newSlot] = b;
                    b = oldNext;
                }
            }
            _buckets = newBuckets;
        }

        public bool Remove(TKey key)
        {
            int slot = Math.Abs(_strategy.ComputeHashCode(key) % _buckets.Length);
            Bucket b = _buckets[slot];
            if (b == null)
            {
                throw new InvalidOperationException("Key doesn't exist.");
            }
            else
            {
                Bucket previous = null;
                while(b != null)
                {
                    if (_strategy.IsItEqual(b.Key, key))
                    {
                        if (previous != null)
                        {
                            previous.Next = b.Next;
                        }
                        else
                        {
                            _buckets[slot] = b.Next;
                        }
                        b = null;
                        _count--;
                        return true;
                    }
                    previous = b;
                    b = b.Next;
                }
                return false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            AddOrReplace(key, value, true);
        }

        private void AddOrReplace(TKey key, TValue value, bool isAdd)
        {
            int hash = _strategy.ComputeHashCode(key);
            int slot = Math.Abs(hash % _buckets.Length);
            Bucket b = _buckets[slot];

            if (b == null)
            {
                AddNewBucket(key, value, slot);
            }
            else
            {
                do
                {
                    if (_strategy.IsItEqual(b.Key, key))
                    {
                        if (isAdd)
                            throw new InvalidOperationException("Can't add existing key.");
                        b.Value = value;
                        return;
                    }
                    b = b.Next;
                }
                while (b != null);
                AddNewBucket(key, value, slot);
            }
        }

        private Bucket AddNewBucket(TKey key, TValue value, int slot)
        {
            _count++;

            // new element is added in first position in bucket
            var b = new Bucket(key, value, _buckets[slot]);
            _buckets[slot] = b;

            // if buckets contain more than 3 elements, increase _buckets
            int avgCount = _count / _buckets.Length;
            if (avgCount > 3)
            {
                Grow();
            }

            return b;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            int hash = _strategy.ComputeHashCode(key);
            int slot = Math.Abs(hash % _buckets.Length);
            Bucket b = _buckets[slot];

            while (b != null)
            {
                if (_strategy.IsItEqual(b.Key, key))
                {
                    value = b.Value;
                    return true;
                }
                b = b.Next;
            }
            return false;
        }

        /// <summary>
        /// Get a value based on a key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The given value</returns>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                    throw new KeyNotFoundException();
                return value;
            }
            set
            {
                AddOrReplace(key, value, false);
            }
        }
        
        /// <summary>
        /// A default strategy
        /// </summary>
        private class DefaultStrategy : IDicStrat<TKey>
        { 
            public int ComputeHashCode(TKey key)
            {
 	            return key.GetHashCode();
            }

            public bool IsItEqual(TKey key1, TKey key2)
            {
                return EqualityComparer<TKey>.Default.Equals(key1, key2);
            }
        }
    }

    public interface IDicStrat<T>
    {
        int ComputeHashCode(T key);
        bool IsItEqual(T key1, T key2);
    }
}

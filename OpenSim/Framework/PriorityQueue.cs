/*
 * Copyright (c) Contributors, http://aurora-sim.org/, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// PriorityQueue.cs
//
// Jim Mischel
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mischel.Collections
{
    //A priority queue from http://www.devsource.com/c/a/Languages/A-Priority-Queue-Implementation-in-C/
    // Writen by Jim Mischel

    [Serializable]
    [ComVisible(false)]
    public struct PriorityQueueItem<TValue, TPriority>
    {
        private TValue _value;
        public TValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private TPriority _priority;
        public TPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public PriorityQueueItem(TValue val, TPriority pri)
        {
            this._value = val;
            this._priority = pri;
        }
    }

    [Serializable]
    [ComVisible(false)]
    public class PriorityQueue<TValue, TPriority> : ICollection,
        IEnumerable<PriorityQueueItem<TValue, TPriority>>
    {
        private PriorityQueueItem<TValue, TPriority>[] items;

        private const Int32 DefaultCapacity = 16;
        private Int32 capacity;
        private Int32 numItems;

        private Comparison<TPriority> compareFunc;

        /// <summary>
        /// Initializes a new instance of the PriorityQueue class that is empty,
        /// has the default initial capacity, and uses the default IComparer.
        /// </summary>
        public PriorityQueue()
            : this(DefaultCapacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(Int32 initialCapacity)
            : this(initialCapacity, Comparer<TPriority>.Default)
        {
        }

        public PriorityQueue(IComparer<TPriority> comparer)
            : this(DefaultCapacity, comparer)
        {
        }

        public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
        {
            Init(initialCapacity, new Comparison<TPriority>(comparer.Compare));
        }

        public PriorityQueue(Comparison<TPriority> comparison)
            : this(DefaultCapacity, comparison)
        {
        }

        public PriorityQueue(int initialCapacity, Comparison<TPriority> comparison)
        {
            Init(initialCapacity, comparison);
        }

        private void Init(int initialCapacity, Comparison<TPriority> comparison)
        {
            numItems = 0;
            compareFunc = comparison;
            SetCapacity(initialCapacity);
        }

        public int Count
        {
            get { return numItems; }
        }

        public int Capacity
        {
            get { return items.Length; }
            set { SetCapacity(value); }
        }

        private void SetCapacity(int newCapacity)
        {
            int newCap = newCapacity;
            if (newCap < DefaultCapacity)
                newCap = DefaultCapacity;

            // throw exception if newCapacity < NumItems
            if (newCap < numItems)
                throw new ArgumentOutOfRangeException("newCapacity", "New capacity is less than Count");

            this.capacity = newCap;
            if (items == null)
            {
                items = new PriorityQueueItem<TValue, TPriority>[newCap];
                return;
            }

            // Resize the array.
            Array.Resize<PriorityQueueItem<TValue, TPriority>>(ref items, newCap);
        }

        public void Enqueue(PriorityQueueItem<TValue, TPriority> newItem)
        {
            if (numItems == capacity)
            {
                // need to increase capacity
                // grow by 50 percent
                SetCapacity((3 * Capacity) / 2);
            }

            int i = numItems;
            ++numItems;
            while ((i > 0) && (compareFunc(items[(i - 1) / 2].Priority, newItem.Priority) < 0))
            {
                items[i] = items[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            items[i] = newItem;
            //if (!VerifyQueue())
            //{
            //    Console.WriteLine("ERROR: Queue out of order!");
            //}
        }

        public void Enqueue(TValue value, TPriority priority)
        {
            Enqueue(new PriorityQueueItem<TValue, TPriority>(value, priority));
        }

        private PriorityQueueItem<TValue, TPriority> RemoveAt(Int32 index)
        {
            PriorityQueueItem<TValue, TPriority> o = items[index];
            --numItems;
            // move the last item to fill the hole
            PriorityQueueItem<TValue, TPriority> tmp = items[numItems];
            // If you forget to clear this, you have a potential memory leak.
            items[numItems] = default(PriorityQueueItem<TValue, TPriority>);
            if (numItems > 0 && index != numItems)
            {
                // If the new item is greater than its parent, bubble up.
                int i = index;
                int parent = (i - 1) / 2;
                while (compareFunc(tmp.Priority, items[parent].Priority) > 0)
                {
                    items[i] = items[parent];
                    i = parent;
                    parent = (i - 1) / 2;
                }

                // if i == index, then we didn't move the item up
                if (i == index)
                {
                    // bubble down ...
                    while (i < (numItems) / 2)
                    {
                        int j = (2 * i) + 1;
                        if ((j < numItems - 1) && (compareFunc(items[j].Priority, items[j + 1].Priority) < 0))
                        {
                            ++j;
                        }
                        if (compareFunc(items[j].Priority, tmp.Priority) <= 0)
                        {
                            break;
                        }
                        items[i] = items[j];
                        i = j;
                    }
                }
                // Be sure to store the item in its place.
                items[i] = tmp;
            }
            //if (!VerifyQueue())
            //{
            //    Console.WriteLine("ERROR: Queue out of order!");
            //}
            return o;
        }

        // Function to check that the queue is coherent.
        public bool VerifyQueue()
        {
            int i = 0;
            while (i < numItems / 2)
            {
                int leftChild = (2 * i) + 1;
                int rightChild = leftChild + 1;
                if (compareFunc(items[i].Priority, items[leftChild].Priority) < 0)
                {
                    return false;
                }
                if (rightChild < numItems && compareFunc(items[i].Priority, items[rightChild].Priority) < 0)
                {
                    return false;
                }
                ++i;
            }
            return true;
        }

        public PriorityQueueItem<TValue, TPriority> Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("The queue is empty");
            return RemoveAt(0);
        }

        public bool TryDequeue(out PriorityQueueItem<TValue, TPriority> value)
        {
            value = new PriorityQueueItem<TValue, TPriority>();
            if (Count == 0)
                return false;
            value = RemoveAt(0);
            return true;
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The passed equality comparison is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="comp">An object that implements the IEqualityComparer interface
        /// for the type of item in the collection.</param>
        public void Remove(TValue item, IEqualityComparer comparer)
        {
            // need to find the PriorityQueueItem that has the Data value of o
            for (int index = 0; index < numItems; ++index)
            {
                if (comparer.Equals(item, items[index].Value))
                {
                    RemoveAt(index);
                    return;
                }
            }
            throw new ApplicationException("The specified itemm is not in the queue.");
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The passed equality comparison is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="comp">An object that implements the IEqualityComparer interface
        /// for the type of item in the collection.</param>
        public TValue Find(TValue item, IComparer<TValue> comparer)
        {
            // need to find the PriorityQueueItem that has the Data value of o
            for (int index = 0; index < numItems; ++index)
            {
                if (comparer.Compare(item, items[index].Value) > 1)
                {
                    return items[index].Value;
                }
            }
            return default(TValue);
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The default type comparison function is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        public void Remove(TValue item)
        {
            Remove(item, EqualityComparer<TValue>.Default);
        }

        public PriorityQueueItem<TValue, TPriority> Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("The queue is empty");
            return items[0];
        }

        // Clear
        public void Clear()
        {
            for (int i = 0; i < numItems; ++i)
            {
                items[i] = default(PriorityQueueItem<TValue, TPriority>);
            }
            numItems = 0;
            TrimExcess();
        }

        /// <summary>
        /// Set the capacity to the actual number of items, if the current
        /// number of items is less than 90 percent of the current capacity.
        /// </summary>
        public void TrimExcess()
        {
            if (numItems < (float)0.9 * capacity)
            {
                SetCapacity(numItems);
            }
        }

        // Contains
        public bool Contains(TValue o)
        {
            foreach (PriorityQueueItem<TValue, TPriority> x in items)
            {
                if (x.Value.Equals(o))
                    return true;
            }
            return false;
        }

        public void CopyTo(PriorityQueueItem<TValue, TPriority>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (numItems == 0)
                return;
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex is equal to or greater than the length of the array.");
            if (numItems > (array.Length - arrayIndex))
                throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from arrayIndex to the end of the destination array.");

            for (int i = 0; i < numItems; i++)
            {
                array[arrayIndex + i] = items[i];
            }
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            this.CopyTo((PriorityQueueItem<TValue, TPriority>[])array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return items.SyncRoot; }
        }

        #endregion

        #region IEnumerable<PriorityQueueItem<TValue,TPriority>> Members

        public IEnumerator<PriorityQueueItem<TValue, TPriority>> GetEnumerator()
        {
            for (int i = 0; i < numItems; i++)
            {
                yield return items[i];
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

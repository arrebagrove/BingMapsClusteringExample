﻿using Bing.Maps;
using System;
using System.Collections.Generic;

namespace BingMapsClusteringEngine
{
    public class ItemLocationCollection : List<ItemLocation>
    {
        #region Public Events

        public delegate void CollectionChangedEvent();
        public event CollectionChangedEvent CollectionChanged;

        #endregion

        #region Public Methods

        public void Add(object item, Location location)
        {
            base.Add(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void AddRange(ItemLocationCollection items)
        {
            base.AddRange(items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void Clear()
        {
            base.Clear();

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public ItemLocation GetItemByIndex(int index)
        {
            if(index < this.Count)
            {
                return this[index];
            }

            return null;
        }

        public ItemLocationCollection GetItemsByIndex(List<int> index)
        {
            var items = new ItemLocationCollection();

            foreach (var i in index)
            {
                if (i < this.Count)
                {
                    items.Add(this[i]);
                }
            }

            return items;
        }
        
        public void Insert(int index, object item, Location location)
        {
            base.Insert(index, new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void InsertRange(int index, ItemLocationCollection items)
        {
            base.InsertRange(index, items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void Remove(object item, Location location)
        {
            base.Remove(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        #endregion
    }
}
